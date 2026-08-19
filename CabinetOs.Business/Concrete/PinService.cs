using AutoMapper;
using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Core.Utils.Validation;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.Pin.Commands;
using CabinetOs.Model.Dtos.Pin.Queries;
using CabinetOs.Model.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CabinetOs.Business.Concrete;

public class PinService : IPinService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;
    private readonly IMapper _mapper;
    public PinService(IUnitOfWork unitOfWork, IValidationService validationService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _mapper = mapper;
    }

    public async Task<Result<Pin>> GetAsync(Expression<Func<Pin, bool>> where, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.GetAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<Pin>.NotFound();
        return Result<Pin>.Success(result);
    }

    public async Task<Result<Pin>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<Pin>.NotFound();
        return Result<Pin>.Success(result);
    }

    public async Task<Result<PinDetailDto>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.GetAsync<PinDetailDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<PinDetailDto>.NotFound();
        return Result<PinDetailDto>.Success(result);
    }

    public async Task<Result<PinDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.GetAsync<PinDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<PinDto>.NotFound();
        return Result<PinDto>.Success(result);
    }

    public async Task<Result<ICollection<Pin>>> GetListAsync(Expression<Func<Pin, bool>>? where = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.GetAllAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<Pin>>.NotFound();
        return Result<ICollection<Pin>>.Success(result);
    }

    public async Task<Result<ICollection<Pin>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<Pin>>.NotFound();
        return Result<ICollection<Pin>>.Success(result);
    }

    public async Task<Result<ICollection<PinDetailDto>>> GetDetailListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.GetAllAsync<PinDetailDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<PinDetailDto>>.NotFound();
        return Result<ICollection<PinDetailDto>>.Success(result);
    }

    public async Task<Result<ICollection<PinDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.GetAllAsync<PinDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<PinDto>>.NotFound();
        return Result<ICollection<PinDto>>.Success(result);
    }

    public async Task<Result<ICollection<SelectItemDto>>> SelectListAsync(Expression<Func<Pin, bool>>? where = default, CancellationToken cancellationToken = default)
    {
        var list = await _unitOfWork.Pins.GetAllAsync<SelectItemDto>(select: s => new SelectItemDto { Value = s.Id.ToString(), Text = s.Name }, where: where, cancellationToken: cancellationToken);
        var selectList = list ?? new List<SelectItemDto>();
        return Result<ICollection<SelectItemDto>>.Success(selectList);
    }

    public async Task<Result> CreateAsync(PinCreateDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Validation(validationResult.Failures, description: $"Validation failed for PinCreateDto");
        await _unitOfWork.Pins.AddAndSaveAsync(_mapper.Map<Pin>(request), cancellationToken);
        return Result.Success();
    }

    public async Task<Result<PinUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.GetAsync<PinUpdateDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<PinUpdateDto>.NotFound();
        return Result<PinUpdateDto>.Success(result);
    }

    public async Task<Result> UpdateAsync(PinUpdateDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Validation(validationResult.Failures);
        var entity = await _unitOfWork.Pins.GetAsync(where: (f) => f.Id == request.Id, cancellationToken: cancellationToken);
        if (entity == null)
            return Result.NotFound();
        await _unitOfWork.Pins.UpdateAndSaveAsync(_mapper.Map(request, entity), cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var affected = await _unitOfWork.Pins.DeleteAndSaveAsync(where: (f) => f.Id == id, cancellationToken);
        if (affected == 0)
            return Result.NotFound();
        return Result.Success();
    }

    public async Task<Result> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var restored = await _unitOfWork.Pins.RestoreAndSaveAsync(where: (f) => f.Id == id, cancellationToken);
        if (restored == 0)
            return Result.NotFound();
        return Result.Success();
    }

    public async Task<Result<PaginationResponse<PinDetailDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.PaginationAsync<PinDetailDto>(configurationProvider: _mapper.ConfigurationProvider, paginationRequest: request, include: i => i.Include(x => x.IoChannel).Include(x => x.Device), cancellationToken: cancellationToken);
        return Result<PaginationResponse<PinDetailDto>>.Success(result);
    }

    public async Task<Result<DatatableResponseClientSide<PinDetailDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.DatatableClientSideAsync<PinDetailDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: i => i.Include(x => x.IoChannel).Include(x => x.Device), cancellationToken: cancellationToken);
        return Result<DatatableResponseClientSide<PinDetailDto>>.Success(result);
    }

    public async Task<Result<DatatableResponseServerSide<PinDetailDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Pins.DatatableServerSideAsync<PinDetailDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: i => i.Include(x => x.IoChannel).Include(x => x.Device), cancellationToken: cancellationToken);
        return Result<DatatableResponseServerSide<PinDetailDto>>.Success(result);
    }
}