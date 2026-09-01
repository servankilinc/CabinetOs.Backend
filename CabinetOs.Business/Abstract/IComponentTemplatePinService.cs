using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.ComponentTemplatePin.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Abstract;

public interface IComponentTemplatePinService
{
    Task<Result<ComponentTemplatePin>> GetAsync(Expression<Func<ComponentTemplatePin, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<ComponentTemplatePin>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ComponentTemplatePinDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ICollection<ComponentTemplatePin>>> GetListAsync(Expression<Func<ComponentTemplatePin, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<ICollection<ComponentTemplatePin>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<ComponentTemplatePinDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
}
