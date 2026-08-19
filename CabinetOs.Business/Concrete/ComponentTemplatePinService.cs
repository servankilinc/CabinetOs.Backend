using AutoMapper;
using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Core.Utils.Validation;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.ComponentTemplatePin.Commands;
using CabinetOs.Model.Dtos.ComponentTemplatePin.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Concrete;

public class ComponentTemplatePinService : IComponentTemplatePinService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;
    private readonly IMapper _mapper;
    public ComponentTemplatePinService(IUnitOfWork unitOfWork, IValidationService validationService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _mapper = mapper;
    }

    public async Task<Result<ComponentTemplatePin>> GetAsync(Expression<Func<ComponentTemplatePin, bool>> where, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplatePins.GetAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ComponentTemplatePin>.NotFound();
        return Result<ComponentTemplatePin>.Success(result);
    }

    public async Task<Result<ComponentTemplatePin>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplatePins.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ComponentTemplatePin>.NotFound();
        return Result<ComponentTemplatePin>.Success(result);
    }

    public async Task<Result<ComponentTemplatePinDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplatePins.GetAsync<ComponentTemplatePinDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ComponentTemplatePinDto>.NotFound();
        return Result<ComponentTemplatePinDto>.Success(result);
    }

    public async Task<Result<ICollection<ComponentTemplatePin>>> GetListAsync(Expression<Func<ComponentTemplatePin, bool>>? where = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplatePins.GetAllAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<ComponentTemplatePin>>.NotFound();
        return Result<ICollection<ComponentTemplatePin>>.Success(result);
    }

    public async Task<Result<ICollection<ComponentTemplatePin>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplatePins.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<ComponentTemplatePin>>.NotFound();
        return Result<ICollection<ComponentTemplatePin>>.Success(result);
    }

    public async Task<Result<ICollection<ComponentTemplatePinDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplatePins.GetAllAsync<ComponentTemplatePinDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<ComponentTemplatePinDto>>.NotFound();
        return Result<ICollection<ComponentTemplatePinDto>>.Success(result);
    }

    public async Task<Result<ICollection<SelectItemDto>>> SelectListAsync(Expression<Func<ComponentTemplatePin, bool>>? where = default, CancellationToken cancellationToken = default)
    {
        var list = await _unitOfWork.ComponentTemplatePins.GetAllAsync<SelectItemDto>(select: s => new SelectItemDto { Value = s.Id.ToString(), Text = s.Name }, where: where, cancellationToken: cancellationToken);
        var selectList = list ?? new List<SelectItemDto>();
        return Result<ICollection<SelectItemDto>>.Success(selectList);
    }

    public async Task<Result> CreateAsync(ComponentTemplatePinCreateDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Validation(validationResult.Failures, description: $"Validation failed for ComponentTemplatePinCreateDto");
        await _unitOfWork.ComponentTemplatePins.AddAndSaveAsync(_mapper.Map<ComponentTemplatePin>(request), cancellationToken);
        return Result.Success();
    }

    public async Task<Result<ComponentTemplatePinUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplatePins.GetAsync<ComponentTemplatePinUpdateDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ComponentTemplatePinUpdateDto>.NotFound();
        return Result<ComponentTemplatePinUpdateDto>.Success(result);
    }

    public async Task<Result> UpdateAsync(ComponentTemplatePinUpdateDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Validation(validationResult.Failures);
        var entity = await _unitOfWork.ComponentTemplatePins.GetAsync(where: (f) => f.Id == request.Id, cancellationToken: cancellationToken);
        if (entity == null)
            return Result.NotFound();
        await _unitOfWork.ComponentTemplatePins.UpdateAndSaveAsync(_mapper.Map(request, entity), cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var affected = await _unitOfWork.ComponentTemplatePins.DeleteAndSaveAsync(where: (f) => f.Id == id, cancellationToken);
        if (affected == 0)
            return Result.NotFound();
        return Result.Success();
    }

    public async Task<Result<PaginationResponse<ComponentTemplatePinDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplatePins.PaginationAsync<ComponentTemplatePinDto>(configurationProvider: _mapper.ConfigurationProvider, paginationRequest: request, include: null, cancellationToken: cancellationToken);
        return Result<PaginationResponse<ComponentTemplatePinDto>>.Success(result);
    }

    public async Task<Result<DatatableResponseClientSide<ComponentTemplatePinDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplatePins.DatatableClientSideAsync<ComponentTemplatePinDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
        return Result<DatatableResponseClientSide<ComponentTemplatePinDto>>.Success(result);
    }

    public async Task<Result<DatatableResponseServerSide<ComponentTemplatePinDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ComponentTemplatePins.DatatableServerSideAsync<ComponentTemplatePinDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
        return Result<DatatableResponseServerSide<ComponentTemplatePinDto>>.Success(result);
    }
}