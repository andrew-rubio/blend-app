using System.ComponentModel.DataAnnotations;

namespace Blend.Api.Models.Admin;

// ─── Request models ──────────────────────────────────────────────────────────

public class CreateStoryRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public string? Author { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public List<string> RelatedRecipeIds { get; set; } = [];

    public int? ReadingTimeMinutes { get; set; }

    public int DisplayOrder { get; set; } = 0;
}

public class UpdateStoryRequest
{
    public string? Title { get; set; }

    public string? CoverImageUrl { get; set; }

    public string? Author { get; set; }

    public string? Content { get; set; }

    public List<string>? RelatedRecipeIds { get; set; }

    public int? ReadingTimeMinutes { get; set; }

    public int? DisplayOrder { get; set; }
}

// ─── Response model ───────────────────────────────────────────────────────────

public class StoryResponse
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public string? Author { get; set; }

    public string? Content { get; set; }

    public List<string> RelatedRecipeIds { get; set; } = [];

    public int? ReadingTimeMinutes { get; set; }

    public int DisplayOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
