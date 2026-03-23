namespace Blend.Api.CookSessions.Models;

/// <summary>
/// Request body for adding a new dish to a Cook Mode session.
/// </summary>
public sealed class AddDishRequest
{
    /// <summary>Display name for the dish. Required.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Optional cuisine type hint (e.g., "Italian", "Japanese").</summary>
    public string? CuisineType { get; init; }

    /// <summary>Optional free-text notes about this dish.</summary>
    public string? Notes { get; init; }
}
