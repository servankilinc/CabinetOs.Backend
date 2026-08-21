using System.Linq.Expressions;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.RolePermission.Commands;
using CabinetOs.Model.Dtos.RolePermission.Queries;
using CabinetOs.Model.Entities;

namespace CabinetOs.Business.Abstract
{
    public interface IRolePermissionService
    {
        Task<Result<RolePermission>> GetAsync(Expression<Func<RolePermission, bool>> where, CancellationToken cancellationToken = default);
        Task<Result<RolePermission>> GetAsync(int permissionId, Guid roleId, CancellationToken cancellationToken = default);
        Task<Result<RolePermissionDto>> GetBaseAsync(int permissionId, Guid roleId, CancellationToken cancellationToken = default);
        Task<Result<ICollection<RolePermission>>> GetListAsync(Expression<Func<RolePermission, bool>> where, CancellationToken cancellationToken = default);
        Task<Result<ICollection<RolePermission>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
        Task<Result<ICollection<RolePermissionDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
        /// <summary>Bir rolun sahip oldugu tum izinleri, Permission bilgileriyle birlikte getirir.</summary>
        Task<Result<ICollection<RolePermissionDto>>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task<Result> CreateAsync(RolePermissionCreateDto request, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(int permissionId, Guid roleId, CancellationToken cancellationToken = default);
        /// <summary>Rolun izin kumesini verilen liste ile birebir degistirir (ekle + sil), tek transaction icinde.</summary>
        Task<Result> SyncRolePermissionsAsync(Guid roleId, ICollection<int> permissionIds, CancellationToken cancellationToken = default);
        Task<Result<PaginationResponse<RolePermissionDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
        Task<Result<DatatableResponseClientSide<RolePermissionDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
        Task<Result<DatatableResponseServerSide<RolePermissionDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    }
}
