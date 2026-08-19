using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Entities;

namespace CabinetOs.Business.Abstract
{
    public interface IAuditLogService
    {
        Task<Result<AuditLog>> GetAsync(Expression<Func<AuditLog, bool>> where, CancellationToken cancellationToken = default);
        Task<Result<AuditLog>> GetAsync(long id, CancellationToken cancellationToken = default);
        Task<Result<ICollection<AuditLog>>> GetListAsync(Expression<Func<AuditLog, bool>> where, CancellationToken cancellationToken = default);
        Task<Result<ICollection<AuditLog>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
        Task<Result<SelectList>> SelectListAsync(Expression<Func<AuditLog, bool>>? where = default, CancellationToken cancellationToken = default);
        Task<Result> CreateAsync(AuditLog request, CancellationToken cancellationToken = default);
        Task<Result<PaginationResponse<AuditLog>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
        Task<Result<DatatableResponseClientSide<AuditLog>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
        Task<Result<DatatableResponseServerSide<AuditLog>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    }
}