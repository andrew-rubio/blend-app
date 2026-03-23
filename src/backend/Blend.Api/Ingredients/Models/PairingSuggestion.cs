namespace Blend.Api.Ingredients.Models;

/// <summary>
/// A scored pairing suggestion between two ingredients (COOK-08 through COOK-10).
/// </summary>
public sealed record PairingSuggestion(
    string PairedIngredientId,
    string PairedIngredientName,
    double Score,
    string? Category,
    string SourceType);
