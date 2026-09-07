using System.Text.Json;
using CabinetOs.Business.Utils;
using CabinetOs.Core.Utils;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.DeviceCommand.Commands;
using CabinetOs.Model.Dtos.DeviceCommand.Queries;
using CabinetOs.Model.Dtos.Realtime.Queries;
using CabinetOs.Model.Dtos.Scada.Commands;
using CabinetOs.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Business.Concrete;

/// <summary>
/// Kumandanin (CabinetOS -> SCADA) is akisi. Ayni sinifin CRUD tarafi uretilmis
/// iskelettir ve buraya karistirilmiyor.
///
/// Sozlesme: <c>docs/api-contract/08-scada-command.md</c>
/// </summary>
public partial class DeviceCommandService
{
    /// <summary>
    /// <c>Cabinet.ScadaCommandTimeoutMs</c> icin guvenlik siniri.
    ///
    /// Deger DTO validator'inda en az 10.000 ms olarak zorlaniyor, ama eski bir
    /// satir ya da elle yapilmis bir guncelleme 0 birakabilir; 0 ms zaman asimi,
    /// hicbir komutun gonderilememesi demektir. Ust sinir da gerekli: bu cagri
    /// SENKRON, yani istemcinin HTTP istegi bu sure boyunca acik kalir.
    /// </summary>
    private const int MinTimeoutMs = 1_000;
    private const int MaxTimeoutMs = 60_000;

    /// <summary>Gecmis ucunun tek seferde donebilecegi en fazla satir.</summary>
    public const int MaxHistoryTake = 100;

