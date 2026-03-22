using System.Net;
using System.Text.Json;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Blend.Api.Services.Cache;

/// <inheritdoc cref="ICacheService"/>
public sealed class CacheService : ICacheService
{
    private readonly IMemoryCache _l1;
    private readonly IRepository<CacheEntry>? _l2;
    private readonly ILogger<CacheService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public CacheService(
        IMemoryCache l1,
        ILogger<CacheService> logger,
        IRepository<CacheEntry>? l2 = null)
    {
        _l1 = l1;
        _l2 = l2;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        // ── L1 check ──────────────────────────────────────────────────────────
        if (_l1.TryGetValue(key, out T? l1Value))
        {
            _logger.LogDebug("Cache L1 hit for key {Key}", key);
            return l1Value;
        }

        // ── L2 check ──────────────────────────────────────────────────────────
        if (_l2 is null)
        {
            return default;
        }

        try
        {
            var entry = await _l2.GetByIdAsync(key, key, cancellationToken);
            if (entry is null)
            {
                return default;
            }

            _logger.LogDebug("Cache L2 hit for key {Key}", key);

            var deserialized = JsonSerializer.Deserialize<T>(entry.Data, JsonOptions);
            if (deserialized is null)
            {
                return default;
            }

            // Repopulate L1 with the remaining TTL (capped at L2's original TTL)
            var remaining = entry.CreatedAt.AddSeconds(entry.Ttl) - DateTimeOffset.UtcNow;
            if (remaining > TimeSpan.Zero)
            {
                _l1.Set(key, deserialized, remaining);
            }

            return deserialized;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "L2 cache read failed for key {Key}; treating as miss", key);
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, TimeSpan l1Ttl, TimeSpan l2Ttl, CancellationToken cancellationToken = default)
    {
        // ── Write L1 ──────────────────────────────────────────────────────────
        _l1.Set(key, value, l1Ttl);

        // ── Write L2 ──────────────────────────────────────────────────────────
        if (_l2 is null)
        {
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            var entry = new CacheEntry
            {
                CacheKey = key,
                Data = json,
                CreatedAt = DateTimeOffset.UtcNow,
                Ttl = (int)l2Ttl.TotalSeconds,
            };

            // Upsert: try create first, fall back to replace on conflict.
            try
            {
                await _l2.CreateAsync(entry, cancellationToken);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                await _l2.UpdateAsync(entry, entry.CacheKey, entry.CacheKey, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "L2 cache write failed for key {Key}; continuing without L2", key);
        }
    }
}
