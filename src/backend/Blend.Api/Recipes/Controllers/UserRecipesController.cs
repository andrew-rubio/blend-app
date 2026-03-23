using System.Security.Claims;
using Blend.Api.Recipes.Services;
using Blend.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Recipes.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public sealed class UserRecipesController : ControllerBase
{
    private readonly IRecipeService? _recipeService;
    private readonly ILogger<UserRecipesController> _logger;

    public UserRecipesController(
        ILogger<UserRecipesController> logger,
        IRecipeService? recipeService = null)
    {
        _logger = logger;
        _recipeService = recipeService;
    }

    // GET /api/v1/users/{userId}/recipes
    [HttpGet("{userId}/recipes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetUserRecipes(
        string userId,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? continuationToken = null,
        CancellationToken ct = default)
    {
        var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (requestingUserId is null)
        {
            return UnauthorizedProblem();
        }

        if (_recipeService is null)
        {
            return ServiceUnavailableProblem();
        }

        var clampedPageSize = Math.Clamp(pageSize, 1, 50);
        var options = new FeedPaginationOptions { PageSize = clampedPageSize, ContinuationToken = continuationToken };
        var result = await _recipeService.GetUserRecipesAsync(userId, requestingUserId, options, ct);
        return Ok(result);
    }

    // GET /api/v1/users/me/recipes
    [HttpGet("me/recipes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetMyRecipes(
        [FromQuery] int pageSize = 20,
        [FromQuery] string? continuationToken = null,
        CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_recipeService is null)
        {
            return ServiceUnavailableProblem();
        }

        var clampedPageSize = Math.Clamp(pageSize, 1, 50);
        var options = new FeedPaginationOptions { PageSize = clampedPageSize, ContinuationToken = continuationToken };
        var result = await _recipeService.GetUserRecipesAsync(userId, userId, options, ct);
        return Ok(result);
    }

    // GET /api/v1/users/me/liked-recipes
    [HttpGet("me/liked-recipes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetMyLikedRecipes(
        [FromQuery] int pageSize = 20,
        [FromQuery] string? continuationToken = null,
        CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_recipeService is null)
        {
            return ServiceUnavailableProblem();
        }

        var clampedPageSize = Math.Clamp(pageSize, 1, 50);
        var options = new FeedPaginationOptions { PageSize = clampedPageSize, ContinuationToken = continuationToken };
        var result = await _recipeService.GetLikedRecipesAsync(userId, options, ct);
        return Ok(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private IActionResult UnauthorizedProblem() =>
        Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized",
            detail: "User identity could not be resolved.");

    private IActionResult ServiceUnavailableProblem() =>
        Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Service unavailable",
            detail: "The recipe service is not available.");
}
