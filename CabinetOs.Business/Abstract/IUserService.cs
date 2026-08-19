using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Entities;
using CabinetOs.Model.Dtos.User.Commands;
using CabinetOs.Model.Dtos.User.Queries;

namespace CabinetOs.Business.Abstract
{
    public interface IUserService
    {
        Task<Result<User>> GetAsync(Expression<Func<User, bool>> where, CancellationToken cancellationToken = default);
        Task<Result<User>> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<UserBaseDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<UserDetailDto>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<ICollection<User>>> GetListAsync(Expression<Func<User, bool>> where, CancellationToken cancellationToken = default);
        Task<Result<ICollection<User>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
        Task<Result<ICollection<UserBaseDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
        Task<Result<ICollection<UserDetailDto>>> GetDetailListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
        Task<Result<SelectList>> SelectListAsync(Expression<Func<User, bool>>? where = default, CancellationToken cancellationToken = default);
        Task<Result> CreateAsync(UserCreateDto request, CancellationToken cancellationToken = default);
        Task<Result<UserUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(UserUpdateDto request, CancellationToken cancellationToken = default);
        Task<Result<PaginationResponse<UserDetailDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
        Task<Result<DatatableResponseClientSide<UserDetailDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
        Task<Result<DatatableResponseServerSide<UserDetailDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    }
}