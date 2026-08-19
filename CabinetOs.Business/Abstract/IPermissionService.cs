using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Entities;
using CabinetOs.Model.Dtos.Permission.Queries;

namespace CabinetOs.Business.Abstract
{
    public interface IPermissionService
    {
        Task<Result<Permission>> GetAsync(Expression<Func<Permission, bool>> where, CancellationToken cancellationToken = default);
        Task<Result<Permission>> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<PemissionDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<ICollection<Permission>>> GetListAsync(Expression<Func<Permission, bool>> where, CancellationToken cancellationToken = default);
        Task<Result<ICollection<Permission>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
        Task<Result<ICollection<PemissionDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
        Task<Result<SelectList>> SelectListAsync(Expression<Func<Permission, bool>>? where = default, CancellationToken cancellationToken = default);
        Task<Result> CreateAsync(Permission request, CancellationToken cancellationToken = default);
        Task<Result<PaginationResponse<PemissionDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
        Task<Result<DatatableResponseClientSide<PemissionDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
        Task<Result<DatatableResponseServerSide<PemissionDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    }
}