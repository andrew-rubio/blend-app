namespace Blend.Api.Domain.Models;

public record IngredientSubstitute(
    string IngredientName,
    IReadOnlyList<string> Substitutes,
    string? Message);
