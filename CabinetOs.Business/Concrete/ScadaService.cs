using CabinetOs.Business.Abstract;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Core.Utils.Validation;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Dtos.Realtime.Queries;
using CabinetOs.Model.Dtos.Scada.Commands;
using CabinetOs.Model.Entities;
using Microsoft.Extensions.Logging;
// Ad ALIASI zorunlu: `DeviceStatus` hem bir lookup ENTITY'si hem bir enum. Ikisi
// de bu dosyanin kapsaminda; nitelenmeden yazilirsa derleyici belirsizlik hatasi
// verir. Burada gecen her `DeviceStatus` ENUM'dur.
using DeviceStatus = CabinetOs.Model.Enums.EntityEnums.DeviceStatus;
using PinDirection = CabinetOs.Model.Enums.EntityEnums.PinDirection;

namespace CabinetOs.Business.Concrete;

/// <summary>
/// SCADA telemetrisinin yazildigi tek yer.
///
/// <b>Sicak yol.</b> Kabin basina saniyede birden fazla ingest bekleniyor; bu
/// yuzden: tek <c>SaveChangesAsync</c>, TRANSACTION YOK (tek kabin, idempotent
/// yazma — yarim kalan bir ingest bir sonrakiyle duzelir), ve DEGERI DEGISMEYEN
/// KANAL ICIN HIC YAZMA YOK.
///
/// <b>Basarili ingest GOVDESIZ 200 doner.</b> Eskiden bir sayac seti donuyordu
/// (<c>accepted/changed/skipped/eventsRecorded</c>); muhatabi yanlisti. SCADA kac
/// okumanin islendigiyle ilgilenmez — sahada tanimsiz bir modul cikmasini tespit
/// etmesi gereken taraf BIZ'iz ve bunun yeri istegin yaniti degil <c>Warning</c>
/// log'udur. Sayaclar o log satirinin icinde, yalnizca atlanan varken yazilir.
///
/// Sozlesme: <c>docs/api-contract/07-scada-ingest.md</c> + <c>09-realtime.md</c>
/// </summary>
public class ScadaService : IScadaService
{
    /// <summary>
    /// Log satirina yazilacak tanimsiz referans ORNEGI ust siniri — liste basina
    /// ayri ayri uygulanir.
    ///
    /// Yanlis yapilandirilmis bir SCADA yuzlerce bilinmeyen referans
    /// gonderebilir; kirpma olmasaydi TEK bir log satiri megabaytlara cikardi.
    /// Sayaclar kirpilmaz, yalnizca ornek listeleri kirpilir.
    /// </summary>
    private const int MaxLoggedRefs = 50;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;
    private readonly IDiagramNotifier _notifier;
    private readonly ILogger<ScadaService> _logger;

    public ScadaService(
        IUnitOfWork unitOfWork,
        IValidationService validationService,
        IDiagramNotifier notifier,
        ILogger<ScadaService> logger)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task<Result> IngestAsync(ScadaIngestRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Validation(validationResult.Failures, description: "Validation failed for ScadaIngestRequest");

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
        var channelChanges = new List<ChannelValueChange>();
        var statusChanges = new List<DeviceStatusChange>();

        // Sayaclar artik YANITA degil LOG SATIRINA gidiyor. Yalnizca atlanan
        // varken yazildiklari icin, saglikli bir sahada hicbir sey loglanmaz;
        // bir sorun varken de "kac tanesi islendi" baglami elde kalir.
        int accepted = 0, changed = 0, eventsRecorded = 0;
        var skipTally = new SkipTally();

        // Olayin SAHADA gerceklestigi an. SCADA gondermediyse kendi saatimize
        // duseriz — iki kolonun esit olmasi "damga gelmedi" demektir ve bu bilgi
        // tek bir zaman damgasi saklansaydi bir daha geri getirilemezdi.
        var occurredAt = request.TimestampUtc ?? now;
        var events = new List<ChannelEvent>();

        foreach (var reading in request.Devices)
        {
            if (!deviceByCode.TryGetValue(reading.ExternalCode, out var device))
            {
                // Tanimayan referans TUM istegi dusurmez: sahada bir modul
                // eklendiginde o kabinin butun telemetrisi durmamali.
                //
                // Cihaz cozulemedigi icin kanallari da cozulemez; SAYACA hepsi
                // ayri ayri girer (cihaz 1 + her kanali 1), ama log satirinda
                // tek bir "MOD-09(3 kanal)" girdisi olarak gorunurler — 512
                // kanalli tanimsiz bir cihaz aksi halde tek basina 513 ornek
                // uretir ve satiri okunmaz hale getirirdi.
                skipTally.AddDevice(reading.ExternalCode, reading.Channels.Count);
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
                    skipTally.AddChannel(reading.ExternalCode, channelReading.ChannelNumber);
                    continue;
                }

                accepted++;

                // DEGISMEYEN KANALA HIC DOKUNULMAZ. Iki kazanc: EF bu satiri
                // UPDATE listesine hic almaz, ve degismeyen bir deger icin
                // yayin uretilmez — 500 kanalli bir kabinde saniyede bir ingest
                // aksi halde saniyede 500 gereksiz guncelleme yayardi.
                if (string.Equals(channel.CurrentValue, channelReading.Value, StringComparison.Ordinal))
                    continue;

                var previousValue = channel.CurrentValue;

                channel.CurrentValue = channelReading.Value;
                channel.ValueUpdatedAt = now;
                changed++;

                channelChanges.Add(new ChannelValueChange
                {
                    IoChannelId = channel.Id,
                    DeviceId = device.Id,
                    ChannelNumber = channel.ChannelNumber,
                    Value = channelReading.Value,
                    UpdatedAt = now
                });

                // Anlik deger her zaman guncellenir; KALICI OLAY ise ayri bir
                // karardir ve cok daha dar bir kumeye yazilir.
                if (ShouldRecordEvent(channel, channelReading.Value))
                {
                    events.Add(new ChannelEvent
                    {
                        IoChannelId = channel.Id,
                        CabinetId = cabinet.Id,
                        Value = channelReading.Value!,
                        PreviousValue = previousValue,
                        OccurredAtUtc = occurredAt,
                        ReceivedAtUtc = now
                    });
                    eventsRecorded++;
                }
            }
        }

