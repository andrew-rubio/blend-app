using System.Security.Claims;
using Blend.Api.CookSessions.Models;
using Blend.Api.CookSessions.Services;
using Blend.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.CookSessions.Controllers;

/// <summary>
/// Manages Cook Mode sessions: creation, ingredient and dish management,
/// smart suggestions, and session lifecycle (pause/complete/resume).
/// All endpoints require a valid JWT.
/// </summary>
[ApiController]
[Route("api/v1/cook-sessions")]
[Authorize]
public sealed class CookSessionsController : ControllerBase
{
    private readonly ICookSessionService? _cookSessionService;
    private readonly ILogger<CookSessionsController> _logger;

    public CookSessionsController(
        ILogger<CookSessionsController> logger,
        ICookSessionService? cookSessionService = null)
    {
        _logger = logger;
        _cookSessionService = cookSessionService;
    }

    // ── POST /api/v1/cook-sessions ────────────────────────────────────────────

    /// <summary>
    /// Creates a new Cook Mode session (COOK-01).
    /// Returns 409 Conflict if the user already has an active session.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CookingSession), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateSession(
        [FromBody] CreateCookSessionRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_cookSessionService is null)
        {
            return ServiceUnavailableProblem();
        }

        var hasActive = await _cookSessionService.HasActiveSessionAsync(userId, ct);
        if (hasActive)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: "An active Cook Mode session already exists. Resume or complete it before starting a new one.");
        }

        var session = await _cookSessionService.CreateSessionAsync(userId, request, ct);
        if (session is null)
        {
            return ServiceUnavailableProblem();
        }

        return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
    }

    // ── GET /api/v1/cook-sessions/active ─────────────────────────────────────

    /// <summary>
    /// Returns the current active or paused session for the authenticated user (COOK-50, COOK-51).
    /// Returns 404 when no active session exists.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(CookingSession), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetActiveSession(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_cookSessionService is null)
        {
            return ServiceUnavailableProblem();
        }

        var session = await _cookSessionService.GetActiveSessionAsync(userId, ct);
        if (session is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not found",
                detail: "No active Cook Mode session found.");
        }

        return Ok(session);
    }

    // ── GET /api/v1/cook-sessions/{id} ────────────────────────────────────────

    /// <summary>Returns a specific Cook Mode session by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CookingSession), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetSession(string id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_cookSessionService is null)
        {
            return ServiceUnavailableProblem();
        }

        var session = await _cookSessionService.GetSessionAsync(id, userId, ct);
        if (session is null)
        {
            return NotFoundProblem(id);
        }

        return Ok(session);
    }

    // ── PUT /api/v1/cook-sessions/{id} ────────────────────────────────────────

    /// <summary>Updates session-level metadata (e.g. notes).</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CookingSession), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdateSession(
        string id,
        [FromBody] UpdateCookSessionRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_cookSessionService is null)
        {
            return ServiceUnavailableProblem();
        }

        var session = await _cookSessionService.GetSessionAsync(id, userId, ct);
        if (session is null)
        {
            return NotFoundProblem(id);
        }

        // Currently only notes are updatable at the session level;
        // return the current session document unchanged (placeholder for future fields).
        return Ok(session);
    }

    // ── POST /api/v1/cook-sessions/{id}/ingredients ───────────────────────────

    /// <summary>
    /// Adds an ingredient to the session or to a specific dish within the session (COOK-03 through COOK-05).
    /// </summary>
    [HttpPost("{id}/ingredients")]
    [ProducesResponseType(typeof(CookingSession), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> AddIngredient(
        string id,
        [FromBody] AddIngredientRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_cookSessionService is null)
        {
            return ServiceUnavailableProblem();
        }

        if (string.IsNullOrWhiteSpace(request.IngredientId))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation failed",
                detail: "ingredientId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation failed",
                detail: "name is required.");
        }

        var session = await _cookSessionService.AddIngredientAsync(id, userId, request, ct);
        if (session is null)
        {
            return NotFoundProblem(id);
        }

        return Ok(session);
    }

    // ── DELETE /api/v1/cook-sessions/{id}/ingredients/{ingredientId} ──────────

    /// <summary>Removes an ingredient from the session or from a specific dish.</summary>
    [HttpDelete("{id}/ingredients/{ingredientId}")]
    [ProducesResponseType(typeof(CookingSession), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RemoveIngredient(
        string id,
        string ingredientId,
        [FromQuery] string? dishId,
        CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_cookSessionService is null)
        {
            return ServiceUnavailableProblem();
        }

        var session = await _cookSessionService.RemoveIngredientAsync(id, userId, ingredientId, dishId, ct);
        if (session is null)
        {
            return NotFoundProblem(id);
        }

        return Ok(session);
    }

    // ── POST /api/v1/cook-sessions/{id}/dishes ────────────────────────────────

    /// <summary>Adds a new dish workspace to the session (COOK-22, COOK-23).</summary>
    [HttpPost("{id}/dishes")]
    [ProducesResponseType(typeof(CookingSession), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> AddDish(
        string id,
        [FromBody] AddDishRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_cookSessionService is null)
        {
            return ServiceUnavailableProblem();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation failed",
                detail: "name is required.");
        }

        var session = await _cookSessionService.AddDishAsync(id, userId, request, ct);
        if (session is null)
        {
            return NotFoundProblem(id);
        }

        return Ok(session);
    }

    // ── DELETE /api/v1/cook-sessions/{id}/dishes/{dishId} ────────────────────

    /// <summary>Removes a dish from the session by its dish ID.</summary>
    [HttpDelete("{id}/dishes/{dishId}")]
    [ProducesResponseType(typeof(CookingSession), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RemoveDish(
        string id,
        string dishId,
        CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_cookSessionService is null)
        {
            return ServiceUnavailableProblem();
        }

        var session = await _cookSessionService.RemoveDishAsync(id, userId, dishId, ct);
        if (session is null)
        {
            return NotFoundProblem(id);
        }

        return Ok(session);
    }

    // ── POST /api/v1/cook-sessions/{id}/complete ──────────────────────────────

    /// <summary>Marks the session as completed, triggering the wrap-up flow.</summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(CookingSession), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CompleteSession(string id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_cookSessionService is null)
        {
            return ServiceUnavailableProblem();
        }

        var session = await _cookSessionService.CompleteSessionAsync(id, userId, ct);
        if (session is null)
        {
            return NotFoundProblem(id);
        }

        return Ok(session);
    }

    // ── POST /api/v1/cook-sessions/{id}/pause ────────────────────────────────

    /// <summary>Pauses the session. A paused session expires after 24 hours (COOK-50, COOK-51).</summary>
    [HttpPost("{id}/pause")]
    [ProducesResponseType(typeof(CookingSession), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> PauseSession(string id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_cookSessionService is null)
        {
            return ServiceUnavailableProblem();
        }

        var session = await _cookSessionService.PauseSessionAsync(id, userId, ct);
        if (session is null)
        {
            return NotFoundProblem(id);
        }

        return Ok(session);
    }

    // ── GET /api/v1/cook-sessions/{id}/suggestions ───────────────────────────

    /// <summary>
    /// Returns smart ingredient suggestions based on current session ingredients,
    /// ranked by aggregate pairing score (COOK-08 through COOK-10, REQ-66).
    /// </summary>
    [HttpGet("{id}/suggestions")]
    [ProducesResponseType(typeof(SessionSuggestionsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetSuggestions(
        string id,
        [FromQuery] string? dishId = null,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_cookSessionService is null)
        {
            return ServiceUnavailableProblem();
        }

        var clampedLimit = Math.Clamp(limit, 1, 50);
        var result = await _cookSessionService.GetSuggestionsAsync(id, userId, dishId, clampedLimit, ct);
        return Ok(result);
    }

    // ── GET /api/v1/cook-sessions/{id}/ingredients/{ingredientId}/detail ──────

    /// <summary>
    /// Returns Knowledge Base data for an ingredient in the context of the session
    /// (COOK-13 through COOK-15).
    /// </summary>
    [HttpGet("{id}/ingredients/{ingredientId}/detail")]
    [ProducesResponseType(typeof(IngredientDetailResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetIngredientDetail(
        string id,
        string ingredientId,
        CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_cookSessionService is null)
        {
            return ServiceUnavailableProblem();
        }

        var detail = await _cookSessionService.GetIngredientDetailAsync(id, userId, ingredientId, ct);
        if (detail is null)
        {
            return NotFoundProblem(ingredientId);
        }

        return Ok(detail);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string? GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);

    private IActionResult UnauthorizedProblem() =>
        Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Unauthorized",
            detail: "Authentication is required.");

    private IActionResult NotFoundProblem(string id)
    {
        _logger.LogWarning("Cook Mode session '{SessionId}' not found.", id);
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Not found",
            detail: $"Cook Mode session '{id}' was not found.");
    }

    private IActionResult ServiceUnavailableProblem() =>
        Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Service unavailable",
            detail: "The Cook Mode session service is not available.");
}
