using System.ComponentModel.DataAnnotations;

namespace Blend.Api.Preferences.Models;

/// <summary>
/// Request body for PUT /api/v1/users/me/preferences.
/// All fields are required; this performs a full overwrite of the preferences object.
/// </summary>
public sealed class UpdatePreferencesRequest
{
    /// <summary>Selected cuisine types. Must be values from the predefined cuisines list.</summary>
    [Required]
    public IReadOnlyList<string> FavoriteCuisines { get; init; } = [];

    /// <summary>Selected dish types. Must be values from the predefined dish types list.</summary>
    [Required]
    public IReadOnlyList<string> FavoriteDishTypes { get; init; } = [];

    /// <summary>Selected dietary plans. Must be values from the predefined diets list.</summary>
    [Required]
    public IReadOnlyList<string> Diets { get; init; } = [];

    /// <summary>Selected intolerances. Must be values from the predefined intolerances list.</summary>
    [Required]
    public IReadOnlyList<string> Intolerances { get; init; } = [];

    /// <summary>Ingredient IDs the user dislikes (excluded from Cook Mode suggestions).</summary>
    [Required]
    public IReadOnlyList<string> DislikedIngredientIds { get; init; } = [];
}
