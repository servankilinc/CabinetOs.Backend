using CabinetOs.Business.Abstract;
using CabinetOs.Business.Settings;
using CabinetOs.Core.Utils.ResultPattern;

namespace CabinetOs.WebAPI.Utils;

/// <summary>
/// <see cref="ICaptureFileStore"/>'un dosya sistemi implementasyonu.
///
/// Servis katmaninda DEGIL WebAPI katmaninda — <see cref="TemplateImageStore"/>
/// ile birebir ayni gerekce: <c>IWebHostEnvironment</c> ve <c>wwwroot</c>
/// barindirma detaylaridir ve <c>CameraService</c>'i dosya sistemine baglamak
/// onu test edilemez hale getirirdi.
///
/// <b>Sonucu acikca:</b> buraya yazilan dosyalar <c>UseStaticFiles</c> ile
/// KIMLIK DOGRULAMASIZ servis edilir; URL'yi bilen goruntuyu indirebilir.
/// Tek engel dosya adinin tahmin edilemez bir <c>Guid</c> olmasidir. Desen
/// <c>CameraCapture.RelativePath</c>'in XML dokumaninda ilan edilmis ve sistem
/// kapali agda calisiyor. Yetkili bir uctan servis istenirse degisecek tek yer
/// asagidaki <c>WebRootPath</c> secimi ve buna eslik edecek yeni bir uctur.
/// </summary>
public sealed class CaptureFileStore : ICaptureFileStore
{
    private readonly IWebHostEnvironment _environment;
    private readonly CameraCaptureSettings _settings;
    private readonly ILogger<CaptureFileStore> _logger;

    public CaptureFileStore(
        IWebHostEnvironment environment,
        CameraCaptureSettings settings,
        ILogger<CaptureFileStore> logger)
    {
        _environment = environment;
        _settings = settings;
        _logger = logger;
    }

    public async Task<Result<StoredCapture>> SaveSnapshotAsync(
        byte[] content, string contentType, CancellationToken cancellationToken = default)
    {
        // Uzanti ISTEMCIDEN DEGIL beyaz listeden geliyor (TemplateImageStore ile
        // ayni kural): kameranin bildirdigi content-type'a guvenip onu dosya
        // adina yazmak, uzantiyi saldirganin kontrolune birakirdi.
        string extension = contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase) ? ".png" : ".jpg";

        try
        {
            var (fullPath, relativePath) = BuildTargetPath(extension);
            await File.WriteAllBytesAsync(fullPath, content, cancellationToken);
            return Result<StoredCapture>.Success(new StoredCapture(relativePath, content.LongLength));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Anlik goruntu diske yazilamadi");
            return Result<StoredCapture>.Failure(description: "Görüntü diske yazılamadı.");
        }
    }

    public Task<Result<StoredCapture>> MoveClipAsync(string sourceFullPath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(sourceFullPath))
                return Task.FromResult(Result<StoredCapture>.Failure(description: "Klip dosyası bulunamadı."));

            long size = new FileInfo(sourceFullPath).Length;
            var (fullPath, relativePath) = BuildTargetPath(".mp4");

            // KOPYALAMA DEGIL TASIMA: klip dosyasi onlarca MB olabilir ve kaynak
            // zaten silinecek gecici bir dizinde. Kopyalamak diski gereksiz yere
            // iki kat kullanirdi.
            //
            // Ayni birim degilse File.Move zaten kopyalayip siler; ayni birimse
            // yalnizca dizin girdisini gunceller.
            File.Move(sourceFullPath, fullPath, overwrite: false);

            return Task.FromResult(Result<StoredCapture>.Success(new StoredCapture(relativePath, size)));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Klip dosyasi tasinamadi: {Source}", sourceFullPath);
            return Task.FromResult(Result<StoredCapture>.Failure(description: "Klip dosyası kaydedilemedi."));
        }
    }

    public void TryDeleteDirectory(string fullPath)
    {
        try
        {
            if (Directory.Exists(fullPath))
                Directory.Delete(fullPath, recursive: true);
        }
        catch (Exception exception)
        {
            // Temizlik basarisizligi, TAMAMLANMIS bir cekimi basarisiz
            // gostermemeli. Yalnizca loglanir.
            _logger.LogWarning(exception, "Gecici klip klasoru silinemedi: {Path}", fullPath);
        }
    }

    /// <summary>
    /// Hedef yolu uretir ve klasoru olusturur.
    ///
    /// Tarihe gore klasorleme (<c>yyyy/MM/dd</c>) sart: tek bir duz klasorde
    /// on binlerce dosya, dizin listeleme islemlerini dakikalar suren hale
    /// getirir.
    /// </summary>
    private (string FullPath, string RelativePath) BuildTargetPath(string extension)
    {
        string relativeFolder = $"{_settings.CaptureRoot.Trim('/')}/{DateTime.UtcNow:yyyy/MM/dd}";
        string fileName = $"{Guid.NewGuid():N}{extension}";

        string webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        string folder = Path.Combine(webRoot, relativeFolder.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(folder);

        // Deger her zaman GORELI ve ileri bolu isaretli: tam URL saklamak,
        // depo tasindiginda binlerce satirin guncellenmesi demekti
        // (bkz. CameraCapture.RelativePath).
        return (Path.Combine(folder, fileName), $"{relativeFolder}/{fileName}");
    }
}
