using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.ComponentTemplatePin.Commands;
using CabinetOs.Model.Dtos.ComponentTemplatePin.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Abstract;

public interface IComponentTemplatePinService
{
    Task<Result<ComponentTemplatePin>> GetAsync(Expression<Func<ComponentTemplatePin, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<ComponentTemplatePin>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ComponentTemplatePinDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ICollection<ComponentTemplatePin>>> GetListAsync(Expression<Func<ComponentTemplatePin, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<ICollection<ComponentTemplatePin>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<ComponentTemplatePinDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<SelectItemDto>>> SelectListAsync(Expression<Func<ComponentTemplatePin, bool>>? where = default, CancellationToken cancellationToken = default);
    Task<Result<CreatedDto>> CreateAsync(ComponentTemplatePinCreateDto request, CancellationToken cancellationToken = default);
    Task<Result<ComponentTemplatePinUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(ComponentTemplatePinUpdateDto request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PaginationResponse<ComponentTemplatePinDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseClientSide<ComponentTemplatePinDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseServerSide<ComponentTemplatePinDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
}