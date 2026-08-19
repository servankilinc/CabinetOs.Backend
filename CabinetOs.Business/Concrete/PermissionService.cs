using AutoMapper;
using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Core.Utils.Validation;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.Permission.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Concrete;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;
    private readonly IMapper _mapper;
    public PermissionService(IUnitOfWork unitOfWork, IValidationService validationService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _mapper = mapper;
    }

    public async Task<Result<Permission>> GetAsync(Expression<Func<Permission, bool>> where, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Permissions.GetAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<Permission>.NotFound();
        return Result<Permission>.Success(result);
    }

    public async Task<Result<Permission>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Permissions.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<Permission>.NotFound();
        return Result<Permission>.Success(result);
    }

    public async Task<Result<PermissionDto>> GetBaseAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Permissions.GetAsync<PermissionDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
        if (result == null)
            return Result<PermissionDto>.NotFound();
        return Result<PermissionDto>.Success(result);
    }

    public async Task<Result<ICollection<Permission>>> GetListAsync(Expression<Func<Permission, bool>>? where = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Permissions.GetAllAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<Permission>>.NotFound();
        return Result<ICollection<Permission>>.Success(result);
    }

    public async Task<Result<ICollection<Permission>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Permissions.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<Permission>>.NotFound();
        return Result<ICollection<Permission>>.Success(result);
    }

    public async Task<Result<ICollection<PermissionDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Permissions.GetAllAsync<PermissionDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<PermissionDto>>.NotFound();
        return Result<ICollection<PermissionDto>>.Success(result);
    }

    public async Task<Result<ICollection<SelectItemDto>>> SelectListAsync(Expression<Func<Permission, bool>>? where = default, CancellationToken cancellationToken = default)
    {
        var list = await _unitOfWork.Permissions.GetAllAsync<SelectItemDto>(select: s => new SelectItemDto { Value = s.Id.ToString(), Text = s.DisplayName }, where: where, cancellationToken: cancellationToken);
        var selectList = list ?? new List<SelectItemDto>();
        return Result<ICollection<SelectItemDto>>.Success(selectList);
    }


    public async Task<Result<PaginationResponse<PermissionDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Permissions.PaginationAsync<PermissionDto>(configurationProvider: _mapper.ConfigurationProvider, paginationRequest: request, include: null, cancellationToken: cancellationToken);
        return Result<PaginationResponse<PermissionDto>>.Success(result);
    }

    public async Task<Result<DatatableResponseClientSide<PermissionDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Permissions.DatatableClientSideAsync<PermissionDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
        return Result<DatatableResponseClientSide<PermissionDto>>.Success(result);
    }

    public async Task<Result<DatatableResponseServerSide<PermissionDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Permissions.DatatableServerSideAsync<PermissionDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
        return Result<DatatableResponseServerSide<PermissionDto>>.Success(result);
    }
}