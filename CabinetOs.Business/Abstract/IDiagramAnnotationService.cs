using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.Pagination;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.DiagramAnnotation.Commands;
using CabinetOs.Model.Dtos.DiagramAnnotation.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Abstract;

public interface IDiagramAnnotationService
{
    Task<Result<DiagramAnnotation>> GetAsync(Expression<Func<DiagramAnnotation, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<DiagramAnnotation>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<DiagramAnnotationDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ICollection<DiagramAnnotation>>> GetListAsync(Expression<Func<DiagramAnnotation, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<ICollection<DiagramAnnotation>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<DiagramAnnotationDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<SelectItemDto>>> SelectListAsync(Expression<Func<DiagramAnnotation, bool>>? where = default, CancellationToken cancellationToken = default);
    Task<Result<CreatedDto>> CreateAsync(DiagramAnnotationCreateDto request, CancellationToken cancellationToken = default);
    Task<Result<DiagramAnnotationUpdateDto>> GetUpdateModelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(DiagramAnnotationUpdateDto request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PaginationResponse<DiagramAnnotationDto>>> PaginationAsync(DynamicPaginationRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseClientSide<DiagramAnnotationDto>>> DatatableClientSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
    Task<Result<DatatableResponseServerSide<DiagramAnnotationDto>>> DatatableServerSideAsync(DynamicDatatableRequest request, CancellationToken cancellationToken = default);
}