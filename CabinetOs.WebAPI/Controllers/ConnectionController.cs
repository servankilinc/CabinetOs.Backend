using CabinetOs.Business.Abstract;
using CabinetOs.Core.BaseRequestModels;
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
}
