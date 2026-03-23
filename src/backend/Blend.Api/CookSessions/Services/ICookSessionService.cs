using Blend.Api.CookSessions.Models;
using Blend.Domain.Entities;

namespace Blend.Api.CookSessions.Services;

/// <summary>
/// Manages Cook Mode sessions: creation, ingredient and dish management,
/// smart suggestions, and session lifecycle transitions.
/// </summary>
public interface ICookSessionService
{
    /// <summary>
    /// Creates a new Cook Mode session for the given user (COOK-01).
    /// Returns <c>null</c> only when a dependency (repository) is unavailable.
    /// </summary>
    Task<CookingSession?> CreateSessionAsync(
        string userId,
        CreateCookSessionRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Returns a specific session by ID, enforcing ownership (userId must match).
    /// </summary>
    Task<CookingSession?> GetSessionAsync(
        string sessionId,
        string userId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the current active or paused session for the user, or <c>null</c>
    /// if none exists (COOK-50, COOK-51).
    /// </summary>
    Task<CookingSession?> GetActiveSessionAsync(
        string userId,
        CancellationToken ct = default);

    /// <summary>
    /// Adds an ingredient to the session or to a specific dish within the session (COOK-03 through COOK-05).
    /// Returns the updated session, or <c>null</c> if the session was not found.
    /// </summary>
    Task<CookingSession?> AddIngredientAsync(
        string sessionId,
        string userId,
        AddIngredientRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Removes an ingredient from the session or from a specific dish (by ingredientId and optional dishId).
    /// Returns the updated session, or <c>null</c> if not found.
    /// </summary>
    Task<CookingSession?> RemoveIngredientAsync(
        string sessionId,
        string userId,
        string ingredientId,
        string? dishId,
        CancellationToken ct = default);

    /// <summary>
    /// Adds a new dish workspace to the session (COOK-22, COOK-23).
    /// Returns the updated session, or <c>null</c> if not found.
    /// </summary>
    Task<CookingSession?> AddDishAsync(
        string sessionId,
        string userId,
        AddDishRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Removes a dish from the session by its ID.
    /// Returns the updated session, or <c>null</c> if not found.
    /// </summary>
    Task<CookingSession?> RemoveDishAsync(
        string sessionId,
        string userId,
        string dishId,
        CancellationToken ct = default);

    /// <summary>
    /// Marks the session as <see cref="CookingSessionStatus.Completed"/>.
    /// Returns the updated session, or <c>null</c> if not found.
    /// </summary>
    Task<CookingSession?> CompleteSessionAsync(
        string sessionId,
        string userId,
        CancellationToken ct = default);

    /// <summary>
    /// Pauses the session and sets a 24-hour TTL for automatic expiry.
    /// Returns the updated session, or <c>null</c> if not found.
    /// </summary>
    Task<CookingSession?> PauseSessionAsync(
        string sessionId,
        string userId,
        CancellationToken ct = default);

    /// <summary>
    /// Generates smart ingredient suggestions for the session based on current
    /// ingredients and user preferences (COOK-08 through COOK-10).
    /// If the Knowledge Base is unavailable, returns an empty list with
    /// <see cref="SessionSuggestionsResult.KbUnavailable"/> set to <c>true</c> (REQ-66).
    /// </summary>
    Task<SessionSuggestionsResult> GetSuggestionsAsync(
        string sessionId,
        string userId,
        string? dishId,
        int limit,
        CancellationToken ct = default);

    /// <summary>
    /// Returns KB data for an ingredient in the context of the session:
    /// flavour profile, substitutes, and a "why it pairs" explanation
    /// derived from co-occurring session ingredients (COOK-13 through COOK-15).
    /// </summary>
    Task<IngredientDetailResult?> GetIngredientDetailAsync(
        string sessionId,
        string userId,
        string ingredientId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns <c>true</c> when the given user has an active session.
    /// Used internally to enforce the one-active-session-per-user constraint (COOK-50).
    /// </summary>
    Task<bool> HasActiveSessionAsync(string userId, CancellationToken ct = default);
}
