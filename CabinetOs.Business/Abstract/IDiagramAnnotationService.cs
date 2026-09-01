using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.ResultPattern;
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
}
