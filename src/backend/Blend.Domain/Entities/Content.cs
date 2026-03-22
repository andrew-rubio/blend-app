using System.Text.Json.Serialization;

namespace Blend.Domain.Entities;

/// <summary>Type of admin-managed content item.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContentType
{
    FeaturedRecipe,
    Story,
    Video,
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
}