    public async Task<Result<DeviceCommandResultDto>> SendAsync(Guid deviceId, DeviceCommandSendRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result<DeviceCommandResultDto>.Validation(validationResult.Failures, description: "Validation failed for DeviceCommandSendRequest");

        // ---------------------------------------------------------- on kontroller
        // Buradaki her kontrol, DeviceCommand SATIRI YAZILMADAN ONCE calisir.
        // Sirasi onemli degil ama konumu kritik: reddedilen bir komut icin satir
        // olusursa, komut gecmisi "gonderildi ama sonuclanmadi" gorunumundeki
        // hic gonderilmemis kayitlarla dolar ve gecmis guvenilmez hale gelir.

        var device = await _unitOfWork.Devices.GetAsync(
            where: d => d.Id == deviceId && d.IsActive,
            include: i => i.Include(d => d.Cabinet),
            tracking: false,
            cancellationToken: cancellationToken);

        if (device == null)
            return Result<DeviceCommandResultDto>.NotFound(description: "Cihaz bulunamadi veya pasif durumda");

        var cabinet = device.Cabinet;
        if (cabinet == null || !cabinet.IsActive)
            return Result<DeviceCommandResultDto>.NotFound(description: "Cihazin kabini bulunamadi veya pasif durumda");

        // SCADA'si kapali kabin: kabin VAR, yanlis olan ayardir -> 404 degil 400.
        // Ingest tarafiyla ayni gerekce (07-scada-ingest.md).
        if (!cabinet.ScadaIsEnabled)
            return Result<DeviceCommandResultDto>.Validation(
                new Dictionary<string, string[]> { ["CabinetId"] = ["Bu kabinde SCADA kapalı; kumanda gönderilemez"] },
                description: "SCADA disabled for cabinet");

        if (string.IsNullOrWhiteSpace(cabinet.ScadaBaseUrl))
            return Result<DeviceCommandResultDto>.Validation(
                new Dictionary<string, string[]> { ["CabinetId"] = ["Kabinin SCADA adresi tanımlı değil"] },
                description: "SCADA base url missing");

        // NOT: Burada eskiden "dis kodu olmayan cihaza kumanda gonderilemez"
        // kontrolu vardi. Kabin = tek kontrol karti oldugu ve gonderi artik
        // cabinetId + pin tasidigi icin ExternalCode'un tel uzerinde karsiligi
        // kalmadi; kontrol de dayanaksiz kaldigi icin kaldirildi.

        // Hedef kanal KOSULSUZ zorunlu (validator de oyle diyor). Burada tekrar
        // kontrol ediliyor cunku gonderi artik kanaldan turetilen bir pin adresi
        // tasiyor: kanalsiz bir kumandanin gidecek adresi olmazdi.
        if (request.IoChannelId is not Guid channelId)
            return Result<DeviceCommandResultDto>.Validation(
                new Dictionary<string, string[]> { ["IoChannelId"] = ["Kumanda için hedef kanal zorunlu"] },
                description: "Channel is required");

        // Kanal, CIHAZLA BIRLIKTE sorgulaniyor: baska bir cihazin kanalina bu
        // cihaz uzerinden komut gonderilmesi engellenmis oluyor. "Yok" ile
        // "baska cihaza ait" ayni mesaji donuyor; ayirmak, baskasinin kanal
        // Id'lerini yoklamaya yarardi.
        var channel = await _unitOfWork.IoChannels.GetAsync(
            where: c => c.Id == channelId && c.DeviceId == deviceId,
            tracking: false,
            cancellationToken: cancellationToken);

        if (channel == null)
            return Result<DeviceCommandResultDto>.Validation(
                new Dictionary<string, string[]> { ["IoChannelId"] = ["Kanal bu cihaza ait değil"] },
                description: "Channel does not belong to device");

        if (!channel.IsEnabled)
            return Result<DeviceCommandResultDto>.Validation(
                new Dictionary<string, string[]> { ["IoChannelId"] = ["Kanal devre dışı"] },
                description: "Channel disabled");

        // Yon kontrolu. Reddedilen YALNIZCA Input; Bidirectional gecerli bir
        // kumanda hedefidir (adi geregi cikis da verebilir) ve onu reddetmek
        // mesru bir komutu engellemek olurdu.
        if (channel.Direction == PinDirection.Input)
            return Result<DeviceCommandResultDto>.Validation(
                new Dictionary<string, string[]> { ["IoChannelId"] = ["Giriş yönlü kanala kumanda gönderilemez"] },
                description: "Channel is input-only");

        // ------------------------------------------------------- NO/NC kutbu
        var polarity = await ResolvePolarityAsync(channel.Id, cancellationToken);
        if (!polarity.IsSuccess)
            return Result<DeviceCommandResultDto>.Validation(polarity.Errors!, description: polarity.Description);

        bool turnOn = request.TurnOn!.Value;
        string sentValue = ContactPolarity.ToWireValue(turnOn, polarity.Contact);

        // ------------------------------------------------------------ satir yazimi
        // Satir GONDERIMDEN ONCE yazilir. Sebep: sunucu tam bu noktada cokerse
        // geriye "gonderildi, sonucu bilinmiyor" (Sent) diye bir iz kalir. Sonra
        // yazilsaydi, sahaya gitmis bir role darbesinin hicbir kaydi olmazdi.
        var issuedAt = DateTime.UtcNow;
        var command = new DeviceCommand
        {
            DeviceId = device.Id,
            IoChannelId = channel.Id,
            CommandType = request.CommandType,
            PayloadJson = BuildPayloadJson(turnOn, sentValue, polarity.Contact),
            Status = CommandStatus.Sent,
            RequestedByUserId = ResolveRequesterId(),
            SentAt = issuedAt
        };

        await _unitOfWork.DeviceCommands.AddAndSaveAsync(command, cancellationToken);

        // ----------------------------------------------------------- gonderim
        var outcome = await _scadaCommandGateway.SendAsync(
            cabinet.ScadaBaseUrl!,
            new ScadaCommandEnvelope
            {
                CommandId = command.Id,
                CabinetId = cabinet.Id,
                Pin = ScadaPinAddress.Format(channel.Direction, channel.ChannelNumber),
                CommandType = request.CommandType,
                Value = sentValue,
                IssuedAtUtc = issuedAt
            },
            TimeSpan.FromMilliseconds(Math.Clamp(cabinet.ScadaCommandTimeoutMs, MinTimeoutMs, MaxTimeoutMs)));

        command.Status = outcome.Status;
        command.ResultMessage = outcome.Message;
        command.RespondedAt = DateTime.UtcNow;

        // SONUCUN YAZIMI IPTAL EDILEMEZ: cagiranin token'i BILEREK gecirilmiyor.
        // Istemci sekmeyi kapattigi icin bu yazim atlanirsa, sahaya gitmis ve
        // cevaplanmis bir komut sonsuza dek "Sent" gorunur.
        await _unitOfWork.DeviceCommands.UpdateAndSaveAsync(command, CancellationToken.None);

        string? requestedByName = ResolveRequesterName();

        // Yayin, komutu GONDERENE degil ayni kabini izleyen digerlerine. Gonderen
        // sonucu asagidaki HTTP yanitinda zaten aliyor.
        await _notifier.CommandCompletedAsync(cabinet.Id, new CommandCompleted
        {
            CommandId = command.Id,
            DeviceId = device.Id,
            IoChannelId = channel.Id,
            ChannelNumber = channel.ChannelNumber,
            CommandType = command.CommandType,
            Status = command.Status,
            ResultMessage = command.ResultMessage,
            RespondedAt = command.RespondedAt,
            RequestedByName = requestedByName
        }, CancellationToken.None);

        return Result<DeviceCommandResultDto>.Success(ToResultDto(command, channel.ChannelNumber, requestedByName));
    }

