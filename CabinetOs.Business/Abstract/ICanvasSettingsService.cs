using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.CanvasSettings.Commands;
using CabinetOs.Model.Dtos.CanvasSettings.Queries;
using CabinetOs.Model.Dtos.Diagram.Queries.Items;
using CabinetOs.Model.Entities;
using System.Linq.Expressions;

namespace CabinetOs.Business.Abstract;

public interface ICanvasSettingsService
{
    /// <summary>
    /// Kabinin canvas tercihlerini yazar; kayit yoksa olusturur (upsert).
    ///
    /// Donus tipi BILEREK <c>DiagramCanvasSettingsDto</c>: istemci yaniti diyagram
    /// aggregate'inin cache'indeki <c>canvasSettings</c> blogunun uzerine yaziyor,
    /// ayri bir tip iki sekli birbirinden sessizce ayirmak olurdu.
    /// </summary>
    Task<Result<DiagramCanvasSettingsDto>> UpsertAsync(Guid cabinetId, CanvasSettingsUpsertDto request, CancellationToken cancellationToken = default);

    Task<Result<CanvasSettings>> GetAsync(Expression<Func<CanvasSettings, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<CanvasSettings>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CanvasSettingsDto>> GetBaseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ICollection<CanvasSettings>>> GetListAsync(Expression<Func<CanvasSettings, bool>> where, CancellationToken cancellationToken = default);
    Task<Result<ICollection<CanvasSettings>>> GetListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
    Task<Result<ICollection<CanvasSettingsDto>>> GetBaseListAsync(DynamicRequest? request = default, CancellationToken cancellationToken = default);
}
