using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace CabinetOs.WebAPI.Controllers;

public class CanvasSettingsController : BaseController
{
    private readonly ICanvasSettingsService _canvasSettingsService;
    public CanvasSettingsController(ILogger<CanvasSettingsController> logger, ICanvasSettingsService canvasSettingsService) : base(logger)
    {
        _canvasSettingsService = canvasSettingsService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _canvasSettingsService.GetBaseAsync(id: id);
        return ToAction(result);
    }

    [HttpGet("{id:guid}/base")]
    public async Task<IActionResult> GetBase(Guid id)
    {
        var result = await _canvasSettingsService.GetBaseAsync(id: id);
        return ToAction(result);
    }

    [HttpPost("list")]
    public async Task<IActionResult> GetList(DynamicRequest? request = default)
    {
        var result = await _canvasSettingsService.GetBaseListAsync(request);
        return ToAction(result);
    }

    [HttpPost("list/base")]
    public async Task<IActionResult> GetBaseList(DynamicRequest? request = default)
    {
        var result = await _canvasSettingsService.GetBaseListAsync(request);
        return ToAction(result);
    }
}
