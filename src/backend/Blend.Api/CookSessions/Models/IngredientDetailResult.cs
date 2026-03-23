namespace Blend.Api.CookSessions.Models;

/// <summary>
/// Detailed ingredient information returned in the context of a cooking session,
/// including flavour profile, substitutes, and a contextual pairing explanation
/// (COOK-13 through COOK-15).
/// </summary>
public sealed class IngredientDetailResult
{
    /// <summary>The ingredient's unique identifier.</summary>
    public string IngredientId { get; init; } = string.Empty;

    /// <summary>Display name of the ingredient.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Broad category (e.g., "vegetable", "protein").</summary>
    public string? Category { get; init; }

    /// <summary>Flavour profile descriptor (e.g., "sweet", "umami").</summary>
    public string? FlavourProfile { get; init; }

    /// <summary>List of substitute ingredient names/IDs.</summary>
    public IReadOnlyList<string> Substitutes { get; init; } = [];

    /// <summary>
    /// A natural-language explanation of why this ingredient pairs well with
    /// the other ingredients currently in the session.
    /// </summary>
    public string? WhyItPairs { get; init; }

    /// <summary>Basic nutritional summary for this ingredient.</summary>
    public string? NutritionSummary { get; init; }
}
