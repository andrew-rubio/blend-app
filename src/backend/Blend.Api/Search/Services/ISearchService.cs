using Blend.Api.Search.Models;
using Blend.Domain.Entities;

namespace Blend.Api.Search.Services;

/// <summary>
/// Provides unified recipe search, recently-viewed tracking, and related operations.
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Performs a unified recipe search by querying both Spoonacular and the internal recipe store
    /// in parallel, merging and ranking the results (EXPL-08 through EXPL-19).
    /// </summary>
    /// <param name="request">The search parameters.</param>
    /// <param name="userId">The authenticated user's ID, or <c>null</c> for anonymous requests.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="UnifiedSearchResponse"/> containing ranked results and metadata.</returns>
    Task<UnifiedSearchResponse> SearchRecipesAsync(
        SearchRecipesRequest request,
        string? userId,
        CancellationToken ct = default);

    /// <summary>
    /// Records that the given user viewed the specified recipe (HOME-23).
    /// </summary>
    /// <param name="recipeId">The recipe identifier.</param>
    /// <param name="dataSource">Whether the recipe is from Spoonacular or the community.</param>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RecordViewAsync(
        string recipeId,
        RecipeDataSource dataSource,
        string userId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the most recently viewed recipes for the given user (HOME-23, HOME-24).
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="pageSize">Maximum number of results to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of recently-viewed activity records ordered by most recent first.</returns>
    Task<IReadOnlyList<Activity>> GetRecentlyViewedAsync(
        string userId,
        int pageSize,
        CancellationToken ct = default);
}
