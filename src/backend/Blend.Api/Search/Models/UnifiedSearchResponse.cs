namespace Blend.Api.Search.Models;

/// <summary>
/// Indicates the origin of a unified search result (EXPL-13).
/// </summary>
public enum RecipeDataSource
{
    /// <summary>The recipe comes from the Spoonacular external API.</summary>
    Spoonacular,

    /// <summary>The recipe was created by the community and stored internally.</summary>
    Community,
}

/// <summary>
/// A single recipe result returned by the unified search endpoint.
/// </summary>
public sealed class UnifiedRecipeResult
{
    /// <summary>Recipe identifier (Spoonacular numeric ID as string, or internal GUID).</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Recipe title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Short description or summary.</summary>
    public string? Description { get; init; }

    /// <summary>Thumbnail or featured image URL.</summary>
    public string? ImageUrl { get; init; }

    /// <summary>Total ready time in minutes (prep + cook).</summary>
    public int? ReadyInMinutes { get; init; }

    /// <summary>Number of servings.</summary>
    public int? Servings { get; init; }

    /// <summary>Cuisine tags (e.g. "Italian", "Mexican").</summary>
    public IReadOnlyList<string> Cuisines { get; init; } = [];

    /// <summary>Dish type tags (e.g. "main course", "dessert").</summary>
    public IReadOnlyList<string> DishTypes { get; init; } = [];

    /// <summary>Number of likes or community popularity metric.</summary>
    public int Popularity { get; init; }

    /// <summary>Where this result originated — <c>spoonacular</c> or <c>community</c> (EXPL-13).</summary>
    public RecipeDataSource DataSource { get; init; }

    /// <summary>When the recipe was created (internal recipes only).</summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>Ranking score used internally for result ordering.</summary>
    public double Score { get; init; }
}

/// <summary>
/// Metadata about the unified search operation.
/// </summary>
public sealed class SearchResponseMetadata
{
    /// <summary>Total number of results across all sources (approximate).</summary>
    public int TotalResults { get; init; }

    /// <summary>Whether the Spoonacular API quota was exhausted and only community results are returned.</summary>
    public bool QuotaExhausted { get; init; }

    /// <summary>
    /// True when the search is operating in degraded mode because Spoonacular is unavailable
    /// (quota exhausted, service down, or not configured). Only community results are returned.
    /// Per PLAT-12 through PLAT-14.
    /// </summary>
    public bool DegradedMode { get; init; }

    /// <summary>Opaque cursor to retrieve the next page, or <c>null</c> if this is the last page.</summary>
    public string? NextCursor { get; init; }
}

/// <summary>
/// Response envelope returned by <c>GET /api/v1/search/recipes</c>.
/// </summary>
public sealed class UnifiedSearchResponse
{
    /// <summary>The list of recipe results for this page.</summary>
    public IReadOnlyList<UnifiedRecipeResult> Results { get; init; } = [];

    /// <summary>Metadata about the search operation.</summary>
    public SearchResponseMetadata Metadata { get; init; } = new();
}
