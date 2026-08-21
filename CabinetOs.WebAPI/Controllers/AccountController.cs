using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CabinetOs.Model.Auth.Login;
using CabinetOs.Model.Auth.Logout;
using CabinetOs.Model.Auth.Refresh;
using CabinetOs.Model.Auth.SignUp;
using CabinetOs.Business.Abstract;
using CabinetOs.WebAPI.Controllers.Base;

namespace CabinetOs.WebAPI.Controllers;

[ApiController]
public class AccountController : BaseController
{
    private readonly IAuthService _authService;
    public AccountController(IAuthService authService, ILogger<AccountController> logger) : base(logger) => _authService = authService;


    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        return ToAction(result);
    }

    [AllowAnonymous]
    [HttpPost("SignUp")]
    public async Task<IActionResult> SignUp(SignUpRequest request)
    {
        var result = await _authService.SignUpAsync(request);

        return ToAction(result);
    }

    [AllowAnonymous]
    [HttpPost("RefreshAuth")]
    public async Task<IActionResult> RefreshAuth(RefreshAuthRequest request)
    {
        var result = await _authService.RefreshAsync(request);

        return ToAction(result);
    }
    
    [Authorize]
    [HttpPost("Logout")]
    public async Task<IActionResult> Logout(LogoutRequest request)
    {
        // Never trust a caller-supplied user id; bind it to the authenticated principal.
        if (!TryGetUserId(out var userId))
            return Forbid();
        request.UserId = userId;

        var result = await _authService.LogoutAsync(request);

        return ToAction(result);
    }

    [Authorize]
    [HttpPost("RevokeAll")]
    public async Task<IActionResult> RevokeAll()
    {
        if (!TryGetUserId(out var userId))
            return Forbid();

        var result = await _authService.RevokeAllAsync(userId);

        return ToAction(result);
    }

    [Authorize]
    [HttpGet("Me")]
    public async Task<IActionResult> Me()
    {
        if (!TryGetUserId(out var userId))
            return Forbid();

        var result = await _authService.GetCurrentUserAsync(userId);

        return ToAction(result);
    }

    private bool TryGetUserId(out Guid userId)
        => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}