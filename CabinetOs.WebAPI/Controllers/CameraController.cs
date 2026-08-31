using CabinetOs.Business.Abstract;
using CabinetOs.Model.Dtos.Camera.Commands;
using CabinetOs.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using static CabinetOs.Model.Enums.EntityEnums;

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

    /// <summary>
    /// Canli izleme bileti.
    ///
    /// Govde YOK: istenen tek sey profil ve o da sorgu dizesinde. Bilet
    /// uretmek bir YAN ETKIDIR (medya gecidinde yol kurulur, onbellege kayit
    /// yazilir), dolayisiyla GET degil POST.
    ///
    /// Donen govdede RTSP adresi ve kamera parolasi <b>yoktur</b>.
    /// </summary>
    [HttpPost("{id:guid}/stream-ticket")]
    public async Task<IActionResult> CreateStreamTicket(Guid id, [FromQuery] StreamProfile profile, CancellationToken cancellationToken)
    {
        var result = await _cameraService.CreateStreamTicketAsync(id, profile, cancellationToken);
        return ToAction(result);
    }

    /// <summary>
    /// Anlik goruntu — <b>satir yazmaz</b>, canli onizleme icindir.
    ///
    /// <b>Bu aksiyon <c>ToAction</c> ile BITMEYEN tek aksiyondur</b> ve bu
    /// bilincli bir istisnadir: govdesi bir DTO degil ikili veridir, bir
    /// zarfa sarilamaz. Hata yolu yine <c>ToAction</c>'dan gecer, yani
    /// basarisizlik normal ProblemDetails sozlesmesini korur.
    /// </summary>
    /// <param name="fresh">
    /// <c>true</c> ise kisa omurlu onbellek atlanir ve kameradan taze kare istenir.
    /// </param>
    [HttpGet("{id:guid}/snapshot")]
    public async Task<IActionResult> GetSnapshot(Guid id, [FromQuery] bool fresh, CancellationToken cancellationToken)
    {
        var result = await _cameraService.GetSnapshotAsync(id, fresh, cancellationToken);
        if (!result.IsSuccess) return ToAction(result);

        return File(result.Data.Content, result.Data.ContentType);
    }

    /// <summary>
    /// Delil cekimi — diske yazar ve bir <c>CameraCapture</c> satiri birakir.
    ///
    /// Anlik goruntu senkron tamamlanir; klip <c>Pending</c> doner ve arka
    /// planda surer. <b>Basarisiz cekim de 200 doner</b>: istek gecerliydi ve
    /// bir satir olustu; basarisizlik <c>status</c> alanindadir. 500 donmek,
    /// istemcinin "kayit olustu mu" sorusunu cevapsiz birakirdi.
    /// </summary>
    [HttpPost("{id:guid}/capture")]
    public async Task<IActionResult> CreateCapture(Guid id, CameraCaptureCreateDto request, CancellationToken cancellationToken)
    {
        var result = await _cameraService.CreateCaptureAsync(id, request, cancellationToken);
        return ToAction(result);
    }

    /// <summary>Kameranin son cekimleri, yeniden eskiye.</summary>
    [HttpGet("{id:guid}/captures")]
    public async Task<IActionResult> GetCaptures(Guid id, [FromQuery] int take, CancellationToken cancellationToken)
    {
        var result = await _cameraService.GetCapturesAsync(id, take <= 0 ? 20 : take, cancellationToken);
        return ToAction(result);
    }
}
