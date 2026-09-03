using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.ChannelEvent.Queries;
using CabinetOs.Model.Dtos.Scada.Commands;

namespace CabinetOs.Business.Abstract;

/// <summary> SCADA'dan gelen telemetrinin girdigi tek kapi. </summary>
public interface IChannelEventService
{
    /// <summary>
    /// Bir kabinin olay gecmisi — yeniden eskiye, sayfali.
    /// Kabin yoksa <c>NotFound</c>.
    /// </summary>
    Task<Result<PaginationResponse<ChannelEventDto>>> GetPagedAsync(ChannelEventQueryRequest request, CancellationToken cancellationToken = default);


    /// <summary>
    /// Bir kabinin telemetri paketini isler.
    ///
    /// Tanimadigi cihaz kodu / kanal numarasi SESSIZCE ATLANIR ve tum istek
    /// reddedilmez; atlananlar SUNUCU TARAFINDA <c>Warning</c> seviyesinde
    /// loglanir. Degeri degismeyen kanal YAZILMAZ ve yayin uretmez.
    ///
    /// Basarili islem GOVDESIZ doner: SCADA kac okumanin islendigiyle
    /// ilgilenmez, sessiz atlamayi tespit etmesi gereken taraf BIZ'iz ve bunun
    /// yeri istegin yaniti degil log'dur.
    /// </summary>
    Task<Result> IngestAsync(ScadaIngestRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Esik suresi boyunca haber alinamayan cihazlari <c>Offline</c>'a ceker ve
    /// degisenleri yayinlar. <c>StaleDeviceSweeper</c> tarafindan periyodik cagrilir.
    ///
    /// Push-only bir modelde bu olmadan olu bir kabin sonsuza dek "Online" gorunur:
    /// son ingest'te yazilan durum, bir daha hicbir sey gelmese bile oldugu gibi kalir.
    /// </summary>
    /// <returns>Offline'a cekilen cihaz sayisi.</returns>
    Task<int> SweepStaleDevicesAsync(TimeSpan staleAfter, CancellationToken cancellationToken = default);
}
