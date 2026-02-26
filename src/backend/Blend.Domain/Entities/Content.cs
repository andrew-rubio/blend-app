namespace Blend.Domain.Entities;

/// <summary>
/// Admin-managed content: featured recipes, stories, videos, and ingredient submissions.
/// Partition key: /contentType
/// </summary>
public class Content : CosmosEntity
{
    /// <summary>Partition key value. One of: featured-recipe, story, video, ingredient-submission.</summary>
    public string ContentType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public ContentStatus Status { get; set; } = ContentStatus.Published;

    public int DisplayOrder { get; set; } = 0;

    public string AuthorId { get; set; } = string.Empty;

    // ─── Featured-recipe fields ─────────────────────────────────────────────
    public string? RecipeId { get; set; }

    /// <summary>spoonacular | community</summary>
    public string? RecipeSource { get; set; }

    // ─── Story fields ────────────────────────────────────────────────────────
    public string? CoverImageUrl { get; set; }

    public string? Author { get; set; }

    /// <summary>Full story body (markdown or HTML).</summary>
    public string? Body { get; set; }

    public List<string> RelatedRecipeIds { get; set; } = [];

    public int? ReadingTimeMinutes { get; set; }

    // ─── Video fields ────────────────────────────────────────────────────────
    public string? VideoUrl { get; set; }

    public string? ThumbnailUrl { get; set; }

    public int? DurationSeconds { get; set; }

    public string? Creator { get; set; }
}

public enum ContentStatus
{
    Published,
    Archived
}
