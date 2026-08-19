using AutoMapper;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Core.Utils.Validation;
using CabinetOs.DataAccess.Abstract;
using CabinetOs.DataAccess.UoW;
using CabinetOs.Model.Entities;

namespace CabinetOs.Business.Concrete
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidationService _validationService;
        private readonly IMapper _mapper;
        public UserRoleService(IUnitOfWork unitOfWork, IValidationService validationService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _validationService = validationService;
            _mapper = mapper;
        }

        public async Task<Result<UserRole>> GetAsync(Expression<Func<UserRole, bool>> where, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.UserRoles.GetAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<UserRole>.NotFound();
            return Result<UserRole>.Success(result);
        }

        public async Task<Result<UserRole>> GetAsync(Guid roleId, Guid userId, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.UserRoles.GetAsync(where: (f) => f.RoleId == roleId && f.UserId == userId, cancellationToken: cancellationToken);
            if (result == null)
                return Result<UserRole>.NotFound();
            return Result<UserRole>.Success(result);
        }

        public async Task<Result<ICollection<UserRole>>> GetListAsync(Expression<Func<UserRole, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.UserRoles.GetAllAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<UserRole>>.NotFound();
            return Result<ICollection<UserRole>>.Success(result);
        }

        public async Task<Result<ICollection<UserRole>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.UserRoles.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<UserRole>>.NotFound();
            return Result<ICollection<UserRole>>.Success(result);
        }

        public async Task<Result> CreateAsync(UserRole request, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.UserRoles.AddAndSaveAsync(request, cancellationToken);
            return Result.Success();
        }

        public async Task<Result> UpdateAsync(UserRole request, CancellationToken cancellationToken = default)
        {
            var entity = await _unitOfWork.UserRoles.GetAsync(where: (f) => f.RoleId == request.RoleId && f.UserId == request.UserId, cancellationToken: cancellationToken);
            if (entity == null)
                return Result.NotFound();
            await _unitOfWork.UserRoles.UpdateAndSaveAsync(_mapper.Map(request, entity), cancellationToken);
            return Result.Success();
        }

        public async Task<Result> DeleteAsync(Guid roleId, Guid userId, CancellationToken cancellationToken = default)
        {
            var affected = await _unitOfWork.UserRoles.DeleteAndSaveAsync(where: (f) => f.RoleId == roleId && f.UserId == userId, cancellationToken);
            if (affected == 0)
                return Result.NotFound();
            return Result.Success();
        }

        public async Task<Result<PaginationResponse<UserRole>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.UserRoles.PaginationAsync(paginationRequest: request, cancellationToken: cancellationToken);
            return Result<PaginationResponse<UserRole>>.Success(result);
        }

        public async Task<Result<DatatableResponseClientSide<UserRole>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.UserRoles.DatatableClientSideAsync(datatableRequest: request, cancellationToken: cancellationToken);
            return Result<DatatableResponseClientSide<UserRole>>.Success(result);
        }

        public async Task<Result<DatatableResponseServerSide<UserRole>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.UserRoles.DatatableServerSideAsync(datatableRequest: request, cancellationToken: cancellationToken);
            return Result<DatatableResponseServerSide<UserRole>>.Success(result);
        }
    }
}