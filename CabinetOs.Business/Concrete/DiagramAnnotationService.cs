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
using CabinetOs.Model.Dtos.DiagramAnnotation.Commands;
using CabinetOs.Model.Dtos.DiagramAnnotation.Queries;

namespace CabinetOs.Business.Concrete
{
    public class DiagramAnnotationService : IDiagramAnnotationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidationService _validationService;
        private readonly IMapper _mapper;
        public DiagramAnnotationService(IUnitOfWork unitOfWork, IValidationService validationService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _validationService = validationService;
            _mapper = mapper;
        }

        public async Task<Result<DiagramAnnotation>> GetAsync(Expression<Func<DiagramAnnotation, bool>> where, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DiagramAnnotations.GetAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<DiagramAnnotation>.NotFound();
            return Result<DiagramAnnotation>.Success(result);
        }

        public async Task<Result<DiagramAnnotation>> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DiagramAnnotations.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<DiagramAnnotation>.NotFound();
            return Result<DiagramAnnotation>.Success(result);
        }

        public async Task<Result<DiagramAnnotationDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DiagramAnnotations.GetAsync<DiagramAnnotationDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<DiagramAnnotationDto>.NotFound();
            return Result<DiagramAnnotationDto>.Success(result);
        }

        public async Task<Result<ICollection<DiagramAnnotation>>> GetListAsync(Expression<Func<DiagramAnnotation, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DiagramAnnotations.GetAllAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<DiagramAnnotation>>.NotFound();
            return Result<ICollection<DiagramAnnotation>>.Success(result);
        }

        public async Task<Result<ICollection<DiagramAnnotation>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DiagramAnnotations.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<DiagramAnnotation>>.NotFound();
            return Result<ICollection<DiagramAnnotation>>.Success(result);
        }

        public async Task<Result<ICollection<DiagramAnnotationDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DiagramAnnotations.GetAllAsync<DiagramAnnotationDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<DiagramAnnotationDto>>.NotFound();
            return Result<ICollection<DiagramAnnotationDto>>.Success(result);
        }

        public async Task<Result<SelectList>> SelectListAsync(Expression<Func<DiagramAnnotation, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var list = await _unitOfWork.DiagramAnnotations.GetAllAsync<object>(select: s => new { s.Id, s.Name }, where: where, cancellationToken: cancellationToken);
            var selectList = new SelectList(list ?? new List<object>(), "Id", "Name");
            return Result<SelectList>.Success(selectList);
        }

        public async Task<Result> CreateAsync(DiagramAnnotationCreateDto request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Validation(validationResult.Failures, description: $"Validation failed for DiagramAnnotationCreateDto");
            await _unitOfWork.DiagramAnnotations.AddAndSaveAsync(_mapper.Map<DiagramAnnotation>(request), cancellationToken);
            return Result.Success();
        }

        public async Task<Result<DiagramAnnotationUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DiagramAnnotations.GetAsync<DiagramAnnotationUpdateDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<DiagramAnnotationUpdateDto>.NotFound();
            return Result<DiagramAnnotationUpdateDto>.Success(result);
        }

        public async Task<Result> UpdateAsync(DiagramAnnotationUpdateDto request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Validation(validationResult.Failures);
            var entity = await _unitOfWork.DiagramAnnotations.GetAsync(where: (f) => f.Id == request.Id, cancellationToken: cancellationToken);
            if (entity == null)
                return Result.NotFound();
            await _unitOfWork.DiagramAnnotations.UpdateAndSaveAsync(_mapper.Map(request, entity), cancellationToken);
            return Result.Success();
        }

        public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var affected = await _unitOfWork.DiagramAnnotations.DeleteAndSaveAsync(where: (f) => f.Id == id, cancellationToken);
            if (affected == 0)
                return Result.NotFound();
            return Result.Success();
        }

        public async Task<Result<PaginationResponse<DiagramAnnotationDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DiagramAnnotations.PaginationAsync<DiagramAnnotationDto>(configurationProvider: _mapper.ConfigurationProvider, paginationRequest: request, include: i => i.Include(x => x.Cabinet), cancellationToken: cancellationToken);
            return Result<PaginationResponse<DiagramAnnotationDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseClientSide<DiagramAnnotationDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DiagramAnnotations.DatatableClientSideAsync<DiagramAnnotationDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: i => i.Include(x => x.Cabinet), cancellationToken: cancellationToken);
            return Result<DatatableResponseClientSide<DiagramAnnotationDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseServerSide<DiagramAnnotationDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.DiagramAnnotations.DatatableServerSideAsync<DiagramAnnotationDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: i => i.Include(x => x.Cabinet), cancellationToken: cancellationToken);
            return Result<DatatableResponseServerSide<DiagramAnnotationDto>>.Success(result);
        }
    }
}