using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebsite_API.Models;
using MyWebsite_API.Services;

namespace MyWebsite_API.Controllers;

[ApiController]
[Route("api/repositories")]
public sealed class RepositoriesController(IRepositoryLinkService repositoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RepositoryDetails>>> GetAll([FromQuery] int? productId)
    {
        return Ok(await repositoryService.GetAllAsync(productId));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RepositoryDetails>> GetById(int id)
    {
        var repository = await repositoryService.GetByIdAsync(id);
        return repository is null ? NotFound() : Ok(repository);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<RepositoryDetails>> Create(RepositoryRequest request)
    {
        var repositoryId = await repositoryService.CreateAsync(request);
        var repository = await repositoryService.GetByIdAsync(repositoryId);
        return CreatedAtAction(nameof(GetById), new { id = repositoryId }, repository);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, RepositoryRequest request)
    {
        return await repositoryService.UpdateAsync(id, request) ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await repositoryService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
