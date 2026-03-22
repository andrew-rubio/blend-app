using Blend.Api.Services.Spoonacular.Models;
using Blend.Domain.Entities;

namespace Blend.Api.Preferences.Services;

/// <summary>
/// Manages user preference retrieval and applies preferences to Spoonacular search parameters.
/// </summary>
public interface IPreferenceService
{
    /// <summary>
    /// Retrieves the saved preferences for the specified user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's preferences, or default empty preferences if the user is not found.</returns>
    Task<UserPreferences> GetUserPreferencesAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enriches Spoonacular search filters with the user's preferences.
    /// Intolerances are applied as strict exclusion filters (PREF-07).
    /// Diets are applied as deprioritisation filters — non-matching recipes are ranked
    /// lower by Spoonacular but are not hidden (PREF-08).
    /// </summary>
    /// <param name="filters">Existing search filters to enrich (may be null).</param>
    /// <param name="preferences">The user's saved preferences.</param>
    /// <returns>A new <see cref="ComplexSearchFilters"/> instance with preferences applied.</returns>
    ComplexSearchFilters ApplyPreferencesToSearch(
        ComplexSearchFilters? filters,
        UserPreferences preferences);

    /// <summary>
    /// Returns the list of ingredient IDs disliked by the user, used by Cook Mode
    /// to exclude unwanted ingredients from suggestions (PREF-11).
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of disliked ingredient IDs, or an empty list if the user is not found.</returns>
    Task<IReadOnlyList<string>> GetExcludedIngredientIdsAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
