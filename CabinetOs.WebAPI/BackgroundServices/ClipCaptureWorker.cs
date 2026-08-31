using CabinetOs.Business.Abstract;

namespace CabinetOs.WebAPI.BackgroundServices;

/// <summary>
/// Klip cekimlerini sirayla yurutur.
///
/// <b>Neden arka planda:</b> bir klip, suresi kadar BEKLEMEK zorundadir.
/// HTTP istegini o kadar acik tutmak istemciyi ve istek havuzunu bosuna mesgul
/// ederdi; bunun yerine <c>CameraCapture</c> satiri <c>Pending</c> yazilip
/// hemen donuluyor ve isin geri kalani buraya dusuyor.
///
/// <b>Sirali calisir</b>, paralel degil: es zamanli klipler ayni kameradan
/// birden fazla RTSP oturumu acabilir ve kucuk bir IP kamera bunu kaldirmaz.
/// Kuyruk zaten elle tetiklenen, nadir istekler tasiyor.
/// </summary>
public class ClipCaptureWorker : BackgroundService
{
    private readonly IClipCaptureQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ClipCaptureWorker> _logger;

    public ClipCaptureWorker(
        IClipCaptureQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<ClipCaptureWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ClipCaptureWorker basladi");

        await foreach (long captureId in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                // Her cekim icin YENI scope: ICameraService (ve altindaki
                // DbContext) scoped'dir. Tek bir uzun omurlu context, saatler
                // boyunca okunan her entity'yi izlemeye devam eder ve bellegi
                // sizdirirdi (StaleDeviceSweeper ile ayni gerekce).
                using var scope = _scopeFactory.CreateScope();
                var cameraService = scope.ServiceProvider.GetRequiredService<ICameraService>();

                await cameraService.RunClipCaptureAsync(captureId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                // Bir cekimin patlamasi worker'i OLDURMEMELI: yutulmazsa
                // BackgroundService sessizce durur ve sonraki tum klipler
                // sonsuza dek Pending kalirdi.
                _logger.LogError(exception, "Klip cekimi {CaptureId} yurutulurken hata", captureId);
            }
        }
    }
}
