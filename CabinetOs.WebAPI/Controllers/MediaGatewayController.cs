using CabinetOs.Business.Abstract;
using CabinetOs.Model.Dtos.Camera.Commands;
using CabinetOs.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CabinetOs.WebAPI.Controllers;

/// <summary>
/// Medya gecidinin (MediaMTX) kimlik dogrulama kancasi.
///
/// <b>Bu ucu bir tarayici cagirmaz</b> — MediaMTX cagirir. Bir istemci bir
/// yayini okumak istediginde gecit once buraya sorar; 200 alirsa akisi acar,
/// 401 alirsa reddeder. Yol <c>mediamtx.yml</c>'deki <c>authHTTPAddress</c>
/// ile eslesmek ZORUNDA.
///
/// Ayri bir controller: kamera CRUD'unun konusu degil, bir dis sistemin geri
/// arama noktasi. <c>CameraController</c>'a konsaydi
/// <c>[AllowAnonymous]</c> bir aksiyon, yetkilendirilmis uclarin arasinda
/// gozden kacardi.
///
/// Sozlesme: <c>docs/api-contract/11-camera.md</c>
/// </summary>
[AllowAnonymous]
public class MediaGatewayController : BaseController
{
    private readonly ICameraService _cameraService;

    public MediaGatewayController(ILogger<MediaGatewayController> logger, ICameraService cameraService) : base(logger)
    {
        _cameraService = cameraService;
    }

    /// <summary>
    /// Bileti dogrular.
    ///
    /// <b>Cevap govdesizdir</b> ve <c>ToAction</c> kullanilmaz: MediaMTX
    /// yalnizca HTTP durum koduna bakar, ProblemDetails okumaz. Bir
    /// ProblemDetails dondurmek, dogrulama hatalarinin ayrintisini kimlik
    /// dogrulamamis bir cagirana sizdirirdi.
    ///
    /// <b>Kendi rate limit politikasi var</b> ve o politikanin
    /// <c>Program.cs</c>'te TANIMLI OLMASI sart: tanimsiz bir ada isaret eden
    /// <c>[EnableRateLimiting]</c> middleware'i patlatir ve uc her istekte 500
    /// doner. (Tam olarak bu, <c>policy_scada_ingest</c> ile bir kez yasandi.)
    /// </summary>
    [HttpPost("auth")]
    [EnableRateLimiting("policy_mediamtx_auth")]
    public async Task<IActionResult> Auth([FromBody] MediaMtxAuthDto request, CancellationToken cancellationToken)
    {
        // YALNIZCA OKUMA. publish reddedilir: bu sisteme disaridan yayin
        // gonderilmesi diye bir senaryo yok ve izin verilseydi biri kameranin
        // yolunu kendi goruntusuyle degistirebilirdi.
        //
        // api / metrics / pprof zaten mediamtx.yml'deki authHTTPExclude
        // sayesinde buraya hic ulasmaz; ulasirlarsa da reddedilirler.
        if (!string.Equals(request?.Action, "read", StringComparison.OrdinalIgnoreCase))
            return Unauthorized();

        // Bilet, istemcinin gonderdigi Basic basliginin PAROLA kismindan gelir:
        // istemci `base64("ticket:" + bilet)` gonderir, MediaMTX ikiye ayirir.
        bool valid = await _cameraService.ValidateStreamTokenAsync(request.Path, request.Password, cancellationToken);

        return valid ? Ok() : Unauthorized();
    }
}
