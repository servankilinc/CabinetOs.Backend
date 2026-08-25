using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.Cabinet.Commands;
using CabinetOs.Model.Dtos.Cabinet.Queries;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Abstract;

public interface ICabinetService
{
    Task<Result<Cabinet>> GetAsync(Expression<Func<Cabinet, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<Cabinet>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CabinetBaseDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CabinetDetailDto>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ICollection<Cabinet>>> GetListAsync(Expression<Func<Cabinet, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<ICollection<Cabinet>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<CabinetBaseDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<CabinetDetailDto>>> GetDetailListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<SelectItemDto>>> SelectListAsync(Expression<Func<Cabinet, bool>>? where = default, CancellationToken cancellationToken = default);
    Task<Result<CreatedDto>> CreateAsync(CabinetCreateDto request, CancellationToken cancellationToken = default);
    Task<Result<CabinetUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(CabinetUpdateDto request, CancellationToken cancellationToken = default);
    Task<Result<PaginationResponse<CabinetDetailDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseClientSide<CabinetDetailDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseServerSide<CabinetDetailDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
}