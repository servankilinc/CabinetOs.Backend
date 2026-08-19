using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Business.Abstract;
using CabinetOs.WebAPI.Controllers.Base;

namespace CabinetOs.WebAPI.Controllers
{
    public class AuditLogController : BaseController
    {
        private readonly IAuditLogService _auditLogService;
        public AuditLogController(ILogger<AuditLogController> logger, IAuditLogService auditLogService) : base(logger)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _auditLogService.GetAsync(id: id);
            return ToAction(result);
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetList(DynamicRequest? request = default)
        {
            var result = await _auditLogService.GetListAsync(request);
            return ToAction(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AuditLog request)
        {
            var result = await _auditLogService.CreateAsync(request);
            return ToAction(result);
        }

        [HttpPost("pagination")]
        public async Task<IActionResult> Pagination(DynamicPaginationRequest request)
        {
            var result = await _auditLogService.PaginationAsync(request);
            return ToAction(result);
        }

        [HttpPost("datatable/client")]
        public async Task<IActionResult> DatatableClientSide(DynamicDatatableRequest request)
        {
            var result = await _auditLogService.DatatableClientSideAsync(request);
            return ToAction(result);
        }

        [HttpPost("datatable/server")]
        public async Task<IActionResult> DatatableServerSide(DynamicDatatableRequest request)
        {
            var result = await _auditLogService.DatatableServerSideAsync(request);
            return ToAction(result);
        }
    }
}