namespace Blend.Api.Services.Spoonacular.Models;

/// <summary>
/// Options for the <c>findByIngredients</c> Spoonacular endpoint.
/// </summary>
public sealed class SearchByIngredientsOptions
{
    /// <summary>Maximum number of results to return (default 10).</summary>
    public int Number { get; init; } = 10;

    /// <summary>
    /// Whether to maximise the number of used ingredients (default true).
    /// When false, results are ranked by fewest missing ingredients.
    /// </summary>
    public bool RankByMaxUsed { get; init; } = true;

    /// <summary>Ignore typical pantry items (salt, water, etc.) when counting missing ingredients.</summary>
    public bool IgnorePantry { get; init; } = true;
}
