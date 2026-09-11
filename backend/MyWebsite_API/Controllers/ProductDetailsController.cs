using Microsoft.AspNetCore.Mvc;
using MyWebsite_API.Models;
using MyWebsite_API.Services;

namespace MyWebsite_API.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductDetailsController(IOverviewService overviewService) : ControllerBase
{
    [HttpGet("{id:int}/details")]
    public async Task<ActionResult<ProductDetailsResponse>> Get(int id)
    {
        var details = await overviewService.GetProductDetailsAsync(id);
        return details is null ? NotFound() : Ok(details);
    }
}
