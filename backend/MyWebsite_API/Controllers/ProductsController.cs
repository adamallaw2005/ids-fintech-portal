using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebsite_API.Models;
using MyWebsite_API.Services;

namespace MyWebsite_API.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? technology)
    {
        return Ok(await productService.GetAllAsync(search, status, technology));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await productService.GetByIdAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Product>> Create(ProductCreateRequest request)
    {
        if (request.Responsibilities.Any(assignment => assignment is null))
        {
            return BadRequest("Assignments cannot contain empty entries.");
        }

        var productId = await productService.CreateAsync(request);
        var product = await productService.GetByIdAsync(productId);
        return CreatedAtAction(nameof(GetById), new { id = productId }, product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductUpdateRequest request)
    {
        if (request.Responsibilities?.Any(assignment => assignment is null) == true)
        {
            return BadRequest("Assignments cannot contain empty entries.");
        }

        return await productService.UpdateAsync(id, request) ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await productService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
