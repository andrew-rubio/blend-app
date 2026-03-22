using Blend.Api.Services.Cache;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Blend.Tests.Unit.Cache;

/// <summary>Unit tests for <see cref="CacheService"/>.</summary>
public class CacheServiceTests : IDisposable
{
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<IRepository<CacheEntry>> _l2Mock;
    private readonly CacheService _sut;

    public CacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _l2Mock = new Mock<IRepository<CacheEntry>>();
        _sut = new CacheService(_memoryCache, NullLogger<CacheService>.Instance, _l2Mock.Object);
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
    }

    // ── L1 hit ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_WhenValueInL1_ReturnsValueWithoutCallingL2()
    {
        _memoryCache.Set("key1", "hello");

        var result = await _sut.GetAsync<string>("key1");

        Assert.Equal("hello", result);
        _l2Mock.Verify(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── L2 hit ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_WhenMissInL1_ChecksL2()
    {
        var entry = new CacheEntry
        {
            CacheKey = "key2",
            Data = """{"value":"world"}""",
            CreatedAt = DateTimeOffset.UtcNow,
            Ttl = 3600,
        };
        _l2Mock.Setup(r => r.GetByIdAsync("key2", "key2", It.IsAny<CancellationToken>()))
               .ReturnsAsync(entry);

        var result = await _sut.GetAsync<Dictionary<string, string>>("key2");

        Assert.NotNull(result);
        Assert.Equal("world", result["value"]);
    }

    [Fact]
    public async Task GetAsync_WhenL2Hit_PopulatesL1ForSubsequentCalls()
    {
        var entry = new CacheEntry
        {
            CacheKey = "key3",
            Data = "\"cached-value\"",
            CreatedAt = DateTimeOffset.UtcNow,
            Ttl = 3600,
        };
        _l2Mock.Setup(r => r.GetByIdAsync("key3", "key3", It.IsAny<CancellationToken>()))
               .ReturnsAsync(entry);

        await _sut.GetAsync<string>("key3");

        // Second call should hit L1
        _l2Mock.Invocations.Clear();
        var result = await _sut.GetAsync<string>("key3");

        Assert.Equal("cached-value", result);
        _l2Mock.Verify(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Cache miss ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_WhenMissInBothTiers_ReturnsDefault()
    {
        _l2Mock.Setup(r => r.GetByIdAsync("miss-key", "miss-key", It.IsAny<CancellationToken>()))
               .ReturnsAsync((CacheEntry?)null);

        var result = await _sut.GetAsync<string>("miss-key");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WhenNoCacheEntryInL2_ReturnsDefault()
    {
        _l2Mock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((CacheEntry?)null);

        var result = await _sut.GetAsync<int?>("any-key");

        Assert.Null(result);
    }

    // ── Write-through ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SetAsync_WritesToBothL1AndL2()
    {
        _l2Mock.Setup(r => r.CreateAsync(It.IsAny<CacheEntry>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((CacheEntry e, CancellationToken _) => e);

        await _sut.SetAsync("write-key", "test-value", TimeSpan.FromMinutes(5), TimeSpan.FromHours(1));

        // L1 should be populated
        Assert.True(_memoryCache.TryGetValue("write-key", out string? l1Val));
        Assert.Equal("test-value", l1Val);

        // L2 should be called
        _l2Mock.Verify(r => r.CreateAsync(
            It.Is<CacheEntry>(e => e.CacheKey == "write-key"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetAsync_WhenL2ConflictOnCreate_FallsBackToUpdate()
    {
        _l2Mock.Setup(r => r.CreateAsync(It.IsAny<CacheEntry>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new Microsoft.Azure.Cosmos.CosmosException(
                   "Conflict", System.Net.HttpStatusCode.Conflict, 0, "act", 1.0));

        _l2Mock.Setup(r => r.UpdateAsync(It.IsAny<CacheEntry>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((CacheEntry e, string _, string __, CancellationToken ___) => e);

        await _sut.SetAsync("conflict-key", "value", TimeSpan.FromMinutes(1), TimeSpan.FromHours(1));

        _l2Mock.Verify(r => r.UpdateAsync(
            It.IsAny<CacheEntry>(), "conflict-key", "conflict-key", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetAsync_WhenL2Fails_DoesNotThrow()
    {
        _l2Mock.Setup(r => r.CreateAsync(It.IsAny<CacheEntry>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new Exception("Network error"));

        // Should not throw even when L2 fails
        await _sut.SetAsync("fault-key", "value", TimeSpan.FromMinutes(1), TimeSpan.FromHours(1));

        Assert.True(_memoryCache.TryGetValue("fault-key", out _), "L1 should still be populated");
    }

    // ── TTL expiry ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_WhenL1TtlExpires_FallsBackToL2()
    {
        var entry = new CacheEntry
        {
            CacheKey = "ttl-key",
            Data = "\"fresh-from-l2\"",
            CreatedAt = DateTimeOffset.UtcNow,
            Ttl = 3600,
        };

        // Seed with 1ms TTL so it expires immediately
        _memoryCache.Set("ttl-key", "stale", new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(1)
        });

        await Task.Delay(10); // let L1 expire

        _l2Mock.Setup(r => r.GetByIdAsync("ttl-key", "ttl-key", It.IsAny<CancellationToken>()))
               .ReturnsAsync(entry);

        var result = await _sut.GetAsync<string>("ttl-key");

        Assert.Equal("fresh-from-l2", result);
    }

    // ── Without L2 ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_WithoutL2_ReturnsNullOnL1Miss()
    {
        var noL2Service = new CacheService(
            _memoryCache,
            NullLogger<CacheService>.Instance,
            l2: null);

        var result = await noL2Service.GetAsync<string>("no-l2-key");

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_WithoutL2_DoesNotThrow()
    {
        var noL2Service = new CacheService(
            _memoryCache,
            NullLogger<CacheService>.Instance,
            l2: null);

        await noL2Service.SetAsync("no-l2-set", "value", TimeSpan.FromMinutes(1), TimeSpan.FromHours(1));

        Assert.True(_memoryCache.TryGetValue("no-l2-set", out _));
    }

    // ── L2 error resilience ───────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_WhenL2Throws_TreatsAsMiss()
    {
        _l2Mock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new Exception("Cosmos error"));

        var result = await _sut.GetAsync<string>("error-key");

        Assert.Null(result);
    }
}
