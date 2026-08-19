using Microsoft.AspNetCore.Mvc;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Business.Abstract;
using CabinetOs.WebAPI.Controllers.Base;
using CabinetOs.Model.Dtos.Company.Commands;

namespace CabinetOs.WebAPI.Controllers;

public class CompanyController : BaseController
{
    private readonly ICompanyService _companyService;
    public CompanyController(ILogger<CompanyController> logger, ICompanyService companyService) : base(logger)
    {
        _companyService = companyService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _companyService.GetBaseAsync(id: id);
        return ToAction(result);
    }

    [HttpGet("{id:guid}/base")]
    public async Task<IActionResult> GetBase(Guid id)
    {
        var result = await _companyService.GetBaseAsync(id: id);
        return ToAction(result);
    }

    [HttpPost("list")]
    public async Task<IActionResult> GetList(DynamicRequest? request = default)
    {
        var result = await _companyService.GetBaseListAsync(request);
        return ToAction(result);
    }

    [HttpPost("list/base")]
    public async Task<IActionResult> GetBaseList(DynamicRequest? request = default)
    {
        var result = await _companyService.GetBaseListAsync(request);
        return ToAction(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CompanyCreateDto request)
    {
        var result = await _companyService.CreateAsync(request);
        return ToAction(result);
    }

    [HttpGet("{id:guid}/update")]
    public async Task<IActionResult> Update(Guid id)
    {
        var result = await _companyService.GetUpdateModelAsync(id: id);
        return ToAction(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update(CompanyUpdateDto request)
    {
        var result = await _companyService.UpdateAsync(request);
        return ToAction(result);
    }

    [HttpGet("selectlist")]
    public async Task<IActionResult> SelectList()
    {
        var result = await _companyService.SelectListAsync();
        return ToAction(result);
    }

    [HttpPost("pagination")]
    public async Task<IActionResult> Pagination(DynamicPaginationRequest request)
    {
        var result = await _companyService.PaginationAsync(request);
        return ToAction(result);
    }

    [HttpPost("datatable/client")]
    public async Task<IActionResult> DatatableClientSide(DynamicDatatableRequest request)
    {
        var result = await _companyService.DatatableClientSideAsync(request);
        return ToAction(result);
    }

    [HttpPost("datatable/server")]
    public async Task<IActionResult> DatatableServerSide(DynamicDatatableRequest request)
    {
        var result = await _companyService.DatatableServerSideAsync(request);
        return ToAction(result);
    }
}
