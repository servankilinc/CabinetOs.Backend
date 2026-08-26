using CabinetOs.Business.Abstract;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Core.Utils.Validation;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Dtos.Realtime.Queries;
using CabinetOs.Model.Dtos.Scada.Commands;
using CabinetOs.Model.Dtos.Scada.Queries;
using CabinetOs.Model.Entities;
// Ad ALIASI zorunlu: `DeviceStatus` hem bir lookup ENTITY'si hem bir enum. Ikisi
// de bu dosyanin kapsaminda; nitelenmeden yazilirsa derleyici belirsizlik hatasi
// verir. Burada gecen her `DeviceStatus` ENUM'dur.
using DeviceStatus = CabinetOs.Model.Enums.EntityEnums.DeviceStatus;

namespace CabinetOs.Business.Concrete;

/// <summary>
/// SCADA telemetrisinin yazildigi tek yer.
///
/// <b>Sicak yol.</b> Kabin basina saniyede birden fazla ingest bekleniyor; bu
/// yuzden: tek <c>SaveChangesAsync</c>, TRANSACTION YOK (tek kabin, idempotent
/// yazma — yarim kalan bir ingest bir sonrakiyle duzelir), ve DEGERI DEGISMEYEN
/// KANAL ICIN HIC YAZMA YOK.
///
/// Sozlesme: <c>docs/api-contract/07-scada-ingest.md</c> + <c>09-realtime.md</c>
/// </summary>
public class ScadaService : IScadaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;
    private readonly IDiagramNotifier _notifier;

    public ScadaService(IUnitOfWork unitOfWork, IValidationService validationService, IDiagramNotifier notifier)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _notifier = notifier;
    }

    public async Task<Result<ScadaIngestResponse>> IngestAsync(ScadaIngestRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result<ScadaIngestResponse>.Validation(validationResult.Failures, description: "Validation failed for ScadaIngestRequest");

        var cabinet = await _unitOfWork.Cabinets.GetAsync(
            where: c => c.Id == request.CabinetId && c.IsActive,
            tracking: true,
            cancellationToken: cancellationToken);

        if (cabinet == null)
            return Result<ScadaIngestResponse>.NotFound(description: "Kabin bulunamadi veya pasif durumda");

        // SCADA'si kapali bir kabine telemetri yazmak celiskilidir ve sessizce
        // kabul etmek yapilandirma hatasini gorunmez kilardi. 404 DEGIL 400:
        // kabin var, ayar yanlis.
        if (!cabinet.ScadaIsEnabled)
            return Result<ScadaIngestResponse>.Validation(
                new Dictionary<string, string[]> { ["CabinetId"] = ["Bu kabinde SCADA kapali"] },
                description: "SCADA disabled for cabinet");

        // Cozumleme sozlukleri. Kod karsilastirmasi BUYUK/KUCUK HARF DUYARSIZ,
        // cunku IX_Device_CabinetId_ExternalCode SQL Server collation'i altinda
        // oyle davraniyor: "mod-01" ve "MOD-01" veritabaninda ayni satirdir,
        // .NET'in ordinal karsilastirmasi ise onlari ayirir ve ayni cihaz
        // "taninmadi" diye atlanirdi.
        var devices = await _unitOfWork.Devices.GetAllAsync(
            where: d => d.CabinetId == cabinet.Id && d.IsActive && d.ExternalCode != null,
            tracking: true,
            cancellationToken: cancellationToken) ?? [];

        var deviceByCode = new Dictionary<string, Device>(StringComparer.OrdinalIgnoreCase);
        foreach (var device in devices)
            deviceByCode[device.ExternalCode!] = device;

        var deviceIds = devices.Select(d => d.Id).ToList();
        var channels = deviceIds.Count == 0
            ? []
            : await _unitOfWork.IoChannels.GetAllAsync(
                where: c => deviceIds.Contains(c.DeviceId) && c.IsEnabled,
                tracking: true,
                cancellationToken: cancellationToken) ?? [];

        var channelByRef = channels.ToDictionary(c => (c.DeviceId, c.ChannelNumber));

        var now = DateTime.UtcNow;
        var response = new ScadaIngestResponse { ReceivedAtUtc = now };
        var channelChanges = new List<ChannelValueChange>();
        var statusChanges = new List<DeviceStatusChange>();

        foreach (var reading in request.Devices)
        {
            if (!deviceByCode.TryGetValue(reading.ExternalCode, out var device))
            {
                // Tanimayan referans TUM istegi dusurmez: sahada bir modul
                // eklendiginde o kabinin butun telemetrisi durmamali.
                Skip(response, reading.ExternalCode);
                // Cihaz cozulemedigi icin kanallari da cozulemez; hepsi atlanir.
                foreach (var channel in reading.Channels)
                    Skip(response, $"{reading.ExternalCode}/ch:{channel.ChannelNumber}");
                continue;
            }

            var previousStatus = device.DeviceStatusId;
            var nextStatus = ResolveStatus(previousStatus, reading.StatusId);

            device.LastSeen = now;
            if (nextStatus != previousStatus)
            {
                device.DeviceStatusId = nextStatus;
                statusChanges.Add(new DeviceStatusChange
                {
                    DeviceId = device.Id,
                    StatusId = (DeviceStatus?)nextStatus,
                    LastSeen = now
                });
            }

            foreach (var channelReading in reading.Channels)
            {
                if (!channelByRef.TryGetValue((device.Id, channelReading.ChannelNumber), out var channel))
                {
                    Skip(response, $"{reading.ExternalCode}/ch:{channelReading.ChannelNumber}");
                    continue;
                }

                response.Accepted++;

                // DEGISMEYEN KANALA HIC DOKUNULMAZ. Iki kazanc: EF bu satiri
                // UPDATE listesine hic almaz, ve degismeyen bir deger icin
                // yayin uretilmez — 500 kanalli bir kabinde saniyede bir ingest
                // aksi halde saniyede 500 gereksiz guncelleme yayardi.
                if (string.Equals(channel.CurrentValue, channelReading.Value, StringComparison.Ordinal))
                    continue;

                channel.CurrentValue = channelReading.Value;
                channel.ValueUpdatedAt = now;
                response.Changed++;

                channelChanges.Add(new ChannelValueChange
                {
                    IoChannelId = channel.Id,
                    DeviceId = device.Id,
                    ChannelNumber = channel.ChannelNumber,
                    Value = channelReading.Value,
                    UpdatedAt = now
                });
            }
        }

        // Kabin durumu = cihazlarinin EN KOTUSU.
        var previousCabinetStatus = cabinet.DeviceStatusId;
        cabinet.DeviceStatusId = WorstStatus(devices);
        cabinet.LastSeen = now;
        cabinet.ScadaLastIngestAt = now;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Yayin YAZMADAN SONRA. Once yayinlansaydi, kaydetme hata verdiginde
        // istemciler veritabaninda olmayan bir degeri gostermis olurdu.
        if (channelChanges.Count > 0)
            await _notifier.ChannelValuesChangedAsync(cabinet.Id, channelChanges, cancellationToken);

        if (statusChanges.Count > 0)
            await _notifier.DeviceStatusesChangedAsync(cabinet.Id, statusChanges, cancellationToken);

        // Kabin olayi HER ingest'te gider (durum degismese bile): govdesindeki
        // scadaLastIngestAt, arayuzdeki "son veri" tazeligi gostergesinin tek
        // kaynagi. Kabin basina saniyede bir olay, kanal basina degil.
        await _notifier.CabinetStatusChangedAsync(new CabinetStatusChange
        {
            CabinetId = cabinet.Id,
            StatusId = (DeviceStatus?)cabinet.DeviceStatusId,
            LastSeen = cabinet.LastSeen,
            ScadaLastIngestAt = cabinet.ScadaLastIngestAt
        }, cancellationToken);

        _ = previousCabinetStatus; // durum farki bugun kullanilmiyor; olay kosulsuz gidiyor

        return Result<ScadaIngestResponse>.Success(response);
    }

    public async Task<int> SweepStaleDevicesAsync(TimeSpan staleAfter, CancellationToken cancellationToken = default)
    {
        var threshold = DateTime.UtcNow - staleAfter;

        // Hangi kabinlerde bayat cihaz var? Once bunu ogrenip SADECE o kabinlerin
        // cihazlarini yukluyoruz: kabin durumunu yeniden hesaplamak icin bayat
        // olanlar degil, o kabindeki TUM cihazlarin durumu gerekiyor.
        var staleCabinetIds = await _unitOfWork.Devices.GetAllAsync(
            select: d => d.CabinetId,
            where: d => d.IsActive
                     && d.LastSeen != null
                     && d.LastSeen < threshold
                     && d.DeviceStatusId != (int)DeviceStatus.Offline,
            cancellationToken: cancellationToken) ?? [];

        var cabinetIds = staleCabinetIds.Distinct().ToList();
        if (cabinetIds.Count == 0) return 0;

        var devices = await _unitOfWork.Devices.GetAllAsync(
            where: d => cabinetIds.Contains(d.CabinetId) && d.IsActive,
            tracking: true,
            cancellationToken: cancellationToken) ?? [];

        var cabinets = await _unitOfWork.Cabinets.GetAllAsync(
            where: c => cabinetIds.Contains(c.Id),
            tracking: true,
            cancellationToken: cancellationToken) ?? [];

        var changesByCabinet = new Dictionary<Guid, List<DeviceStatusChange>>();
        int swept = 0;

        foreach (var device in devices)
        {
            bool isStale = device.LastSeen != null
                        && device.LastSeen < threshold
                        && device.DeviceStatusId != (int)DeviceStatus.Offline;
            if (!isStale) continue;

            device.DeviceStatusId = (int)DeviceStatus.Offline;
            swept++;

            if (!changesByCabinet.TryGetValue(device.CabinetId, out var list))
                changesByCabinet[device.CabinetId] = list = [];

            list.Add(new DeviceStatusChange
            {
                DeviceId = device.Id,
                StatusId = DeviceStatus.Offline,
                LastSeen = device.LastSeen
            });
        }

        if (swept == 0) return 0;

        var devicesByCabinet = devices.GroupBy(d => d.CabinetId).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var cabinet in cabinets)
        {
            if (devicesByCabinet.TryGetValue(cabinet.Id, out var cabinetDevices))
                cabinet.DeviceStatusId = WorstStatus(cabinetDevices);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var (cabinetId, changes) in changesByCabinet)
        {
            await _notifier.DeviceStatusesChangedAsync(cabinetId, changes, cancellationToken);

            var cabinet = cabinets.FirstOrDefault(c => c.Id == cabinetId);
            if (cabinet == null) continue;

            await _notifier.CabinetStatusChangedAsync(new CabinetStatusChange
            {
                CabinetId = cabinet.Id,
                StatusId = (DeviceStatus?)cabinet.DeviceStatusId,
                LastSeen = cabinet.LastSeen,
                ScadaLastIngestAt = cabinet.ScadaLastIngestAt
            }, cancellationToken);
        }

        return swept;
    }

    // ==================== YARDIMCILAR ====================

    private static void Skip(ScadaIngestResponse response, string reference)
    {
        response.Skipped++;
        // Referans listesi KIRPILIR ama sayac kirpilmaz: yanlis yapilandirilmis bir
        // SCADA yuzlerce bilinmeyen kanal gonderebilir ve govde patlardi.
        if (response.SkippedRefs.Count < ScadaIngestResponse.MaxSkippedRefs)
            response.SkippedRefs.Add(reference);
    }

    /// <summary>
    /// Cihazin yeni durumu.
    ///
    /// SCADA bir durum bildirdiyse o kazanir. Bildirmediyse (<c>null</c> =
    /// "dokunma") kural sudur: <b>supurucu Offline'a ceker, ingest geri getirir.</b>
    /// Cihazdan okuma geldiyse cihaz yasiyordur; ondan haber alinmadigi icin
    /// Offline'a cekilmis bir kaydi oylece birakmak, telemetri yeniden aksa bile
    /// kabini sonsuza dek olu gostermek olurdu.
    ///
    /// <c>Warning</c>/<c>Critical</c>/<c>Maintenance</c> ise SCADA'nin BILEREK
    /// yazdigi durumlardir; sirf paket geldi diye Online'a cevrilmezler.
    /// </summary>
    private static int? ResolveStatus(int? current, DeviceStatus? reported)
    {
        if (reported.HasValue) return (int)reported.Value;
        if (current == null || current == (int)DeviceStatus.Offline) return (int)DeviceStatus.Online;
        return current;
    }

    /// <summary>
    /// Kabin rozeti icin cihaz durumlarinin en kotusu.
    ///
    /// Enum'un SAYISAL sirasi kullanilamaz: <c>Maintenance = 4</c> en buyuk deger
    /// ama en kotu durum degil — bakimdaki tek bir cihaz tum kabini "Bakimda"
    /// gosterirdi. Siralama burada ACIKCA tanimlanir.
    ///
    /// Durumu <c>null</c> olan cihaz (hic telemetri alinmamis) hesaba KATILMAZ;
    /// hicbir cihazin durumu yoksa kabin durumu da <c>null</c> kalir.
    /// </summary>
    private static int? WorstStatus(IEnumerable<Device> devices)
    {
        int? worst = null;
        int worstRank = -1;

        foreach (var device in devices)
        {
            if (device.DeviceStatusId is not int status) continue;
            int rank = SeverityRank(status);
            if (rank <= worstRank) continue;
            worstRank = rank;
            worst = status;
        }

        return worst;
    }

    private static int SeverityRank(int status) => status switch
    {
        (int)DeviceStatus.Critical => 4,
        // Offline, Warning'den daha kotu: uyari veren bir cihazdan hala haber
        // aliniyor, offline olandan alinmiyor.
        (int)DeviceStatus.Offline => 3,
        (int)DeviceStatus.Warning => 2,
        // Bakim BILEREK yapilir; alarm degildir ama "her sey normal" de degildir.
        (int)DeviceStatus.Maintenance => 1,
        _ => 0
    };
}
