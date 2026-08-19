using AutoMapper;
using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Core.Utils.Validation;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.Device.Commands;
using CabinetOs.Model.Dtos.Device.Queries;
using CabinetOs.Model.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CabinetOs.Business.Concrete;

public class DeviceService : IDeviceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;
    private readonly IMapper _mapper;
    public DeviceService(IUnitOfWork unitOfWork, IValidationService validationService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _mapper = mapper;
    }

    public async Task<Result<Device>> GetAsync(Expression<Func<Device, bool>> where, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Devices.GetAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<Device>.NotFound();
        return Result<Device>.Success(result);
    }

    public async Task<Result<Device>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Devices.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<Device>.NotFound();
        return Result<Device>.Success(result);
    }

    public async Task<Result<DeviceDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Devices.GetAsync<DeviceDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<DeviceDto>.NotFound();
        return Result<DeviceDto>.Success(result);
    }

    public async Task<Result<DeviceDetailDto>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Devices.GetAsync<DeviceDetailDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<DeviceDetailDto>.NotFound();
        return Result<DeviceDetailDto>.Success(result);
    }

    public async Task<Result<ICollection<Device>>> GetListAsync(Expression<Func<Device, bool>>? where = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Devices.GetAllAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<Device>>.NotFound();
        return Result<ICollection<Device>>.Success(result);
    }

    public async Task<Result<ICollection<Device>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Devices.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<Device>>.NotFound();
        return Result<ICollection<Device>>.Success(result);
    }

    public async Task<Result<ICollection<DeviceDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Devices.GetAllAsync<DeviceDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<DeviceDto>>.NotFound();
        return Result<ICollection<DeviceDto>>.Success(result);
    }

    public async Task<Result<ICollection<DeviceDetailDto>>> GetDetailListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Devices.GetAllAsync<DeviceDetailDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<DeviceDetailDto>>.NotFound();
        return Result<ICollection<DeviceDetailDto>>.Success(result);
    }

    public async Task<Result<ICollection<SelectItemDto>>> SelectListAsync(Expression<Func<Device, bool>>? where = default, CancellationToken cancellationToken = default)
    {
        var list = await _unitOfWork.Devices.GetAllAsync<SelectItemDto>(select: s => new SelectItemDto { Value = s.Id.ToString(), Text = s.Name }, where: where, cancellationToken: cancellationToken);
        var selectList = list ?? new List<SelectItemDto>();
        return Result<ICollection<SelectItemDto>>.Success(selectList);
    }

    public async Task<Result> CreateAsync(DeviceCreateDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Validation(validationResult.Failures, description: $"Validation failed for DeviceCreateDto");
        await _unitOfWork.Devices.AddAndSaveAsync(_mapper.Map<Device>(request), cancellationToken);
        return Result.Success();
    }

    public async Task<Result<DeviceUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Devices.GetAsync<DeviceUpdateDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<DeviceUpdateDto>.NotFound();
        return Result<DeviceUpdateDto>.Success(result);
    }

    public async Task<Result> UpdateAsync(DeviceUpdateDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Validation(validationResult.Failures);
        var entity = await _unitOfWork.Devices.GetAsync(where: (f) => f.Id == request.Id, cancellationToken: cancellationToken);
        if (entity == null)
            return Result.NotFound();
        await _unitOfWork.Devices.UpdateAndSaveAsync(_mapper.Map(request, entity), cancellationToken);
        return Result.Success();
    }

    public async Task<Result<PaginationResponse<DeviceDetailDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Devices.PaginationAsync<DeviceDetailDto>(configurationProvider: _mapper.ConfigurationProvider, paginationRequest: request, include: i => i.Include(x => x.ComponentTemplate).Include(x => x.DeviceStatus), cancellationToken: cancellationToken);
        return Result<PaginationResponse<DeviceDetailDto>>.Success(result);
    }

    public async Task<Result<DatatableResponseClientSide<DeviceDetailDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Devices.DatatableClientSideAsync<DeviceDetailDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: i => i.Include(x => x.ComponentTemplate).Include(x => x.DeviceStatus), cancellationToken: cancellationToken);
        return Result<DatatableResponseClientSide<DeviceDetailDto>>.Success(result);
    }

    public async Task<Result<DatatableResponseServerSide<DeviceDetailDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Devices.DatatableServerSideAsync<DeviceDetailDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: i => i.Include(x => x.ComponentTemplate).Include(x => x.DeviceStatus), cancellationToken: cancellationToken);
        return Result<DatatableResponseServerSide<DeviceDetailDto>>.Success(result);
    }
}