using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Entities;
using CabinetOs.Model.Dtos.DeviceCommand.Commands;
using CabinetOs.Model.Dtos.DeviceCommand.Queries;

namespace CabinetOs.Business.Abstract
{
    public interface IDeviceCommandService
    {
        Task<Result<DeviceCommand>> GetAsync(Expression<Func<DeviceCommand, bool>> where, CancellationToken cancellationToken = default);
        Task<Result<DeviceCommand>> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<DeviceCommandDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<ICollection<DeviceCommand>>> GetListAsync(Expression<Func<DeviceCommand, bool>> where, CancellationToken cancellationToken = default);
        Task<Result<ICollection<DeviceCommand>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
        Task<Result<ICollection<DeviceCommandDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
        Task<Result<SelectList>> SelectListAsync(Expression<Func<DeviceCommand, bool>>? where = default, CancellationToken cancellationToken = default);
        Task<Result> CreateAsync(DeviceCommandCreateDto request, CancellationToken cancellationToken = default);
        Task<Result<DeviceCommandUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(DeviceCommandUpdateDto request, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result> RestoreAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<PaginationResponse<DeviceCommandDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
        Task<Result<DatatableResponseClientSide<DeviceCommandDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
        Task<Result<DatatableResponseServerSide<DeviceCommandDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    }
}