using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebsite_API.Models;
using MyWebsite_API.Services;

namespace MyWebsite_API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var response = await authService.LoginAsync(request);
        return response is null ? Unauthorized("Invalid email, password, or inactive account.") : Ok(response);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("admin-password/setup")]
    public async Task<IActionResult> SetupAdminPassword(SetupAdminPasswordRequest request)
    {
        var configured = await authService.SetupAdminPromotionPasswordAsync(request.Password);
        return configured
            ? NoContent()
            : Conflict("The administrator promotion password has already been configured.");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("admin-password/change")]
    public async Task<IActionResult> ChangeAdminPassword(AdminPasswordChangeRequest request)
    {
        var changed = await authService.ChangeAdminPromotionPasswordAsync(
            request.CurrentPassword,
            request.NewPassword);

        return changed ? NoContent() : BadRequest("The current administrator promotion password is incorrect.");
    }

    [Authorize]
    [HttpPost("become-admin")]
    public async Task<ActionResult<AuthResponse>> BecomeAdmin(AdminPromotionRequest request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized();
        }

        var response = await authService.PromoteToAdminAsync(userId, request.AdminPromotionPassword);
        return response is null
            ? BadRequest("The administrator promotion password is incorrect or has not been configured.")
            : Ok(response);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized();
        }

        var changed = await authService.ChangePasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword);

        return changed ? NoContent() : BadRequest("The current password is incorrect.");
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return NoContent();
    }
}
