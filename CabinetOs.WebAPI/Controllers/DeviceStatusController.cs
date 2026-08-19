using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace CabinetOs.WebAPI.Controllers;

public class DeviceStatusController : BaseController
{
    private readonly IDeviceStatusService _deviceStatusService;
    public DeviceStatusController(ILogger<DeviceStatusController> logger, IDeviceStatusService deviceStatusService) : base(logger)
    {
        _deviceStatusService = deviceStatusService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _deviceStatusService.GetBaseAsync(id: id);
        return ToAction(result);
    }

    [HttpGet("{id:int}/base")]
    public async Task<IActionResult> GetBase(int id)
    {
        var result = await _deviceStatusService.GetBaseAsync(id: id);
        return ToAction(result);
    }

    [HttpPost("list")]
    public async Task<IActionResult> GetList(DynamicRequest? request = default)
    {
        var result = await _deviceStatusService.GetBaseListAsync(request);
        return ToAction(result);
    }

    [HttpPost("list/base")]
    public async Task<IActionResult> GetBaseList(DynamicRequest? request = default)
    {
        var result = await _deviceStatusService.GetBaseListAsync(request);
        return ToAction(result);
    }


    [HttpGet("selectlist")]
    public async Task<IActionResult> SelectList()
    {
        var result = await _deviceStatusService.SelectListAsync();
        return ToAction(result);
    }

    [HttpPost("pagination")]
    public async Task<IActionResult> Pagination(DynamicPaginationRequest request)
    {
        var result = await _deviceStatusService.PaginationAsync(request);
        return ToAction(result);
    }

    [HttpPost("datatable/client")]
    public async Task<IActionResult> DatatableClientSide(DynamicDatatableRequest request)
    {
        var result = await _deviceStatusService.DatatableClientSideAsync(request);
        return ToAction(result);
    }

    [HttpPost("datatable/server")]
    public async Task<IActionResult> DatatableServerSide(DynamicDatatableRequest request)
    {
        var result = await _deviceStatusService.DatatableServerSideAsync(request);
        return ToAction(result);
    }
}
