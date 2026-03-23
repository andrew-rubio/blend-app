namespace Blend.Api.CookSessions.Models;

/// <summary>
/// Request body for updating a Cook Mode session's top-level fields.
/// All fields are optional; only provided (non-null) fields are applied.
/// </summary>
public sealed class UpdateCookSessionRequest
{
    /// <summary>Optional notes to store on the session.</summary>
    public string? Notes { get; init; }
}
