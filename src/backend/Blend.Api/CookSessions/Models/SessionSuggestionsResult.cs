namespace Blend.Api.CookSessions.Models;

/// <summary>
/// A single smart ingredient suggestion produced by the pairing engine.
/// </summary>
public sealed record SmartSuggestion(
    string IngredientId,
    string Name,
    double AggregateScore,
    string? Category,
    string Reason);

/// <summary>
/// The result of a smart suggestions query, including a flag indicating
/// whether the Knowledge Base was reachable.
/// </summary>
public sealed class SessionSuggestionsResult
{
    /// <summary>Ordered list of ingredient suggestions (highest score first).</summary>
    public IReadOnlyList<SmartSuggestion> Suggestions { get; init; } = [];

    /// <summary>
    /// <c>true</c> when the Knowledge Base was unreachable during this request
    /// and suggestions could not be generated (REQ-66).
    /// </summary>
    public bool KbUnavailable { get; init; }
}
