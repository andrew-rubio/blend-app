using Blend.Api.Ingredients.Models;
using Blend.Api.Ingredients.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Ingredients.Controllers;

/// <summary>
/// Provides ingredient autocomplete, detail, pairing and substitution endpoints
/// backed by the Ingredient Knowledge Base (ADR 0005, COOK-06 through COOK-15, PLAT-50 through PLAT-52).
/// </summary>
[ApiController]
[Route("api/v1/ingredients")]
[AllowAnonymous]
public sealed class IngredientsController : ControllerBase
{
    private readonly IKnowledgeBaseService? _kb;
    private readonly ILogger<IngredientsController> _logger;

    public IngredientsController(
        ILogger<IngredientsController> logger,
        IKnowledgeBaseService? kb = null)
    {
        _logger = logger;
        _kb = kb;
    }

    // GET /api/v1/ingredients/health
    /// <summary>
    /// Returns the availability status of the Ingredient Knowledge Base (PLAT-50 through PLAT-52).
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(KbHealthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealth(CancellationToken ct)
    {
        if (_kb is null)
        {
            return Ok(new KbHealthResponse(KbStatus.Unavailable, DateTimeOffset.UtcNow));
        }

        var available = await _kb.IsAvailableAsync(ct);
        var status = available ? KbStatus.Healthy : KbStatus.Unavailable;
        return Ok(new KbHealthResponse(status, DateTimeOffset.UtcNow));
    }

    // GET /api/v1/ingredients/search?q={query}
    /// <summary>
    /// Autocomplete ingredient search — returns up to 10 matching suggestions (COOK-06, COOK-07).
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<IngredientSearchResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        if (_kb is null)
        {
            return KbUnavailableProblem();
        }

        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(Array.Empty<IngredientSearchResult>());
        }

        var clampedLimit = Math.Clamp(limit, 1, 50);
        var results = await _kb.SearchIngredientsAsync(q, clampedLimit, ct);
        return Ok(results);
    }

    // GET /api/v1/ingredients/{id}
    /// <summary>
    /// Returns full ingredient details — name, category, flavour profile, substitutes, nutrition (COOK-13 through COOK-15).
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(IngredientDocument), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetIngredient(string id, CancellationToken ct)
    {
        if (_kb is null)
        {
            return KbUnavailableProblem();
        }

        var ingredient = await _kb.GetIngredientAsync(id, ct);
        if (ingredient is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: $"Ingredient '{id}' was not found.");
        }

        return Ok(ingredient);
    }

    // GET /api/v1/ingredients/{id}/pairings
    /// <summary>
    /// Returns scored pairing suggestions sorted by score descending (COOK-08 through COOK-10).
    /// </summary>
    [HttpGet("{id}/pairings")]
    [ProducesResponseType(typeof(IReadOnlyList<PairingSuggestion>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetPairings(
        string id,
        [FromQuery] string? category = null,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        if (_kb is null)
        {
            return KbUnavailableProblem();
        }

        var clampedLimit = Math.Clamp(limit, 1, 100);
        var pairings = await _kb.GetPairingsAsync(id, category, clampedLimit, ct);
        return Ok(pairings);
    }

    // GET /api/v1/ingredients/{id}/substitutes
    /// <summary>
    /// Returns substitution suggestions with compatibility notes.
    /// </summary>
    [HttpGet("{id}/substitutes")]
    [ProducesResponseType(typeof(IReadOnlyList<SubstituteSuggestion>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetSubstitutes(string id, CancellationToken ct)
    {
        if (_kb is null)
        {
            return KbUnavailableProblem();
        }

        var substitutes = await _kb.GetSubstitutesAsync(id, ct);
        return Ok(substitutes);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private IActionResult KbUnavailableProblem() =>
        Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Service unavailable",
            detail: "The Ingredient Knowledge Base is not available.");
}
