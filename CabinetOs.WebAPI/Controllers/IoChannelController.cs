using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Business.Abstract;
using CabinetOs.WebAPI.Controllers.Base;
using CabinetOs.Model.Dtos.IoChannel.Commands;
using CabinetOs.Model.Dtos.IoChannel.Queries;

namespace CabinetOs.WebAPI.Controllers
{
    public class IoChannelController : BaseController
    {
        private readonly IIoChannelService _ioChannelService;
        public IoChannelController(ILogger<IoChannelController> logger, IIoChannelService ioChannelService) : base(logger)
        {
            _ioChannelService = ioChannelService;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _ioChannelService.GetAsync(id: id);
            return ToAction(result);
        }

        [HttpGet("{id:guid}/base")]
        public async Task<IActionResult> GetBase(Guid id)
        {
            var result = await _ioChannelService.GetBaseAsync(id: id);
            return ToAction(result);
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetList(DynamicRequest? request = default)
        {
            var result = await _ioChannelService.GetListAsync(request);
            return ToAction(result);
        }

        [HttpPost("list/base")]
        public async Task<IActionResult> GetBaseList(DynamicRequest? request = default)
        {
            var result = await _ioChannelService.GetBaseListAsync(request);
            return ToAction(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(IoChannelCreateDto request)
        {
            var result = await _ioChannelService.CreateAsync(request);
            return ToAction(result);
        }

        [HttpGet("{id:guid}/update")]
        public async Task<IActionResult> Update(Guid id)
        {
            var result = await _ioChannelService.GetUpdateModelAsync(id: id);
            return ToAction(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(IoChannelUpdateDto request)
        {
            var result = await _ioChannelService.UpdateAsync(request);
            return ToAction(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _ioChannelService.DeleteAsync(id: id);
            return ToAction(result);
        }

        [HttpGet("{id:guid}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var result = await _ioChannelService.RestoreAsync(id: id);
            return ToAction(result);
        }

        [HttpPost("pagination")]
        public async Task<IActionResult> Pagination(DynamicPaginationRequest request)
        {
            var result = await _ioChannelService.PaginationAsync(request);
            return ToAction(result);
        }

        [HttpPost("datatable/client")]
        public async Task<IActionResult> DatatableClientSide(DynamicDatatableRequest request)
        {
            var result = await _ioChannelService.DatatableClientSideAsync(request);
            return ToAction(result);
        }

        [HttpPost("datatable/server")]
        public async Task<IActionResult> DatatableServerSide(DynamicDatatableRequest request)
        {
            var result = await _ioChannelService.DatatableServerSideAsync(request);
            return ToAction(result);
        }
    }
}