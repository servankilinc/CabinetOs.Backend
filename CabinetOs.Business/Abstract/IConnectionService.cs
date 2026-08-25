using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.Connection.Commands;
using CabinetOs.Model.Dtos.Connection.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Abstract;

public interface IConnectionService
{
    Task<Result<Connection>> GetAsync(Expression<Func<Connection, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<Connection>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ConnectionDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ICollection<Connection>>> GetListAsync(Expression<Func<Connection, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<ICollection<Connection>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<ConnectionDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<SelectItemDto>>> SelectListAsync(Expression<Func<Connection, bool>>? where = default, CancellationToken cancellationToken = default);
    Task<Result<CreatedDto>> CreateAsync(ConnectionCreateDto request, CancellationToken cancellationToken = default);
    Task<Result<ConnectionUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(ConnectionUpdateDto request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> RestoreAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PaginationResponse<ConnectionDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseClientSide<ConnectionDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseServerSide<ConnectionDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
}