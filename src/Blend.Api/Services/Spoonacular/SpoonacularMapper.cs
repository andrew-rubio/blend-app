using Blend.Api.Domain.Models;

namespace Blend.Api.Services.Spoonacular;

/// <summary>
/// Maps raw Spoonacular API DTOs to internal domain types.
/// Handles missing/null fields gracefully.
/// </summary>
internal static class SpoonacularMapper
{
    public static RecipeSummary ToRecipeSummary(SpoonacularRecipeSummaryDto dto) =>
        new(
            Id: dto.Id,
            Title: dto.Title,
            Image: dto.Image,
            UsedIngredientCount: dto.UsedIngredientCount,
            MissedIngredientCount: dto.MissedIngredientCount,
            Likes: dto.Likes);

    public static RecipeDetail ToRecipeDetail(SpoonacularRecipeDetailDto dto) =>
        new(
            Id: dto.Id,
            Title: dto.Title,
            Image: dto.Image,
            ReadyInMinutes: dto.ReadyInMinutes,
            Servings: dto.Servings,
            Summary: dto.Summary,
            Cuisines: dto.Cuisines.AsReadOnly(),
            DishTypes: dto.DishTypes.AsReadOnly(),
            Ingredients: dto.ExtendedIngredients
                .Select(i => new RecipeIngredient(i.Id, i.Name, i.Amount, i.Unit, i.Image))
                .ToList()
                .AsReadOnly(),
            Instructions: dto.AnalyzedInstructions
                .Select(ins => new RecipeInstruction(
                    ins.Name,
                    ins.Steps.Select(s => new RecipeStep(s.Number, s.Step)).ToList().AsReadOnly()))
                .ToList()
                .AsReadOnly(),
            Vegetarian: dto.Vegetarian,
            Vegan: dto.Vegan,
            GlutenFree: dto.GlutenFree,
            DairyFree: dto.DairyFree);

    public static IngredientSubstitute ToIngredientSubstitute(SpoonacularSubstituteResponse dto) =>
        new(
            IngredientName: dto.Ingredient,
            Substitutes: dto.Substitutes.AsReadOnly(),
            Message: dto.Message);
}
