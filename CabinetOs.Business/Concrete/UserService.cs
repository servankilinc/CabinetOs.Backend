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
using CabinetOs.Model.Dtos.User.Commands;
using CabinetOs.Model.Dtos.User.Queries;

namespace CabinetOs.Business.Concrete
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidationService _validationService;
        private readonly IMapper _mapper;
        public UserService(IUnitOfWork unitOfWork, IValidationService validationService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _validationService = validationService;
            _mapper = mapper;
        }

        public async Task<Result<User>> GetAsync(Expression<Func<User, bool>> where, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Users.GetAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<User>.NotFound();
            return Result<User>.Success(result);
        }

        public async Task<Result<User>> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Users.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<User>.NotFound();
            return Result<User>.Success(result);
        }

        public async Task<Result<UserBaseDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Users.GetAsync<UserBaseDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<UserBaseDto>.NotFound();
            return Result<UserBaseDto>.Success(result);
        }

        public async Task<Result<UserDetailDto>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Users.GetAsync<UserDetailDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<UserDetailDto>.NotFound();
            return Result<UserDetailDto>.Success(result);
        }

        public async Task<Result<ICollection<User>>> GetListAsync(Expression<Func<User, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Users.GetAllAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<User>>.NotFound();
            return Result<ICollection<User>>.Success(result);
        }

        public async Task<Result<ICollection<User>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Users.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<User>>.NotFound();
            return Result<ICollection<User>>.Success(result);
        }

        public async Task<Result<ICollection<UserBaseDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Users.GetAllAsync<UserBaseDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<UserBaseDto>>.NotFound();
            return Result<ICollection<UserBaseDto>>.Success(result);
        }

        public async Task<Result<ICollection<UserDetailDto>>> GetDetailListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Users.GetAllAsync<UserDetailDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<UserDetailDto>>.NotFound();
            return Result<ICollection<UserDetailDto>>.Success(result);
        }

        public async Task<Result<SelectList>> SelectListAsync(Expression<Func<User, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var list = await _unitOfWork.Users.GetAllAsync<object>(select: s => new { s.Id, s.UserName }, where: where, cancellationToken: cancellationToken);
            var selectList = new SelectList(list ?? new List<object>(), "Id", "UserName");
            return Result<SelectList>.Success(selectList);
        }

        public async Task<Result> CreateAsync(UserCreateDto request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Validation(validationResult.Failures, description: $"Validation failed for UserCreateDto");
            await _unitOfWork.Users.AddAndSaveAsync(_mapper.Map<User>(request), cancellationToken);
            return Result.Success();
        }

        public async Task<Result<UserUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Users.GetAsync<UserUpdateDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<UserUpdateDto>.NotFound();
            return Result<UserUpdateDto>.Success(result);
        }

        public async Task<Result> UpdateAsync(UserUpdateDto request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Validation(validationResult.Failures);
            var entity = await _unitOfWork.Users.GetAsync(where: (f) => f.Id == request.Id, cancellationToken: cancellationToken);
            if (entity == null)
                return Result.NotFound();
            await _unitOfWork.Users.UpdateAndSaveAsync(_mapper.Map(request, entity), cancellationToken);
            return Result.Success();
        }

        public async Task<Result<PaginationResponse<UserDetailDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Users.PaginationAsync<UserDetailDto>(configurationProvider: _mapper.ConfigurationProvider, paginationRequest: request, include: i => i.Include(x => x.Comany), cancellationToken: cancellationToken);
            return Result<PaginationResponse<UserDetailDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseClientSide<UserDetailDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Users.DatatableClientSideAsync<UserDetailDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: i => i.Include(x => x.Comany), cancellationToken: cancellationToken);
            return Result<DatatableResponseClientSide<UserDetailDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseServerSide<UserDetailDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Users.DatatableServerSideAsync<UserDetailDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: i => i.Include(x => x.Comany), cancellationToken: cancellationToken);
            return Result<DatatableResponseServerSide<UserDetailDto>>.Success(result);
        }
    }
}