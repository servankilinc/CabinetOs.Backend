using CabinetOs.Business.Abstract;
using CabinetOs.Model.Dtos.Scada.Commands;
using CabinetOs.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CabinetOs.WebAPI.Controllers;

/// <summary>
/// SCADA'nin telemetriyi BIZE push ettigi uc.
///
/// Sozlesme: <c>docs/api-contract/07-scada-ingest.md</c> — bu dokuman ayni zamanda
/// SCADA ekibine verilecek entegrasyon spesifikasyonudur.
/// </summary>
public class ScadaController : BaseController
{
    private readonly IChannelEventService _channelEventService;

    public ScadaController(ILogger<ScadaController> logger, IChannelEventService channelEventService) : base(logger)
    {
        _channelEventService = channelEventService;
    }

    /// <summary>
    /// Bir kabinin telemetri paketi.
    ///
    /// <b>[AllowAnonymous]</b>: SCADA bir kullanici degil, JWT'si yok. Kimlik
    /// govdedeki <c>cabinetId</c>. Bunun bir sir olmadigi ve sertlestirmenin
    /// (<c>Cabinet.IngestKey</c>) hangi kosulda zorunlu hale geldigi
    /// <see cref="ScadaIngestRequest"/> uzerinde yaziyor.
    ///
    /// <b>Ayri rate limit politikasi</b>: varsayilan limiter IP'ye gore
    /// bolumlendiriyor ve 50 istek/10 sn veriyor. SCADA tek IP oldugu icin butun
    /// ingest trafigi TEK bolume duser ve saniyede bes istekte bogulurdu.
    /// </summary>
    [HttpPost("ingest")]
    [AllowAnonymous]
    [EnableRateLimiting("policy_scada_ingest")]
    public async Task<IActionResult> Ingest(ScadaIngestRequest request, CancellationToken cancellationToken)
    {
        var result = await _channelEventService.IngestAsync(request, cancellationToken);
        return ToAction(result);
    }
}
