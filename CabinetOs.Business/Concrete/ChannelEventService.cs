using AutoMapper;
using CabinetOs.Business.Abstract;
using CabinetOs.Business.Utils.DiagramNotifier;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Core.Utils.Validation;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Dtos.ChannelEvent.Queries;
using CabinetOs.Model.Dtos.Realtime.Queries;
using CabinetOs.Model.Dtos.Scada.Commands;
using CabinetOs.Model.Entities;
using Microsoft.Extensions.Logging;

using DeviceStatus = CabinetOs.Model.Enums.EntityEnums.DeviceStatus;
using PinDirection = CabinetOs.Model.Enums.EntityEnums.PinDirection;

namespace CabinetOs.Business.Concrete;

/// <summary>
/// Kanal olaylarinin okuma yolu. 
/// SCADA telemetrisinin yazildigi tek yer.
///
/// <b>Sicak yol.</b> Kart olay gudumlu tek cerceve gonderiyor; bu yuzden:
/// istek basina TEK okuma, tek <c>SaveChangesAsync</c>, TRANSACTION YOK (tek
/// kanal, idempotent yazma — yarim kalan bir ingest bir sonrakiyle duzelir), ve
/// DEGERI DEGISMEYEN KANAL ICIN HIC YAZMA YOK.
///
/// <b>Basarili ingest GOVDESIZ 200 doner.</b> Eskiden bir sayac seti donuyordu
/// (<c>accepted/changed/skipped/eventsRecorded</c>); muhatabi yanlisti. SCADA
/// islenip islenmedigiyle ilgilenmez — sahada tanimsiz bir pin cikmasini tespit
/// etmesi gereken taraf BIZ'iz ve bunun yeri istegin yaniti degil <c>Warning</c>
/// log'udur.
///
/// Sozlesme: <c>docs/api-contract/07-scada-ingest.md</c> + <c>09-realtime.md</c>
/// </summary>
public class ChannelEventService : IChannelEventService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;
    private readonly IDiagramNotifier _notifier;
    private readonly ILogger<ChannelEventService> _logger;
    private readonly IMapper _mapper;

    public ChannelEventService(IUnitOfWork unitOfWork,
        IValidationService validationService,
        IDiagramNotifier notifier,
        ILogger<ChannelEventService> logger,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _notifier = notifier;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<PaginationResponse<ChannelEventDto>>> GetPagedAsync(ChannelEventQueryRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result<PaginationResponse<ChannelEventDto>>.Validation(validationResult.Failures, description: "Validation failed for ChannelEventQueryRequest");

        // Kabin kontrolu ONCE: aksi halde var olmayan bir kabin icin bos liste
        // donerdi ve "kabin yok" ile "kabinde olay yok" ayirt edilemezdi.
        // Pasif kabin DAHIL EDILIR — pasife alinmis bir kabinin gecmisi de okunabilmeli.
        var cabinetExists = await _unitOfWork.Cabinets.IsExistAsync(
            where: c => c.Id == request.CabinetId,
            cancellationToken: cancellationToken);

        if (!cabinetExists)
            return Result<PaginationResponse<ChannelEventDto>>.NotFound(description: "Kabin bulunamadi");

        var page = await _unitOfWork.ChannelEvents.GetPagedAsync(
            _mapper.ConfigurationProvider,
            request.CabinetId,
            request.IoChannelId,
            request.FromUtc,
            request.ToUtc,
            request.ToPaginationRequest(),
            cancellationToken);

        return Result<PaginationResponse<ChannelEventDto>>.Success(page);
    }
     

    public async Task<Result> IngestAsync(ScadaIngestRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Validation(validationResult.Failures, description: "Validation failed for ScadaIngestRequest");

        // Dogrulama gectiyse ayristirma da gecer — ikisi AYNI ayristiriciyi
        // kullaniyor. Yine de sessizce varsaymiyoruz: TryParse'in sonucu
        // kullanilmadan once kontrol ediliyor ki iki taraf ileride ayrisirsa
        // bu, gizli bir yanlis okuma degil gorunur bir 400 olsun.
        if (!ScadaPinAddress.TryParse(request.Pin, out var pin))
            return Result.Validation(
                new Dictionary<string, string[]> { ["Pin"] = ["Gecersiz pin adresi"] },
                description: "Invalid pin address");

        var cabinet = await _unitOfWork.Cabinets.GetAsync(
            where: c => c.Id == request.CabinetId && c.IsActive,
            tracking: true,
            cancellationToken: cancellationToken);

        if (cabinet == null)
            return Result.NotFound(description: "Kabin bulunamadi veya pasif durumda");

        // SCADA'si kapali bir kabine telemetri yazmak celiskilidir ve sessizce
        // kabul etmek yapilandirma hatasini gorunmez kilardi. 404 DEGIL 400:
        // kabin var, ayar yanlis.
        if (!cabinet.ScadaIsEnabled)
            return Result.Validation(
                new Dictionary<string, string[]> { ["CabinetId"] = ["Bu kabinde SCADA kapali"] },
                description: "SCADA disabled for cabinet");

        var now = DateTime.UtcNow;

        // KANAL TEK SORGUDA, JOIN'SIZ COZULUR. Kabin bir kontrol kartidir ve
        // IoChannel.CabinetId denormalize tutuldugu icin adres dogrudan
        // benzersiz indekse dusuyor (IX_IoChannel_CabinetId_Direction_ChannelNumber).
        // Eskiden burada kabinin BUTUN cihazlari yuklenip bir sozluk kuruluyordu;
        // tek okumali ingest cok daha sik geldigi icin o maliyet artik kabul
        // edilemezdi.
        var channel = await _unitOfWork.IoChannels.GetAsync(
            where: c => c.CabinetId == cabinet.Id
                     && c.Direction == pin.Direction
                     && c.ChannelNumber == pin.ChannelNumber
                     && c.IsEnabled,
            tracking: true,
            cancellationToken: cancellationToken);

        if (channel == null)
        {
            // K7: tanimadigimiz referans istegi DUSURMEZ. Sahada bir pin
            // baglandiginda o kabinin butun telemetrisinin durmasi, tek bir
            // bilinmeyen pinden cok daha kotudur.
            //
            // Ama sessiz atlama tespit edilemezse yanlis yapilandirilmis bir
            // SCADA aylarca 200 alip hicbir sey yazmaz. Yanit govdesiz oldugu
            // icin bu log satiri, atlamayi gorunur kilan TEK seydir.
            //
            // Bastirma/deduplikasyon YOK (bilincli): sorun cikarsa care, pin
            // basina bastirmayi IDistributedCache ile eklemektir.
            _logger.LogWarning(
                "Kabin {CabinetId}: {Pin} pini tanimsiz (ya da devre disi); telemetri atlandi.",
                cabinet.Id,
                pin.ToString());

            return Result.Success();
        }

        var device = await _unitOfWork.Devices.GetAsync(
            where: d => d.Id == channel.DeviceId,
            tracking: true,
            cancellationToken: cancellationToken);

        var statusChanges = new List<DeviceStatusChange>();
        bool deviceStatusChanged = false;

        if (device != null)
        {
            var previousStatus = device.DeviceStatusId;
            var nextStatus = ResolveStatus(previousStatus);

            device.LastSeen = now;
            if (nextStatus != previousStatus)
            {
                device.DeviceStatusId = nextStatus;
                deviceStatusChanged = true;
                statusChanges.Add(new DeviceStatusChange
                {
                    DeviceId = device.Id,
                    StatusId = (DeviceStatus?)nextStatus,
                    LastSeen = now
                });
            }
        }

        var channelChanges = new List<ChannelValueChange>();
        ChannelEvent? channelEvent = null;

        // DEGISMEYEN KANALA HIC DOKUNULMAZ. Iki kazanc: EF bu satiri UPDATE
        // listesine hic almaz, ve degismeyen bir deger icin yayin uretilmez —
        // seğiren bir sensor aksi halde her cerceve icin bir yayin dogururdu.
        if (!string.Equals(channel.CurrentValue, request.Value, StringComparison.Ordinal))
        {
            var previousValue = channel.CurrentValue;

            channel.CurrentValue = request.Value;
            channel.ValueUpdatedAt = now;

            channelChanges.Add(new ChannelValueChange
            {
                IoChannelId = channel.Id,
                DeviceId = channel.DeviceId,
                ChannelNumber = channel.ChannelNumber,
                Value = request.Value,
                UpdatedAt = now
            });

            // Anlik deger her zaman guncellenir; KALICI OLAY ise ayri bir
            // karardir ve cok daha dar bir kumeye yazilir.
            if (ShouldRecordEvent(channel, request.Value))
            {
                // Olayin SAHADA gerceklestigi an. SCADA gondermediyse kendi
                // saatimize duseriz — iki kolonun esit olmasi "damga gelmedi"
                // demektir ve bu bilgi tek bir zaman damgasi saklansaydi bir
                // daha geri getirilemezdi.
                channelEvent = new ChannelEvent
                {
                    IoChannelId = channel.Id,
                    CabinetId = cabinet.Id,
                    Value = request.Value!,
                    PreviousValue = previousValue,
                    OccurredAtUtc = request.TimestampUtc ?? now,
                    ReceivedAtUtc = now
                };

                // Olay kanal guncellemesiyle AYNI SaveChanges'te iner. Ayri bir
                // kaydetme olsaydi ikisi arasinda kalan bir hata, degeri
                // yazilmis ama olayi yazilmamis bir kanal birakirdi.
                _unitOfWork.ChannelEvents.Add(channelEvent);
            }
        }

        // KABIN DURUMU YALNIZCA GEREKTIGINDE HESAPLANIR. Cihazin durumu
        // degismediyse kabin durumu da degisemez; her cerceve icin kabinin butun
        // cihazlarini okumak, tek okumali ingest'in sikliginda savunulamazdi.
        if (deviceStatusChanged)
            cabinet.DeviceStatusId = await ComputeCabinetStatusAsync(cabinet.Id, device!, cancellationToken);

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
        // kaynagi.
        await _notifier.CabinetStatusChangedAsync(new CabinetStatusChange
        {
            CabinetId = cabinet.Id,
            StatusId = (DeviceStatus?)cabinet.DeviceStatusId,
            LastSeen = cabinet.LastSeen,
            ScadaLastIngestAt = cabinet.ScadaLastIngestAt
        }, cancellationToken);

        return Result.Success();
    }

    /// <summary>
    /// Kabin rozeti: cihaz durumlarinin en kotusu.
    ///
    /// Yalnizca DURUM alani okunuyor (tam varlik degil) — burasi sicak yol ve
    /// hesaplama icin cihazlarin baska hicbir alani gerekmiyor. Az once
    /// degistirdigimiz cihazin durumu sorguya dahil edilmez: veritabaninda hala
    /// eski degeri duruyor, dolayisiyla yeni degeri ayrica eklenir.
    /// </summary>
    private async Task<int?> ComputeCabinetStatusAsync(Guid cabinetId, Device changedDevice, CancellationToken cancellationToken)
    {
        var statuses = await _unitOfWork.Devices.GetAllAsync(
            select: d => d.DeviceStatusId,
            where: d => d.CabinetId == cabinetId && d.IsActive && d.Id != changedDevice.Id,
            cancellationToken: cancellationToken) ?? [];

        return WorstStatus([.. statuses, changedDevice.DeviceStatusId]);
    }

    public async Task<int> SetOfflineDevicesAsync(TimeSpan staleAfter, CancellationToken cancellationToken = default)
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
                cabinet.DeviceStatusId = WorstStatus(cabinetDevices.Select(d => d.DeviceStatusId));
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

    /// <summary>
    /// Bu deger degisimi kalici bir <see cref="ChannelEvent"/> olarak
    /// yazilmali mi? Cagrildigi yerde degerin DEGISTIGI zaten bilinir.
    ///
    /// Karar zinciri (sirayla):
    /// <list type="number">
    /// <item><b>Yon.</b> Yalnizca giris kanallari olay uretir. Bir cikisi biz
    /// surduugumuzde donen deger bir saha olayi degil, kendi komutumuzun
    /// yankisidir ve kaydi zaten <c>DeviceCommand</c>'dadir. <c>Bidirectional</c>
    /// da disaridadir: yonu belirsiz bir kanalin olayi da belirsizdir.</item>
    /// <item><b>Deger.</b> Okunabilir ve olay kolonuna sigan her deger yazilir.</item>
    /// </list>
    ///
    /// <b>Kanal bazinda opt-in YOK.</b> Eskiden zincirde <c>IoChannel.IsEventLogged</c>
    /// bayragi ve <c>EventTriggerValue</c> tetikleyicisi vardi; ikisi de kaldirildi.
    /// Tetikleyici tek bir degeri ifade edebiliyordu, oysa iki-uc farkli degerde de
    /// olay istenebiliyor; bayragin ise hicbir yazma yolu yoktu (her kanalda
    /// <c>false</c> kaliyor ve tablo pratikte hic yazilmiyordu). Artik giris
    /// kanalindaki HER deger degisimi olaydir.
    /// </summary>
    private static bool ShouldRecordEvent(IoChannel channel, string? value)
    {
        if (channel.Direction != PinDirection.Input) return false;

        // Deger okunamadi ("kanal var ama cevap yok"). Kaydedilecek bir DEGER
        // yok; anlik deger yine de null'a cekilir, ama olay uretilmez.
        if (value == null) return false;

        // Govde 256 karaktere kadar deger kabul ediyor, olay kolonu 32.
        // Bu turda olay kaynagi 1/0 gonderen giris pinleridir; 32 karakteri asan
        // bir deger tanim geregi bu ozelligin kaydettigi sey degildir. Sessizce
        // atlanir — istegi dusurmek, K7'nin "tanimadigi referansi atla, batch'i
        // reddetme" kuralini bozardi.
        if (value.Length > 32) return false;

        return true;
    }

    /// <summary>
    /// Cihazin yeni durumu.
    ///
    /// Kural tek: <b>supurucu Offline'a ceker, ingest geri getirir.</b> Cihazdan
    /// okuma geldiyse cihaz yasiyordur; ondan haber alinmadigi icin Offline'a
    /// cekilmis bir kaydi oylece birakmak, telemetri yeniden aksa bile kabini
    /// sonsuza dek olu gostermek olurdu.
    ///
    /// <b>SCADA artik durum BILDIRMIYOR.</b> Eski govdede bir <c>statusId</c>
    /// alani vardi ve doluysa aynen yazilirdi; kartin protokolunde boyle bir
    /// kavram olmadigi icin kaldirildi. <c>Warning</c>/<c>Critical</c>/
    /// <c>Maintenance</c> bu uctan yazilamaz — yazilmislarsa (baska bir yoldan)
    /// oldugu gibi korunurlar, sirf cerceve geldi diye Online'a cevrilmezler.
    /// </summary>
    private static int? ResolveStatus(int? current)
    {
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
    private static int? WorstStatus(IEnumerable<int?> statuses)
    {
        int? worst = null;
        int worstRank = -1;

        foreach (var candidate in statuses)
        {
            if (candidate is not int status) continue;
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
