using Microsoft.AspNetCore.Mvc;
using CabinetOs.WebAPI.Controllers.Base;
using CabinetOs.Model.Dtos.Diagram.Commands;
using CabinetOs.Business.Utils.Diagram;

namespace CabinetOs.WebAPI.Controllers;

/// <summary>
/// YALNIZCA diyagram grafi. Palet ve sablon yazarligi <c>ComponentTemplateController</c>'da,
/// canvas tercihleri <c>CanvasSettingsController</c>'dadir.
/// </summary>
public class DiagramController : BaseController
{
    private readonly IDiagramService _diagramService;

    public DiagramController(ILogger<DiagramController> logger, IDiagramService diagramService) : base(logger)
    {
        _diagramService = diagramService;
    }


    [HttpGet("cabinet/{cabinetId:guid}")]
    public async Task<IActionResult> GetCabinetDiagram(Guid cabinetId, CancellationToken cancellationToken)
    {
        var result = await _diagramService.GetAsync(cabinetId, cancellationToken);
        return ToAction(result);
    }

    [HttpPost("cabinet/{cabinetId:guid}/save")]
    public async Task<IActionResult> Save(Guid cabinetId, DiagramSaveRequest request, CancellationToken cancellationToken)
    {
        var result = await _diagramService.SaveAsync(cabinetId, request, cancellationToken);
        return ToAction(result);
    }
}
