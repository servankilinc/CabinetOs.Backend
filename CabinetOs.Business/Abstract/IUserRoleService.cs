using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Entities;

namespace CabinetOs.Business.Abstract
{
    public interface IUserRoleService
    {
        Task<Result<UserRole>> GetAsync(Expression<Func<UserRole, bool>> where, CancellationToken cancellationToken = default);
        Task<Result<UserRole>> GetAsync(Guid roleId, Guid userId, CancellationToken cancellationToken = default);
        Task<Result<ICollection<UserRole>>> GetListAsync(Expression<Func<UserRole, bool>> where, CancellationToken cancellationToken = default);
        Task<Result<ICollection<UserRole>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
        Task<Result> CreateAsync(UserRole request, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(UserRole request, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(Guid roleId, Guid userId, CancellationToken cancellationToken = default);
        Task<Result<PaginationResponse<UserRole>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
        Task<Result<DatatableResponseClientSide<UserRole>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
        Task<Result<DatatableResponseServerSide<UserRole>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    }
}