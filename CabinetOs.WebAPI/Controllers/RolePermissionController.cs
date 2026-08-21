using Microsoft.AspNetCore.Mvc;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Business.Abstract;
using CabinetOs.WebAPI.Controllers.Base;
using CabinetOs.Model.Dtos.RolePermission.Commands;

namespace CabinetOs.WebAPI.Controllers
{
    public class RolePermissionController : BaseController
    {
        private readonly IRolePermissionService _rolePermissionService;
        public RolePermissionController(ILogger<RolePermissionController> logger, IRolePermissionService rolePermissionService) : base(logger)
        {
            _rolePermissionService = rolePermissionService;
        }

        [HttpGet("{permissionId:int}/{roleId:guid}")]
        public async Task<IActionResult> Get(int permissionId, Guid roleId)
        {
            var result = await _rolePermissionService.GetAsync(permissionId: permissionId, roleId: roleId);
            return ToAction(result);
        }

        [HttpGet("{permissionId:int}/{roleId:guid}/base")]
        public async Task<IActionResult> GetBase(int permissionId, Guid roleId)
        {
            var result = await _rolePermissionService.GetBaseAsync(permissionId: permissionId, roleId: roleId);
            return ToAction(result);
        }

        [HttpGet("role/{roleId:guid}")]
        public async Task<IActionResult> GetByRole(Guid roleId)
        {
            var result = await _rolePermissionService.GetByRoleAsync(roleId);
            return ToAction(result);
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetList(DynamicRequest? request = default)
        {
            var result = await _rolePermissionService.GetListAsync(request);
            return ToAction(result);
        }

        [HttpPost("list/base")]
        public async Task<IActionResult> GetBaseList(DynamicRequest? request = default)
        {
            var result = await _rolePermissionService.GetBaseListAsync(request);
            return ToAction(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RolePermissionCreateDto request)
        {
            var result = await _rolePermissionService.CreateAsync(request);
            return ToAction(result);
        }

        /// <summary>Rolun izin kumesini gonderilen liste ile birebir degistirir. Izin matrisi ekrani bunu kullanir.</summary>
        [HttpPut("role/{roleId:guid}/sync")]
        public async Task<IActionResult> Sync(Guid roleId, [FromBody] ICollection<int> permissionIds)
        {
            var result = await _rolePermissionService.SyncRolePermissionsAsync(roleId, permissionIds);
            return ToAction(result);
        }

        [HttpDelete("{permissionId:int}/{roleId:guid}")]
        public async Task<IActionResult> Delete(int permissionId, Guid roleId)
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