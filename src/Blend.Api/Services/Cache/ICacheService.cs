namespace Blend.Api.Services.Cache;

/// <summary>
/// Two-tier cache service: L1 (in-process IMemoryCache) and L2 (Cosmos DB) with configurable TTLs.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Retrieve a cached value. Checks L1 first, then L2.
    /// Returns <c>null</c> on a full miss.
    /// </summary>
    Task<(T? Value, string? Tier)> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Store a value in both L1 and L2 with independent TTLs.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan l1Ttl, TimeSpan l2Ttl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a cached entry from both tiers.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
