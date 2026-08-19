using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Model.Dtos.Connection.Commands;
using CabinetOs.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace CabinetOs.WebAPI.Controllers;

public class ConnectionController : BaseController
{
    private readonly IConnectionService _connectionService;
    public ConnectionController(ILogger<ConnectionController> logger, IConnectionService connectionService) : base(logger)
    {
        _connectionService = connectionService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _connectionService.GetBaseAsync(id: id);
        return ToAction(result);
    }

    [HttpGet("{id:guid}/base")]
    public async Task<IActionResult> GetBase(Guid id)
    {
        var result = await _connectionService.GetBaseAsync(id: id);
        return ToAction(result);
    }

    [HttpPost("list")]
    public async Task<IActionResult> GetList(DynamicRequest? request = default)
    {
        var result = await _connectionService.GetBaseListAsync(request);
        return ToAction(result);
    }

    [HttpPost("list/base")]
    public async Task<IActionResult> GetBaseList(DynamicRequest? request = default)
    {
        var result = await _connectionService.GetBaseListAsync(request);
        return ToAction(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ConnectionCreateDto request)
    {
        var result = await _connectionService.CreateAsync(request);
        return ToAction(result);
    }

    [HttpGet("{id:guid}/update")]
    public async Task<IActionResult> Update(Guid id)
    {
        var result = await _connectionService.GetUpdateModelAsync(id: id);
        return ToAction(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update(ConnectionUpdateDto request)
    {
        var result = await _connectionService.UpdateAsync(request);
        return ToAction(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _connectionService.DeleteAsync(id: id);
        return ToAction(result);
    }

    [HttpGet("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id)
    {
        var result = await _connectionService.RestoreAsync(id: id);
        return ToAction(result);
    }

    [HttpGet("selectlist")]
    public async Task<IActionResult> SelectList()
    {
        var result = await _connectionService.SelectListAsync();
        return ToAction(result);
    }

    [HttpPost("pagination")]
    public async Task<IActionResult> Pagination(DynamicPaginationRequest request)
    {
        var result = await _connectionService.PaginationAsync(request);
        return ToAction(result);
    }

    [HttpPost("datatable/client")]
    public async Task<IActionResult> DatatableClientSide(DynamicDatatableRequest request)
    {
        var result = await _connectionService.DatatableClientSideAsync(request);
        return ToAction(result);
    }

    [HttpPost("datatable/server")]
    public async Task<IActionResult> DatatableServerSide(DynamicDatatableRequest request)
    {
        var result = await _connectionService.DatatableServerSideAsync(request);
        return ToAction(result);
    }
}
