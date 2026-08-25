using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.Pin.Commands;
using CabinetOs.Model.Dtos.Pin.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Abstract;

public interface IPinService
{
    Task<Result<Pin>> GetAsync(Expression<Func<Pin, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<Pin>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PinDetailDto>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PinDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ICollection<Pin>>> GetListAsync(Expression<Func<Pin, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<ICollection<Pin>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<PinDetailDto>>> GetDetailListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<PinDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<SelectItemDto>>> SelectListAsync(Expression<Func<Pin, bool>>? where = default, CancellationToken cancellationToken = default);
    Task<Result<CreatedDto>> CreateAsync(PinCreateDto request, CancellationToken cancellationToken = default);
    Task<Result<PinUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(PinUpdateDto request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> RestoreAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PaginationResponse<PinDetailDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseClientSide<PinDetailDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseServerSide<PinDetailDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
}