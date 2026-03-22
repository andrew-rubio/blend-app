using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Blend.Api.Services.Spoonacular.Models;

namespace Blend.Api.Services.Spoonacular;

/// <summary>
/// Builds canonical, normalised Spoonacular cache keys (per ADR 0009).
/// <list type="bullet">
///   <item><description>Search results: <c>spoon:search:{sha256-hash}</c></description></item>
///   <item><description>Recipe detail: <c>spoon:recipe:{spoonacularId}</c></description></item>
///   <item><description>Ingredient substitutes: <c>spoon:substitute:{ingredientName}</c></description></item>
/// </list>
/// Normalisation (lowercase, sorted parameters) ensures that equivalent queries share the
/// same cache entry.
/// </summary>
public static class SpoonacularCacheKeys
{
    /// <summary>Builds the cache key for a <c>findByIngredients</c> search.</summary>
    public static string ForSearchByIngredients(
        IReadOnlyList<string> ingredients,
        SearchByIngredientsOptions? options)
    {
        var parts = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["ingredients"] = string.Join(",", ingredients
                .Select(i => i.Trim().ToLowerInvariant())
                .Order()),
            ["number"] = (options?.Number ?? 10).ToString(),
            ["rankByMaxUsed"] = (options?.RankByMaxUsed ?? true).ToString(),
            ["ignorePantry"] = (options?.IgnorePantry ?? true).ToString(),
        };

        return BuildSearchKey(parts);
    }

    /// <summary>Builds the cache key for a <c>complexSearch</c> query.</summary>
    public static string ForComplexSearch(string query, ComplexSearchFilters? filters)
    {
        var parts = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["q"] = query.Trim().ToLowerInvariant(),
            ["number"] = (filters?.Number ?? 10).ToString(),
        };

        if (!string.IsNullOrWhiteSpace(filters?.Cuisine))
        {
            parts["cuisine"] = filters.Cuisine.ToLowerInvariant();
        }

        if (!string.IsNullOrWhiteSpace(filters?.Diet))
        {
            parts["diet"] = filters.Diet.ToLowerInvariant();
        }

        if (!string.IsNullOrWhiteSpace(filters?.Intolerances))
        {
            parts["intolerances"] = NormaliseList(filters.Intolerances);
        }

        if (filters?.MaxReadyTime is not null)
        {
            parts["maxReadyTime"] = filters.MaxReadyTime.Value.ToString();
        }

        return BuildSearchKey(parts);
    }

    /// <summary>Builds the cache key for a single recipe detail.</summary>
    public static string ForRecipe(int recipeId) =>
        $"spoon:recipe:{recipeId}";

    /// <summary>Builds the cache key for a bulk recipe detail request.</summary>
    public static string ForRecipeBulk(IReadOnlyList<int> recipeIds)
    {
        var sorted = string.Join(",", recipeIds.Order());
        return $"spoon:recipe:bulk:{Hash(sorted)}";
    }

    /// <summary>Builds the cache key for ingredient substitutes.</summary>
    public static string ForSubstitute(string ingredientName) =>
        $"spoon:substitute:{ingredientName.Trim().ToLowerInvariant()}";

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string BuildSearchKey(SortedDictionary<string, string> parts)
    {
        // Serialise to a deterministic JSON string for hashing
        var canonical = JsonSerializer.Serialize(parts);
        return $"spoon:search:{Hash(canonical)}";
    }

    private static string NormaliseList(string csv) =>
        string.Join(",", csv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => s.ToLowerInvariant())
            .Order());

    internal static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes)[..16]; // 16 hex chars (64-bit) is sufficient
    }
}