        // Kabin durumu = cihazlarinin EN KOTUSU.
        var previousCabinetStatus = cabinet.DeviceStatusId;
        cabinet.DeviceStatusId = WorstStatus(devices);
        cabinet.LastSeen = now;
        cabinet.ScadaLastIngestAt = now;

        // Olaylar kanal guncellemeleriyle AYNI SaveChanges'te iner. Ayri bir
        // kaydetme olsaydi ikisi arasinda kalan bir hata, degeri yazilmis ama
        // olayi yazilmamis (ya da tersi) bir kanal birakirdi.
        foreach (var channelEvent in events)
            _unitOfWork.ChannelEvents.Add(channelEvent);

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

        // Sessiz atlamayi gorunur kilan TEK sey. Yanit gövdesizdir; SCADA kac
        // okumanin islendigiyle ilgilenmez, sahada tanimsiz bir modul cikmasini
        // tespit etmesi gereken taraf biziz.
        //
        // Bastirma/deduplikasyon YOK (bilincli): yanlis yapilandirilmis bir
        // kabin, ingest sikligi neyse o kadar satir yazar. Sorun cikarsa care,
        // referans basina bastirmayi IDistributedCache ile eklemektir.
        if (skipTally.Total > 0)
        {
            _logger.LogWarning(
                "Kabin {CabinetId}: {SkippedCount} telemetri referansi tanimsiz. " +
                "Tanimsiz cihazlar: {UnknownDevices}. Tanimsiz kanallar: {UnknownChannels}. " +
                "accepted={Accepted} changed={Changed} eventsRecorded={EventsRecorded}",
                cabinet.Id,
                skipTally.Total,
                skipTally.DescribeDevices(),
                skipTally.DescribeChannels(),
                accepted,
                changed,
                eventsRecorded);
        }

        return Result.Success();
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
    /// <item><b>Opt-in.</b> Kanal isaretli degilse yazilmaz. Bir kabinde onlarca
    /// giris pini vardir ve hepsinin gecmisi istenmiyor — kullanilmayan uclar,
    /// yedek hatlar, kurulumda salinan kanallar.</item>
    /// <item><b>Tetikleyici.</b> <c>EventTriggerValue</c> doluysa yalnizca o
    /// degere gecis olaydir: hareket sensorunde <c>0→1</c> olaydir,
    /// <c>1→0</c> degildir.</item>
    /// </list>
    /// </summary>
    private static bool ShouldRecordEvent(IoChannel channel, string? value)
    {
        if (channel.Direction != PinDirection.Input) return false;
        if (!channel.IsEventLogged) return false;

        // Deger okunamadi ("kanal var ama cevap yok"). Kaydedilecek bir DEGER
        // yok; anlik deger yine de null'a cekilir, ama olay uretilmez.
        if (value == null) return false;

        // Govde 256 karaktere kadar deger kabul ediyor, olay kolonu 32.
        // Bu turda olay kaynagi 1/0 gonderen giris pinleridir; 32 karakteri asan
        // bir deger tanim geregi bu ozelligin kaydettigi sey degildir. Sessizce
        // atlanir — istegi dusurmek, K7'nin "tanimadigi referansi atla, batch'i
        // reddetme" kuralini bozardi.
        if (value.Length > 32) return false;

        return channel.EventTriggerValue == null
            || string.Equals(channel.EventTriggerValue, value, StringComparison.Ordinal);
    }

    /// <summary>
    /// Bir ingest istegi boyunca cozumlenemeyen referanslari toplar.
    ///
    /// Cihaz ve kanal AYRI listelerde tutulur: tanimsiz bir cihazin butun
    /// kanallari da tanimsizdir, ama onlari tek tek yazmak log satirini
    /// sisirmekten baska bir sey yapmaz — cihaz girdisi kanal sayisini kendi
    /// icinde tasir. <see cref="Total"/> ise eski davranisi korur ve her ikisini
    /// de tek tek sayar (cihaz 1 + her kanali 1).
    /// </summary>
    private sealed class SkipTally
    {
        private readonly List<string> _devices = [];
        private readonly List<string> _channels = [];

        /// <summary>Atlanan referans sayisi — ORNEK listeleri kirpilsa bile TAM.</summary>
        public int Total { get; private set; }

        public void AddDevice(string externalCode, int channelCount)
        {
            Total += 1 + channelCount;
            if (_devices.Count < MaxLoggedRefs)
                _devices.Add($"{externalCode}({channelCount} kanal)");
        }

        public void AddChannel(string externalCode, int channelNumber)
        {
            Total++;
            if (_channels.Count < MaxLoggedRefs)
                _channels.Add($"{externalCode}/ch:{channelNumber}");
        }

        public string DescribeDevices() => Describe(_devices);
        public string DescribeChannels() => Describe(_channels);

        // Bos liste "yok" yazar: log satirinda bos bir alan, "hic yoktu" ile
        // "yazilmayi unuttuk" arasinda ayrim birakmazdi.
        private static string Describe(List<string> refs) =>
            refs.Count == 0 ? "yok" : string.Join(", ", refs);
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
