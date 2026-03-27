using Blend.Domain.Entities;

namespace Blend.Api.Admin.Models;

// ── Request models ─────────────────────────────────────────────────────────────

/// <summary>Request body to create a featured recipe entry.</summary>
public sealed class CreateFeaturedRecipeRequest
{
    public string RecipeId { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public int DisplayOrder { get; init; }
}

/// <summary>Request body to update a featured recipe entry.</summary>
public sealed class UpdateFeaturedRecipeRequest
{
    public string? RecipeId { get; init; }
    public string? Source { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public int? DisplayOrder { get; init; }
}

/// <summary>Request body to create a story.</summary>
public sealed class CreateStoryRequest
{
    public string Title { get; init; } = string.Empty;
    public string? CoverImageUrl { get; init; }
    public string? Author { get; init; }
    public string? Content { get; init; }
    public IReadOnlyList<string>? RelatedRecipeIds { get; init; }
    public int? ReadingTimeMinutes { get; init; }
}

/// <summary>Request body to update a story.</summary>
public sealed class UpdateStoryRequest
{
    public string? Title { get; init; }
    public string? CoverImageUrl { get; init; }
    public string? Author { get; init; }
    public string? Content { get; init; }
    public IReadOnlyList<string>? RelatedRecipeIds { get; init; }
    public int? ReadingTimeMinutes { get; init; }
}

/// <summary>Request body to add a video.</summary>
public sealed class CreateVideoRequest
{
    public string Title { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public string? VideoUrl { get; init; }
    public int? DurationSeconds { get; init; }
    public string? Creator { get; init; }
}

/// <summary>Request body to update a video.</summary>
public sealed class UpdateVideoRequest
{
    public string? Title { get; init; }
    public string? ThumbnailUrl { get; init; }
    public string? VideoUrl { get; init; }
    public int? DurationSeconds { get; init; }
    public string? Creator { get; init; }
}

/// <summary>Request body to reject an ingredient submission.</summary>
public sealed class RejectSubmissionRequest
{
    public string? Reason { get; init; }
}

// ── Response models ────────────────────────────────────────────────────────────

/// <summary>Response model for a single admin-managed content item.</summary>
public sealed class ContentResponse
{
    public string Id { get; init; } = string.Empty;
    public ContentType ContentType { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Body { get; init; }
    public string? ThumbnailUrl { get; init; }
    public string? MediaUrl { get; init; }
    public string? AuthorName { get; init; }
    public bool IsPublished { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public int? DisplayOrder { get; init; }
    public string? RecipeId { get; init; }
    public string? Source { get; init; }
    public IReadOnlyList<string>? RelatedRecipeIds { get; init; }
    public int? ReadingTimeMinutes { get; init; }
    public int? DurationSeconds { get; init; }

    /// <summary>Maps a <see cref="Content"/> entity to the API response model.</summary>
    public static ContentResponse FromEntity(Content c) => new()
    {
        Id = c.Id,
        ContentType = c.ContentType,
        Title = c.Title,
        Body = c.Body,
        ThumbnailUrl = c.ThumbnailUrl,
        MediaUrl = c.MediaUrl,
        AuthorName = c.AuthorName,
        IsPublished = c.IsPublished,
        PublishedAt = c.PublishedAt,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        DisplayOrder = c.DisplayOrder,
        RecipeId = c.RecipeId,
        Source = c.Source,
        RelatedRecipeIds = c.RelatedRecipeIds,
        ReadingTimeMinutes = c.ReadingTimeMinutes,
        DurationSeconds = c.DurationSeconds,
    };
}

/// <summary>Response model for an ingredient submission in the admin queue.</summary>
public sealed class AdminSubmissionResponse
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Category { get; init; }
    public string? Description { get; init; }
    public SubmissionStatus Status { get; init; }
    public string? SubmittedByUserId { get; init; }
    public string? RejectionReason { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    /// <summary>Maps a <see cref="Content"/> entity to this response model.</summary>
    public static AdminSubmissionResponse FromEntity(Content c) => new()
    {
        Id = c.Id,
        Name = c.Title,
        Category = c.Category,
        Description = c.Body,
        Status = c.SubmissionStatus ?? SubmissionStatus.Pending,
        SubmittedByUserId = c.SubmittedByUserId,
        RejectionReason = c.RejectionReason,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
    };
}

/// <summary>Paged response for admin submission listing.</summary>
public sealed class AdminSubmissionsPageResponse
{
    public IReadOnlyList<AdminSubmissionResponse> Items { get; init; } = [];
    public string? NextCursor { get; init; }
    public bool HasMore { get; init; }
}
