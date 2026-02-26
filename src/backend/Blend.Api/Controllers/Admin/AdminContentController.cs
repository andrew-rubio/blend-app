using System.Security.Claims;
using Blend.Api.Models.Admin;
using Blend.Domain.Entities;
using Blend.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/content")]
[Authorize(Policy = "RequireAdmin")]
public class AdminContentController : ControllerBase
{
    private readonly IRepository<Content> _contentRepository;
    private readonly ILogger<AdminContentController> _logger;

    public AdminContentController(
        IRepository<Content> contentRepository,
        ILogger<AdminContentController> logger)
    {
        _contentRepository = contentRepository;
        _logger = logger;
    }

    // ─── Featured Recipes ─────────────────────────────────────────────────────

    [HttpGet("featured-recipes")]
    public async Task<IActionResult> GetFeaturedRecipes(
        [FromQuery] int pageSize = 20,
        [FromQuery] string? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentRepository.QueryAsync(
            "SELECT * FROM c WHERE c.contentType = @type ORDER BY c.displayOrder ASC",
            new PaginationOptions { PageSize = pageSize, ContinuationToken = continuationToken },
            new Dictionary<string, object> { ["@type"] = "featured-recipe" },
            partitionKey: "featured-recipe",
            cancellationToken: cancellationToken);

        return Ok(new
        {
            items = result.Items.Select(MapToFeaturedRecipeResponse),
            continuationToken = result.ContinuationToken,
            hasMore = result.HasMore
        });
    }

    [HttpPost("featured-recipes")]
    public async Task<IActionResult> CreateFeaturedRecipe(
        [FromBody] CreateFeaturedRecipeRequest request,
        CancellationToken cancellationToken = default)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        var content = new Content
        {
            ContentType = "featured-recipe",
            Title = request.Title,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            DisplayOrder = request.DisplayOrder,
            RecipeId = request.RecipeId,
            RecipeSource = request.Source,
            AuthorId = adminId
        };

        var created = await _contentRepository.CreateAsync(content, cancellationToken);
        _logger.LogInformation("Admin {AdminId} created featured recipe {Id}", adminId, created.Id);

