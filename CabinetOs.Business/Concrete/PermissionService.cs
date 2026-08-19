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
using CabinetOs.Model.Dtos.Permission.Queries;

namespace CabinetOs.Business.Concrete
{
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

        public async Task<Result<Permission>> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Permissions.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<Permission>.NotFound();
            return Result<Permission>.Success(result);
        }

        public async Task<Result<PemissionDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Permissions.GetAsync<PemissionDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<PemissionDto>.NotFound();
            return Result<PemissionDto>.Success(result);
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

        public async Task<Result<ICollection<PemissionDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Permissions.GetAllAsync<PemissionDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<PemissionDto>>.NotFound();
            return Result<ICollection<PemissionDto>>.Success(result);
        }

        public async Task<Result<SelectList>> SelectListAsync(Expression<Func<Permission, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var list = await _unitOfWork.Permissions.GetAllAsync<object>(select: s => new { s.Id, s.DisplayName }, where: where, cancellationToken: cancellationToken);
            var selectList = new SelectList(list ?? new List<object>(), "Id", "DisplayName");
            return Result<SelectList>.Success(selectList);
        }

        public async Task<Result> CreateAsync(Permission request, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.Permissions.AddAndSaveAsync(request, cancellationToken);
            return Result.Success();
        }

        public async Task<Result<PaginationResponse<PemissionDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Permissions.PaginationAsync<PemissionDto>(configurationProvider: _mapper.ConfigurationProvider, paginationRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<PaginationResponse<PemissionDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseClientSide<PemissionDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Permissions.DatatableClientSideAsync<PemissionDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<DatatableResponseClientSide<PemissionDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseServerSide<PemissionDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Permissions.DatatableServerSideAsync<PemissionDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<DatatableResponseServerSide<PemissionDto>>.Success(result);
        }
    }
}