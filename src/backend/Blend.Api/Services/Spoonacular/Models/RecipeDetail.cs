namespace Blend.Api.Services.Spoonacular.Models;

/// <summary>
/// A single ingredient in a recipe (used within <see cref="RecipeDetail"/>).
/// </summary>
public sealed class RecipeIngredientInfo
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string OriginalString { get; init; } = string.Empty;
    public double Amount { get; init; }
    public string Unit { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
}

/// <summary>
/// Internal domain model for the full recipe detail returned by Spoonacular's
/// <c>/recipes/{id}/information</c> endpoint.
/// </summary>
public sealed class RecipeDetail
{
    /// <summary>Spoonacular numeric recipe identifier.</summary>
    public int SpoonacularId { get; init; }

    public string Title { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public int ReadyInMinutes { get; init; }
    public int Servings { get; init; }

    /// <summary>HTML summary of the recipe.</summary>
    public string? Summary { get; init; }

    /// <summary>Step-by-step instructions (may be HTML).</summary>
    public string? Instructions { get; init; }

    public IReadOnlyList<RecipeIngredientInfo> Ingredients { get; init; } = [];
    public IReadOnlyList<string> Cuisines { get; init; } = [];
    public IReadOnlyList<string> DishTypes { get; init; } = [];
    public IReadOnlyList<string> Diets { get; init; } = [];

    public string? SourceUrl { get; init; }
    public bool Vegetarian { get; init; }
    public bool Vegan { get; init; }
    public bool GlutenFree { get; init; }
    public bool DairyFree { get; init; }
}
