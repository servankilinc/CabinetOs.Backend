using CabinetOs.Business.Abstract;
using CabinetOs.Model.Dtos.ChannelEvent.Queries;
using CabinetOs.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace CabinetOs.WebAPI.Controllers;

/// <summary>
/// Kanal olaylarinin okundugu uc.
///
/// <b>Salt okunur.</b> Create/Update/Delete YOKTUR: olaylari yalnizca SCADA
/// ingest'i uretir ve bir olay sonradan degistirilemez. Jenerik CRUD sablonundan
/// bilerek ayrilir — <c>DiagramController</c> gibi EKRAN basina tasarlanmistir.
///
/// Sozlesme: <c>docs/api-contract/12-channel-events.md</c>
/// </summary>
public class ChannelEventController : BaseController
{
    private readonly IChannelEventService _channelEventService;

    public ChannelEventController(ILogger<ChannelEventController> logger, IChannelEventService channelEventService) : base(logger)
    {
        _channelEventService = channelEventService;
    }

    /// <summary>
    /// Bir kabinin olay gecmisi — yeniden eskiye, sayfali.
    ///
    /// GET degil POST: filtre govdesi (kabin, kanal, tarih araligi, sayfalama)
    /// query string'e sikistirilmaktansa tiplenmis bir govdede tasinir — kod
    /// tabaninin diger listeleme uclariyla ayni desen.
    /// </summary>
    [HttpPost("list")]
    public async Task<IActionResult> List(ChannelEventQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await _channelEventService.GetPagedAsync(request, cancellationToken);
        return ToAction(result);
    }
}
