using System.Security.Claims;
using Blend.Api.Search.Models;
using Blend.Api.Search.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Search.Controllers;

/// <summary>
/// Provides endpoints for the unified recipe search and recently-viewed tracking (EXPL-08).
/// </summary>
[ApiController]
[Route("api/v1")]
public sealed class SearchController : ControllerBase
{
    private readonly ISearchService? _searchService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(
        ILogger<SearchController> logger,
        ISearchService? searchService = null)
    {
        _logger = logger;
        _searchService = searchService;
    }

    // GET /api/v1/search/recipes
    /// <summary>
    /// Unified recipe search — merges results from Spoonacular and the internal recipe store (EXPL-08).
    /// </summary>
    [HttpGet("search/recipes")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UnifiedSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SearchRecipes([FromQuery] SearchRecipesRequest request, CancellationToken ct)
    {
        if (_searchService is null)
        {
            return ServiceUnavailableProblem();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var response = await _searchService.SearchRecipesAsync(request, userId, ct);
        return Ok(response);
    }

    // GET /api/v1/users/me/recently-viewed
    /// <summary>
    /// Returns the most recently viewed recipes for the authenticated user (HOME-23, HOME-24).
    /// </summary>
    [HttpGet("users/me/recently-viewed")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetRecentlyViewed(
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_searchService is null)
        {
            return ServiceUnavailableProblem();
        }

        var clampedPageSize = Math.Clamp(pageSize, 1, 50);
        var results = await _searchService.GetRecentlyViewedAsync(userId, clampedPageSize, ct);
        return Ok(results);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private IActionResult UnauthorizedProblem() =>
        Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized",
            detail: "User identity could not be resolved.");

    private IActionResult ServiceUnavailableProblem() =>
        Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Service unavailable",
            detail: "The search service is not available.");
}
