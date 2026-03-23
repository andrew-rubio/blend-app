using System.Security.Claims;
using Blend.Api.Home.Models;
using Blend.Api.Home.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Home.Controllers;

/// <summary>
/// Provides the aggregated home page endpoint (HOME-01 through HOME-24, ADR 0006).
/// </summary>
[ApiController]
[Route("api/v1")]
public sealed class HomeController : ControllerBase
{
    private readonly IHomeService? _homeService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        ILogger<HomeController> logger,
        IHomeService? homeService = null)
    {
        _logger = logger;
        _homeService = homeService;
    }

    // GET /api/v1/home
    /// <summary>
    /// Returns all home page sections in a single aggregated response.
    /// Guest users receive featured and community content but no recently-viewed entries.
    /// </summary>
    [HttpGet("home")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HomeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHome(CancellationToken ct)
    {
        if (_homeService is null)
        {
            return ServiceUnavailableProblem();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var response = await _homeService.GetHomeAsync(userId, ct);
        return Ok(response);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private IActionResult ServiceUnavailableProblem() =>
        Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Service unavailable",
            detail: "The home service is not available.");
}
