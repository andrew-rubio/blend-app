using System.Security.Claims;
using Blend.Api.Ingredients.Models;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Ingredients.Controllers;

/// <summary>
/// Handles user ingredient submissions for admin review (SETT-08 through SETT-12).
/// </summary>
[ApiController]
[Route("api/v1/ingredients/submissions")]
[Authorize]
public sealed class IngredientSubmissionsController : ControllerBase
{
    private readonly IRepository<Content>? _contentRepository;
    private readonly ILogger<IngredientSubmissionsController> _logger;

    public IngredientSubmissionsController(
        ILogger<IngredientSubmissionsController> logger,
        IRepository<Content>? contentRepository = null)
    {
        _logger = logger;
        _contentRepository = contentRepository;
    }

    // POST /api/v1/ingredients/submissions
    /// <summary>Submits a new ingredient for admin review (SETT-10).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(IngredientSubmissionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Submit(
        [FromBody] IngredientSubmissionRequest request,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_contentRepository is null)
        {
            return ServiceUnavailableProblem();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Validation failed",
                detail: "Ingredient name is required.");
        }

        var submission = new Content
        {
            Id = Guid.NewGuid().ToString(),
            ContentType = ContentType.IngredientSubmission,
            Title = request.Name.Trim(),
            Body = request.Description?.Trim(),
            Category = request.Category?.Trim(),
            SubmittedByUserId = userId,
            SubmissionStatus = SubmissionStatus.Pending,
            IsPublished = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var created = await _contentRepository.CreateAsync(submission, ct);
        var response = IngredientSubmissionResponse.FromEntity(created);

        return CreatedAtAction(nameof(GetMySubmissions), null, response);
    }

    // GET /api/v1/ingredients/submissions/mine
    /// <summary>Lists the ingredient submissions created by the current user.</summary>
    [HttpGet("mine")]
    [ProducesResponseType(typeof(IReadOnlyList<IngredientSubmissionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetMySubmissions(CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_contentRepository is null)
        {
            return ServiceUnavailableProblem();
        }

        var query = $"SELECT * FROM c WHERE c.contentType = 'IngredientSubmission' AND c.submittedByUserId = '{userId}' ORDER BY c.createdAt DESC";
        var submissions = await _contentRepository.GetByQueryAsync(query, null, ct);

        var response = submissions.Select(IngredientSubmissionResponse.FromEntity).ToList();
        return Ok(response);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private IActionResult UnauthorizedProblem() =>
        Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized",
            detail: "User identity could not be resolved.");

    private IActionResult ServiceUnavailableProblem() =>
        Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Service unavailable",
            detail: "The content store is not available.");
}
