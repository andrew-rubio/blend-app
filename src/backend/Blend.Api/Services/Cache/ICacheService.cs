namespace Blend.Api.Services.Cache;

/// <summary>
/// Two-tier cache service (per ADR 0009).
/// <list type="bullet">
///   <item><description>L1 – <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache"/> (in-process, fast)</description></item>
///   <item><description>L2 – Cosmos DB <c>cache</c> container (durable, TTL-driven auto-expiry)</description></item>
/// </list>
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Reads from L1 first, then L2 on a miss.
    /// Returns <c>null</c> when not found in either tier.
    /// </summary>
    /// <typeparam name="T">Deserialisation target type.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the value to both L1 and L2 simultaneously.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="value">Value to cache.</param>
    /// <param name="l1Ttl">Time-to-live for the in-process L1 cache entry.</param>
    /// <param name="l2Ttl">Time-to-live for the Cosmos DB L2 cache document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetAsync<T>(string key, T value, TimeSpan l1Ttl, TimeSpan l2Ttl, CancellationToken cancellationToken = default);
}
