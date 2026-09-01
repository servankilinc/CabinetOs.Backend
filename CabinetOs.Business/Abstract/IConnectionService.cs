using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.ResultPattern;
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
}
