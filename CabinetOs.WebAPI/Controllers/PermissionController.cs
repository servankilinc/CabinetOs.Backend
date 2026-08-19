using Microsoft.AspNetCore.Mvc;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Business.Abstract;
using CabinetOs.WebAPI.Controllers.Base;

namespace CabinetOs.WebAPI.Controllers;

public class PermissionController : BaseController
{
    private readonly IPermissionService _permissionService;
    public PermissionController(ILogger<PermissionController> logger, IPermissionService permissionService) : base(logger)
    {
        _permissionService = permissionService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _permissionService.GetBaseAsync(id: id);
        return ToAction(result);
    }

    [HttpGet("{id:int}/base")]
    public async Task<IActionResult> GetBase(int id)
    {
        var result = await _permissionService.GetBaseAsync(id: id);
        return ToAction(result);
    }

    [HttpPost("list")]
    public async Task<IActionResult> GetList(DynamicRequest? request = default)
    {
        var result = await _permissionService.GetBaseListAsync(request);
        return ToAction(result);
    }

    [HttpPost("list/base")]
    public async Task<IActionResult> GetBaseList(DynamicRequest? request = default)
    {
        var result = await _permissionService.GetBaseListAsync(request);
        return ToAction(result);
    }


    [HttpGet("selectlist")]
    public async Task<IActionResult> SelectList()
    {
        var result = await _permissionService.SelectListAsync();
        return ToAction(result);
    }

    [HttpPost("pagination")]
    public async Task<IActionResult> Pagination(DynamicPaginationRequest request)
    {
        var result = await _permissionService.PaginationAsync(request);
        return ToAction(result);
    }

    [HttpPost("datatable/client")]
    public async Task<IActionResult> DatatableClientSide(DynamicDatatableRequest request)
    {
        var result = await _permissionService.DatatableClientSideAsync(request);
        return ToAction(result);
    }

    [HttpPost("datatable/server")]
    public async Task<IActionResult> DatatableServerSide(DynamicDatatableRequest request)
    {
        var result = await _permissionService.DatatableServerSideAsync(request);
        return ToAction(result);
    }
}
