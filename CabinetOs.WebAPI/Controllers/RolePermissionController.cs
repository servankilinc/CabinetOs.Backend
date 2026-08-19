using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Business.Abstract;
using CabinetOs.WebAPI.Controllers.Base;
using CabinetOs.Model.Entities;

namespace CabinetOs.WebAPI.Controllers
{
    public class RolePermissionController : BaseController
    {
        private readonly IRolePermissionService _rolePermissionService;
        public RolePermissionController(ILogger<RolePermissionController> logger, IRolePermissionService rolePermissionService) : base(logger)
        {
            _rolePermissionService = rolePermissionService;
        }

        [HttpGet("{permissionId:guid}/{roleId:guid}")]
        public async Task<IActionResult> Get(Guid permissionId, Guid roleId)
        {
            var result = await _rolePermissionService.GetAsync(permissionId: permissionId, roleId: roleId);
            return ToAction(result);
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetList(DynamicRequest? request = default)
        {
            var result = await _rolePermissionService.GetListAsync(request);
            return ToAction(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RolePermission request)
        {
            var result = await _rolePermissionService.CreateAsync(request);
            return ToAction(result);
        }

        [HttpGet("{permissionId:guid}/{roleId:guid}/update")]
        public async Task<IActionResult> Update(Guid permissionId, Guid roleId)
        {
            var result = await _rolePermissionService.GetAsync(permissionId: permissionId, roleId: roleId);
            return ToAction(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(RolePermission request)
        {
            var result = await _rolePermissionService.UpdateAsync(request);
            return ToAction(result);
        }

        [HttpDelete("{permissionId:guid}/{roleId:guid}")]
        public async Task<IActionResult> Delete(Guid permissionId, Guid roleId)
        {
            var result = await _rolePermissionService.DeleteAsync(permissionId: permissionId, roleId: roleId);
            return ToAction(result);
        }

        [HttpPost("pagination")]
        public async Task<IActionResult> Pagination(DynamicPaginationRequest request)
        {
            var result = await _rolePermissionService.PaginationAsync(request);
            return ToAction(result);
        }

        [HttpPost("datatable/client")]
        public async Task<IActionResult> DatatableClientSide(DynamicDatatableRequest request)
        {
            var result = await _rolePermissionService.DatatableClientSideAsync(request);
            return ToAction(result);
        }

        [HttpPost("datatable/server")]
        public async Task<IActionResult> DatatableServerSide(DynamicDatatableRequest request)
        {
            var result = await _rolePermissionService.DatatableServerSideAsync(request);
            return ToAction(result);
        }
    }
}