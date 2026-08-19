using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Business.Abstract;
using CabinetOs.WebAPI.Controllers.Base;
using CabinetOs.Model.Dtos.DiagramAnnotation.Commands;
using CabinetOs.Model.Dtos.DiagramAnnotation.Queries;

namespace CabinetOs.WebAPI.Controllers
{
    public class DiagramAnnotationController : BaseController
    {
        private readonly IDiagramAnnotationService _diagramAnnotationService;
        public DiagramAnnotationController(ILogger<DiagramAnnotationController> logger, IDiagramAnnotationService diagramAnnotationService) : base(logger)
        {
            _diagramAnnotationService = diagramAnnotationService;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _diagramAnnotationService.GetAsync(id: id);
            return ToAction(result);
        }

        [HttpGet("{id:guid}/base")]
        public async Task<IActionResult> GetBase(Guid id)
        {
            var result = await _diagramAnnotationService.GetBaseAsync(id: id);
            return ToAction(result);
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetList(DynamicRequest? request = default)
        {
            var result = await _diagramAnnotationService.GetListAsync(request);
            return ToAction(result);
        }

        [HttpPost("list/base")]
        public async Task<IActionResult> GetBaseList(DynamicRequest? request = default)
        {
            var result = await _diagramAnnotationService.GetBaseListAsync(request);
            return ToAction(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DiagramAnnotationCreateDto request)
        {
            var result = await _diagramAnnotationService.CreateAsync(request);
            return ToAction(result);
        }

        [HttpGet("{id:guid}/update")]
        public async Task<IActionResult> Update(Guid id)
        {
            var result = await _diagramAnnotationService.GetUpdateModelAsync(id: id);
            return ToAction(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(DiagramAnnotationUpdateDto request)
        {
            var result = await _diagramAnnotationService.UpdateAsync(request);
            return ToAction(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _diagramAnnotationService.DeleteAsync(id: id);
            return ToAction(result);
        }

        [HttpPost("pagination")]
        public async Task<IActionResult> Pagination(DynamicPaginationRequest request)
        {
            var result = await _diagramAnnotationService.PaginationAsync(request);
            return ToAction(result);
        }

        [HttpPost("datatable/client")]
        public async Task<IActionResult> DatatableClientSide(DynamicDatatableRequest request)
        {
            var result = await _diagramAnnotationService.DatatableClientSideAsync(request);
            return ToAction(result);
        }

        [HttpPost("datatable/server")]
        public async Task<IActionResult> DatatableServerSide(DynamicDatatableRequest request)
        {
            var result = await _diagramAnnotationService.DatatableServerSideAsync(request);
            return ToAction(result);
        }
    }
}