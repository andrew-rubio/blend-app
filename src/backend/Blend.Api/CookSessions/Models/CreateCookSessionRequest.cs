namespace Blend.Api.CookSessions.Models;

/// <summary>
/// Request body for creating a new Cook Mode session.
/// </summary>
public sealed class CreateCookSessionRequest
{
    /// <summary>
    /// Optional recipe ID to pre-populate the session's first dish with ingredients
    /// from the specified recipe.
    /// </summary>
    public string? RecipeId { get; init; }

    /// <summary>
    /// Optional name for the initial dish. Defaults to "My Dish" when not provided.
    /// </summary>
    public string? InitialDishName { get; init; }
}
