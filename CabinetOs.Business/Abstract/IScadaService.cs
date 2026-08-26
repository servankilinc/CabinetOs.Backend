using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.Scada.Commands;
using CabinetOs.Model.Dtos.Scada.Queries;

namespace CabinetOs.Business.Abstract;

/// <summary>
/// SCADA'dan gelen telemetrinin girdigi tek kapi.
///
/// K1'in tenant hazirlik kurali burada da gecerli: <c>companyId</c> parametresi
/// YOK, <c>IgnoreQueryFilters</c> YOK.
/// </summary>
public interface IScadaService
{
    /// <summary>
    /// Bir kabinin telemetri paketini isler.
    ///
    /// Tanimadigi cihaz kodu / kanal numarasi SESSIZCE ATLANIR ve sayilir; tum
    /// istek reddedilmez. Degeri degismeyen kanal YAZILMAZ ve yayin uretmez.
    /// </summary>
    Task<Result<ScadaIngestResponse>> IngestAsync(ScadaIngestRequest request, CancellationToken cancellationToken = default);

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
