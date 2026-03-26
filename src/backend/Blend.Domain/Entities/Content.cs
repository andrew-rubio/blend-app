using System.Text.Json.Serialization;

namespace Blend.Domain.Entities;

/// <summary>Type of admin-managed content item.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContentType
{
    FeaturedRecipe,
    Story,
    Video,
    IngredientSubmission,
}

/// <summary>Review status of a user-submitted content item.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubmissionStatus
{
    Pending,
    Approved,
    Rejected,
}

/// <summary>Admin-managed content item (featured recipes, stories, videos).</summary>
public sealed class Content
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("contentType")]
    public ContentType ContentType { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("body")]
    public string? Body { get; init; }

    [JsonPropertyName("thumbnailUrl")]
    public string? ThumbnailUrl { get; init; }

    [JsonPropertyName("mediaUrl")]
    public string? MediaUrl { get; init; }

    [JsonPropertyName("authorName")]
    public string? AuthorName { get; init; }

    [JsonPropertyName("isPublished")]
    public bool IsPublished { get; init; }

    [JsonPropertyName("publishedAt")]
    public DateTimeOffset? PublishedAt { get; init; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; init; }

    // ── Ordering ───────────────────────────────────────────────────────────────

    /// <summary>Display order for home page aggregation sorting.</summary>
    [JsonPropertyName("displayOrder")]
    public int? DisplayOrder { get; init; }

    // ── Featured recipe fields ─────────────────────────────────────────────────

    /// <summary>Reference to an existing recipe (for featured recipes).</summary>
    [JsonPropertyName("recipeId")]
    public string? RecipeId { get; init; }

    /// <summary>Recipe source ('spoonacular' or 'community') for featured recipes.</summary>
    [JsonPropertyName("source")]
    public string? Source { get; init; }

    // ── Story fields ───────────────────────────────────────────────────────────

    /// <summary>Related recipe IDs referenced in a story.</summary>
    [JsonPropertyName("relatedRecipeIds")]
    public IReadOnlyList<string>? RelatedRecipeIds { get; init; }

    /// <summary>Estimated reading time in minutes (for stories).</summary>
    [JsonPropertyName("readingTimeMinutes")]
    public int? ReadingTimeMinutes { get; init; }

    // ── Video fields ───────────────────────────────────────────────────────────

    /// <summary>Duration of the video in seconds.</summary>
    [JsonPropertyName("durationSeconds")]
    public int? DurationSeconds { get; init; }

    // ── Ingredient submission fields ───────────────────────────────────────────

    /// <summary>The user who submitted this ingredient for review.</summary>
    [JsonPropertyName("submittedByUserId")]
    public string? SubmittedByUserId { get; init; }

    /// <summary>Ingredient category (for ingredient submissions).</summary>
    [JsonPropertyName("category")]
    public string? Category { get; init; }

    /// <summary>Review status for user-submitted content.</summary>
    [JsonPropertyName("submissionStatus")]
    public SubmissionStatus? SubmissionStatus { get; init; }

    /// <summary>Rejection reason for rejected ingredient submissions.</summary>
    [JsonPropertyName("rejectionReason")]
    public string? RejectionReason { get; init; }
}
