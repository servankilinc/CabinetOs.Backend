using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.Company.Commands;
using CabinetOs.Model.Dtos.Company.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Abstract;

public interface ICompanyService
{
    Task<Result<Company>> GetAsync(Expression<Func<Company, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<Company>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CompanyDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ICollection<Company>>> GetListAsync(Expression<Func<Company, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<ICollection<Company>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<CompanyDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<SelectItemDto>>> SelectListAsync(Expression<Func<Company, bool>>? where = default, CancellationToken cancellationToken = default);
    Task<Result> CreateAsync(CompanyCreateDto request, CancellationToken cancellationToken = default);
    Task<Result<CompanyUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(CompanyUpdateDto request, CancellationToken cancellationToken = default);
    Task<Result<PaginationResponse<CompanyDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseClientSide<CompanyDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseServerSide<CompanyDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
}