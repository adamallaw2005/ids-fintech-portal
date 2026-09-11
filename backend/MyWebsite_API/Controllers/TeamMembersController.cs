using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebsite_API.Models;
using MyWebsite_API.Services;

namespace MyWebsite_API.Controllers;

[ApiController]
[Route("api/team-members")]
public sealed class TeamMembersController(ITeamMemberService teamMemberService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TeamMemberSummary>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? status)
    {
        return Ok(await teamMemberService.GetAllAsync(search, status));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TeamMember>> GetById(int id)
    {
        var teamMember = await teamMemberService.GetByIdAsync(id);
        return teamMember is null ? NotFound() : Ok(teamMember);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<TeamMember>> Create(TeamMemberRequest request)
    {
        if (request.ProductAssignments?.Any(x => x is null) == true || request.ClientAssignments?.Any(x => x is null) == true)
            return BadRequest("Assignments cannot contain null entries.");
        var teamMemberId = await teamMemberService.CreateAsync(request);
        var teamMember = await teamMemberService.GetByIdAsync(teamMemberId);
        return CreatedAtAction(nameof(GetById), new { id = teamMemberId }, teamMember);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TeamMemberRequest request)
    {
        if (request.ProductAssignments?.Any(x => x is null) == true || request.ClientAssignments?.Any(x => x is null) == true)
            return BadRequest("Assignments cannot contain null entries.");
        return await teamMemberService.UpdateAsync(id, request) ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await teamMemberService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
