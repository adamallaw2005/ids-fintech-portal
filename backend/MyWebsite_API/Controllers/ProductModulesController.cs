using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebsite_API.Models;
using MyWebsite_API.Services;

namespace MyWebsite_API.Controllers;

[ApiController]
[Route("api/product-modules")]
public sealed class ProductModulesController(IProductModuleService productModuleService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductModuleDetails>>> GetAll(
        [FromQuery] int? productId,
        [FromQuery] string? status)
    {
        return Ok(await productModuleService.GetAllAsync(productId, status));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductModuleDetails>> GetById(int id)
    {
        var module = await productModuleService.GetByIdAsync(id);
        return module is null ? NotFound() : Ok(module);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductModuleDetails>> Create(ProductModuleRequest request)
    {
        var moduleId = await productModuleService.CreateAsync(request);
        var module = await productModuleService.GetByIdAsync(moduleId);
        return CreatedAtAction(nameof(GetById), new { id = moduleId }, module);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductModuleRequest request)
    {
        return await productModuleService.UpdateAsync(id, request) ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await productModuleService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
