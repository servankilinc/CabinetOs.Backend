using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace CabinetOs.WebAPI.Controllers;

public class DiagramAnnotationController : BaseController
{
    private readonly IDiagramAnnotationService _diagramAnnotationService;
    public DiagramAnnotationController(ILogger<DiagramAnnotationController> logger, IDiagramAnnotationService diagramAnnotationService) : base(logger)
    {
        _diagramAnnotationService = diagramAnnotationService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _diagramAnnotationService.GetBaseAsync(id: id);
        return ToAction(result);
    }

    [HttpGet("{id:guid}/base")]
    public async Task<IActionResult> GetBase(Guid id)
    {
        var result = await _diagramAnnotationService.GetBaseAsync(id: id);
        return ToAction(result);
    }

    [HttpPost("list")]
    public async Task<IActionResult> GetList(DynamicRequest? request = default)
    {
        var result = await _diagramAnnotationService.GetBaseListAsync(request);
        return ToAction(result);
    }

    [HttpPost("list/base")]
    public async Task<IActionResult> GetBaseList(DynamicRequest? request = default)
    {
        var result = await _diagramAnnotationService.GetBaseListAsync(request);
        return ToAction(result);
    }
}