    /// <summary>
    /// Cihazin son kumandalari, yeniden eskiye.
    ///
    /// Bilinmeyen bir cihaz Id'si BOS LISTE dondurur, 404 degil: gecmis sorgusu
    /// icin ayrica cihazi dogrulamak her cagriya ikinci bir sorgu eklerdi ve
    /// "kumandasi yok" ile "cihazi yok" arasindaki ayrimin bu ucta bir karsiligi
    /// yok — cihaz ekrani zaten cihazi yuklemis durumda.
    /// </summary>
    public async Task<Result<ICollection<DeviceCommandResultDto>>> GetRecentAsync(Guid deviceId, int take, CancellationToken cancellationToken = default)
    {
        var rows = await _unitOfWork.DeviceCommands.GetRecentForDeviceAsync(
            deviceId,
            Math.Clamp(take, 1, MaxHistoryTake),
            cancellationToken);

        ICollection<DeviceCommandResultDto> list = rows
            .Select(row => ToResultDto(row, row.IoChannel?.ChannelNumber, row.RequesterUser?.FullName ?? row.RequesterUser?.UserName))
            .ToList();

        return Result<ICollection<DeviceCommandResultDto>>.Success(list);
    }

    // ==================== YARDIMCILAR ====================

    /// <summary>
    /// Saklanan ve SCADA'ya gonderilen payload. Istemci ham JSON gondermiyor
    /// (bkz. <see cref="DeviceCommandSendRequest"/>); string'i sunucu kuruyor ki
    /// veritabaninda duran metin ile tel uzerinde giden metin AYNI olsun.
    /// </summary>
    private static string BuildPayloadJson(bool turnOn, string value, PinFunction? polarity) =>
        JsonSerializer.Serialize(new ScadaCommandPayload(turnOn, value, polarity), ProjectJsonOptions.SerializerOptions);

     
    /// <summary>
    /// Kanalin NO/NC kutbu — hangi kontagin YUKU tasidigi.
    ///
    /// Tipik bir role kanalinda uc pin olur (COM + NO + NC), yani "ikisi de var"
    /// NORMAL durumdur ve karari kablo verir: yuk hangi kontaga cekilmisse o
    /// kazanir. Bunun bilinen bedeli, <b>kabloyu silmenin komutun anlamini
    /// degistirebilmesidir</b>; kanalda acik bir kutup alani bu riski tasimazdi
    /// ama sema genisletmemek bilincli bir tercih.
    ///
    /// Belirsizlikte varsayilan SECILMEZ: ikisi de kabloluysa hangisinin yuk
    /// oldugunu bilemeyiz ve yanlis tahmin sahada roleyi ters surerdi. Hicbiri
    /// kablolu degilse NO varsayilir (yaygin kabul) — henuz kablolanmamis bir
    /// roleyi test etmek mesru bir istektir ve engellemek gereksiz olurdu.
    /// </summary>
    private async Task<PolarityResolution> ResolvePolarityAsync(Guid ioChannelId, CancellationToken cancellationToken)
    {
        var contacts = await _unitOfWork.Pins.GetAllAsync(
            select: p => new { p.Id, p.Function },
            where: p => p.IoChannelId == ioChannelId
                     && (p.Function == PinFunction.NO || p.Function == PinFunction.NC),
            cancellationToken: cancellationToken) ?? [];

        // Kutup sorusu olmayan kanal: LED, duz dijital cikis, kuru kontak.
        if (contacts.Count == 0)
            return PolarityResolution.Resolved(null);

        var distinct = contacts.Select(c => c.Function).Distinct().ToList();
        if (distinct.Count == 1)
            return PolarityResolution.Resolved(distinct[0]);

        // Hem NO hem NC pini var -> kabloya bak.
        var contactIds = contacts.Select(c => c.Id).ToList();
        var wiredPinIds = await _unitOfWork.Connections.GetAllAsync(
            select: c => new { c.SourcePinId, c.TargetPinId },
            where: c => contactIds.Contains(c.SourcePinId) || contactIds.Contains(c.TargetPinId),
            cancellationToken: cancellationToken) ?? [];

        var wired = new HashSet<Guid>();
        foreach (var connection in wiredPinIds)
        {
            wired.Add(connection.SourcePinId);
            wired.Add(connection.TargetPinId);
        }

        var wiredFunctions = contacts
            .Where(c => wired.Contains(c.Id))
            .Select(c => c.Function)
            .Distinct()
            .ToList();

        if (wiredFunctions.Count == 1)
            return PolarityResolution.Resolved(wiredFunctions[0]);

        if (wiredFunctions.Count > 1)
            return PolarityResolution.Ambiguous();

        _logger.LogWarning(
            "Kanal {IoChannelId}: NO ve NC kontaklarinin hicbiri kablolu degil; " +
            "kutup NO varsayildi. Yuk kablolandiginda komut anlami degisebilir.",
            ioChannelId);

        return PolarityResolution.Resolved(PinFunction.NO);
    }

