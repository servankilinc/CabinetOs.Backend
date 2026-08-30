using Microsoft.EntityFrameworkCore;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.DataAccess.Abstract;
using CabinetOs.DataAccess.Contexts;
using CabinetOs.DataAccess.Repository;
using CabinetOs.Model.Dtos.ChannelEvent.Queries;
using CabinetOs.Model.Entities;

namespace CabinetOs.DataAccess.Concrete
{
    public class ChannelEventRepository : RepositoryBase<ChannelEvent, AppDbContext>, IChannelEventRepository
    {
        public ChannelEventRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<PaginationResponse<ChannelEventDto>> GetPagedAsync(
            Guid cabinetId,
            Guid? ioChannelId,
            DateTime? fromUtc,
            DateTime? toUtc,
            PaginationRequest pagination,
            CancellationToken cancellationToken = default)
        {
            var query = _context.ChannelEvents
                .AsNoTracking()
                .Where(e => e.CabinetId == cabinetId);

            if (ioChannelId.HasValue)
                query = query.Where(e => e.IoChannelId == ioChannelId.Value);

            // Aralik OccurredAtUtc uzerinden — kullanicinin sordugu sey "saha ne
            // zaman oldu", "bize ne zaman ulasti" degil.
            if (fromUtc.HasValue)
                query = query.Where(e => e.OccurredAtUtc >= fromUtc.Value);

            if (toUtc.HasValue)
                query = query.Where(e => e.OccurredAtUtc <= toUtc.Value);

            return await query
                // Esitlikte Id kirilir: ayni damgayi tasiyan iki olay (tek ingest
                // govdesinde gelen iki kanal) aksi halde her sayfalamada yer
                // degistirebilir ve bir satir iki kez ya da hic gorunmezdi.
                .OrderByDescending(e => e.OccurredAtUtc)
                .ThenByDescending(e => e.Id)
                .Select(e => new ChannelEventDto
                {
                    Id = e.Id,
                    IoChannelId = e.IoChannelId,
                    CabinetId = e.CabinetId,
                    // Turev alanlar: IoChannel'in soft-delete query filter'i
                    // yuzunden silinmis kanalda null gelir — bkz. ChannelEventDto.
                    ChannelName = e.IoChannel!.Name,
                    ChannelNumber = e.IoChannel!.ChannelNumber,
                    DeviceId = e.IoChannel!.DeviceId,
                    DeviceName = e.IoChannel!.Device!.Name,
                    DeviceExternalCode = e.IoChannel!.Device!.ExternalCode,
                    Value = e.Value,
                    PreviousValue = e.PreviousValue,
                    OccurredAtUtc = e.OccurredAtUtc,
                    ReceivedAtUtc = e.ReceivedAtUtc
                })
                .ToPaginateAsync(pagination, cancellationToken);
        }
    }
}
