using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebsite_API.Models;
using MyWebsite_API.Services;

namespace MyWebsite_API.Controllers;

[ApiController]
[Route("api/deployment-modules")]
public sealed class DeploymentModulesController(IDeploymentModuleService deploymentModuleService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DeploymentModuleDetails>>> GetAll(
        [FromQuery] int? deploymentId,
        [FromQuery] int? productId)
    {
        return Ok(await deploymentModuleService.GetAllAsync(deploymentId, productId));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DeploymentModuleDetails>> GetById(int id)
    {
        var deploymentModule = await deploymentModuleService.GetByIdAsync(id);
        return deploymentModule is null ? NotFound() : Ok(deploymentModule);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<DeploymentModuleDetails>> Create(DeploymentModuleRequest request)
    {
        var deploymentModuleId = await deploymentModuleService.CreateAsync(request);

        if (deploymentModuleId is null)
        {
            return BadRequest("The selected module does not belong to the selected deployment's product.");
        }

        var deploymentModule = await deploymentModuleService.GetByIdAsync(deploymentModuleId.Value);
        return CreatedAtAction(nameof(GetById), new { id = deploymentModuleId.Value }, deploymentModule);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, DeploymentModuleRequest request)
    {
        return await deploymentModuleService.UpdateAsync(id, request) ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await deploymentModuleService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
