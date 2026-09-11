using Microsoft.AspNetCore.Mvc;
using MyWebsite_API.Models;
using MyWebsite_API.Services;

namespace MyWebsite_API.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(IOverviewService overviewService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Get()
    {
        return Ok(await overviewService.GetDashboardAsync());
    }
}
