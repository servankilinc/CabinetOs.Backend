using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.CanvasSettings.Queries;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Abstract;

public interface ICanvasSettingsService
{
    Task<Result<CanvasSettings>> GetAsync(Expression<Func<CanvasSettings, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<CanvasSettings>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CanvasSettingsDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ICollection<CanvasSettings>>> GetListAsync(Expression<Func<CanvasSettings, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<ICollection<CanvasSettings>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<CanvasSettingsDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
}
