namespace Blend.Api.Ingredients.Models;

/// <summary>
/// A single autocomplete suggestion returned by ingredient search (COOK-06, COOK-07).
/// </summary>
public sealed record IngredientSearchResult(
    string IngredientId,
    string Name,
    string? Category,
    string? FlavourProfile);
