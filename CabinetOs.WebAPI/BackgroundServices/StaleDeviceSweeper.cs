using CabinetOs.Business.Abstract;

namespace CabinetOs.WebAPI.BackgroundServices;

/// <summary>
/// Bayat cihaz supurucusu.
///
/// <b>Neden gerekli.</b> Telemetri PUSH modelinde calisiyor: SCADA gonderir, biz
/// dinleriz. Hicbir sey gelmediginde de hicbir sey olmaz — son ingest'te yazilan
/// durum oldugu gibi kalir. Yani fisi cekilmis bir kabin sonsuza dek "Online"
/// gorunur. "Veri gelmiyor" durumunu ancak ZAMANIN gecmesi tespit edebilir ve bunu
/// yapacak tek sey periyodik bir istektir.
///
/// Esik ve periyot <c>appsettings</c>'ten gelir (<c>Scada:StaleAfterSeconds</c>,
/// <c>Scada:SweepIntervalSeconds</c>): saha kosullari kuruluma gore degisir ve
/// bunun icin yeniden derleme gerekmemeli.
/// </summary>
public class StaleDeviceSweeper : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StaleDeviceSweeper> _logger;
    private readonly TimeSpan _staleAfter;
    private readonly TimeSpan _interval;

    private const int DefaultStaleAfterSeconds = 120;
    private const int DefaultSweepIntervalSeconds = 60;

    public StaleDeviceSweeper(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<StaleDeviceSweeper> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        int staleAfterSeconds = configuration.GetValue("Scada:StaleAfterSeconds", DefaultStaleAfterSeconds);
        int intervalSeconds = configuration.GetValue("Scada:SweepIntervalSeconds", DefaultSweepIntervalSeconds);

        // Esik periyottan kisa olursa cihazlar iki tarama arasinda bayatlar ve
        // Offline/Online arasinda gidip gelir. Alt sinir bunu imkansiz kilar.
        _interval = TimeSpan.FromSeconds(Math.Max(5, intervalSeconds));
        _staleAfter = TimeSpan.FromSeconds(Math.Max(intervalSeconds * 2, staleAfterSeconds));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "StaleDeviceSweeper basladi: her {Interval} sn, esik {Stale} sn",
            _interval.TotalSeconds, _staleAfter.TotalSeconds);

        using var timer = new PeriodicTimer(_interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                // Her turda YENI scope: IChannelEventService (ve altindaki DbContext)
                // scoped'dir. Tek bir uzun omurlu context, saatler icinde tum
                // okunan entity'leri izlemeye devam eder ve bellegi sizdirirdi.
                using var scope = _scopeFactory.CreateScope();
                var channelEventService = scope.ServiceProvider.GetRequiredService<IChannelEventService>();

                int swept = await channelEventService.SweepStaleDevicesAsync(_staleAfter, stoppingToken);
                if (swept > 0)
                    _logger.LogInformation("{Count} cihaz Offline'a cekildi", swept);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                // Bir turun patlamasi supurucuyu OLDURMEMELI: yutulmazsa
                // BackgroundService sessizce durur ve uygulama ayakta oldugu halde
                // bir daha hicbir cihaz Offline'a cekilmez.
                _logger.LogError(exception, "StaleDeviceSweeper turu basarisiz");
            }
        }
    }
}
