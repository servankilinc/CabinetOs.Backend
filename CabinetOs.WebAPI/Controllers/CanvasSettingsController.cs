using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Model.Dtos.CanvasSettings.Commands;
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

    /// <summary>
    /// Kabinin canvas tercihlerini yazar; kayit yoksa olusturur (upsert).
    ///
    /// <c>cabinetId</c> ROTADAN alinir, govdede yoktur: ikisi de bulunsaydi
    /// celisebilir ve hangisinin kazandigi belirsiz kalirdi.
    /// </summary>
    [HttpPut("cabinet/{cabinetId:guid}")]
    public async Task<IActionResult> Upsert(Guid cabinetId, CanvasSettingsUpsertDto request, CancellationToken cancellationToken)
    {
        var result = await _canvasSettingsService.UpsertAsync(cabinetId, request, cancellationToken);
        return ToAction(result);
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
