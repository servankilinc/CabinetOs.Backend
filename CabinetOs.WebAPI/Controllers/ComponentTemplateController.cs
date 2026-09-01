using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace CabinetOs.WebAPI.Controllers;

public class ComponentTemplateController : BaseController
{
    private readonly IComponentTemplateService _componentTemplateService;
    public ComponentTemplateController(ILogger<ComponentTemplateController> logger, IComponentTemplateService componentTemplateService) : base(logger)
    {
        _componentTemplateService = componentTemplateService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _componentTemplateService.GetComponentTemplateDetailDtoAsync(id: id);
        return ToAction(result);
    }

    [HttpGet("{id:guid}/base")]
    public async Task<IActionResult> GetBase(Guid id)
    {
        var result = await _componentTemplateService.GetBaseAsync(id: id);
        return ToAction(result);
    }

    [HttpGet("{id:guid}/componentTemplateDetailDto")]
    public async Task<IActionResult> GetComponentTemplateDetailDto(Guid id)
    {
        var result = await _componentTemplateService.GetComponentTemplateDetailDtoAsync(id: id);
        return ToAction(result);
    }

    [HttpPost("list")]
    public async Task<IActionResult> GetList(DynamicRequest? request = default)
    {
        var result = await _componentTemplateService.GetComponentTemplateDetailDtoListAsync(request);
        return ToAction(result);
    }

    [HttpPost("list/base")]
    public async Task<IActionResult> GetBaseList(DynamicRequest? request = default)
    {
        var result = await _componentTemplateService.GetBaseListAsync(request);
        return ToAction(result);
    }

    [HttpPost("list/componentTemplateDetailDto")]
    public async Task<IActionResult> GetComponentTemplateDetailDtoList(DynamicRequest? request = default)
    {
        var result = await _componentTemplateService.GetComponentTemplateDetailDtoListAsync(request);
        return ToAction(result);
    }
}
