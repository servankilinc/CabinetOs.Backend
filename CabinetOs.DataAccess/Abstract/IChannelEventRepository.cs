using CabinetOs.Core.Utils.Pagination;
using CabinetOs.DataAccess.Repository;
using CabinetOs.Model.Dtos.ChannelEvent.Queries;
using CabinetOs.Model.Entities;

namespace CabinetOs.DataAccess.Abstract
{
    public interface IChannelEventRepository : IRepository<ChannelEvent>, IRepositoryAsync<ChannelEvent>
    {
        /// <summary>
        /// Bir kabinin olaylari — yeniden eskiye, sayfali.
        /// </summary>
        /// <remarks>
        /// Jenerik <c>GetAllAsync</c> ile yazilamiyor: o imzalarda sayfalama yok
        /// (<c>IDeviceCommandRepository.GetRecentForDeviceAsync</c> ile ayni engel).
        /// Sinirsiz cekip bellekte kirpmak, olay tablosu buyudukce tum tabloyu
        /// okumak demekti — bu tablonun tek buyume yonu var.
        ///
        /// Filtreler <c>IX_ChannelEvent_CabinetId_OccurredAtUtc</c> ve
        /// <c>IX_ChannelEvent_IoChannelId_OccurredAtUtc</c> indekslerine oturur.
        /// </remarks>
        Task<PaginationResponse<ChannelEventDto>> GetPagedAsync(
            Guid cabinetId,
            Guid? ioChannelId,
            DateTime? fromUtc,
            DateTime? toUtc,
            PaginationRequest pagination,
            CancellationToken cancellationToken = default);
    }
}
