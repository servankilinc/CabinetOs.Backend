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

namespace CabinetOs.Business.Concrete
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidationService _validationService;
        private readonly IMapper _mapper;
        public AuditLogService(IUnitOfWork unitOfWork, IValidationService validationService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _validationService = validationService;
            _mapper = mapper;
        }

        public async Task<Result<AuditLog>> GetAsync(Expression<Func<AuditLog, bool>> where, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.AuditLogs.GetAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<AuditLog>.NotFound();
            return Result<AuditLog>.Success(result);
        }

        public async Task<Result<AuditLog>> GetAsync(long id, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.AuditLogs.GetAsync(where: (f) => f.Id == id, cancellationToken: cancellationToken);
            if (result == null)
                return Result<AuditLog>.NotFound();
            return Result<AuditLog>.Success(result);
        }

        public async Task<Result<ICollection<AuditLog>>> GetListAsync(Expression<Func<AuditLog, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.AuditLogs.GetAllAsync(where: where, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<AuditLog>>.NotFound();
            return Result<ICollection<AuditLog>>.Success(result);
        }

        public async Task<Result<ICollection<AuditLog>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.AuditLogs.GetAllAsync(filter: request?.Filter, sorts: request?.Sorts, tracking: false, cancellationToken: cancellationToken);
            if (result == null)
                return Result<ICollection<AuditLog>>.NotFound();
            return Result<ICollection<AuditLog>>.Success(result);
        }

        public async Task<Result<SelectList>> SelectListAsync(Expression<Func<AuditLog, bool>>? where = default, CancellationToken cancellationToken = default)
        {
            var list = await _unitOfWork.AuditLogs.GetAllAsync<object>(select: s => new { s.Id, s.Details }, where: where, cancellationToken: cancellationToken);
            var selectList = new SelectList(list ?? new List<object>(), "Id", "Details");
            return Result<SelectList>.Success(selectList);
        }

        public async Task<Result> CreateAsync(AuditLog request, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.AuditLogs.AddAndSaveAsync(request, cancellationToken);
            return Result.Success();
        }

        public async Task<Result<PaginationResponse<AuditLog>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.AuditLogs.PaginationAsync(paginationRequest: request, cancellationToken: cancellationToken);
            return Result<PaginationResponse<AuditLog>>.Success(result);
        }

        public async Task<Result<DatatableResponseClientSide<AuditLog>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.AuditLogs.DatatableClientSideAsync(datatableRequest: request, cancellationToken: cancellationToken);
            return Result<DatatableResponseClientSide<AuditLog>>.Success(result);
        }

        public async Task<Result<DatatableResponseServerSide<AuditLog>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _unitOfWork.AuditLogs.DatatableServerSideAsync(datatableRequest: request, cancellationToken: cancellationToken);
            return Result<DatatableResponseServerSide<AuditLog>>.Success(result);
        }
    }
}