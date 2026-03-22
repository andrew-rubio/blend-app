using Blend.Api.Services.Spoonacular.Models;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Blend.Api.Preferences.Services;

/// <summary>
/// Retrieves user preferences from the Cosmos DB user document and applies them
/// to Spoonacular search parameters (PREF-07, PREF-08, PREF-11).
/// </summary>
public sealed class PreferenceService : IPreferenceService
{
    private readonly IRepository<User>? _userRepository;
    private readonly ILogger<PreferenceService> _logger;

    public PreferenceService(
        ILogger<PreferenceService> logger,
        IRepository<User>? userRepository = null)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    /// <inheritdoc />
    public async Task<UserPreferences> GetUserPreferencesAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (_userRepository is null)
        {
            _logger.LogWarning("User repository is not available; returning default preferences for user {UserId}.", userId);
            return new UserPreferences();
        }

        var user = await _userRepository.GetByIdAsync(userId, userId, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found; returning default preferences.", userId);
            return new UserPreferences();
        }

        return user.Preferences;
    }

    /// <inheritdoc />
    public ComplexSearchFilters ApplyPreferencesToSearch(
        ComplexSearchFilters? filters,
        UserPreferences preferences)
    {
        // Intolerances → strict exclusion via Spoonacular's intolerances parameter (PREF-07).
        // Any existing intolerances are preserved; user intolerances are merged in.
        var allIntolerances = MergeCommaList(
            filters?.Intolerances,
            preferences.Intolerances);

        // Diets → deprioritisation via Spoonacular's diet parameter (PREF-08).
        // Spoonacular ranks non-matching recipes lower when a diet filter is supplied.
        // Only the first diet is used since Spoonacular's complexSearch accepts a single
        // diet value; multiple diets would require separate requests.
        var diet = !string.IsNullOrWhiteSpace(filters?.Diet)
            ? filters.Diet
            : (preferences.Diets.Count > 0 ? preferences.Diets[0] : null);

        // Cuisines → preference boosting via Spoonacular's cuisine parameter.
        var allCuisines = MergeCommaList(
            filters?.Cuisine,
            preferences.FavoriteCuisines);

        return new ComplexSearchFilters
        {
            Cuisine = allCuisines,
            Diet = diet,
            Intolerances = allIntolerances,
            MaxReadyTime = filters?.MaxReadyTime,
            Number = filters?.Number ?? 10,
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetExcludedIngredientIdsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var preferences = await GetUserPreferencesAsync(userId, cancellationToken);
        return preferences.DislikedIngredientIds;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Merges a comma-separated string and a list of values, deduplicating by
    /// case-insensitive comparison, and returns a comma-separated result or null.
    /// </summary>
    private static string? MergeCommaList(string? existing, IEnumerable<string> additional)
    {
        var parts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(existing))
        {
            foreach (var part in existing.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                parts.Add(part);
            }
        }

        foreach (var item in additional)
        {
            if (!string.IsNullOrWhiteSpace(item))
            {
                parts.Add(item);
            }
        }

        return parts.Count > 0 ? string.Join(",", parts) : null;
    }
}
