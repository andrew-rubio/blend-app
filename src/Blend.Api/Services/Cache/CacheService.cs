using System.Text.Json;
using Blend.Api.Infrastructure.Cosmos;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blend.Api.Services.Cache;

/// <summary>
/// Two-tier cache: L1 = IMemoryCache (in-process), L2 = Cosmos DB (durable, TTL-based).
/// Reads: L1 → L2 → null. Writes: L1 and L2 in parallel.
/// CosmosClient is optional; when absent, L2 operations are silently skipped.
/// </summary>
public sealed class CacheService : ICacheService
{
    private readonly IMemoryCache _l1;
    private readonly CosmosClient? _cosmosClient;
    private readonly CacheOptions _options;
    private readonly ILogger<CacheService> _logger;

    public CacheService(
        IMemoryCache l1,
        IOptions<CacheOptions> options,
        ILogger<CacheService> logger,
        IServiceProvider serviceProvider)
    {
        _l1 = l1;
        _options = options.Value;
        _logger = logger;
        _cosmosClient = serviceProvider.GetService<CosmosClient>();
    }

    /// <inheritdoc/>
    public async Task<(T? Value, string? Tier)> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        // L1 check
        if (_l1.TryGetValue(key, out T? l1Value))
        {
            _logger.LogDebug("Cache L1 hit for key {Key}", key);
            return (l1Value, "l1-cache");
        }

        // L2 check
        var container = GetContainer();
        if (container is null)
            return (default, null);

        try
        {
            var response = await container.ReadItemAsync<CacheEntry>(
                key, new PartitionKey(key), cancellationToken: cancellationToken);

            var entry = response.Resource;
            var value = JsonSerializer.Deserialize<T>(entry.Data);

            if (value is not null)
            {
                _logger.LogDebug("Cache L2 hit for key {Key}", key);
                return (value, "l2-cache");
            }
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogDebug("Cache L2 miss for key {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "L2 cache read failed for key {Key}; treating as miss", key);
        }

        return (default, null);
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, TimeSpan l1Ttl, TimeSpan l2Ttl, CancellationToken cancellationToken = default)
    {
        // Write L1
        var l1Options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = l1Ttl };
        _l1.Set(key, value, l1Options);

        // Write L2
        var container = GetContainer();
        if (container is null)
            return;

        try
        {
            var entry = new CacheEntry
            {
                Id = key,
                CacheKey = key,
                Data = JsonSerializer.Serialize(value),
                CreatedAt = DateTimeOffset.UtcNow,
                Ttl = (int)l2Ttl.TotalSeconds
            };

            await container.UpsertItemAsync(entry, new PartitionKey(key), cancellationToken: cancellationToken);
            _logger.LogDebug("Cache L2 write for key {Key} with TTL {Ttl}s", key, entry.Ttl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "L2 cache write failed for key {Key}; L1 still populated", key);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _l1.Remove(key);

        var container = GetContainer();
        if (container is null)
            return;

        try
        {
            await container.DeleteItemAsync<CacheEntry>(key, new PartitionKey(key), cancellationToken: cancellationToken);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Already gone — that's fine
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "L2 cache delete failed for key {Key}", key);
        }
    }

    private Container? GetContainer()
    {
        if (_cosmosClient is null)
            return null;

        try
        {
            return _cosmosClient.GetContainer(_options.DatabaseName, _options.ContainerName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve Cosmos DB cache container");
            return null;
        }
    }
}
