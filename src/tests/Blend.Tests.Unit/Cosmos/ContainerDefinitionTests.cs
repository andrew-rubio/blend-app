using Blend.Infrastructure.Cosmos.Configuration;
using Xunit;

namespace Blend.Tests.Unit.Cosmos;

/// <summary>
/// Unit tests for container definitions and partition key resolution.
/// </summary>
public class ContainerDefinitionTests
{
    [Fact]
    public void ContainerDefinitions_All_Contains8Containers()
    {
        Assert.Equal(8, ContainerDefinitions.All.Count);
    }

    [Theory]
    [InlineData("users",              "/id")]
    [InlineData("recipes",            "/authorId")]
    [InlineData("connections",        "/userId")]
    [InlineData("activity",           "/userId")]
    [InlineData("content",            "/contentType")]
    [InlineData("notifications",      "/recipientUserId")]
    [InlineData("cache",              "/cacheKey")]
    [InlineData("ingredientPairings", "/ingredientId")]
    public void ContainerDefinitions_PartitionKeys_AreCorrect(string containerName, string expectedPartitionKey)
    {
        var def = ContainerDefinitions.All.Single(d => d.Name == containerName);
        Assert.Equal(expectedPartitionKey, def.PartitionKeyPath);
    }

    [Theory]
    [InlineData("notifications", true)]
    [InlineData("cache",         true)]
    public void ContainerDefinitions_TtlContainers_HaveTtlSet(string containerName, bool expectTtl)
    {
        var def = ContainerDefinitions.All.Single(d => d.Name == containerName);
        Assert.Equal(expectTtl, def.TtlSeconds.HasValue);
    }

    [Theory]
    [InlineData("users")]
    [InlineData("recipes")]
    [InlineData("connections")]
    [InlineData("activity")]
    [InlineData("content")]
    [InlineData("ingredientPairings")]
    public void ContainerDefinitions_NonTtlContainers_HaveNullTtl(string containerName)
    {
        var def = ContainerDefinitions.All.Single(d => d.Name == containerName);
        Assert.Null(def.TtlSeconds);
    }

    [Fact]
    public void ContainerDefinitions_AllNames_AreUnique()
    {
        var names = ContainerDefinitions.All.Select(d => d.Name).ToList();
        Assert.Equal(names.Count, names.Distinct().Count());
    }

    [Fact]
    public void ContainerDefinitions_NotificationsTtl_Is90Days()
    {
        var def = ContainerDefinitions.All.Single(d => d.Name == "notifications");
        Assert.Equal(7776000, def.TtlSeconds); // 90 * 24 * 60 * 60
    }

    [Fact]
    public void ContainerDefinitions_CacheTtl_Is24Hours()
    {
        var def = ContainerDefinitions.All.Single(d => d.Name == "cache");
        Assert.Equal(86400, def.TtlSeconds); // 24 * 60 * 60
    }

    [Fact]
    public void CosmosOptions_DefaultValues_AreCorrect()
    {
        var opts = new CosmosOptions();
        Assert.Equal("blend", opts.DatabaseName);
        Assert.True(opts.EnsureCreated);
        Assert.Null(opts.ProvisionedThroughput);
        Assert.Equal(9, opts.MaxRetryAttemptsOnRateLimitedRequests);
        Assert.Equal(30, opts.MaxRetryWaitTimeInSeconds);
        Assert.Equal(60, opts.RequestTimeoutInSeconds);
    }
}
