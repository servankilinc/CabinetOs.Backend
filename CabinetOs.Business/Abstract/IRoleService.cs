using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Entities;
using CabinetOs.Model.Dtos.Role.Commands;
using CabinetOs.Model.Dtos.Role.Queries;

namespace CabinetOs.Business.Abstract
{
    public interface IRoleService
    {
        Task<Result<Role>> GetAsync(Expression<Func<Role, bool>> where, CancellationToken cancellationToken = default);
        Task<Result<Role>> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<RoleDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<ICollection<Role>>> GetListAsync(Expression<Func<Role, bool>> where, CancellationToken cancellationToken = default);
        Task<Result<ICollection<Role>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
        Task<Result<ICollection<RoleDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
        Task<Result<SelectList>> SelectListAsync(Expression<Func<Role, bool>>? where = default, CancellationToken cancellationToken = default);
        Task<Result> CreateAsync(RoleCreateDto request, CancellationToken cancellationToken = default);
        Task<Result<RoleUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(RoleUpdateDto request, CancellationToken cancellationToken = default);
        Task<Result<PaginationResponse<RoleDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
        Task<Result<DatatableResponseClientSide<RoleDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
        Task<Result<DatatableResponseServerSide<RoleDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    }
}