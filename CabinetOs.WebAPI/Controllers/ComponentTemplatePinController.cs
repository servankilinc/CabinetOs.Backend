using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace CabinetOs.WebAPI.Controllers;

public class ComponentTemplatePinController : BaseController
{
    private readonly IComponentTemplatePinService _componentTemplatePinService;
    public ComponentTemplatePinController(ILogger<ComponentTemplatePinController> logger, IComponentTemplatePinService componentTemplatePinService) : base(logger)
    {
        _componentTemplatePinService = componentTemplatePinService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _componentTemplatePinService.GetBaseAsync(id: id);
        return ToAction(result);
    }

    [HttpGet("{id:guid}/base")]
    public async Task<IActionResult> GetBase(Guid id)
    {
        var result = await _componentTemplatePinService.GetBaseAsync(id: id);
        return ToAction(result);
    }

    [HttpPost("list")]
    public async Task<IActionResult> GetList(DynamicRequest? request = default)
    {
        var result = await _componentTemplatePinService.GetBaseListAsync(request);
        return ToAction(result);
    }

    [HttpPost("list/base")]
    public async Task<IActionResult> GetBaseList(DynamicRequest? request = default)
    {
        var result = await _componentTemplatePinService.GetBaseListAsync(request);
        return ToAction(result);
    }
}
