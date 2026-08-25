using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.CanvasSettings.Commands;
using CabinetOs.Model.Dtos.CanvasSettings.Queries;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Abstract;

public interface ICanvasSettingsService
{
    Task<Result<CanvasSettings>> GetAsync(Expression<Func<CanvasSettings, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<CanvasSettings>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CanvasSettingsDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ICollection<CanvasSettings>>> GetListAsync(Expression<Func<CanvasSettings, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<ICollection<CanvasSettings>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<CanvasSettingsDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<SelectItemDto>>> SelectListAsync(Expression<Func<CanvasSettings, bool>>? where = default, CancellationToken cancellationToken = default);
    Task<Result<CreatedDto>> CreateAsync(CanvasSettingsCreateDto request, CancellationToken cancellationToken = default);
    Task<Result<CanvasSettingsUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(CanvasSettingsUpdateDto request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PaginationResponse<CanvasSettingsDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseClientSide<CanvasSettingsDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseServerSide<CanvasSettingsDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
}