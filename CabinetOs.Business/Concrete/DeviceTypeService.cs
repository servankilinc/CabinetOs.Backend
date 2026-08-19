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
using CabinetOs.Model.Dtos.DeviceType.Queries;

namespace CabinetOs.Business.Concrete
{
    public class DeviceTypeService : IDeviceTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidationService _validationService;
        private readonly IMapper _mapper;
        public DeviceTypeService(IUnitOfWork unitOfWork, IValidationService validationService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _validationService = validationService;
            _mapper = mapper;
        }

        public async Task<Result<DeviceType>> GetAsync(Expression<Func<DeviceType, bool>> where, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DeviceTypes.GetAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<DeviceType>.NotFound();
            return Result<DeviceType>.Success(result);
        }

        public async Task<Result<DeviceType>> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DeviceTypes.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<DeviceType>.NotFound();
            return Result<DeviceType>.Success(result);
        }

        public async Task<Result<DeviceTypeDto>> GetBaseAsync(int id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DeviceTypes.GetAsync<DeviceTypeDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<DeviceTypeDto>.NotFound();
            return Result<DeviceTypeDto>.Success(result);
        }

        public async Task<Result<ICollection<DeviceType>>> GetListAsync(Expression<Func<DeviceType, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DeviceTypes.GetAllAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<DeviceType>>.NotFound();
            return Result<ICollection<DeviceType>>.Success(result);
        }

        public async Task<Result<ICollection<DeviceType>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DeviceTypes.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<DeviceType>>.NotFound();
            return Result<ICollection<DeviceType>>.Success(result);
        }

        public async Task<Result<ICollection<DeviceTypeDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DeviceTypes.GetAllAsync<DeviceTypeDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<DeviceTypeDto>>.NotFound();
            return Result<ICollection<DeviceTypeDto>>.Success(result);
        }

        public async Task<Result<SelectList>> SelectListAsync(Expression<Func<DeviceType, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var list = await _unitOfWork.DeviceTypes.GetAllAsync<object>(select: s => new { s.Id, s.Name }, where: where, cancellationToken: cancellationToken);
            var selectList = new SelectList(list ?? new List<object>(), "Id", "Name");
            return Result<SelectList>.Success(selectList);
        }

        public async Task<Result> CreateAsync(DeviceType request, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.DeviceTypes.AddAndSaveAsync(request, cancellationToken);
            return Result.Success();
        }

        public async Task<Result<PaginationResponse<DeviceTypeDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DeviceTypes.PaginationAsync<DeviceTypeDto>(configurationProvider: _mapper.ConfigurationProvider, paginationRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<PaginationResponse<DeviceTypeDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseClientSide<DeviceTypeDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DeviceTypes.DatatableClientSideAsync<DeviceTypeDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<DatatableResponseClientSide<DeviceTypeDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseServerSide<DeviceTypeDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DeviceTypes.DatatableServerSideAsync<DeviceTypeDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<DatatableResponseServerSide<DeviceTypeDto>>.Success(result);
        }
    }
}