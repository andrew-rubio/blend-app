using Blend.Api.Ingredients.Models;

namespace Blend.Api.Ingredients.Services;

/// <summary>
/// Provides ingredient search, detail lookup, pairing scores, and substitution suggestions
/// backed by Azure AI Search and Cosmos DB (per ADR 0005, COOK-06 through COOK-15, PLAT-50 through PLAT-52).
/// </summary>
public interface IKnowledgeBaseService
{
    /// <summary>
    /// Searches for ingredients matching the given partial query using the Azure AI Search
    /// suggest API (COOK-06, COOK-07).
    /// </summary>
    /// <param name="query">Partial ingredient name to match.</param>
    /// <param name="limit">Maximum number of results to return (default 10).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Ranked list of matching ingredient suggestions.</returns>
    Task<IReadOnlyList<IngredientSearchResult>> SearchIngredientsAsync(
        string query,
        int limit = 10,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves full details for a single ingredient by ID (COOK-13 through COOK-15).
    /// </summary>
    /// <param name="id">The ingredient identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The ingredient document, or <c>null</c> if not found.</returns>
    Task<IngredientDocument?> GetIngredientAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Returns pairing suggestions for the given ingredient, optionally filtered by category
    /// and sorted by score descending (COOK-08 through COOK-10).
    /// </summary>
    /// <param name="ingredientId">The primary ingredient identifier.</param>
    /// <param name="category">Optional category filter (e.g., "vegetable").</param>
    /// <param name="limit">Maximum results to return (default 20).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<PairingSuggestion>> GetPairingsAsync(
        string ingredientId,
        string? category = null,
        int limit = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Returns substitution suggestions for the given ingredient.
    /// </summary>
    /// <param name="ingredientId">The ingredient to find substitutes for.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<SubstituteSuggestion>> GetSubstitutesAsync(
        string ingredientId,
        CancellationToken ct = default);

    /// <summary>
    /// Checks whether the Knowledge Base is available (circuit breaker pattern — PLAT-51).
    /// Returns <c>false</c> when the circuit is open (3 consecutive failures within the last 60 seconds).
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Aggregates a new user rating into the community pairing score for the given ingredient pair (COOK-52 through COOK-55).
    /// The rating is normalised from [1,5] to [0,1].
    /// Updates both directions of the pair (A→B and B→A).
    /// Creates the pairing document if it does not yet exist.
    /// </summary>
    /// <param name="ingredientId1">First ingredient in the pair.</param>
    /// <param name="ingredientId2">Second ingredient in the pair.</param>
    /// <param name="normalizedRating">Rating already normalised to [0,1].</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdatePairingScoreAsync(
        string ingredientId1,
        string ingredientId2,
        double normalizedRating,
        CancellationToken ct = default);
}
