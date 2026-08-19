using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Entities;

namespace CabinetOs.Business.Abstract
{
    public interface IRolePermissionService
    {
        Task<Result<RolePermission>> GetAsync(Expression<Func<RolePermission, bool>> where, CancellationToken cancellationToken = default);
        Task<Result<RolePermission>> GetAsync(Guid permissionId, Guid roleId, CancellationToken cancellationToken = default);
        Task<Result<ICollection<RolePermission>>> GetListAsync(Expression<Func<RolePermission, bool>> where, CancellationToken cancellationToken = default);
        Task<Result<ICollection<RolePermission>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
        Task<Result> CreateAsync(RolePermission request, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(RolePermission request, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(Guid permissionId, Guid roleId, CancellationToken cancellationToken = default);
        Task<Result<PaginationResponse<RolePermission>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
        Task<Result<DatatableResponseClientSide<RolePermission>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
        Task<Result<DatatableResponseServerSide<RolePermission>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    }
}