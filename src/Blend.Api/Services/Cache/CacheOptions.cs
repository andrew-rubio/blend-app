namespace Blend.Api.Services.Cache;

/// <summary>
/// Configuration options for the two-tier cache.
/// </summary>
public sealed class CacheOptions
{
    public const string SectionName = "Cache";

    /// <summary>Cosmos DB connection string for L2 cache.</summary>
    public string? CosmosConnectionString { get; set; }

    /// <summary>Cosmos DB database name.</summary>
    public string DatabaseName { get; set; } = "blend";

    /// <summary>Cosmos DB container name for cache entries.</summary>
    public string ContainerName { get; set; } = "cache";
}
