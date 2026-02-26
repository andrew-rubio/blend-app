namespace Blend.Api.Domain.Models;

public record RecipeSummary(
    int Id,
    string Title,
    string? Image,
    int? UsedIngredientCount,
    int? MissedIngredientCount,
    double? Likes);
