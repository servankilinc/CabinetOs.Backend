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
using CabinetOs.Model.Dtos.IoChannel.Commands;
using CabinetOs.Model.Dtos.IoChannel.Queries;

namespace CabinetOs.Business.Concrete
{
    public class IoChannelService : IIoChannelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidationService _validationService;
        private readonly IMapper _mapper;
        public IoChannelService(IUnitOfWork unitOfWork, IValidationService validationService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _validationService = validationService;
            _mapper = mapper;
        }

        public async Task<Result<IoChannel>> GetAsync(Expression<Func<IoChannel, bool>> where, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.IoChannels.GetAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<IoChannel>.NotFound();
            return Result<IoChannel>.Success(result);
        }

        public async Task<Result<IoChannel>> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.IoChannels.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<IoChannel>.NotFound();
            return Result<IoChannel>.Success(result);
        }

        public async Task<Result<IoChannelDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.IoChannels.GetAsync<IoChannelDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<IoChannelDto>.NotFound();
            return Result<IoChannelDto>.Success(result);
        }

        public async Task<Result<ICollection<IoChannel>>> GetListAsync(Expression<Func<IoChannel, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.IoChannels.GetAllAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<IoChannel>>.NotFound();
            return Result<ICollection<IoChannel>>.Success(result);
        }

        public async Task<Result<ICollection<IoChannel>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.IoChannels.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<IoChannel>>.NotFound();
            return Result<ICollection<IoChannel>>.Success(result);
        }

        public async Task<Result<ICollection<IoChannelDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.IoChannels.GetAllAsync<IoChannelDto>(configurationProvider: _mapper.ConfigurationProvider, filter: request?.Filter, sorts: request?.Sorts, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<IoChannelDto>>.NotFound();
            return Result<ICollection<IoChannelDto>>.Success(result);
        }

        public async Task<Result<SelectList>> SelectListAsync(Expression<Func<IoChannel, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var list = await _unitOfWork.IoChannels.GetAllAsync<object>(select: s => new { s.Id, s.Name }, where: where, cancellationToken: cancellationToken);
            var selectList = new SelectList(list ?? new List<object>(), "Id", "Name");
            return Result<SelectList>.Success(selectList);
        }

        public async Task<Result> CreateAsync(IoChannelCreateDto request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Validation(validationResult.Failures, description: $"Validation failed for IoChannelCreateDto");
            await _unitOfWork.IoChannels.AddAndSaveAsync(_mapper.Map<IoChannel>(request), cancellationToken);
            return Result.Success();
        }

        public async Task<Result<IoChannelUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.IoChannels.GetAsync<IoChannelUpdateDto>(configurationProvider: _mapper.ConfigurationProvider, where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<IoChannelUpdateDto>.NotFound();
            return Result<IoChannelUpdateDto>.Success(result);
        }

        public async Task<Result> UpdateAsync(IoChannelUpdateDto request, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Validation(validationResult.Failures);
            var entity = await _unitOfWork.IoChannels.GetAsync(where: (f) => f.Id == request.Id, cancellationToken: cancellationToken);
            if (entity == null)
                return Result.NotFound();
            await _unitOfWork.IoChannels.UpdateAndSaveAsync(_mapper.Map(request, entity), cancellationToken);
            return Result.Success();
        }

        public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var affected = await _unitOfWork.IoChannels.DeleteAndSaveAsync(where: (f) => f.Id == id, cancellationToken);
            if (affected == 0)
                return Result.NotFound();
            return Result.Success();
        }

        public async Task<Result> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var restored = await _unitOfWork.IoChannels.RestoreAndSaveAsync(where: (f) => f.Id == id, cancellationToken);
            if (restored == 0)
                return Result.NotFound();
            return Result.Success();
        }

        public async Task<Result<PaginationResponse<IoChannelDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.IoChannels.PaginationAsync<IoChannelDto>(configurationProvider: _mapper.ConfigurationProvider, paginationRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<PaginationResponse<IoChannelDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseClientSide<IoChannelDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.IoChannels.DatatableClientSideAsync<IoChannelDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<DatatableResponseClientSide<IoChannelDto>>.Success(result);
        }

        public async Task<Result<DatatableResponseServerSide<IoChannelDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.IoChannels.DatatableServerSideAsync<IoChannelDto>(configurationProvider: _mapper.ConfigurationProvider, datatableRequest: request, include: null, cancellationToken: cancellationToken);
            return Result<DatatableResponseServerSide<IoChannelDto>>.Success(result);
        }
    }
}