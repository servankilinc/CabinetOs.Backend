using AutoMapper;
using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Core.Utils.Validation;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.DeviceCommand.Commands;
using CabinetOs.Model.Dtos.DeviceCommand.Queries;
using CabinetOs.Model.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CabinetOs.Business.Concrete;

public class DeviceCommandService : IDeviceCommandService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;
    private readonly IMapper _mapper;
    public DeviceCommandService(IUnitOfWork unitOfWork, IValidationService validationService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _mapper = mapper;
    }

    public async Task<Result<DeviceCommand>> GetAsync(Expression<Func<DeviceCommand, bool>> where, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.DeviceCommands.GetAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<DeviceCommand>.NotFound();
        return Result<DeviceCommand>.Success(result);
    }

    public async Task<Result<DeviceCommand>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.DeviceCommands.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<DeviceCommand>.NotFound();
        return Result<DeviceCommand>.Success(result);
    }

    public async Task<Result<DeviceCommandDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.DeviceCommands.GetAsync<DeviceCommandDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<DeviceCommandDto>.NotFound();
        return Result<DeviceCommandDto>.Success(result);
    }

    public async Task<Result<ICollection<DeviceCommand>>> GetListAsync(Expression<Func<DeviceCommand, bool>>? where = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.DeviceCommands.GetAllAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<DeviceCommand>>.NotFound();
        return Result<ICollection<DeviceCommand>>.Success(result);
    }

    public async Task<Result<ICollection<DeviceCommand>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.DeviceCommands.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<DeviceCommand>>.NotFound();
        return Result<ICollection<DeviceCommand>>.Success(result);
    }

    public async Task<Result<ICollection<DeviceCommandDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.DeviceCommands.GetAllAsync<DeviceCommandDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<DeviceCommandDto>>.NotFound();
        return Result<ICollection<DeviceCommandDto>>.Success(result);
    }

    public async Task<Result<ICollection<SelectItemDto>>> SelectListAsync(Expression<Func<DeviceCommand, bool>>? where = default, CancellationToken cancellationToken = default)
    {
        var list = await _unitOfWork.DeviceCommands.GetAllAsync<SelectItemDto>(select: s => new SelectItemDto { Value = s.Id.ToString(), Text = s.PayloadJson ?? string.Empty }, where: where, cancellationToken: cancellationToken);
        var selectList = list ?? new List<SelectItemDto>();
        return Result<ICollection<SelectItemDto>>.Success(selectList);
    }

    public async Task<Result> CreateAsync(DeviceCommandCreateDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Validation(validationResult.Failures, description: $"Validation failed for DeviceCommandCreateDto");
        await _unitOfWork.DeviceCommands.AddAndSaveAsync(_mapper.Map<DeviceCommand>(request), cancellationToken);
        return Result.Success();
    }

    public async Task<Result<DeviceCommandUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.DeviceCommands.GetAsync<DeviceCommandUpdateDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<DeviceCommandUpdateDto>.NotFound();
        return Result<DeviceCommandUpdateDto>.Success(result);
    }

    public async Task<Result> UpdateAsync(DeviceCommandUpdateDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Validation(validationResult.Failures);
        var entity = await _unitOfWork.DeviceCommands.GetAsync(where: (f) => f.Id == request.Id, cancellationToken: cancellationToken);
        if (entity == null)
            return Result.NotFound();
        await _unitOfWork.DeviceCommands.UpdateAndSaveAsync(_mapper.Map(request, entity), cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var affected = await _unitOfWork.DeviceCommands.DeleteAndSaveAsync(where: (f) => f.Id == id, cancellationToken);
        if (affected == 0)
            return Result.NotFound();
        return Result.Success();
    }

    public async Task<Result> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var restored = await _unitOfWork.DeviceCommands.RestoreAndSaveAsync(where: (f) => f.Id == id, cancellationToken);
        if (restored == 0)
            return Result.NotFound();
        return Result.Success();
    }

    public async Task<Result<PaginationResponse<DeviceCommandDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.DeviceCommands.PaginationAsync<DeviceCommandDto>(configurationProvider: _mapper.ConfigurationProvider, paginationRequest: request, include: i => i.Include(x => x.Device).Include(x => x.RequesterUser), cancellationToken: cancellationToken);
        return Result<PaginationResponse<DeviceCommandDto>>.Success(result);
    }

    public async Task<Result<DatatableResponseClientSide<DeviceCommandDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.DeviceCommands.DatatableClientSideAsync<DeviceCommandDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: i => i.Include(x => x.Device).Include(x => x.RequesterUser), cancellationToken: cancellationToken);
        return Result<DatatableResponseClientSide<DeviceCommandDto>>.Success(result);
    }

    public async Task<Result<DatatableResponseServerSide<DeviceCommandDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.DeviceCommands.DatatableServerSideAsync<DeviceCommandDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: i => i.Include(x => x.Device).Include(x => x.RequesterUser), cancellationToken: cancellationToken);
        return Result<DatatableResponseServerSide<DeviceCommandDto>>.Success(result);
    }
}