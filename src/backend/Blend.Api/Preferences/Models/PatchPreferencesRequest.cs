namespace Blend.Api.Preferences.Models;

/// <summary>
/// Request body for PATCH /api/v1/users/me/preferences.
/// All fields are optional; only non-null fields are updated.
/// </summary>
public sealed class PatchPreferencesRequest
{
    /// <summary>
    /// Selected cuisine types to replace the current value.
    /// Must be values from the predefined cuisines list.
    /// Null means this field will not be updated.
    /// </summary>
    public IReadOnlyList<string>? FavoriteCuisines { get; init; }

    /// <summary>
    /// Selected dish types to replace the current value.
    /// Must be values from the predefined dish types list.
    /// Null means this field will not be updated.
    /// </summary>
    public IReadOnlyList<string>? FavoriteDishTypes { get; init; }

    /// <summary>
    /// Selected dietary plans to replace the current value.
    /// Must be values from the predefined diets list.
    /// Null means this field will not be updated.
    /// </summary>
    public IReadOnlyList<string>? Diets { get; init; }

    /// <summary>
    /// Selected intolerances to replace the current value.
    /// Must be values from the predefined intolerances list.
    /// Null means this field will not be updated.
    /// </summary>
    public IReadOnlyList<string>? Intolerances { get; init; }

    /// <summary>
    /// Ingredient IDs the user dislikes to replace the current value.
    /// Null means this field will not be updated.
    /// </summary>
    public IReadOnlyList<string>? DislikedIngredientIds { get; init; }
}
