using Microsoft.AspNetCore.Mvc;
using CabinetOs.Business.Abstract;
using CabinetOs.WebAPI.Controllers.Base;
using CabinetOs.WebAPI.Utils;
using CabinetOs.Model.Dtos.Diagram.Commands;

namespace CabinetOs.WebAPI.Controllers;

public class DiagramController : BaseController
{
    private readonly IDiagramService _diagramService;
    private readonly TemplateImageStore _imageStore;

    public DiagramController(ILogger<DiagramController> logger, IDiagramService diagramService, TemplateImageStore imageStore) : base(logger)
    {
        _diagramService = diagramService;
        _imageStore = imageStore;
    }


    [HttpGet("cabinet/{cabinetId:guid}")]
    public async Task<IActionResult> GetCabinetDiagram(Guid cabinetId, CancellationToken cancellationToken)
    {
        var result = await _diagramService.GetAsync(cabinetId, cancellationToken);
        return ToAction(result);
    }

    [HttpGet("palette")]
    public async Task<IActionResult> GetPalette(CancellationToken cancellationToken)
    {
        var result = await _diagramService.GetPaletteAsync(cancellationToken);
        return ToAction(result);
    }

    [HttpPut("cabinet/{cabinetId:guid}/canvas-settings")]
    public async Task<IActionResult> UpsertCanvasSettings(Guid cabinetId, CanvasSettingsUpsertDto request, CancellationToken cancellationToken)
    {
        var result = await _diagramService.UpsertCanvasSettingsAsync(cabinetId, request, cancellationToken);
        return ToAction(result);
    }

    [HttpPost("cabinet/{cabinetId:guid}/save")]
    public async Task<IActionResult> Save(Guid cabinetId, DiagramSaveRequest request, CancellationToken cancellationToken)
    {
        var result = await _diagramService.SaveAsync(cabinetId, request, cancellationToken);
        return ToAction(result);
    }

    /// <summary> Palet(component-template) sablonunu ve pin(component-template-pin) semasini TEK transaction'da olusturur. </summary>
    [HttpPost("template")]
    public async Task<IActionResult> CreateTemplate(DiagramTemplateCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await _diagramService.CreateTemplateAsync(request, cancellationToken);
        return ToAction(result);
    }

    /// <summary> Sablon arka plan gorselini yukler ve GOreli URL'sini doner. Yuklenen dosya <c>wwwroot/uploads/templates</c> altina yazilir </summary>
    [HttpPost("template/image")]
    public async Task<IActionResult> UploadTemplateImage(IFormFile? file, CancellationToken cancellationToken)
    {
        var result = await _imageStore.SaveAsync(file, cancellationToken);
        return ToAction(result);
    }
}
