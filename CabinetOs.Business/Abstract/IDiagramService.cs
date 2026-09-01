using CabinetOs.Core.Utils.ResultPattern;
using CabinetOs.Model.Dtos.Diagram.Commands;
using CabinetOs.Model.Dtos.Diagram.Queries;

namespace CabinetOs.Business.Abstract;

/// <summary>
/// YALNIZCA diyagram grafi. Palet okuma/yazarligi <c>IComponentTemplateService</c>'te,
/// canvas tercihleri <c>ICanvasSettingsService</c>'tedir — ikisi de baska aggregate'ler.
/// </summary>
public interface IDiagramService
{
    Task<Result<DiagramDto>> GetAsync(Guid cabinetId, CancellationToken cancellationToken = default);

    Task<Result<DiagramSaveResponse>> SaveAsync(Guid cabinetId, DiagramSaveRequest request, CancellationToken cancellationToken = default);
}
