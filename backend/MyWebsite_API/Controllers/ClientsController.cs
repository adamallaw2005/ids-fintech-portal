using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebsite_API.Models;
using MyWebsite_API.Services;

namespace MyWebsite_API.Controllers;

[ApiController]
[Route("api/clients")]
public sealed class ClientsController(IClientService clientService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Client>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? country,
        [FromQuery] string? status,
        [FromQuery] int? productId)
    {
        return Ok(await clientService.GetAllAsync(search, country, status, productId));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Client>> GetById(int id)
    {
        var client = await clientService.GetByIdAsync(id);
        return client is null ? NotFound() : Ok(client);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Client>> Create(ClientCreateRequest request)
    {
        if (request.Responsibilities.Any(assignment => assignment is null))
        {
            return BadRequest("Assignments cannot contain empty entries.");
        }

        var clientId = await clientService.CreateAsync(request);
        var client = await clientService.GetByIdAsync(clientId);
        return CreatedAtAction(nameof(GetById), new { id = clientId }, client);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ClientUpdateRequest request)
    {
        if (request.Responsibilities?.Any(assignment => assignment is null) == true)
        {
            return BadRequest("Assignments cannot contain empty entries.");
        }

        return await clientService.UpdateAsync(id, request) ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await clientService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
