using CabinetOs.Business.Abstract;
using CabinetOs.Model.Dtos.Camera.Commands;
using CabinetOs.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace CabinetOs.WebAPI.Controllers;

/// <summary>
/// Kamera yonetimi.
///
/// <b>Jenerik CRUD sablonundan ayrilir</b> (datatable / dinamik filtre /
/// selectlist yok) — <c>DiagramController</c> gibi EKRAN basina tasarlanmistir.
/// Tek listeleme sorusu var: "bu kabinde hangi kameralar var".
///
/// <b>DELETE ucu YOKTUR.</b> <c>Camera</c> <c>IActivatableEntity</c>'dir;
/// pasife alma <c>PUT</c> ile <c>isActive: false</c> gondererek yapilir.
///
/// Sozlesme: <c>docs/api-contract/11-camera.md</c>
/// </summary>
public class CameraController : BaseController
{
    private readonly ICameraService _cameraService;

    public CameraController(ILogger<CameraController> logger, ICameraService cameraService) : base(logger)
    {
        _cameraService = cameraService;
    }

    /// <summary>Bir kabindeki kameralar.</summary>
    [HttpGet("cabinet/{cabinetId:guid}")]
    public async Task<IActionResult> ListByCabinet(Guid cabinetId, [FromQuery] bool includePassive, CancellationToken cancellationToken)
    {
        var result = await _cameraService.GetListAsync(cabinetId, includePassive, cancellationToken);
        return ToAction(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _cameraService.GetAsync(id, cancellationToken);
        return ToAction(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CameraCreateDto request, CancellationToken cancellationToken)
    {
        var result = await _cameraService.CreateAsync(request, cancellationToken);
        return ToAction(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update(CameraUpdateDto request, CancellationToken cancellationToken)
    {
        var result = await _cameraService.UpdateAsync(request, cancellationToken);
        return ToAction(result);
    }

    /// <summary>
    /// Bir yoklama sonucunu yazar.
    ///
    /// <b>Bunu cagiran bir arka plan servisi bu turda YAZILMADI</b> — yoklayiciyi
    /// kullanici kendisi yazacak. Uc, o servisin harici bir surec olmasi
    /// durumunda kullanacagi giristir.
    /// </summary>
    [HttpPost("{id:guid}/probe-result")]
    public async Task<IActionResult> RecordProbeResult(Guid id, CameraProbeResultDto request, CancellationToken cancellationToken)
    {
        var result = await _cameraService.RecordProbeResultAsync(id, request, cancellationToken);
        return ToAction(result);
    }
}
