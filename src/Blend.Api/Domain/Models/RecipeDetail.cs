namespace Blend.Api.Domain.Models;

public record RecipeIngredient(
    int Id,
    string Name,
    double Amount,
    string Unit,
    string? Image);

public record RecipeInstruction(
    string Name,
    IReadOnlyList<RecipeStep> Steps);

public record RecipeStep(
    int Number,
    string Step);

public record RecipeDetail(
    int Id,
    string Title,
    string? Image,
    int? ReadyInMinutes,
    int? Servings,
    string? Summary,
    IReadOnlyList<string> Cuisines,
    IReadOnlyList<string> DishTypes,
    IReadOnlyList<RecipeIngredient> Ingredients,
    IReadOnlyList<RecipeInstruction> Instructions,
    bool? Vegetarian,
    bool? Vegan,
    bool? GlutenFree,
    bool? DairyFree);
