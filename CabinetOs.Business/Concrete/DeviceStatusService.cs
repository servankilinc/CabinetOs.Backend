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
using CabinetOs.Model.Dtos.DeviceStatus.Queries;

namespace CabinetOs.Business.Concrete
{
    public class DeviceStatusService : IDeviceStatusService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidationService _validationService;
        private readonly IMapper _mapper;
        public DeviceStatusService(IUnitOfWork unitOfWork, IValidationService validationService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _validationService = validationService;
            _mapper = mapper;
        }

        public async Task<Result<DeviceStatus>> GetAsync(Expression<Func<DeviceStatus, bool>> where, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DeviceStatuses.GetAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<DeviceStatus>.NotFound();
            return Result<DeviceStatus>.Success(result);
        }

        public async Task<Result<DeviceStatus>> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DeviceStatuses.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<DeviceStatus>.NotFound();
            return Result<DeviceStatus>.Success(result);
        }

        public async Task<Result<DeviceStatusDto>> GetBaseAsync(int id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DeviceStatuses.GetAsync<DeviceStatusDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<DeviceStatusDto>.NotFound();
            return Result<DeviceStatusDto>.Success(result);
        }

        public async Task<Result<ICollection<DeviceStatus>>> GetListAsync(Expression<Func<DeviceStatus, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DeviceStatuses.GetAllAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<DeviceStatus>>.NotFound();
            return Result<ICollection<DeviceStatus>>.Success(result);
        }

        public async Task<Result<ICollection<DeviceStatus>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DeviceStatuses.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<DeviceStatus>>.NotFound();
            return Result<ICollection<DeviceStatus>>.Success(result);
        }

        public async Task<Result<ICollection<DeviceStatusDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DeviceStatuses.GetAllAsync<DeviceStatusDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<DeviceStatusDto>>.NotFound();
            return Result<ICollection<DeviceStatusDto>>.Success(result);
        }

        public async Task<Result<SelectList>> SelectListAsync(Expression<Func<DeviceStatus, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var list = await _unitOfWork.DeviceStatuses.GetAllAsync<object>(select: s => new { s.Id, s.Name }, where: where, cancellationToken: cancellationToken);
            var selectList = new SelectList(list ?? new List<object>(), "Id", "Name");
            return Result<SelectList>.Success(selectList);
        }

        public async Task<Result> CreateAsync(DeviceStatus request, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.DeviceStatuses.AddAndSaveAsync(request, cancellationToken);
            return Result.Success();
        }

        public async Task<Result<PaginationResponse<DeviceStatusDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DeviceStatuses.PaginationAsync<DeviceStatusDto>(configurationProvider: _mapper.ConfigurationProvider, paginationRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<PaginationResponse<DeviceStatusDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseClientSide<DeviceStatusDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DeviceStatuses.DatatableClientSideAsync<DeviceStatusDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<DatatableResponseClientSide<DeviceStatusDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseServerSide<DeviceStatusDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DeviceStatuses.DatatableServerSideAsync<DeviceStatusDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<DatatableResponseServerSide<DeviceStatusDto>>.Success(result);
        }
    }
}