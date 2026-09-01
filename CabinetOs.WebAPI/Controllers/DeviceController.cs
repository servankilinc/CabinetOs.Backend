using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Model.Dtos.DeviceCommand.Commands;
using CabinetOs.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace CabinetOs.WebAPI.Controllers;

public class DeviceController : BaseController
{
    private readonly IDeviceService _deviceService;
    private readonly IDeviceCommandService _deviceCommandService;

    public DeviceController(ILogger<DeviceController> logger, IDeviceService deviceService, IDeviceCommandService deviceCommandService) : base(logger)
    {
        _deviceService = deviceService;
        _deviceCommandService = deviceCommandService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _deviceService.GetDetailAsync(id: id);
        return ToAction(result);
    }

    [HttpGet("{id:guid}/base")]
    public async Task<IActionResult> GetBase(Guid id)
    {
        var result = await _deviceService.GetBaseAsync(id: id);
        return ToAction(result);
    }

    [HttpGet("{id:guid}/detail")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var result = await _deviceService.GetDetailAsync(id: id);
        return ToAction(result);
    }

    [HttpPost("list")]
    public async Task<IActionResult> GetList(DynamicRequest? request = default)
    {
        var result = await _deviceService.GetDetailListAsync(request);
        return ToAction(result);
    }

    [HttpPost("list/base")]
    public async Task<IActionResult> GetBaseList(DynamicRequest? request = default)
    {
        var result = await _deviceService.GetBaseListAsync(request);
        return ToAction(result);
    }

    [HttpPost("list/detail")]
    public async Task<IActionResult> GetDetailList(DynamicRequest? request = default)
    {
        var result = await _deviceService.GetDetailListAsync(request);
        return ToAction(result);
    }

    #region Komut İşlemleri
    /// <summary> <b>Uzun surebilir.</b> Yanit SCADA cevaplayana ya da zaman asimina kadar donmez (<c>Cabinet.ScadaCommandTimeoutMs</c>, en az 10 sn). </summary>
    [HttpPost("{deviceId:guid}/command")]
    public async Task<IActionResult> SendCommand(Guid deviceId, DeviceCommandSendRequest request, CancellationToken cancellationToken)
    {
        var result = await _deviceCommandService.SendAsync(deviceId, request, cancellationToken);
        return ToAction(result);
    }

    /// <summary>Cihazin son komutlarını, yeniden eskiye verir.</summary>
    [HttpGet("{deviceId:guid}/commands")]
    public async Task<IActionResult> GetCommands(Guid deviceId, [FromQuery] int take = 20, CancellationToken cancellationToken = default)
    {
        var result = await _deviceCommandService.GetRecentAsync(deviceId, take, cancellationToken);
        return ToAction(result);
    }
    #endregion
}
