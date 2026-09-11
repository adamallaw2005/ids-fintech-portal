using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebsite_API.Models;
using MyWebsite_API.Services;

namespace MyWebsite_API.Controllers;

[ApiController]
[Route("api/environments")]
public sealed class EnvironmentsController(IEnvironmentService environmentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EnvironmentDetails>>> GetAll(
        [FromQuery] int? deploymentId,
        [FromQuery] string? type)
    {
        return Ok(await environmentService.GetAllAsync(deploymentId, type));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EnvironmentDetails>> GetById(int id)
    {
        var environment = await environmentService.GetByIdAsync(id);
        return environment is null ? NotFound() : Ok(environment);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<EnvironmentDetails>> Create(EnvironmentRequest request)
    {
        var environmentId = await environmentService.CreateAsync(request);
        var environment = await environmentService.GetByIdAsync(environmentId);
        return CreatedAtAction(nameof(GetById), new { id = environmentId }, environment);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, EnvironmentRequest request)
    {
        return await environmentService.UpdateAsync(id, request) ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await environmentService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
