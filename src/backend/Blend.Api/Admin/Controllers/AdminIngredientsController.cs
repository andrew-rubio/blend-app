using Blend.Api.Admin.Models;
using Blend.Api.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Admin.Controllers;

/// <summary>
/// Admin endpoints for the ingredient submission approval queue.
/// All routes require the Admin role.
/// </summary>
[ApiController]
[Authorize(Policy = "RequireAdmin")]
public sealed class AdminIngredientsController : ControllerBase
{
    private readonly IAdminContentService? _adminService;
    private readonly ILogger<AdminIngredientsController> _logger;

    public AdminIngredientsController(
        ILogger<AdminIngredientsController> logger,
        IAdminContentService? adminService = null)
    {
        _logger = logger;
        _adminService = adminService;
    }

    // GET /api/v1/admin/ingredients/submissions
    /// <summary>Lists pending (or filtered) ingredient submissions.</summary>
    [HttpGet("api/v1/admin/ingredients/submissions")]
    [ProducesResponseType(typeof(AdminSubmissionsPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetSubmissions(
        [FromQuery] string? status = null,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cursor = null,
        CancellationToken ct = default)
    {
        if (_adminService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _adminService.GetIngredientSubmissionsAsync(status, pageSize, cursor, ct);
        return Ok(result);
    }

    // POST /api/v1/admin/ingredients/submissions/{id}/approve
    /// <summary>Approves an ingredient submission.</summary>
    [HttpPost("api/v1/admin/ingredients/submissions/{id}/approve")]
    [ProducesResponseType(typeof(AdminSubmissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ApproveSubmission(string id, CancellationToken ct = default)
    {
        if (_adminService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _adminService.ApproveIngredientSubmissionAsync(id, ct);
        if (result is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found",
                detail: "Ingredient submission not found.");
        }

        return Ok(result);
    }

    // POST /api/v1/admin/ingredients/submissions/{id}/reject
    /// <summary>Rejects an ingredient submission with an optional reason.</summary>
    [HttpPost("api/v1/admin/ingredients/submissions/{id}/reject")]
    [ProducesResponseType(typeof(AdminSubmissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RejectSubmission(
        string id,
        [FromBody] RejectSubmissionRequest? request = null,
        CancellationToken ct = default)
    {
        if (_adminService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _adminService.RejectIngredientSubmissionAsync(id, request?.Reason, ct);
        if (result is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found",
                detail: "Ingredient submission not found.");
        }

        return Ok(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private IActionResult ServiceUnavailableProblem() =>
        Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Service unavailable",
            detail: "The admin service is not available.");
}
