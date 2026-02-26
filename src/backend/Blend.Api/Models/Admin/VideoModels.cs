using System.ComponentModel.DataAnnotations;

namespace Blend.Api.Models.Admin;

// ─── Request models ──────────────────────────────────────────────────────────

public class CreateVideoRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;

    public string? ThumbnailUrl { get; set; }

    [Required]
    public string VideoUrl { get; set; } = string.Empty;

    public int? DurationSeconds { get; set; }

    public string? Creator { get; set; }

    public int DisplayOrder { get; set; } = 0;
}

public class UpdateVideoRequest
{
    public string? Title { get; set; }

    public string? ThumbnailUrl { get; set; }

    public string? VideoUrl { get; set; }

    public int? DurationSeconds { get; set; }

    public string? Creator { get; set; }

    public int? DisplayOrder { get; set; }
}

// ─── Response model ───────────────────────────────────────────────────────────

public class VideoResponse
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? ThumbnailUrl { get; set; }

    public string VideoUrl { get; set; } = string.Empty;

    public int? DurationSeconds { get; set; }

    public string? Creator { get; set; }

    public int DisplayOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
