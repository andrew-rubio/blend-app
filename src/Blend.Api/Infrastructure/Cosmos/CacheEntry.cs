using Newtonsoft.Json;

namespace Blend.Api.Infrastructure.Cosmos;

/// <summary>
/// Document shape stored in the Cosmos DB cache container.
/// Uses TTL-based auto-expiration (ttl field, per ADR 0009).
/// </summary>
public sealed class CacheEntry
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("cacheKey")]
    public string CacheKey { get; set; } = string.Empty;

    [JsonProperty("data")]
    public string Data { get; set; } = string.Empty;

    [JsonProperty("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Cosmos DB TTL in seconds. -1 = inherit container default; null = no expiry.</summary>
    [JsonProperty("ttl")]
    public int? Ttl { get; set; }
}
