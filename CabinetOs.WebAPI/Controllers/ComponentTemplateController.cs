using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Model.Dtos.ComponentTemplate.Commands;
using CabinetOs.WebAPI.Controllers.Base;
using CabinetOs.WebAPI.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CabinetOs.WebAPI.Controllers;

public class ComponentTemplateController : BaseController
{
    private readonly IComponentTemplateService _componentTemplateService;
    private readonly TemplateImageStore _imageStore;
    public ComponentTemplateController(ILogger<ComponentTemplateController> logger, IComponentTemplateService componentTemplateService, TemplateImageStore imageStore) : base(logger)
    {
        _componentTemplateService = componentTemplateService;
        _imageStore = imageStore;
    }

    /// <summary>
    /// Stencil kutuphanesi. Her kabinette ayni oldugu icin diyagram aggregate'ine
    /// gomulmez; ayri uc + uzun staleTime.
    ///
    /// <c>{id:guid}</c> ile CAKISMAZ: kisit "palette" segmentini eslestirmez.
    /// </summary>
    [HttpGet("palette")]
    public async Task<IActionResult> GetPalette(CancellationToken cancellationToken)
    {
        var result = await _componentTemplateService.GetPaletteAsync(cancellationToken);
        return ToAction(result);
    }

    /// <summary> Sablonu ve pin semasini TEK transaction'da olusturur. </summary>
    [HttpPost]
    public async Task<IActionResult> Create(ComponentTemplateCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await _componentTemplateService.CreateAsync(request, cancellationToken);
        return ToAction(result);
    }

    /// <summary> Sablon arka plan gorselini yukler ve goreli URL'sini doner. Yuklenen dosya <c>wwwroot/uploads/templates</c> altina yazilir </summary>
    [HttpPost("image")]
    public async Task<IActionResult> UploadImage(IFormFile? file, CancellationToken cancellationToken)
    {
        var result = await _imageStore.SaveAsync(file, cancellationToken);
        return ToAction(result);
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
