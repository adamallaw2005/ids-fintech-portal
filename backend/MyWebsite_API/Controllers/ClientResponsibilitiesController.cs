using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebsite_API.Models;
using MyWebsite_API.Services;

namespace MyWebsite_API.Controllers;

[ApiController]
[Route("api/client-responsibilities")]
public sealed class ClientResponsibilitiesController(IClientResponsibilityService responsibilityService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClientResponsibilityDetails>>> GetAll(
        [FromQuery] int? clientId,
        [FromQuery] int? teamMemberId)
    {
        return Ok(await responsibilityService.GetAllAsync(clientId, teamMemberId));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClientResponsibilityDetails>> GetById(int id)
    {
        var responsibility = await responsibilityService.GetByIdAsync(id);
        return responsibility is null ? NotFound() : Ok(responsibility);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ClientResponsibilityDetails>> Create(ClientResponsibilityRequest request)
    {
        var responsibilityId = await responsibilityService.CreateAsync(request);
        var responsibility = await responsibilityService.GetByIdAsync(responsibilityId);
        return CreatedAtAction(nameof(GetById), new { id = responsibilityId }, responsibility);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ClientResponsibilityRequest request)
    {
        return await responsibilityService.UpdateAsync(id, request) ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await responsibilityService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
