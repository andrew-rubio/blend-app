using Blend.Api.Services.Spoonacular;
using Blend.Api.Services.Spoonacular.Models;

namespace Blend.Tests.Unit.Spoonacular;

/// <summary>Unit tests for <see cref="SpoonacularCacheKeys"/> normalisation.</summary>
public class SpoonacularCacheKeysTests
{
    // ── ForRecipe ─────────────────────────────────────────────────────────────

    [Fact]
    public void ForRecipe_ReturnsExpectedKey()
    {
        Assert.Equal("spoon:recipe:42", SpoonacularCacheKeys.ForRecipe(42));
    }

    // ── ForSubstitute ─────────────────────────────────────────────────────────

    [Fact]
    public void ForSubstitute_NormalisesToLowercase()
    {
        var key1 = SpoonacularCacheKeys.ForSubstitute("Butter");
        var key2 = SpoonacularCacheKeys.ForSubstitute("butter");
        Assert.Equal(key1, key2);
    }

    [Fact]
    public void ForSubstitute_TrimsWhitespace()
    {
        var key1 = SpoonacularCacheKeys.ForSubstitute("  butter  ");
        var key2 = SpoonacularCacheKeys.ForSubstitute("butter");
        Assert.Equal(key1, key2);
    }

    // ── ForSearchByIngredients ────────────────────────────────────────────────

    [Fact]
    public void ForSearchByIngredients_SameIngredientsDifferentOrder_ProducesSameKey()
    {
        var key1 = SpoonacularCacheKeys.ForSearchByIngredients(["apple", "banana", "cherry"], null);
        var key2 = SpoonacularCacheKeys.ForSearchByIngredients(["cherry", "apple", "banana"], null);
        Assert.Equal(key1, key2);
    }

    [Fact]
    public void ForSearchByIngredients_DifferentIngredients_ProduceDifferentKeys()
    {
        var key1 = SpoonacularCacheKeys.ForSearchByIngredients(["apple", "banana"], null);
        var key2 = SpoonacularCacheKeys.ForSearchByIngredients(["apple", "mango"], null);
        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void ForSearchByIngredients_IngredientCaseDifference_ProducesSameKey()
    {
        var key1 = SpoonacularCacheKeys.ForSearchByIngredients(["Apple", "Banana"], null);
        var key2 = SpoonacularCacheKeys.ForSearchByIngredients(["apple", "banana"], null);
        Assert.Equal(key1, key2);
    }

    [Fact]
    public void ForSearchByIngredients_DifferentOptions_ProduceDifferentKeys()
    {
        var opts1 = new SearchByIngredientsOptions { Number = 5 };
        var opts2 = new SearchByIngredientsOptions { Number = 10 };
        var key1 = SpoonacularCacheKeys.ForSearchByIngredients(["apple"], opts1);
        var key2 = SpoonacularCacheKeys.ForSearchByIngredients(["apple"], opts2);
        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void ForSearchByIngredients_StartsWithSearchPrefix()
    {
        var key = SpoonacularCacheKeys.ForSearchByIngredients(["apple"], null);
        Assert.StartsWith("spoon:search:", key);
    }

    // ── ForComplexSearch ──────────────────────────────────────────────────────

    [Fact]
    public void ForComplexSearch_SameQueryDifferentCase_ProducesSameKey()
    {
        var key1 = SpoonacularCacheKeys.ForComplexSearch("Pasta", null);
        var key2 = SpoonacularCacheKeys.ForComplexSearch("pasta", null);
        Assert.Equal(key1, key2);
    }

    [Fact]
    public void ForComplexSearch_DifferentCuisine_ProduceDifferentKeys()
    {
        var filters1 = new ComplexSearchFilters { Cuisine = "Italian" };
        var filters2 = new ComplexSearchFilters { Cuisine = "Mexican" };
        var key1 = SpoonacularCacheKeys.ForComplexSearch("pasta", filters1);
        var key2 = SpoonacularCacheKeys.ForComplexSearch("pasta", filters2);
        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void ForComplexSearch_StartsWithSearchPrefix()
    {
        var key = SpoonacularCacheKeys.ForComplexSearch("pizza", null);
        Assert.StartsWith("spoon:search:", key);
    }

    [Fact]
    public void ForComplexSearch_IntoleranceOrdering_ProducesSameKey()
    {
        var f1 = new ComplexSearchFilters { Intolerances = "gluten,dairy" };
        var f2 = new ComplexSearchFilters { Intolerances = "dairy,gluten" };
        var key1 = SpoonacularCacheKeys.ForComplexSearch("soup", f1);
        var key2 = SpoonacularCacheKeys.ForComplexSearch("soup", f2);
        Assert.Equal(key1, key2);
    }

    // ── ForRecipeBulk ─────────────────────────────────────────────────────────

    [Fact]
    public void ForRecipeBulk_SameIdsDifferentOrder_ProducesSameKey()
    {
        var key1 = SpoonacularCacheKeys.ForRecipeBulk([1, 2, 3]);
        var key2 = SpoonacularCacheKeys.ForRecipeBulk([3, 1, 2]);
        Assert.Equal(key1, key2);
    }

    [Fact]
    public void ForRecipeBulk_DifferentIds_ProduceDifferentKeys()
    {
        var key1 = SpoonacularCacheKeys.ForRecipeBulk([1, 2, 3]);
        var key2 = SpoonacularCacheKeys.ForRecipeBulk([1, 2, 4]);
        Assert.NotEqual(key1, key2);
    }
}
