using Blend.Api.Services.Spoonacular.Models;

namespace Blend.Api.Services.Spoonacular;

/// <summary>
/// Provides access to Spoonacular recipe data through a cache-aware abstraction.
/// All methods return a <see cref="SpoonacularResult{T}"/> that includes provenance
/// and graceful-degradation flags (PLAT-38, PLAT-41).
/// </summary>
public interface ISpoonacularService
{
    /// <summary>
    /// Searches for recipes by a list of available ingredients using Spoonacular's
    /// <c>/recipes/findByIngredients</c> endpoint.
    /// </summary>
    Task<SpoonacularResult<IReadOnlyList<RecipeSummary>>> SearchByIngredientsAsync(
        IReadOnlyList<string> ingredients,
        SearchByIngredientsOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a full-text and filtered recipe search using Spoonacular's
    /// <c>/recipes/complexSearch</c> endpoint.
    /// </summary>
    Task<SpoonacularResult<IReadOnlyList<RecipeSummary>>> ComplexSearchAsync(
        string query,
        ComplexSearchFilters? filters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves full recipe information for a single recipe using Spoonacular's
    /// <c>/recipes/{id}/information</c> endpoint.
    /// </summary>
    Task<SpoonacularResult<RecipeDetail>> GetRecipeInformationAsync(
        int recipeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves full recipe information for multiple recipes in a single API call
    /// using Spoonacular's <c>/recipes/informationBulk</c> endpoint.
    /// </summary>
    Task<SpoonacularResult<IReadOnlyList<RecipeDetail>>> GetRecipeBulkInformationAsync(
        IReadOnlyList<int> recipeIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves ingredient substitutes using Spoonacular's
    /// <c>/food/ingredients/substitutes</c> endpoint.
    /// </summary>
    Task<SpoonacularResult<IngredientSubstitute>> GetIngredientSubstitutesAsync(
        string ingredientName,
        CancellationToken cancellationToken = default);
}
