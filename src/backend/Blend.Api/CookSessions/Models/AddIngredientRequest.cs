namespace Blend.Api.CookSessions.Models;

/// <summary>
/// Request body for adding an ingredient to a Cook Mode session or dish.
/// </summary>
public sealed class AddIngredientRequest
{
    /// <summary>
    /// The ingredient ID from the Knowledge Base. Required.
    /// </summary>
    public string IngredientId { get; init; } = string.Empty;

    /// <summary>
    /// The display name of the ingredient. Required.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Optional free-text notes about this ingredient.</summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Optional dish ID to scope the ingredient to a specific dish.
    /// When null, the ingredient is added to the session's shared ingredient list.
    /// </summary>
    public string? DishId { get; init; }
}
