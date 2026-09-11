using Microsoft.AspNetCore.Mvc;
using MyWebsite_API.Models;
using MyWebsite_API.Services;

namespace MyWebsite_API.Controllers;

[ApiController]
[Route("api/clients")]
public sealed class ClientDetailsController(IOverviewService overviewService) : ControllerBase
{
    [HttpGet("{id:int}/details")]
    public async Task<ActionResult<ClientDetailsResponse>> Get(int id)
    {
        var details = await overviewService.GetClientDetailsAsync(id);
        return details is null ? NotFound() : Ok(details);
    }
}
