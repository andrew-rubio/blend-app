using System.Text.Json.Serialization;

namespace Blend.Domain.Entities;

/// <summary>
/// L2 cache entry for a Spoonacular API response (per ADR 0009).
/// The <see cref="CacheKey"/> is also the Cosmos DB document <c>id</c>.
/// </summary>
public sealed class CacheEntry
{
    [JsonPropertyName("id")]
    public string Id => CacheKey;

    [JsonPropertyName("cacheKey")]
    public string CacheKey { get; init; } = string.Empty;

    /// <summary>Serialised JSON payload from Spoonacular.</summary>
    [JsonPropertyName("data")]
    public string Data { get; init; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Time-to-live in seconds. Cosmos DB will auto-expire the document.</summary>
    [JsonPropertyName("ttl")]
    public int Ttl { get; init; }
}