    /// <summary>
    /// Kutup cozumlemesinin sonucu. <c>Result&lt;T&gt;</c> yerine ozel bir tip:
    /// "cozuldu ama deger null" (kutup sorusu olmayan kanal) ile "cozulemedi"
    /// ayrimini <c>PinFunction?</c> tek basina tasiyamazdi.
    /// </summary>
    private readonly record struct PolarityResolution(bool IsSuccess, PinFunction? Contact, Dictionary<string, string[]>? Errors, string? Description)
    {
        public static PolarityResolution Resolved(PinFunction? contact) => new(true, contact, null, null);

        public static PolarityResolution Ambiguous() => new(
            false,
            null,
            new Dictionary<string, string[]>
            {
                ["IoChannelId"] = ["Bu kanalda hem NO hem NC kontağı kablolu; hangisinin yükü taşıdığı belirsiz. Kullanılmayan kabloyu kaldırın."]
            },
            "Ambiguous contact polarity");
    }

    private Guid? ResolveRequesterId()
    {
        var identifier = _httpContextManager.GetNameIdentifier();
        if (!identifier.IsSuccess) return null;
        return Guid.TryParse(identifier.Data, out var userId) ? userId : null;
    }

    private string? ResolveRequesterName()
    {
        var name = _httpContextManager.GetName();
        return name.IsSuccess ? name.Data : null;
    }

    private static DeviceCommandResultDto ToResultDto(DeviceCommand command, int? channelNumber, string? requestedByName)
    {
        var payload = ReadPayload(command.PayloadJson);

        return new DeviceCommandResultDto
        {
            Id = command.Id,
            DeviceId = command.DeviceId,
            IoChannelId = command.IoChannelId,
            ChannelNumber = channelNumber,
            CommandType = command.CommandType,
            PayloadJson = command.PayloadJson,
            SentValue = payload?.Value,
            ResolvedPolarity = payload?.Polarity,
            Status = command.Status,
            ResultMessage = command.ResultMessage,
            SentAt = command.SentAt,
            RespondedAt = command.RespondedAt,
            ElapsedMs = ElapsedMs(command.SentAt, command.RespondedAt),
            RequestedByUserId = command.RequestedByUserId,
            RequestedByName = requestedByName
        };
    }

    /// <summary>
    /// <c>SentValue</c> / <c>ResolvedPolarity</c> saklanan payload'dan OKUNUR,
    /// ayri kolonlardan degil.
    ///
    /// Sebep: gecmis ucu ile gonderim ucu AYNI sekli dondurmek zorunda
    /// (<see cref="DeviceCommandResultDto"/>) ve gecmiste elimizdeki tek kayit
    /// payload. Kutbu ayrica kolonlastirmak, ayni bilgiyi iki yerde tutup
    /// ayrismalarina izin vermek olurdu.
    ///
    /// Bozuk/eski bir payload sessizce <c>null</c> uretir: gecmis listesi tek bir
    /// okunamayan satir yuzunden 500 vermemeli.
    /// </summary>
    private static ScadaCommandPayload? ReadPayload(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson)) return null;

        try
        {
            return JsonSerializer.Deserialize<ScadaCommandPayload>(payloadJson, ProjectJsonOptions.SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Iki uc de AYNI hesabi kullanir; istemcinin tarih aritmetigi yapmasina gerek
    /// kalmaz. Negatif deger uretilmez: saat geri alinmis bir sunucuda RespondedAt
    /// SentAt'ten kucuk cikabilir ve "-3 ms" arayuzde anlamsiz olurdu.
    /// </summary>
    private static int? ElapsedMs(DateTime? sentAt, DateTime? respondedAt)
    {
        if (sentAt is not DateTime sent || respondedAt is not DateTime responded) return null;
        return (int)Math.Max(0, (responded - sent).TotalMilliseconds);
    }
}
