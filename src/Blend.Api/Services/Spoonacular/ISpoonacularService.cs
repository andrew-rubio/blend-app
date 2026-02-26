using Blend.Api.Domain.Models;

namespace Blend.Api.Services.Spoonacular;

/// <summary>
/// Service for querying the Spoonacular API.
/// All methods are cache-aware and fail gracefully.
/// </summary>
public interface ISpoonacularService
{
    /// <summary>
    /// Search recipes by available ingredients (maps to Spoonacular findByIngredients).
    /// Cache key: spoon:search:{hash}
    /// </summary>
    Task<SpoonacularResult<IReadOnlyList<RecipeSummary>>> SearchByIngredientsAsync(
        IEnumerable<string> ingredients,
        SearchByIngredientsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Full-text / filtered recipe search (maps to Spoonacular complexSearch).
    /// Cache key: spoon:search:{hash}
    /// </summary>
    Task<SpoonacularResult<IReadOnlyList<RecipeSummary>>> ComplexSearchAsync(
        ComplexSearchOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve full recipe details (maps to Spoonacular /recipes/{id}/information).
    /// Cache key: spoon:recipe:{id}
    /// </summary>
    Task<SpoonacularResult<RecipeDetail?>> GetRecipeInformationAsync(
        int recipeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch-retrieve recipe details (maps to Spoonacular /recipes/informationBulk).
    /// Cache key: spoon:recipe:{id} per recipe.
    /// </summary>
    Task<SpoonacularResult<IReadOnlyList<RecipeDetail>>> GetRecipeBulkInformationAsync(
        IEnumerable<int> recipeIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve ingredient substitutes (maps to Spoonacular /food/ingredients/{name}/substitutes).
    /// Cache key: spoon:substitute:{name}
    /// </summary>
    Task<SpoonacularResult<IngredientSubstitute?>> GetIngredientSubstitutesAsync(
        string ingredientName,
        CancellationToken cancellationToken = default);

    /// <summary>Current quota usage as a fraction (0–1). Updated on each API response.</summary>
    double CurrentQuotaUsageFraction { get; }
}
