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
using CabinetOs.Model.Dtos.Company.Commands;
using CabinetOs.Model.Dtos.Company.Queries;

namespace CabinetOs.Business.Concrete
{
    public class CompanyService : ICompanyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidationService _validationService;
        private readonly IMapper _mapper;
        public CompanyService(IUnitOfWork unitOfWork, IValidationService validationService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _validationService = validationService;
            _mapper = mapper;
        }

        public async Task<Result<Company>> GetAsync(Expression<Func<Company, bool>> where, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Companies.GetAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<Company>.NotFound();
            return Result<Company>.Success(result);
        }

        public async Task<Result<Company>> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Companies.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<Company>.NotFound();
            return Result<Company>.Success(result);
        }

        public async Task<Result<CompanyDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Companies.GetAsync<CompanyDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<CompanyDto>.NotFound();
            return Result<CompanyDto>.Success(result);
        }

        public async Task<Result<ICollection<Company>>> GetListAsync(Expression<Func<Company, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Companies.GetAllAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<Company>>.NotFound();
            return Result<ICollection<Company>>.Success(result);
        }

        public async Task<Result<ICollection<Company>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Companies.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<Company>>.NotFound();
            return Result<ICollection<Company>>.Success(result);
        }

        public async Task<Result<ICollection<CompanyDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Companies.GetAllAsync<CompanyDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<CompanyDto>>.NotFound();
            return Result<ICollection<CompanyDto>>.Success(result);
        }

        public async Task<Result<SelectList>> SelectListAsync(Expression<Func<Company, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var list = await _unitOfWork.Companies.GetAllAsync<object>(select: s => new { s.Id, s.Name }, where: where, cancellationToken: cancellationToken);
            var selectList = new SelectList(list ?? new List<object>(), "Id", "Name");
            return Result<SelectList>.Success(selectList);
        }

        public async Task<Result> CreateAsync(CompanyCreateDto request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Validation(validationResult.Failures, description: $"Validation failed for CompanyCreateDto");
            await _unitOfWork.Companies.AddAndSaveAsync(_mapper.Map<Company>(request), cancellationToken);
            return Result.Success();
        }

        public async Task<Result<CompanyUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Companies.GetAsync<CompanyUpdateDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<CompanyUpdateDto>.NotFound();
            return Result<CompanyUpdateDto>.Success(result);
        }

        public async Task<Result> UpdateAsync(CompanyUpdateDto request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Validation(validationResult.Failures);
            var entity = await _unitOfWork.Companies.GetAsync(where: (f) => f.Id == request.Id, cancellationToken: cancellationToken);
            if (entity == null)
                return Result.NotFound();
            await _unitOfWork.Companies.UpdateAndSaveAsync(_mapper.Map(request, entity), cancellationToken);
            return Result.Success();
        }

        public async Task<Result<PaginationResponse<CompanyDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Companies.PaginationAsync<CompanyDto>(configurationProvider: _mapper.ConfigurationProvider, paginationRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<PaginationResponse<CompanyDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseClientSide<CompanyDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Companies.DatatableClientSideAsync<CompanyDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<DatatableResponseClientSide<CompanyDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseServerSide<CompanyDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.Companies.DatatableServerSideAsync<CompanyDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<DatatableResponseServerSide<CompanyDto>>.Success(result);
        }
    }
}