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
using CabinetOs.Model.Dtos.Role.Commands;
using CabinetOs.Model.Dtos.Role.Queries;

namespace CabinetOs.Business.Concrete
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidationService _validationService;
        private readonly IMapper _mapper;
        public RoleService(IUnitOfWork unitOfWork, IValidationService validationService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _validationService = validationService;
            _mapper = mapper;
        }

        public async Task<Result<Role>> GetAsync(Expression<Func<Role, bool>> where, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Roles.GetAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<Role>.NotFound();
            return Result<Role>.Success(result);
        }

        public async Task<Result<Role>> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Roles.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<Role>.NotFound();
            return Result<Role>.Success(result);
        }

        public async Task<Result<RoleDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Roles.GetAsync<RoleDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<RoleDto>.NotFound();
            return Result<RoleDto>.Success(result);
        }

        public async Task<Result<ICollection<Role>>> GetListAsync(Expression<Func<Role, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Roles.GetAllAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<Role>>.NotFound();
            return Result<ICollection<Role>>.Success(result);
        }

        public async Task<Result<ICollection<Role>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Roles.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<Role>>.NotFound();
            return Result<ICollection<Role>>.Success(result);
        }

        public async Task<Result<ICollection<RoleDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Roles.GetAllAsync<RoleDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<RoleDto>>.NotFound();
            return Result<ICollection<RoleDto>>.Success(result);
        }

        public async Task<Result<SelectList>> SelectListAsync(Expression<Func<Role, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var list = await _unitOfWork.Roles.GetAllAsync<object>(select: s => new { s.Id, s.Name }, where: where, cancellationToken: cancellationToken);
            var selectList = new SelectList(list ?? new List<object>(), "Id", "Name");
            return Result<SelectList>.Success(selectList);
        }

        public async Task<Result> CreateAsync(RoleCreateDto request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Validation(validationResult.Failures, description: $"Validation failed for RoleCreateDto");
            await _unitOfWork.Roles.AddAndSaveAsync(_mapper.Map<Role>(request), cancellationToken);
            return Result.Success();
        }

        public async Task<Result<RoleUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Roles.GetAsync<RoleUpdateDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<RoleUpdateDto>.NotFound();
            return Result<RoleUpdateDto>.Success(result);
        }

        public async Task<Result> UpdateAsync(RoleUpdateDto request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Validation(validationResult.Failures);
            var entity = await _unitOfWork.Roles.GetAsync(where: (f) => f.Id == request.Id, cancellationToken: cancellationToken);
            if (entity == null)
                return Result.NotFound();
            await _unitOfWork.Roles.UpdateAndSaveAsync(_mapper.Map(request, entity), cancellationToken);
            return Result.Success();
        }

        public async Task<Result<PaginationResponse<RoleDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Roles.PaginationAsync<RoleDto>(configurationProvider: _mapper.ConfigurationProvider, paginationRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<PaginationResponse<RoleDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseClientSide<RoleDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Roles.DatatableClientSideAsync<RoleDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<DatatableResponseClientSide<RoleDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseServerSide<RoleDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Roles.DatatableServerSideAsync<RoleDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<DatatableResponseServerSide<RoleDto>>.Success(result);
        }
    }
}