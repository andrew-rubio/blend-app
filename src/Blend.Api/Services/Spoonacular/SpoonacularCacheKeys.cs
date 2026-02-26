using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Blend.Api.Domain.Models;

namespace Blend.Api.Services.Spoonacular;

/// <summary>
/// Generates normalised, deterministic cache keys for Spoonacular queries.
/// Equivalent queries always produce the same key regardless of parameter ordering.
/// </summary>
internal static class SpoonacularCacheKeys
{
    /// <summary>
    /// Cache key for a SearchByIngredients query.
    /// Normalises ingredient names to lowercase, sorts them, hashes the result.
    /// </summary>
    public static string ForIngredientSearch(IEnumerable<string> ingredients, SearchByIngredientsOptions? options)
    {
        var sorted = ingredients
            .Select(i => i.Trim().ToLowerInvariant())
            .Order()
            .ToList();

        var payload = new
        {
            ingredients = sorted,
            number = options?.Number,
            ranking = options?.Ranking,
            ignorePantry = options?.IgnorePantry
        };

        return $"spoon:search:{Hash(payload)}";
    }

    /// <summary>
    /// Cache key for a ComplexSearch query.
    /// Normalises all string parameters to lowercase.
    /// </summary>
    public static string ForComplexSearch(ComplexSearchOptions options)
    {
        var payload = new
        {
            query = options.Query?.Trim().ToLowerInvariant(),
            cuisine = options.Cuisine?.Trim().ToLowerInvariant(),
            diet = options.Diet?.Trim().ToLowerInvariant(),
            intolerances = options.Intolerances?.Trim().ToLowerInvariant(),
            maxReadyTime = options.MaxReadyTime,
            number = options.Number,
            offset = options.Offset
        };

        return $"spoon:search:{Hash(payload)}";
    }

    /// <summary>Cache key for a single recipe detail.</summary>
    public static string ForRecipe(int recipeId) => $"spoon:recipe:{recipeId}";

    /// <summary>Cache key for ingredient substitutes.</summary>
    public static string ForSubstitute(string ingredientName) =>
        $"spoon:substitute:{ingredientName.Trim().ToLowerInvariant()}";

    private static string Hash(object payload)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = false });
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes).ToLowerInvariant()[..16];
    }
}
