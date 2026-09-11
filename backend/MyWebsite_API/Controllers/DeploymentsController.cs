using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebsite_API.Models;
using MyWebsite_API.Services;

namespace MyWebsite_API.Controllers;

[ApiController]
[Route("api/deployments")]
public sealed class DeploymentsController(IDeploymentService deploymentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DeploymentSummary>>> GetAll(
        [FromQuery] int? clientId,
        [FromQuery] int? productId,
        [FromQuery] string? version,
        [FromQuery] string? status,
        [FromQuery] string? environment)
    {
        return Ok(await deploymentService.GetAllAsync(clientId, productId, version, status, environment));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DeploymentDetails>> GetById(int id)
    {
        var deployment = await deploymentService.GetByIdAsync(id);
        return deployment is null ? NotFound() : Ok(deployment);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<DeploymentDetails>> Create(DeploymentRequest request)
    {
        var deploymentId = await deploymentService.CreateAsync(request);
        var deployment = await deploymentService.GetByIdAsync(deploymentId);
        return CreatedAtAction(nameof(GetById), new { id = deploymentId }, deployment);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, DeploymentRequest request)
    {
        return await deploymentService.UpdateAsync(id, request) ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await deploymentService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
