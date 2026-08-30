using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.ChannelEvent.Queries;

namespace CabinetOs.Business.Abstract;

/// <summary>
/// Kanal olaylarinin OKUMA yolu. Yazma yolu yoktur ve olmayacaktir: olaylari
/// yalnizca <c>ScadaService.IngestAsync</c> uretir. Disaridan olay yazdirmak,
/// delilin kaynagini belirsizlestirirdi.
///
/// K1'in tenant hazirlik kurali gecerli: <c>companyId</c> parametresi YOK.
/// </summary>
public interface IChannelEventService
{
    /// <summary>
    /// Bir kabinin olay gecmisi — yeniden eskiye, sayfali.
    /// Kabin yoksa <c>NotFound</c>.
    /// </summary>
    Task<Result<PaginationResponse<ChannelEventDto>>> GetPagedAsync(
        ChannelEventQueryRequest request,
        CancellationToken cancellationToken = default);
}
