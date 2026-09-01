using System.Collections.Concurrent;
using CabinetOs.Business.Abstract;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.Camera.Commands;
using CabinetOs.Model.Dtos.Camera.Queries;
using CabinetOs.Model.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Business.Concrete;

/// <summary>
/// Anlik goruntu ve cekim yolu.
///
/// <b>Iki ayri amac, iki ayri uc:</b>
/// <list type="bullet">
/// <item><c>GetSnapshotAsync</c> — canli onizleme. Satir YAZMAZ, onbelleklidir.</item>
/// <item><c>CreateCaptureAsync</c> — DELIL. Diske yazar ve
/// <c>CameraCapture</c> satiri birakir; basarisiz olsa bile.</item>
/// </list>
/// Ikisini tek uca indirmek, her onizlemenin bir delil satiri uretmesi (ya da
/// hicbir delilin kalmamasi) demek olurdu.
/// </summary>
public partial class CameraService
{
    /// <summary>
    /// Kamera basina anlik goruntu kilidi — <b>STATIC OLMAK ZORUNDA</b>.
    ///
    /// <c>CameraService</c> scoped'dir: her HTTP istegi kendi ornegini alir.
    /// Alan ornek duzeyinde olsaydi her istek KENDI kilidini yaratir ve
    /// es zamanli 8 istek kameraya 8 istek gonderirdi — yani kilidin engellemek
    /// icin var oldugu sey aynen gerceklesirdi. (Referans projedeki hata tam
    /// olarak buydu: <c>SnapshotService</c> transient, sozluk ornek alani.)
    /// </summary>
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> SnapshotLocks = new();

    private const string SnapshotCacheKeyPrefix = "snapshot_";
    private const string SnapshotTypeCacheKeyPrefix = "snapshot_ct_";

    /// <summary>
    /// Anlik goruntu — canli onizleme icin.
    /// </summary>
    /// <param name="fresh">
    /// <c>true</c> ise onbellek atlanir. Operator "goruntuyu tazele" dedigi
    /// zaman 3 saniyelik de olsa eski bir kare gormemeli.
    /// </param>
    public async Task<Result<SnapshotPayload>> GetSnapshotAsync(
        Guid cameraId, bool fresh = false, CancellationToken cancellationToken = default)
    {
        string cacheKey = SnapshotCacheKeyPrefix + cameraId;
        string typeCacheKey = SnapshotTypeCacheKeyPrefix + cameraId;

        if (!fresh)
        {
            var cached = await ReadCachedSnapshotAsync(cacheKey, typeCacheKey, cancellationToken);
            if (cached != null) return Result<SnapshotPayload>.Success(cached);
        }

        var camera = await _unitOfWork.Cameras.GetAsync(
            where: c => c.Id == cameraId,
            cancellationToken: cancellationToken);

        if (camera == null)
            return Result<SnapshotPayload>.NotFound(description: "Kamera bulunamadi");

        // TEK UCUS (single-flight): ayni kameranin goruntusunu es zamanli
        // isteyen istemciler kameraya TEK istek uretir. Grid'de 8 kutucuk ayni
        // anda yenilendiginde kilit olmasaydi kamera 8 kez zorlanirdi ve
        // kucuk bir IP kamera bunu kaldiramaz.
        var gate = SnapshotLocks.GetOrAdd(cameraId, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            // Kilit beklerken baska biri doldurmus olabilir — cift kontrol.
            if (!fresh)
            {
                var cached = await ReadCachedSnapshotAsync(cacheKey, typeCacheKey, cancellationToken);
                if (cached != null) return Result<SnapshotPayload>.Success(cached);
            }

            var result = await _snapshotGateway.GetSnapshotAsync(camera, cancellationToken);
            if (!result.IsSuccess) return result;

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_captureSettings.SnapshotCacheSeconds)
            };
            await _cache.SetAsync(cacheKey, result.Data.Content, options, cancellationToken);
            await _cache.SetStringAsync(typeCacheKey, result.Data.ContentType, options, cancellationToken);

            return result;
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<SnapshotPayload?> ReadCachedSnapshotAsync(string cacheKey, string typeCacheKey, CancellationToken cancellationToken)
    {
        byte[]? content = await _cache.GetAsync(cacheKey, cancellationToken);
        if (content == null || content.Length == 0) return null;

        string contentType = await _cache.GetStringAsync(typeCacheKey, cancellationToken) ?? "image/jpeg";
        return new SnapshotPayload(content, contentType);
    }

