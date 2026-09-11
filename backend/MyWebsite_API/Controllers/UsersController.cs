using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebsite_API.Models;
using MyWebsite_API.Services;

namespace MyWebsite_API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public sealed class UsersController(IUserManagementService userManagementService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? roleName,
        [FromQuery] bool? isActive)
    {
        return Ok(await userManagementService.GetAllAsync(search, roleName, isActive));
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
    {
        var user = await userManagementService.CreateAsync(request);
        return user is null
            ? BadRequest("RoleName must be either Admin or User, and the email must be unique.")
            : Created($"api/users/{user.UserId}", user);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateUserRequest request)
    {
        var updated = await userManagementService.UpdateAsync(id, request);
        return updated
            ? NoContent()
            : BadRequest("The user does not exist or the email is already in use.");
    }

    [HttpPut("{id:int}/role")]
    public async Task<IActionResult> UpdateRole(int id, UpdateUserRoleRequest request)
    {
        var updated = await userManagementService.UpdateRoleAsync(id, request.RoleName);
        return updated ? NoContent() : BadRequest("RoleName must be either Admin or User, and the user must exist.");
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateUserStatusRequest request)
    {
        return await userManagementService.UpdateStatusAsync(id, request.IsActive)
            ? NoContent()
            : NotFound();
    }
}
