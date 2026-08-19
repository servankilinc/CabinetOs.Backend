using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.DeviceType.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Abstract;

public interface IDeviceTypeService
{
    Task<Result<DeviceType>> GetAsync(Expression<Func<DeviceType, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<DeviceType>> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<DeviceTypeDto>> GetBaseAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<ICollection<DeviceType>>> GetListAsync(Expression<Func<DeviceType, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<ICollection<DeviceType>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<DeviceTypeDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<SelectItemDto>>> SelectListAsync(Expression<Func<DeviceType, bool>>? where = default, CancellationToken cancellationToken = default);
    Task<Result<PaginationResponse<DeviceTypeDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseClientSide<DeviceTypeDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseServerSide<DeviceTypeDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
}