    public async Task<Result<CameraCaptureDto>> CreateCaptureAsync(
        Guid cameraId, CameraCaptureCreateDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result<CameraCaptureDto>.Validation(validationResult.Failures, description: "Validation failed for CameraCaptureCreateDto");

        var camera = await _unitOfWork.Cameras.GetAsync(
            where: c => c.Id == cameraId,
            cancellationToken: cancellationToken);

        if (camera == null)
            return Result<CameraCaptureDto>.NotFound(description: "Kamera bulunamadi");

        if (!camera.IsActive)
            return StreamProblem<CameraCaptureDto>("IsActive", "Pasif kameradan çekim yapılamaz.");

        return request.Type == CaptureType.Clip
            ? await StartClipCaptureAsync(camera, request, cancellationToken)
            : await CaptureSnapshotAsync(camera, cancellationToken);
    }

    /// <summary>
    /// Anlik goruntu cekimi — senkron.
    ///
    /// Onbellek BILEREK ATLANIYOR: delil, istegin geldigi ana ait olmali.
    /// Uc saniyelik de olsa onbellekten bir kare yazmak, "olay anindaki goruntu"
    /// iddiasini sessizce yanlislardi.
    /// </summary>
    private async Task<Result<CameraCaptureDto>> CaptureSnapshotAsync(Camera camera, CancellationToken cancellationToken)
    {
        var capture = NewCapture(camera.Id, CaptureType.Snapshot, durationSec: null);

        var snapshotResult = await _snapshotGateway.GetSnapshotAsync(camera, cancellationToken);

        if (!snapshotResult.IsSuccess)
        {
            // BASARISIZ CEKIM DE SATIR BIRAKIR: "o anda goruntu YOK" bilgisinin
            // kendisi delildir (bkz. CameraCapture.FailureReason).
            capture.Status = CaptureStatus.Failed;
            capture.FailureReason = Truncate(snapshotResult.Error.Description, 512);
        }
        else
        {
            var storeResult = await _captureFileStore.SaveSnapshotAsync(
                snapshotResult.Data.Content, snapshotResult.Data.ContentType, cancellationToken);

            if (!storeResult.IsSuccess)
            {
                capture.Status = CaptureStatus.Failed;
                capture.FailureReason = Truncate(storeResult.Error.Description, 512);
            }
            else
            {
                capture.Status = CaptureStatus.Available;
                capture.RelativePath = storeResult.Data.RelativePath;
                capture.SizeBytes = storeResult.Data.SizeBytes;
            }
        }

        await _unitOfWork.CameraCaptures.AddAndSaveAsync(capture, cancellationToken);
        return Result<CameraCaptureDto>.Success(ToDto(capture));
    }

    /// <summary>
    /// Klip cekimini baslatir ve HEMEN doner.
    ///
    /// Satir <c>Pending</c> yazilir: 10 saniyelik bir klip en az 10 saniye
    /// surer ve HTTP istegini o kadar acik tutmak istemciyi de istek havuzunu
    /// da bosuna mesgul ederdi. <c>CameraCapture.Status</c>'un XML dokumani
    /// <c>Pending</c>'in var olma sebebini tam olarak bu senaryo diye anlatiyor.
    /// </summary>
    private async Task<Result<CameraCaptureDto>> StartClipCaptureAsync(
        Camera camera, CameraCaptureCreateDto request, CancellationToken cancellationToken)
    {
        // Klip ana akimdan alinir; tali akimin cozunurlugu delil icin yetersiz.
        if (!camera.MainStreamEnabled)
            return StreamProblem<CameraCaptureDto>("MainStreamEnabled", "Klip ana akımdan alınır; bu kamerada ana akım kapalı.");

        int duration = request.DurationSec!.Value;
        if (duration > _captureSettings.MaxClipDurationSec)
            return StreamProblem<CameraCaptureDto>(
                "DurationSec",
                $"Klip süresi en fazla {_captureSettings.MaxClipDurationSec} saniye olabilir.");

        if (string.IsNullOrWhiteSpace(_mediaMtxSettings.RecordRoot))
            return Result<CameraCaptureDto>.Failure(
                description: "Klip çekimi yapılandırılmamış: Mediamtx:RecordRoot tanımlı değil.");

        var capture = NewCapture(camera.Id, CaptureType.Clip, duration);
        capture.Status = CaptureStatus.Pending;

        await _unitOfWork.CameraCaptures.AddAndSaveAsync(capture, cancellationToken);

        // Kuyruk BELLEK ICI: uygulama bu noktadan sonra yeniden baslarsa satir
        // Pending olarak asili kalir. Kalici bir kuyruk bu turda yazilmadi
        // (bkz. IClipCaptureQueue) — elle tetiklenen, nadir bir istek.
        _clipCaptureQueue.Enqueue(capture.Id);

        return Result<CameraCaptureDto>.Success(ToDto(capture));
    }

