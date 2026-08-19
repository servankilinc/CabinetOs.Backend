using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Entities;
using CabinetOs.Model.Dtos.ComponentTemplate.Commands;
using CabinetOs.Model.Dtos.ComponentTemplate.Queries;

namespace CabinetOs.Business.Abstract
{
    public interface IComponentTemplateService
    {
        Task<Result<ComponentTemplate>> GetAsync(Expression<Func<ComponentTemplate, bool>> where, CancellationToken cancellationToken = default);
        Task<Result<ComponentTemplate>> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<ComponentTemplateBaseDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<ComponentTemplateDetailDto>> GetComponentTemplateDetailDtoAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<ICollection<ComponentTemplate>>> GetListAsync(Expression<Func<ComponentTemplate, bool>> where, CancellationToken cancellationToken = default);
        Task<Result<ICollection<ComponentTemplate>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
        Task<Result<ICollection<ComponentTemplateBaseDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
        Task<Result<ICollection<ComponentTemplateDetailDto>>> GetComponentTemplateDetailDtoListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
        Task<Result<SelectList>> SelectListAsync(Expression<Func<ComponentTemplate, bool>>? where = default, CancellationToken cancellationToken = default);
        Task<Result> CreateAsync(ComponentTemplateCreateDto request, CancellationToken cancellationToken = default);
        Task<Result<ComponentTemplateUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(ComponentTemplateUpdateDto request, CancellationToken cancellationToken = default);
        Task<Result<PaginationResponse<ComponentTemplateDetailDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
        Task<Result<DatatableResponseClientSide<ComponentTemplateDetailDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
        Task<Result<DatatableResponseServerSide<ComponentTemplateDetailDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    }
}