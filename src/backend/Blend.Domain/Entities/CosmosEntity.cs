using System.Text.Json.Serialization;

namespace Blend.Domain.Entities;

/// <summary>
/// Base class for all Cosmos DB entities.
/// Provides common properties required for all documents.
/// </summary>
public abstract class CosmosEntity
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("_ts")]
    public long? Timestamp { get; set; }

    [JsonPropertyName("_etag")]
    public string? ETag { get; set; }

    [JsonPropertyName("ttl")]
    public int? Ttl { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
