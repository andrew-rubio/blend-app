namespace Blend.Api.Services.Spoonacular.Models;

/// <summary>
/// Internal domain model representing a brief recipe summary returned by Spoonacular's
/// <c>findByIngredients</c> or <c>complexSearch</c> endpoints.
/// </summary>
public sealed class RecipeSummary
{
    /// <summary>Spoonacular numeric recipe identifier.</summary>
    public int SpoonacularId { get; init; }

    /// <summary>Recipe title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Thumbnail image URL.</summary>
    public string? ImageUrl { get; init; }

    /// <summary>Estimated preparation time in minutes (from complexSearch).</summary>
    public int? ReadyInMinutes { get; init; }

    /// <summary>Number of servings (from complexSearch).</summary>
    public int? Servings { get; init; }

    /// <summary>Cuisine tags (from complexSearch).</summary>
    public IReadOnlyList<string> Cuisines { get; init; } = [];

    /// <summary>Dish-type tags (from complexSearch).</summary>
    public IReadOnlyList<string> DishTypes { get; init; } = [];

    /// <summary>Number of matched ingredients (from findByIngredients).</summary>
    public int? UsedIngredientCount { get; init; }

    /// <summary>Number of missing ingredients (from findByIngredients).</summary>
    public int? MissedIngredientCount { get; init; }

    /// <summary>Community like count (from findByIngredients).</summary>
    public int? Likes { get; init; }
}
