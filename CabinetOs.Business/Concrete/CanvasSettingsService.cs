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
using CabinetOs.Model.Dtos.CanvasSettings.Commands;
using CabinetOs.Model.Dtos.CanvasSettings.Queries;

namespace CabinetOs.Business.Concrete
{
    public class CanvasSettingsService : ICanvasSettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidationService _validationService;
        private readonly IMapper _mapper;
        public CanvasSettingsService(IUnitOfWork unitOfWork, IValidationService validationService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _validationService = validationService;
            _mapper = mapper;
        }

        public async Task<Result<CanvasSettings>> GetAsync(Expression<Func<CanvasSettings, bool>> where, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.CanvasSettings.GetAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<CanvasSettings>.NotFound();
            return Result<CanvasSettings>.Success(result);
        }

        public async Task<Result<CanvasSettings>> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.CanvasSettings.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<CanvasSettings>.NotFound();
            return Result<CanvasSettings>.Success(result);
        }

        public async Task<Result<CanvasSettingsDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.CanvasSettings.GetAsync<CanvasSettingsDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<CanvasSettingsDto>.NotFound();
            return Result<CanvasSettingsDto>.Success(result);
        }

        public async Task<Result<ICollection<CanvasSettings>>> GetListAsync(Expression<Func<CanvasSettings, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.CanvasSettings.GetAllAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<CanvasSettings>>.NotFound();
            return Result<ICollection<CanvasSettings>>.Success(result);
        }

        public async Task<Result<ICollection<CanvasSettings>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.CanvasSettings.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<CanvasSettings>>.NotFound();
            return Result<ICollection<CanvasSettings>>.Success(result);
        }

        public async Task<Result<ICollection<CanvasSettingsDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.CanvasSettings.GetAllAsync<CanvasSettingsDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<CanvasSettingsDto>>.NotFound();
            return Result<ICollection<CanvasSettingsDto>>.Success(result);
        }

        public async Task<Result<SelectList>> SelectListAsync(Expression<Func<CanvasSettings, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var list = await _unitOfWork.CanvasSettings.GetAllAsync<object>(select: s => new { s.Id, s.GridColor }, where: where, cancellationToken: cancellationToken);
            var selectList = new SelectList(list ?? new List<object>(), "Id", "GridColor");
            return Result<SelectList>.Success(selectList);
        }

        public async Task<Result> CreateAsync(CanvasSettingsCreateDto request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Validation(validationResult.Failures, description: $"Validation failed for CanvasSettingsCreateDto");
            await _unitOfWork.CanvasSettings.AddAndSaveAsync(_mapper.Map<CanvasSettings>(request), cancellationToken);
            return Result.Success();
        }

        public async Task<Result<CanvasSettingsUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.CanvasSettings.GetAsync<CanvasSettingsUpdateDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<CanvasSettingsUpdateDto>.NotFound();
            return Result<CanvasSettingsUpdateDto>.Success(result);
        }

        public async Task<Result> UpdateAsync(CanvasSettingsUpdateDto request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Validation(validationResult.Failures);
            var entity = await _unitOfWork.CanvasSettings.GetAsync(where: (f) => f.Id == request.Id, cancellationToken: cancellationToken);
            if (entity == null)
                return Result.NotFound();
            await _unitOfWork.CanvasSettings.UpdateAndSaveAsync(_mapper.Map(request, entity), cancellationToken);
            return Result.Success();
        }

        public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var affected = await _unitOfWork.CanvasSettings.DeleteAndSaveAsync(where: (f) => f.Id == id, cancellationToken);
            if (affected == 0)
                return Result.NotFound();
            return Result.Success();
        }

        public async Task<Result<PaginationResponse<CanvasSettingsDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.CanvasSettings.PaginationAsync<CanvasSettingsDto>(configurationProvider: _mapper.ConfigurationProvider, paginationRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<PaginationResponse<CanvasSettingsDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseClientSide<CanvasSettingsDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.CanvasSettings.DatatableClientSideAsync<CanvasSettingsDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<DatatableResponseClientSide<CanvasSettingsDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseServerSide<CanvasSettingsDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.CanvasSettings.DatatableServerSideAsync<CanvasSettingsDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<DatatableResponseServerSide<CanvasSettingsDto>>.Success(result);
        }
    }
}