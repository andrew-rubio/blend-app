namespace Blend.Api.Search.Models;

/// <summary>
/// Query parameters for the unified recipe search endpoint (EXPL-08).
/// </summary>
public sealed class SearchRecipesRequest
{
    /// <summary>Free-text search query (recipe name, ingredients, keywords).</summary>
    public string? Q { get; init; }

    /// <summary>Comma-separated cuisine filter (e.g. "Italian,Mexican").</summary>
    public string? Cuisines { get; init; }

    /// <summary>Comma-separated diet filter (e.g. "vegetarian,vegan").</summary>
    public string? Diets { get; init; }

    /// <summary>Comma-separated dish type filter (e.g. "main course,dessert").</summary>
    public string? DishTypes { get; init; }

    /// <summary>Maximum prep + cook time in minutes.</summary>
    public int? MaxReadyTime { get; init; }

    /// <summary>Sort order: <c>relevance</c> (default), <c>popularity</c>, <c>time</c>, <c>newest</c>.</summary>
    public string Sort { get; init; } = "relevance";

    /// <summary>Opaque pagination cursor from a previous response.</summary>
    public string? Cursor { get; init; }

    /// <summary>Items per page. Default 20, max 50.</summary>
    public int PageSize { get; init; } = 20;
}
