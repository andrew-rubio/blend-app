using Blend.Domain.Entities;
using Blend.Domain.Interfaces;
using Blend.Infrastructure.Cosmos.Configuration;
using Xunit;

namespace Blend.Tests.Unit;

/// <summary>
/// Unit tests for container definitions and partition key configuration.
/// </summary>
public class ContainerDefinitionTests
{
    private readonly CosmosOptions _options = new();

    [Fact]
    public void ContainerDefinitions_All_Returns8Containers()
    {
        var defs = ContainerDefinitions.All(_options).ToList();
        Assert.Equal(8, defs.Count);
    }

    [Fact]
    public void Users_Container_HasCorrectPartitionKey()
    {
        var def = ContainerDefinitions.Users(_options);
        Assert.Equal("/id", def.PartitionKeyPath);
        Assert.Null(def.DefaultTtlSeconds);
    }

    [Fact]
    public void Recipes_Container_HasCorrectPartitionKey()
    {
        var def = ContainerDefinitions.Recipes(_options);
        Assert.Equal("/authorId", def.PartitionKeyPath);
        Assert.Null(def.DefaultTtlSeconds);
    }

    [Fact]
    public void Connections_Container_HasCorrectPartitionKey()
    {
        var def = ContainerDefinitions.Connections(_options);
        Assert.Equal("/userId", def.PartitionKeyPath);
        Assert.Null(def.DefaultTtlSeconds);
    }

    [Fact]
    public void Activity_Container_HasCorrectPartitionKey()
    {
        var def = ContainerDefinitions.Activity(_options);
        Assert.Equal("/userId", def.PartitionKeyPath);
        Assert.Null(def.DefaultTtlSeconds);
    }

    [Fact]
    public void Content_Container_HasCorrectPartitionKey()
    {
        var def = ContainerDefinitions.Content(_options);
        Assert.Equal("/contentType", def.PartitionKeyPath);
        Assert.Null(def.DefaultTtlSeconds);
    }

    [Fact]
    public void Notifications_Container_HasTtlConfigured()
    {
        var def = ContainerDefinitions.Notifications(_options);
        Assert.Equal("/recipientUserId", def.PartitionKeyPath);
        Assert.NotNull(def.DefaultTtlSeconds);
        Assert.True(def.DefaultTtlSeconds > 0);
    }

    [Fact]
    public void Cache_Container_HasTtlConfigured()
    {
        var def = ContainerDefinitions.Cache(_options);
        Assert.Equal("/cacheKey", def.PartitionKeyPath);
        Assert.NotNull(def.DefaultTtlSeconds);
        Assert.True(def.DefaultTtlSeconds > 0);
    }

    [Fact]
    public void IngredientPairings_Container_HasCorrectPartitionKey()
    {
        var def = ContainerDefinitions.IngredientPairings(_options);
        Assert.Equal("/ingredientId", def.PartitionKeyPath);
        Assert.Null(def.DefaultTtlSeconds);
    }

    [Fact]
    public void Notifications_DefaultTtl_Is30Days()
    {
        var def = ContainerDefinitions.Notifications(_options);
        Assert.Equal(2_592_000, def.DefaultTtlSeconds);
    }

    [Fact]
    public void Cache_DefaultTtl_Is24Hours()
    {
        var def = ContainerDefinitions.Cache(_options);
        Assert.Equal(86_400, def.DefaultTtlSeconds);
    }

    [Theory]
    [InlineData("users")]
    [InlineData("recipes")]
    [InlineData("connections")]
    [InlineData("activity")]
    [InlineData("content")]
    [InlineData("notifications")]
    [InlineData("cache")]
    [InlineData("ingredientPairings")]
    public void AllContainerNames_ArePresent(string expectedName)
    {
        var names = ContainerDefinitions.All(_options).Select(d => d.Name).ToList();
        Assert.Contains(expectedName, names);
    }
}
