using AutoMapper;
using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Core.Utils.Validation;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.Connection.Commands;
using CabinetOs.Model.Dtos.Connection.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Concrete;

public class ConnectionService : IConnectionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;
    private readonly IMapper _mapper;
    public ConnectionService(IUnitOfWork unitOfWork, IValidationService validationService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _mapper = mapper;
    }

    public async Task<Result<Connection>> GetAsync(Expression<Func<Connection, bool>> where, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Connections.GetAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<Connection>.NotFound();
        return Result<Connection>.Success(result);
    }

    public async Task<Result<Connection>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Connections.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<Connection>.NotFound();
        return Result<Connection>.Success(result);
    }

    public async Task<Result<ConnectionDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Connections.GetAsync<ConnectionDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ConnectionDto>.NotFound();
        return Result<ConnectionDto>.Success(result);
    }

    public async Task<Result<ICollection<Connection>>> GetListAsync(Expression<Func<Connection, bool>>? where = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Connections.GetAllAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<Connection>>.NotFound();
        return Result<ICollection<Connection>>.Success(result);
    }

    public async Task<Result<ICollection<Connection>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Connections.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<Connection>>.NotFound();
        return Result<ICollection<Connection>>.Success(result);
    }

    public async Task<Result<ICollection<ConnectionDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Connections.GetAllAsync<ConnectionDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<ConnectionDto>>.NotFound();
        return Result<ICollection<ConnectionDto>>.Success(result);
    }

    public async Task<Result<ICollection<SelectItemDto>>> SelectListAsync(Expression<Func<Connection, bool>>? where = default, CancellationToken cancellationToken = default)
    {
        var list = await _unitOfWork.Connections.GetAllAsync<SelectItemDto>(select: s => new SelectItemDto { Value = s.Id.ToString(), Text = s.Label }, where: where, cancellationToken: cancellationToken);
        var selectList = list ?? new List<SelectItemDto>();
        return Result<ICollection<SelectItemDto>>.Success(selectList);
    }

    public async Task<Result> CreateAsync(ConnectionCreateDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Validation(validationResult.Failures, description: $"Validation failed for ConnectionCreateDto");
        await _unitOfWork.Connections.AddAndSaveAsync(_mapper.Map<Connection>(request), cancellationToken);
        return Result.Success();
    }

    public async Task<Result<ConnectionUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Connections.GetAsync<ConnectionUpdateDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ConnectionUpdateDto>.NotFound();
        return Result<ConnectionUpdateDto>.Success(result);
    }

    public async Task<Result> UpdateAsync(ConnectionUpdateDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Validation(validationResult.Failures);
        var entity = await _unitOfWork.Connections.GetAsync(where: (f) => f.Id == request.Id, cancellationToken: cancellationToken);
        if (entity == null)
            return Result.NotFound();
        await _unitOfWork.Connections.UpdateAndSaveAsync(_mapper.Map(request, entity), cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var affected = await _unitOfWork.Connections.DeleteAndSaveAsync(where: (f) => f.Id == id, cancellationToken);
        if (affected == 0)
            return Result.NotFound();
        return Result.Success();
    }

    public async Task<Result> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var restored = await _unitOfWork.Connections.RestoreAndSaveAsync(where: (f) => f.Id == id, cancellationToken);
        if (restored == 0)
            return Result.NotFound();
        return Result.Success();
    }

    public async Task<Result<PaginationResponse<ConnectionDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Connections.PaginationAsync<ConnectionDto>(configurationProvider: _mapper.ConfigurationProvider, paginationRequest: request, include: null, cancellationToken: cancellationToken);
        return Result<PaginationResponse<ConnectionDto>>.Success(result);
    }

    public async Task<Result<DatatableResponseClientSide<ConnectionDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Connections.DatatableClientSideAsync<ConnectionDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
        return Result<DatatableResponseClientSide<ConnectionDto>>.Success(result);
    }

    public async Task<Result<DatatableResponseServerSide<ConnectionDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Connections.DatatableServerSideAsync<ConnectionDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
        return Result<DatatableResponseServerSide<ConnectionDto>>.Success(result);
    }
}