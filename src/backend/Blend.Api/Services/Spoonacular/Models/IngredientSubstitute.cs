namespace Blend.Api.Services.Spoonacular.Models;

/// <summary>
/// Internal domain model for the ingredient substitutes returned by Spoonacular's
/// <c>/food/ingredients/substitutes</c> endpoint.
/// </summary>
public sealed class IngredientSubstitute
{
    /// <summary>The ingredient name for which substitutes were found.</summary>
    public string Ingredient { get; init; } = string.Empty;

    /// <summary>
    /// List of human-readable substitute descriptions, e.g.
    /// "1/2 cup of unsalted butter = 1/2 cup of margarine".
    /// </summary>
    public IReadOnlyList<string> Substitutes { get; init; } = [];

    /// <summary>Optional informational message from Spoonacular.</summary>
    public string? Message { get; init; }
}
