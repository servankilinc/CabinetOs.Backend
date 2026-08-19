using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.DeviceStatus.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Abstract;

public interface IDeviceStatusService
{
    Task<Result<DeviceStatus>> GetAsync(Expression<Func<DeviceStatus, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<DeviceStatus>> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<DeviceStatusDto>> GetBaseAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<ICollection<DeviceStatus>>> GetListAsync(Expression<Func<DeviceStatus, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<ICollection<DeviceStatus>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<DeviceStatusDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<SelectItemDto>>> SelectListAsync(Expression<Func<DeviceStatus, bool>>? where = default, CancellationToken cancellationToken = default);
    Task<Result<PaginationResponse<DeviceStatusDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseClientSide<DeviceStatusDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseServerSide<DeviceStatusDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
}