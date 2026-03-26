using Blend.Api.Admin.Models;
using Blend.Api.Admin.Services;
using Blend.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Admin.Controllers;

/// <summary>
/// Admin CRUD endpoints for featured recipes, stories, and videos (PLAT-25 through PLAT-34).
/// All routes require the Admin role.
/// </summary>
[ApiController]
[Authorize(Policy = "RequireAdmin")]
public sealed class AdminContentController : ControllerBase
{
    private readonly IAdminContentService? _adminService;
    private readonly ILogger<AdminContentController> _logger;

    public AdminContentController(
        ILogger<AdminContentController> logger,
        IAdminContentService? adminService = null)
    {
        _logger = logger;
        _adminService = adminService;
    }

    // ── Featured Recipes ──────────────────────────────────────────────────────

    // GET /api/v1/admin/content/featured-recipes
    /// <summary>Lists all featured recipe entries (PLAT-25).</summary>
    [HttpGet("api/v1/admin/content/featured-recipes")]
    [ProducesResponseType(typeof(IReadOnlyList<ContentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetFeaturedRecipes(CancellationToken ct = default)
    {
        if (_adminService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _adminService.GetFeaturedRecipesAsync(ct);
        return Ok(result);
    }

    // POST /api/v1/admin/content/featured-recipes
    /// <summary>Adds a featured recipe entry (PLAT-26).</summary>
    [HttpPost("api/v1/admin/content/featured-recipes")]
    [ProducesResponseType(typeof(ContentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateFeaturedRecipe(
        [FromBody] CreateFeaturedRecipeRequest request,
        CancellationToken ct = default)
    {
        if (_adminService is null)
        {
            return ServiceUnavailableProblem();
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Validation failed",
                detail: "Title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.RecipeId))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Validation failed",
                detail: "RecipeId is required.");
        }

        var result = await _adminService.CreateFeaturedRecipeAsync(request, ct);
        return CreatedAtAction(nameof(GetFeaturedRecipes), null, result);
    }

    // PUT /api/v1/admin/content/featured-recipes/{id}
    /// <summary>Updates a featured recipe entry (PLAT-27).</summary>
    [HttpPut("api/v1/admin/content/featured-recipes/{id}")]
    [ProducesResponseType(typeof(ContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdateFeaturedRecipe(
        string id,
        [FromBody] UpdateFeaturedRecipeRequest request,
        CancellationToken ct = default)
    {
        if (_adminService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _adminService.UpdateFeaturedRecipeAsync(id, request, ct);
        if (result is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found",
                detail: "Featured recipe not found.");
        }

        return Ok(result);
    }

    // DELETE /api/v1/admin/content/featured-recipes/{id}
    /// <summary>Removes a featured recipe entry (PLAT-27).</summary>
    [HttpDelete("api/v1/admin/content/featured-recipes/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> DeleteFeaturedRecipe(string id, CancellationToken ct = default)
    {
        if (_adminService is null)
        {
            return ServiceUnavailableProblem();
        }

        var deleted = await _adminService.DeleteContentAsync(id, ContentType.FeaturedRecipe, ct);
        if (!deleted)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found",
                detail: "Featured recipe not found.");
        }

        return NoContent();
    }

    // ── Stories ───────────────────────────────────────────────────────────────

    // GET /api/v1/admin/content/stories
    /// <summary>Lists all stories (PLAT-28).</summary>
    [HttpGet("api/v1/admin/content/stories")]
    [ProducesResponseType(typeof(IReadOnlyList<ContentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetStories(CancellationToken ct = default)
    {
        if (_adminService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _adminService.GetStoriesAsync(ct);
        return Ok(result);
    }

    // POST /api/v1/admin/content/stories
    /// <summary>Creates a new story (PLAT-29).</summary>
    [HttpPost("api/v1/admin/content/stories")]
    [ProducesResponseType(typeof(ContentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateStory(
        [FromBody] CreateStoryRequest request,
        CancellationToken ct = default)
    {
        if (_adminService is null)
        {
            return ServiceUnavailableProblem();
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Validation failed",
                detail: "Title is required.");
        }

        var result = await _adminService.CreateStoryAsync(request, ct);
        return CreatedAtAction(nameof(GetStories), null, result);
    }

    // PUT /api/v1/admin/content/stories/{id}
    /// <summary>Updates a story (PLAT-30).</summary>
    [HttpPut("api/v1/admin/content/stories/{id}")]
    [ProducesResponseType(typeof(ContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdateStory(
        string id,
        [FromBody] UpdateStoryRequest request,
        CancellationToken ct = default)
    {
        if (_adminService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _adminService.UpdateStoryAsync(id, request, ct);
        if (result is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found",
                detail: "Story not found.");
        }

        return Ok(result);
    }

    // DELETE /api/v1/admin/content/stories/{id}
    /// <summary>Deletes a story (PLAT-30).</summary>
    [HttpDelete("api/v1/admin/content/stories/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> DeleteStory(string id, CancellationToken ct = default)
    {
        if (_adminService is null)
        {
            return ServiceUnavailableProblem();
        }

        var deleted = await _adminService.DeleteContentAsync(id, ContentType.Story, ct);
        if (!deleted)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found",
                detail: "Story not found.");
        }

        return NoContent();
    }

    // ── Videos ────────────────────────────────────────────────────────────────

    // GET /api/v1/admin/content/videos
    /// <summary>Lists all videos (PLAT-31).</summary>
    [HttpGet("api/v1/admin/content/videos")]
    [ProducesResponseType(typeof(IReadOnlyList<ContentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetVideos(CancellationToken ct = default)
    {
        if (_adminService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _adminService.GetVideosAsync(ct);
        return Ok(result);
    }

    // POST /api/v1/admin/content/videos
    /// <summary>Adds a video (PLAT-32).</summary>
    [HttpPost("api/v1/admin/content/videos")]
    [ProducesResponseType(typeof(ContentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateVideo(
        [FromBody] CreateVideoRequest request,
        CancellationToken ct = default)
    {
        if (_adminService is null)
        {
            return ServiceUnavailableProblem();
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Validation failed",
                detail: "Title is required.");
        }

        var result = await _adminService.CreateVideoAsync(request, ct);
        return CreatedAtAction(nameof(GetVideos), null, result);
    }

    // PUT /api/v1/admin/content/videos/{id}
    /// <summary>Updates a video (PLAT-33).</summary>
    [HttpPut("api/v1/admin/content/videos/{id}")]
    [ProducesResponseType(typeof(ContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdateVideo(
        string id,
        [FromBody] UpdateVideoRequest request,
        CancellationToken ct = default)
    {
        if (_adminService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _adminService.UpdateVideoAsync(id, request, ct);
        if (result is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found",
                detail: "Video not found.");
        }

        return Ok(result);
    }

    // DELETE /api/v1/admin/content/videos/{id}
    /// <summary>Deletes a video (PLAT-34).</summary>
    [HttpDelete("api/v1/admin/content/videos/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> DeleteVideo(string id, CancellationToken ct = default)
    {
        if (_adminService is null)
        {
            return ServiceUnavailableProblem();
        }

        var deleted = await _adminService.DeleteContentAsync(id, ContentType.Video, ct);
        if (!deleted)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found",
                detail: "Video not found.");
        }

        return NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private IActionResult ServiceUnavailableProblem() =>
        Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Service unavailable",
            detail: "The admin content service is not available.");
}