    /// <summary>
    /// Klibi fiilen ceker — <c>ClipCaptureWorker</c> cagirir.
    ///
    /// Akis: gecici bir KAYIT YOLU kur → sure kadar bekle → yolu sil (segment
    /// boylece kapanir) → uretilen dosyayi kalici konuma tasi.
    ///
    /// <b>Canli yol kullanilamaz</b>: uzerinde kaydi acmak, o an izleyen
    /// herkesin akisini kopartirdi.
    /// </summary>
    public async Task RunClipCaptureAsync(long captureId, CancellationToken cancellationToken = default)
    {
        var capture = await _unitOfWork.CameraCaptures.GetAsync(
            where: c => c.Id == captureId,
            tracking: true,
            cancellationToken: cancellationToken);

        if (capture == null)
        {
            _logger.LogWarning("Klip cekimi {CaptureId} bulunamadi; atlaniyor.", captureId);
            return;
        }

        var camera = await _unitOfWork.Cameras.GetAsync(
            where: c => c.Id == capture.CameraId,
            cancellationToken: cancellationToken);

        if (camera == null)
        {
            await FailCaptureAsync(capture, "Kamera bulunamadı.", cancellationToken);
            return;
        }

        int duration = capture.DurationSec ?? 0;
        string pathName = IMediaGateway.ClipPathName(captureId);

        // Klasor adi YOL ADINDAN turer, captureId'den DEGIL: MediaMTX
        // recordPath'teki %path yer tutucusunu yol adiyla doldurur. Ikisi
        // ayrisirsa gecit clip_42/ altina yazar, biz 42/ klasorune bakar ve
        // "dosya uretmedi" deriz.
        string tempFolder = Path.Combine(_mediaMtxSettings.RecordRoot, pathName);

        // Yol GERCEKTEN kuruldu mu? finally'deki temizlik buna bakiyor:
        // kurulmamis bir yolu silmeye calismak MediaMTX log'una gereksiz bir
        // "path not found" ERR satiri dusurur ve gercek hatayi golgeler.
        bool pathCreated = false;

        try
        {
            // Segment suresi klip suresinden UZUN: boylece istenen sure tek bir
            // dosyaya duser. Esit olsaydi rotasyon tam sinirda gerceklesip
            // goruntuyu iki dosyaya bolebilirdi.
            string segmentDuration = $"{duration + (_captureSettings.ClipFinalizeGraceMs / 1000) + 5}s";

            // recordPath %path ICERMEK ZORUNDA — MediaMTX aksi halde
            // yapilandirmayi reddeder ("'recordPath' must contain %path") ve
            // kayit hic baslamaz. Yer tutucuyu kendimiz cozup yola gomemeyiz:
            // dogrulama literal olarak "%path" metnini arar.
            string recordPath = Path.Combine(_mediaMtxSettings.RecordRoot, "%path", "%Y-%m-%d_%H-%M-%S-%f")
                .Replace('\\', '/');

            var ensureResult = await _mediaGateway.EnsureClipPathAsync(
                camera, captureId, recordPath, segmentDuration, cancellationToken);

            if (!ensureResult.IsSuccess)
            {
                await FailCaptureAsync(capture, ensureResult.Error.Description, cancellationToken);
                return;
            }

            pathCreated = true;

            // Kaydin FIILEN basladigi an. Istek anindaki degeri birakmak,
            // CapturedAtUtc'yi birkac saniye yanlislardi — ve bu kolon
            // "goruntunun ani" olarak tanimli.
            capture.CapturedAtUtc = DateTime.UtcNow;

            await Task.Delay(TimeSpan.FromMilliseconds(duration * 1000 + _captureSettings.ClipFinalizeGraceMs), cancellationToken);

            // Yolu silmek MediaMTX'in kaydi sonlandirmasini saglar: fmp4
            // segmenti ancak kapandiginda oynatilabilir hale gelir.
            await _mediaGateway.DeletePathAsync(pathName, cancellationToken);
            await Task.Delay(TimeSpan.FromMilliseconds(_captureSettings.ClipFinalizeGraceMs), cancellationToken);

            string? clipFile = FindNewestClip(tempFolder);
            if (clipFile == null)
            {
                await FailCaptureAsync(capture,
                    "Medya geçidi klip dosyası üretmedi. Kameraya bağlanılamamış olabilir.", cancellationToken);
                return;
            }

            var storeResult = await _captureFileStore.MoveClipAsync(clipFile, cancellationToken);
            if (!storeResult.IsSuccess)
            {
                await FailCaptureAsync(capture, storeResult.Error.Description, cancellationToken);
                return;
            }

            capture.Status = CaptureStatus.Available;
            capture.RelativePath = storeResult.Data.RelativePath;
            capture.SizeBytes = storeResult.Data.SizeBytes;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Klip cekimi {CaptureId} basarisiz", captureId);
            await FailCaptureAsync(capture, "Klip çekimi sırasında beklenmeyen bir hata oluştu.", cancellationToken);
        }
        finally
        {
            // Kurulmus bir yol HER KOSULDA dusurulmeli: birakilirsa kameradan
            // surekli akis cekmeye ve diske yazmaya devam ederdi. Yukaridaki
            // basarili yolda zaten silindi, buradaki cagri hata/iptal
            // durumlarinin guvenlik agi (DeletePathAsync 404'u basari sayiyor,
            // yani iki kez silmek zararsiz).
            if (pathCreated)
                await _mediaGateway.DeletePathAsync(pathName, CancellationToken.None);

            _captureFileStore.TryDeleteDirectory(tempFolder);
        }
    }

