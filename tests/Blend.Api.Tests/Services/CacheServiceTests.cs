using Blend.Api.Services.Cache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Blend.Api.Tests.Services;

public class CacheServiceTests : IDisposable
{
    private readonly IMemoryCache _memoryCache;
    private readonly CacheService _sut;

    public CacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        var opts = Options.Create(new CacheOptions());
        // Build a minimal service provider with no CosmosClient (L2 silently skipped)
        var sp = new ServiceCollection().BuildServiceProvider();
        _sut = new CacheService(_memoryCache, opts, NullLogger<CacheService>.Instance, sp);
    }

    [Fact]
    public async Task SetAsync_ThenGetAsync_ReturnsL1Hit()
    {
        await _sut.SetAsync("test-key", "hello", TimeSpan.FromMinutes(10), TimeSpan.FromHours(1));

        var (value, tier) = await _sut.GetAsync<string>("test-key");

        Assert.Equal("hello", value);
        Assert.Equal("l1-cache", tier);
    }

    [Fact]
    public async Task GetAsync_NoEntry_ReturnsMiss()
    {
        var (value, tier) = await _sut.GetAsync<string>("nonexistent");

        Assert.Null(value);
        Assert.Null(tier);
    }

    [Fact]
    public async Task RemoveAsync_ClearsL1Entry()
    {
        await _sut.SetAsync("remove-key", 42, TimeSpan.FromMinutes(10), TimeSpan.FromHours(1));

        await _sut.RemoveAsync("remove-key");

        var (value, _) = await _sut.GetAsync<int?>("remove-key");
        Assert.Null(value);
    }

    [Fact]
    public async Task SetAsync_WithExpiredL1Ttl_MissesL1()
    {
        // TTL of 1 tick — effectively expired immediately
        await _sut.SetAsync("expire-key", "soon-gone", TimeSpan.FromTicks(1), TimeSpan.FromHours(1));

        // Small delay to ensure expiry
        await Task.Delay(50);

        var (value, tier) = await _sut.GetAsync<string>("expire-key");

        // Without L2, it should be a miss
        Assert.Null(value);
        Assert.Null(tier);
    }

    [Fact]
    public async Task SetAsync_ComplexObject_RoundTripsCorrectly()
    {
        var obj = new TestData { Name = "test", Value = 99 };
        await _sut.SetAsync("complex-key", obj, TimeSpan.FromMinutes(5), TimeSpan.FromHours(1));

        var (result, tier) = await _sut.GetAsync<TestData>("complex-key");

        Assert.NotNull(result);
        Assert.Equal("test", result.Name);
        Assert.Equal(99, result.Value);
        Assert.Equal("l1-cache", tier);
    }

    [Fact]
    public async Task SetAsync_DifferentKeys_ReturnDifferentValues()
    {
        await _sut.SetAsync("key1", "value1", TimeSpan.FromMinutes(5), TimeSpan.FromHours(1));
        await _sut.SetAsync("key2", "value2", TimeSpan.FromMinutes(5), TimeSpan.FromHours(1));

        var (v1, _) = await _sut.GetAsync<string>("key1");
        var (v2, _) = await _sut.GetAsync<string>("key2");

        Assert.Equal("value1", v1);
        Assert.Equal("value2", v2);
    }

    public void Dispose() => _memoryCache.Dispose();

    private sealed record TestData { public string Name { get; init; } = ""; public int Value { get; init; } }
}
