using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.IoChannel.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Abstract;

public interface IIoChannelService
{
    Task<Result<IoChannel>> GetAsync(Expression<Func<IoChannel, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<IoChannel>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IoChannelDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ICollection<IoChannel>>> GetListAsync(Expression<Func<IoChannel, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<ICollection<IoChannel>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<IoChannelDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
}