        return CreatedAtAction(
            nameof(GetFeaturedRecipes),
            new { },
            MapToFeaturedRecipeResponse(created));
    }

    [HttpPut("featured-recipes/{id}")]
    public async Task<IActionResult> UpdateFeaturedRecipe(
        string id,
        [FromBody] UpdateFeaturedRecipeRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _contentRepository.GetByIdAsync(id, "featured-recipe", cancellationToken);
        if (existing is null) return NotFound();

        if (request.Title is not null) existing.Title = request.Title;
        if (request.Description is not null) existing.Description = request.Description;
        if (request.ImageUrl is not null) existing.ImageUrl = request.ImageUrl;
        if (request.DisplayOrder.HasValue) existing.DisplayOrder = request.DisplayOrder.Value;

        var updated = await _contentRepository.UpdateAsync(existing, "featured-recipe", cancellationToken);
        return Ok(MapToFeaturedRecipeResponse(updated));
    }

    [HttpDelete("featured-recipes/{id}")]
    public async Task<IActionResult> DeleteFeaturedRecipe(
        string id,
        CancellationToken cancellationToken = default)
    {
        var deleted = await _contentRepository.DeleteAsync(id, "featured-recipe", cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    // ─── Stories ──────────────────────────────────────────────────────────────

    [HttpGet("stories")]
    public async Task<IActionResult> GetStories(
        [FromQuery] int pageSize = 20,
        [FromQuery] string? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentRepository.QueryAsync(
            "SELECT * FROM c WHERE c.contentType = @type ORDER BY c.displayOrder ASC",
            new PaginationOptions { PageSize = pageSize, ContinuationToken = continuationToken },
            new Dictionary<string, object> { ["@type"] = "story" },
            partitionKey: "story",
            cancellationToken: cancellationToken);

        return Ok(new
        {
            items = result.Items.Select(MapToStoryResponse),
            continuationToken = result.ContinuationToken,
            hasMore = result.HasMore
        });
    }

    [HttpPost("stories")]
    public async Task<IActionResult> CreateStory(
        [FromBody] CreateStoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        var content = new Content
        {
            ContentType = "story",
            Title = request.Title,
            CoverImageUrl = request.CoverImageUrl,
            Author = request.Author,
            Body = request.Content,
            RelatedRecipeIds = request.RelatedRecipeIds,
            ReadingTimeMinutes = request.ReadingTimeMinutes,
            DisplayOrder = request.DisplayOrder,
            AuthorId = adminId
        };

        var created = await _contentRepository.CreateAsync(content, cancellationToken);
        _logger.LogInformation("Admin {AdminId} created story {Id}", adminId, created.Id);

        return CreatedAtAction(
            nameof(GetStories),
            new { },
            MapToStoryResponse(created));
    }

    [HttpPut("stories/{id}")]
    public async Task<IActionResult> UpdateStory(
        string id,
        [FromBody] UpdateStoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _contentRepository.GetByIdAsync(id, "story", cancellationToken);
        if (existing is null) return NotFound();

        if (request.Title is not null) existing.Title = request.Title;
        if (request.CoverImageUrl is not null) existing.CoverImageUrl = request.CoverImageUrl;
        if (request.Author is not null) existing.Author = request.Author;
        if (request.Content is not null) existing.Body = request.Content;
        if (request.RelatedRecipeIds is not null) existing.RelatedRecipeIds = request.RelatedRecipeIds;
        if (request.ReadingTimeMinutes.HasValue) existing.ReadingTimeMinutes = request.ReadingTimeMinutes;
        if (request.DisplayOrder.HasValue) existing.DisplayOrder = request.DisplayOrder.Value;

        var updated = await _contentRepository.UpdateAsync(existing, "story", cancellationToken);
        return Ok(MapToStoryResponse(updated));
    }

    [HttpDelete("stories/{id}")]
    public async Task<IActionResult> DeleteStory(
        string id,
        CancellationToken cancellationToken = default)
    {
        var deleted = await _contentRepository.DeleteAsync(id, "story", cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    // ─── Videos ───────────────────────────────────────────────────────────────

    [HttpGet("videos")]
    public async Task<IActionResult> GetVideos(
        [FromQuery] int pageSize = 20,
        [FromQuery] string? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _contentRepository.QueryAsync(
            "SELECT * FROM c WHERE c.contentType = @type ORDER BY c.displayOrder ASC",
            new PaginationOptions { PageSize = pageSize, ContinuationToken = continuationToken },
            new Dictionary<string, object> { ["@type"] = "video" },
            partitionKey: "video",
            cancellationToken: cancellationToken);

        return Ok(new
        {
            items = result.Items.Select(MapToVideoResponse),
            continuationToken = result.ContinuationToken,
            hasMore = result.HasMore
        });
    }

    [HttpPost("videos")]
    public async Task<IActionResult> CreateVideo(
        [FromBody] CreateVideoRequest request,
        CancellationToken cancellationToken = default)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        var content = new Content
        {
            ContentType = "video",
            Title = request.Title,
            ThumbnailUrl = request.ThumbnailUrl,
            VideoUrl = request.VideoUrl,
            DurationSeconds = request.DurationSeconds,
            Creator = request.Creator,
            DisplayOrder = request.DisplayOrder,
            AuthorId = adminId
        };

        var created = await _contentRepository.CreateAsync(content, cancellationToken);
        _logger.LogInformation("Admin {AdminId} created video {Id}", adminId, created.Id);

        return CreatedAtAction(
            nameof(GetVideos),
            new { },
            MapToVideoResponse(created));
    }

    [HttpPut("videos/{id}")]
    public async Task<IActionResult> UpdateVideo(
        string id,
        [FromBody] UpdateVideoRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _contentRepository.GetByIdAsync(id, "video", cancellationToken);
        if (existing is null) return NotFound();

        if (request.Title is not null) existing.Title = request.Title;
        if (request.ThumbnailUrl is not null) existing.ThumbnailUrl = request.ThumbnailUrl;
        if (request.VideoUrl is not null) existing.VideoUrl = request.VideoUrl;
        if (request.DurationSeconds.HasValue) existing.DurationSeconds = request.DurationSeconds;
        if (request.Creator is not null) existing.Creator = request.Creator;
        if (request.DisplayOrder.HasValue) existing.DisplayOrder = request.DisplayOrder.Value;

        var updated = await _contentRepository.UpdateAsync(existing, "video", cancellationToken);
        return Ok(MapToVideoResponse(updated));
    }

    [HttpDelete("videos/{id}")]
    public async Task<IActionResult> DeleteVideo(
        string id,
        CancellationToken cancellationToken = default)
    {
        var deleted = await _contentRepository.DeleteAsync(id, "video", cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    // ─── Mapping helpers ──────────────────────────────────────────────────────

    private static FeaturedRecipeResponse MapToFeaturedRecipeResponse(Content c) => new()
    {
        Id = c.Id,
        RecipeId = c.RecipeId ?? string.Empty,
        Source = c.RecipeSource ?? string.Empty,
        Title = c.Title,
        Description = c.Description,
        ImageUrl = c.ImageUrl,
        DisplayOrder = c.DisplayOrder,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };

    private static StoryResponse MapToStoryResponse(Content c) => new()
    {
        Id = c.Id,
        Title = c.Title,
        CoverImageUrl = c.CoverImageUrl,
        Author = c.Author,
        Content = c.Body,
        RelatedRecipeIds = c.RelatedRecipeIds,
        ReadingTimeMinutes = c.ReadingTimeMinutes,
        DisplayOrder = c.DisplayOrder,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };

    private static VideoResponse MapToVideoResponse(Content c) => new()
    {
        Id = c.Id,
        Title = c.Title,
        ThumbnailUrl = c.ThumbnailUrl,
        VideoUrl = c.VideoUrl ?? string.Empty,
        DurationSeconds = c.DurationSeconds,
        Creator = c.Creator,
        DisplayOrder = c.DisplayOrder,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };
}
