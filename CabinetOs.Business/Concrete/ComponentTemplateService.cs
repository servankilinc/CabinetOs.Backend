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
using CabinetOs.Model.Dtos.ComponentTemplate.Commands;
using CabinetOs.Model.Dtos.ComponentTemplate.Queries;

namespace CabinetOs.Business.Concrete
{
    public class ComponentTemplateService : IComponentTemplateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidationService _validationService;
        private readonly IMapper _mapper;
        public ComponentTemplateService(IUnitOfWork unitOfWork, IValidationService validationService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _validationService = validationService;
            _mapper = mapper;
        }

        public async Task<Result<ComponentTemplate>> GetAsync(Expression<Func<ComponentTemplate, bool>> where, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.ComponentTemplates.GetAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ComponentTemplate>.NotFound();
            return Result<ComponentTemplate>.Success(result);
        }

        public async Task<Result<ComponentTemplate>> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.ComponentTemplates.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ComponentTemplate>.NotFound();
            return Result<ComponentTemplate>.Success(result);
        }

        public async Task<Result<ComponentTemplateBaseDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.ComponentTemplates.GetAsync<ComponentTemplateBaseDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ComponentTemplateBaseDto>.NotFound();
            return Result<ComponentTemplateBaseDto>.Success(result);
        }

        public async Task<Result<ComponentTemplateDetailDto>> GetComponentTemplateDetailDtoAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.ComponentTemplates.GetAsync<ComponentTemplateDetailDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ComponentTemplateDetailDto>.NotFound();
            return Result<ComponentTemplateDetailDto>.Success(result);
        }

        public async Task<Result<ICollection<ComponentTemplate>>> GetListAsync(Expression<Func<ComponentTemplate, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.ComponentTemplates.GetAllAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<ComponentTemplate>>.NotFound();
            return Result<ICollection<ComponentTemplate>>.Success(result);
        }

        public async Task<Result<ICollection<ComponentTemplate>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.ComponentTemplates.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<ComponentTemplate>>.NotFound();
            return Result<ICollection<ComponentTemplate>>.Success(result);
        }

        public async Task<Result<ICollection<ComponentTemplateBaseDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.ComponentTemplates.GetAllAsync<ComponentTemplateBaseDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<ComponentTemplateBaseDto>>.NotFound();
            return Result<ICollection<ComponentTemplateBaseDto>>.Success(result);
        }

        public async Task<Result<ICollection<ComponentTemplateDetailDto>>> GetComponentTemplateDetailDtoListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.ComponentTemplates.GetAllAsync<ComponentTemplateDetailDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<ComponentTemplateDetailDto>>.NotFound();
            return Result<ICollection<ComponentTemplateDetailDto>>.Success(result);
        }

        public async Task<Result<SelectList>> SelectListAsync(Expression<Func<ComponentTemplate, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var list = await _unitOfWork.ComponentTemplates.GetAllAsync<object>(select: s => new { s.Id, s.Name }, where: where, cancellationToken: cancellationToken);
            var selectList = new SelectList(list ?? new List<object>(), "Id", "Name");
            return Result<SelectList>.Success(selectList);
        }

        public async Task<Result> CreateAsync(ComponentTemplateCreateDto request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Validation(validationResult.Failures, description: $"Validation failed for ComponentTemplateCreateDto");
            await _unitOfWork.ComponentTemplates.AddAndSaveAsync(_mapper.Map<ComponentTemplate>(request), cancellationToken);
            return Result.Success();
        }

        public async Task<Result<ComponentTemplateUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.ComponentTemplates.GetAsync<ComponentTemplateUpdateDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ComponentTemplateUpdateDto>.NotFound();
            return Result<ComponentTemplateUpdateDto>.Success(result);
        }

        public async Task<Result> UpdateAsync(ComponentTemplateUpdateDto request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Validation(validationResult.Failures);
            var entity = await _unitOfWork.ComponentTemplates.GetAsync(where: (f) => f.Id == request.Id, cancellationToken: cancellationToken);
            if (entity == null)
                return Result.NotFound();
            await _unitOfWork.ComponentTemplates.UpdateAndSaveAsync(_mapper.Map(request, entity), cancellationToken);
            return Result.Success();
        }

        public async Task<Result<PaginationResponse<ComponentTemplateDetailDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.ComponentTemplates.PaginationAsync<ComponentTemplateDetailDto>(configurationProvider: _mapper.ConfigurationProvider, paginationRequest: request, include: i => i.Include(x => x.DeviceType), cancellationToken: cancellationToken);
            return Result<PaginationResponse<ComponentTemplateDetailDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseClientSide<ComponentTemplateDetailDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.ComponentTemplates.DatatableClientSideAsync<ComponentTemplateDetailDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: i => i.Include(x => x.DeviceType), cancellationToken: cancellationToken);
            return Result<DatatableResponseClientSide<ComponentTemplateDetailDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseServerSide<ComponentTemplateDetailDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.ComponentTemplates.DatatableServerSideAsync<ComponentTemplateDetailDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: i => i.Include(x => x.DeviceType), cancellationToken: cancellationToken);
            return Result<DatatableResponseServerSide<ComponentTemplateDetailDto>>.Success(result);
        }
    }
}