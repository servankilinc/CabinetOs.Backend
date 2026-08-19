using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CabinetOs.Core.BaseRequestModels;
using CabinetOs.Business.Abstract;
using CabinetOs.WebAPI.Controllers.Base;

namespace CabinetOs.WebAPI.Controllers
{
    public class UserRoleController : BaseController
    {
        private readonly IUserRoleService _userRoleService;
        public UserRoleController(ILogger<UserRoleController> logger, IUserRoleService userRoleService) : base(logger)
        {
            _userRoleService = userRoleService;
        }

        [HttpGet("{roleId:guid}/{userId:guid}")]
        public async Task<IActionResult> Get(Guid roleId, Guid userId)
        {
            var result = await _userRoleService.GetAsync(roleId: roleId, userId: userId);
            return ToAction(result);
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetList(DynamicRequest? request = default)
        {
            var result = await _userRoleService.GetListAsync(request);
            return ToAction(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserRole request)
        {
            var result = await _userRoleService.CreateAsync(request);
            return ToAction(result);
        }

        [HttpGet("{roleId:guid}/{userId:guid}/update")]
        public async Task<IActionResult> Update(Guid roleId, Guid userId)
        {
            var result = await _userRoleService.GetAsync(roleId: roleId, userId: userId);
            return ToAction(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(UserRole request)
        {
            var result = await _userRoleService.UpdateAsync(request);
            return ToAction(result);
        }

        [HttpDelete("{roleId:guid}/{userId:guid}")]
        public async Task<IActionResult> Delete(Guid roleId, Guid userId)
        {
            var result = await _userRoleService.DeleteAsync(roleId: roleId, userId: userId);
            return ToAction(result);
        }

        [HttpPost("pagination")]
        public async Task<IActionResult> Pagination(DynamicPaginationRequest request)
        {
            var result = await _userRoleService.PaginationAsync(request);
            return ToAction(result);
        }

        [HttpPost("datatable/client")]
        public async Task<IActionResult> DatatableClientSide(DynamicDatatableRequest request)
        {
            var result = await _userRoleService.DatatableClientSideAsync(request);
            return ToAction(result);
        }

        [HttpPost("datatable/server")]
        public async Task<IActionResult> DatatableServerSide(DynamicDatatableRequest request)
        {
            var result = await _userRoleService.DatatableServerSideAsync(request);
            return ToAction(result);
        }
    }
}