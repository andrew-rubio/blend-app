namespace Blend.Domain.Entities;

/// <summary>
/// L2 cache entry for Spoonacular API responses.
/// Partition key: /cacheKey
/// TTL: configurable (default 24 hours = 86400 seconds)
/// </summary>
public class CacheEntry : CosmosEntity
{
    public string CacheKey { get; set; } = string.Empty;

    public string SerializedValue { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public string? SourceUrl { get; set; }

    public Dictionary<string, string> Tags { get; set; } = [];
}
