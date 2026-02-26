using Blend.Api.Domain.Models;
using Blend.Api.Services.Spoonacular;
using Xunit;

namespace Blend.Api.Tests.Services;

public class SpoonacularCacheKeyTests
{
    [Fact]
    public void ForIngredientSearch_SameIngredientsDifferentOrder_ProduceSameKey()
    {
        var key1 = SpoonacularCacheKeys.ForIngredientSearch(
            ["chicken", "garlic", "onion"], null);
        var key2 = SpoonacularCacheKeys.ForIngredientSearch(
            ["onion", "chicken", "garlic"], null);

        Assert.Equal(key1, key2);
    }

    [Fact]
    public void ForIngredientSearch_DifferentIngredients_ProduceDifferentKeys()
    {
        var key1 = SpoonacularCacheKeys.ForIngredientSearch(["chicken"], null);
        var key2 = SpoonacularCacheKeys.ForIngredientSearch(["beef"], null);

        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void ForIngredientSearch_CaseInsensitive()
    {
        var key1 = SpoonacularCacheKeys.ForIngredientSearch(["Chicken", "Garlic"], null);
        var key2 = SpoonacularCacheKeys.ForIngredientSearch(["chicken", "garlic"], null);

        Assert.Equal(key1, key2);
    }

    [Fact]
    public void ForIngredientSearch_HasCorrectPrefix()
    {
        var key = SpoonacularCacheKeys.ForIngredientSearch(["chicken"], null);
        Assert.StartsWith("spoon:search:", key);
    }

    [Fact]
    public void ForComplexSearch_SameOptions_ProduceSameKey()
    {
        var opts = new ComplexSearchOptions { Query = "Pasta", Cuisine = "Italian" };
        var key1 = SpoonacularCacheKeys.ForComplexSearch(opts);
        var key2 = SpoonacularCacheKeys.ForComplexSearch(opts);

        Assert.Equal(key1, key2);
    }

    [Fact]
    public void ForComplexSearch_CaseInsensitive()
    {
        var key1 = SpoonacularCacheKeys.ForComplexSearch(new ComplexSearchOptions { Query = "PASTA" });
        var key2 = SpoonacularCacheKeys.ForComplexSearch(new ComplexSearchOptions { Query = "pasta" });

        Assert.Equal(key1, key2);
    }

    [Fact]
    public void ForComplexSearch_DifferentQueries_ProduceDifferentKeys()
    {
        var key1 = SpoonacularCacheKeys.ForComplexSearch(new ComplexSearchOptions { Query = "pasta" });
        var key2 = SpoonacularCacheKeys.ForComplexSearch(new ComplexSearchOptions { Query = "pizza" });

        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void ForRecipe_FormatsCorrectly()
    {
        var key = SpoonacularCacheKeys.ForRecipe(12345);
        Assert.Equal("spoon:recipe:12345", key);
    }

    [Fact]
    public void ForSubstitute_NormalisesToLowercase()
    {
        var key1 = SpoonacularCacheKeys.ForSubstitute("Butter");
        var key2 = SpoonacularCacheKeys.ForSubstitute("butter");
        Assert.Equal(key1, key2);
        Assert.Equal("spoon:substitute:butter", key1);
    }

    [Fact]
    public void ForSubstitute_TrimsWhitespace()
    {
        var key1 = SpoonacularCacheKeys.ForSubstitute("  butter  ");
        var key2 = SpoonacularCacheKeys.ForSubstitute("butter");
        Assert.Equal(key1, key2);
    }
}
