namespace Blend.Api.Ingredients.Models;

/// <summary>
/// A substitution suggestion for an ingredient (COOK-13 through COOK-15).
/// </summary>
public sealed record SubstituteSuggestion(
    string SubstituteIngredientId,
    string SubstituteIngredientName,
    string? CompatibilityNote);
