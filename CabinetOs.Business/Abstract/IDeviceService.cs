using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.Device.Commands;
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
    Task<Result<ICollection<SelectItemDto>>> SelectListAsync(Expression<Func<Device, bool>>? where = default, CancellationToken cancellationToken = default);
    Task<Result> CreateAsync(DeviceCreateDto request, CancellationToken cancellationToken = default);
    Task<Result<DeviceUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(DeviceUpdateDto request, CancellationToken cancellationToken = default);
    Task<Result<PaginationResponse<DeviceDetailDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseClientSide<DeviceDetailDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseServerSide<DeviceDetailDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
}