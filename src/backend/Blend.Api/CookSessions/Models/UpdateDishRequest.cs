using System.ComponentModel.DataAnnotations;

namespace Blend.Api.CookSessions.Models;

/// <summary>
/// Request body for updating a dish within a cook session.
/// All fields are optional; only provided (non-null) fields are applied.
/// </summary>
public sealed class UpdateDishRequest
{
    /// <summary>New display name for the dish.</summary>
    [StringLength(200)]
    public string? Name { get; init; }

    /// <summary>Updated notes for the dish.</summary>
    [StringLength(2000)]
    public string? Notes { get; init; }
}
