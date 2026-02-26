namespace Blend.Domain.Entities;

/// <summary>
/// Admin-managed content such as featured recipes, banners, curated collections.
/// Partition key: /contentType
/// </summary>
public class Content : CosmosEntity
{
    public string ContentType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Body { get; set; }

    public string? ImageUrl { get; set; }

    public ContentStatus Status { get; set; } = ContentStatus.Draft;

    public DateTimeOffset? PublishedAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public int SortOrder { get; set; } = 0;

    public Dictionary<string, string> Metadata { get; set; } = [];

    public List<string> Tags { get; set; } = [];

    public string AuthorId { get; set; } = string.Empty;
}

public enum ContentStatus
{
    Draft,
    Published,
    Archived
}
