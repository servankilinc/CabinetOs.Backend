using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Business.Abstract;
using CabinetOs.WebAPI.Controllers.Base;
using CabinetOs.Model.Dtos.ComponentTemplatePin.Commands;
using CabinetOs.Model.Dtos.ComponentTemplatePin.Queries;

namespace CabinetOs.WebAPI.Controllers
{
    public class ComponentTemplatePinController : BaseController
    {
        private readonly IComponentTemplatePinService _componentTemplatePinService;
        public ComponentTemplatePinController(ILogger<ComponentTemplatePinController> logger, IComponentTemplatePinService componentTemplatePinService) : base(logger)
        {
            _componentTemplatePinService = componentTemplatePinService;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _componentTemplatePinService.GetAsync(id: id);
            return ToAction(result);
        }

        [HttpGet("{id:guid}/base")]
        public async Task<IActionResult> GetBase(Guid id)
        {
            var result = await _componentTemplatePinService.GetBaseAsync(id: id);
            return ToAction(result);
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetList(DynamicRequest? request = default)
        {
            var result = await _componentTemplatePinService.GetListAsync(request);
            return ToAction(result);
        }

        [HttpPost("list/base")]
        public async Task<IActionResult> GetBaseList(DynamicRequest? request = default)
        {
            var result = await _componentTemplatePinService.GetBaseListAsync(request);
            return ToAction(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ComponentTemplatePinCreateDto request)
        {
            var result = await _componentTemplatePinService.CreateAsync(request);
            return ToAction(result);
        }

        [HttpGet("{id:guid}/update")]
        public async Task<IActionResult> Update(Guid id)
        {
            var result = await _componentTemplatePinService.GetUpdateModelAsync(id: id);
            return ToAction(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(ComponentTemplatePinUpdateDto request)
        {
            var result = await _componentTemplatePinService.UpdateAsync(request);
            return ToAction(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _componentTemplatePinService.DeleteAsync(id: id);
            return ToAction(result);
        }

        [HttpPost("pagination")]
        public async Task<IActionResult> Pagination(DynamicPaginationRequest request)
        {
            var result = await _componentTemplatePinService.PaginationAsync(request);
            return ToAction(result);
        }

        [HttpPost("datatable/client")]
        public async Task<IActionResult> DatatableClientSide(DynamicDatatableRequest request)
        {
            var result = await _componentTemplatePinService.DatatableClientSideAsync(request);
            return ToAction(result);
        }

        [HttpPost("datatable/server")]
        public async Task<IActionResult> DatatableServerSide(DynamicDatatableRequest request)
        {
            var result = await _componentTemplatePinService.DatatableServerSideAsync(request);
            return ToAction(result);
        }
    }
}