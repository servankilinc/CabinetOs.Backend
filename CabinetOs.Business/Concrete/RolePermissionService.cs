using AutoMapper;
using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Core.Utils.Validation;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Concrete;

public class RolePermissionService : IRolePermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;
    private readonly IMapper _mapper;
    public RolePermissionService(IUnitOfWork unitOfWork, IValidationService validationService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _mapper = mapper;
    }

    public async Task<Result<RolePermission>> GetAsync(Expression<Func<RolePermission, bool>> where, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.RolePermissions.GetAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<RolePermission>.NotFound();
        return Result<RolePermission>.Success(result);
    }

    public async Task<Result<RolePermission>> GetAsync(Guid permissionId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.RolePermissions.GetAsync(where: (f) => f.PermissionId == permissionId && f.RoleId == roleId, cancellationToken: cancellationToken);
        if (result == null)
            return Result<RolePermission>.NotFound();
        return Result<RolePermission>.Success(result);
    }

    public async Task<Result<ICollection<RolePermission>>> GetListAsync(Expression<Func<RolePermission, bool>>? where = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.RolePermissions.GetAllAsync(where: where, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<RolePermission>>.NotFound();
        return Result<ICollection<RolePermission>>.Success(result);
    }

    public async Task<Result<ICollection<RolePermission>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.RolePermissions.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
        if (result == null)
            return Result<ICollection<RolePermission>>.NotFound();
        return Result<ICollection<RolePermission>>.Success(result);
    }

    public async Task<Result> CreateAsync(RolePermission request, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.RolePermissions.AddAndSaveAsync(request, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> UpdateAsync(RolePermission request, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.RolePermissions.GetAsync(where: (f) => f.PermissionId == request.PermissionId && f.RoleId == request.RoleId, cancellationToken: cancellationToken);
        if (entity == null)
            return Result.NotFound();
        await _unitOfWork.RolePermissions.UpdateAndSaveAsync(_mapper.Map(request, entity), cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid permissionId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var affected = await _unitOfWork.RolePermissions.DeleteAndSaveAsync(where: (f) => f.PermissionId == permissionId && f.RoleId == roleId, cancellationToken);
        if (affected == 0)
            return Result.NotFound();
        return Result.Success();
    }

    public async Task<Result<PaginationResponse<RolePermission>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.RolePermissions.PaginationAsync(paginationRequest: request, cancellationToken: cancellationToken);
        return Result<PaginationResponse<RolePermission>>.Success(result);
    }

    public async Task<Result<DatatableResponseClientSide<RolePermission>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.RolePermissions.DatatableClientSideAsync(datatableRequest: request, cancellationToken: cancellationToken);
        return Result<DatatableResponseClientSide<RolePermission>>.Success(result);
    }

    public async Task<Result<DatatableResponseServerSide<RolePermission>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.RolePermissions.DatatableServerSideAsync(datatableRequest: request, cancellationToken: cancellationToken);
        return Result<DatatableResponseServerSide<RolePermission>>.Success(result);
    }
}