    /// <summary>
    /// Gecici klasordeki en yeni klip dosyasi.
    ///
    /// MediaMTX dosya adini zaman sablonundan uretir, dolayisiyla adi onceden
    /// bilemiyoruz — klasore bakmak tek yol.
    /// </summary>
    private static string? FindNewestClip(string folder)
    {
        if (!Directory.Exists(folder)) return null;

        return new DirectoryInfo(folder)
            .GetFiles("*.mp4", SearchOption.AllDirectories)
            .Where(f => f.Length > 0)
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .FirstOrDefault()?.FullName;
    }

    private async Task FailCaptureAsync(CameraCapture capture, string? reason, CancellationToken cancellationToken)
    {
        capture.Status = CaptureStatus.Failed;
        capture.FailureReason = Truncate(reason, 512);
        capture.RelativePath = null;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<Result<ICollection<CameraCaptureDto>>> GetCapturesAsync(
        Guid cameraId, int take = 20, CancellationToken cancellationToken = default)
    {
        bool cameraExists = await _unitOfWork.Cameras.IsExistAsync(
            where: c => c.Id == cameraId,
            cancellationToken: cancellationToken);

        // "Kamera yok" ile "cekimi yok" ayirt edilebilmeli — GetListAsync'teki
        // kabin kontrolüyle ayni gerekce.
        if (!cameraExists)
            return Result<ICollection<CameraCaptureDto>>.NotFound(description: "Kamera bulunamadi");

        // Sinir hem alttan hem ustten: 0 ve negatif anlamsiz, sinirsiz ise
        // cekim gecmisi buyudukce tum tabloyu okumak demek.
        int safeTake = Math.Clamp(take, 1, 200);

        var captures = await _unitOfWork.CameraCaptures.GetRecentForCameraAsync(cameraId, safeTake, cancellationToken);

        return Result<ICollection<CameraCaptureDto>>.Success(captures.Select(ToDto).ToList());
    }

    private CameraCapture NewCapture(Guid cameraId, CaptureType type, int? durationSec) => new()
    {
        CameraId = cameraId,
        Type = type,
        Status = CaptureStatus.Pending,
        CapturedAtUtc = DateTime.UtcNow,
        DurationSec = durationSec,
        // Saklama suresi YAZMA ANINDA sabitlenir: politika sonradan
        // kisaltildiginda mevcut delilin omru geriye donuk degismesin.
        // Bu kolona bakan bir temizlik isi hala YOK.
        ExpiresAt = _captureSettings.CaptureRetentionDays > 0
            ? DateTime.UtcNow.AddDays(_captureSettings.CaptureRetentionDays)
            : null,
        RequestedByUserId = CurrentUserId()
    };

    /// <summary>
    /// Cekimi isteyen operator. Kimlik cozumlenemezse <c>null</c> — cekimin
    /// kendisi, "kim istedi" bilinmedigi icin reddedilecek kadar onemsiz degil.
    /// </summary>
    private Guid? CurrentUserId()
    {
        var identifier = _httpContextManager.GetNameIdentifier();
        if (!identifier.IsSuccess) return null;
        return Guid.TryParse(identifier.Data, out var userId) ? userId : null;
    }

    /// <summary>
    /// Elle esleme — <c>CameraDto.Projection</c> ile ayni gerekce: okuma yoluna
    /// hangi alanlarin ciktiginin TEK ve acik listesi. Burada ifade agaci
    /// gerekmiyor cunku kaynak zaten bellekteki varliklar.
    /// </summary>
    private static CameraCaptureDto ToDto(CameraCapture capture) => new()
    {
        Id = capture.Id,
        CameraId = capture.CameraId,
        Type = capture.Type,
        Status = capture.Status,
        CapturedAtUtc = capture.CapturedAtUtc,
        DurationSec = capture.DurationSec,
        RelativePath = capture.RelativePath,
        SizeBytes = capture.SizeBytes,
        FailureReason = capture.FailureReason,
        ExpiresAt = capture.ExpiresAt,
        RequestedByUserId = capture.RequestedByUserId
    };
}
