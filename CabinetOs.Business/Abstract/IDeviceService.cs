using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.Device.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Abstract;

public interface IDeviceService
{
    Task<Result<Device>> GetAsync(Expression<Func<Device, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<Device>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<DeviceDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<DeviceDetailDto>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ICollection<Device>>> GetListAsync(Expression<Func<Device, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<ICollection<Device>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<DeviceDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<DeviceDetailDto>>> GetDetailListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
}
