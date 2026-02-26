using System.Security.Claims;
using Blend.Api.Models.Admin;
using Blend.Api.Services;
using Blend.Domain.Entities;
using Blend.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/ingredients")]
[Authorize(Policy = "RequireAdmin")]
public class AdminIngredientsController : ControllerBase
{
    private readonly IRepository<IngredientSubmission> _submissionsRepository;
    private readonly INotificationService _notificationService;
    private readonly IKnowledgeBaseService _knowledgeBaseService;
    private readonly ILogger<AdminIngredientsController> _logger;

    public AdminIngredientsController(
        IRepository<IngredientSubmission> submissionsRepository,
        INotificationService notificationService,
        IKnowledgeBaseService knowledgeBaseService,
        ILogger<AdminIngredientsController> logger)
    {
        _submissionsRepository = submissionsRepository;
        _notificationService = notificationService;
        _knowledgeBaseService = knowledgeBaseService;
        _logger = logger;
    }

    /// <summary>
    /// Lists ingredient submissions, optionally filtered by status.
    /// GET /api/v1/admin/ingredients/submissions?status=pending
    /// </summary>
    [HttpGet("submissions")]
    public async Task<IActionResult> GetSubmissions(
        [FromQuery] string? status = null,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        string query;
        IDictionary<string, object> parameters;

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!IsValidStatus(status))
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid status",
                    Detail = "Status must be one of: pending, approved, rejected",
                    Status = 400
                });

            var statusEnum = ParseStatus(status);
            query = "SELECT * FROM c WHERE c.contentType = 'ingredient-submission' AND c.submissionStatus = @status ORDER BY c.submittedAt ASC";
            parameters = new Dictionary<string, object> { ["@status"] = (int)statusEnum };
        }
        else
        {
            query = "SELECT * FROM c WHERE c.contentType = 'ingredient-submission' ORDER BY c.submittedAt ASC";
            parameters = new Dictionary<string, object>();
        }

        var result = await _submissionsRepository.QueryAsync(
            query,
            new PaginationOptions { PageSize = pageSize, ContinuationToken = continuationToken },
            parameters,
            partitionKey: "ingredient-submission",
            cancellationToken: cancellationToken);

        return Ok(new PagedIngredientSubmissionsResponse
        {
            Items = result.Items.Select(MapToSubmissionResponse).ToList(),
            ContinuationToken = result.ContinuationToken,
            HasMore = result.HasMore
        });
    }

    /// <summary>
    /// Approves an ingredient submission, adds to KB, and notifies the submitter.
    /// POST /api/v1/admin/ingredients/submissions/{id}/approve
    /// </summary>
    [HttpPost("submissions/{id}/approve")]
    public async Task<IActionResult> ApproveSubmission(
        string id,
        CancellationToken cancellationToken = default)
    {
        var submission = await _submissionsRepository.GetByIdAsync(id, "ingredient-submission", cancellationToken);
        if (submission is null) return NotFound();

        if (submission.SubmissionStatus != IngredientSubmissionStatus.Pending)
            return Conflict(new ProblemDetails
            {
                Title = "Already reviewed",
                Detail = $"Submission is already '{submission.SubmissionStatus}'.",
                Status = 409
            });

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        submission.SubmissionStatus = IngredientSubmissionStatus.Approved;
        submission.ReviewedAt = DateTimeOffset.UtcNow;
        submission.ReviewedByAdminId = adminId;

        await _submissionsRepository.UpdateAsync(submission, "ingredient-submission", cancellationToken);

        await _knowledgeBaseService.AddIngredientAsync(
            submission.IngredientName,
            submission.Description,
            submission.Category,
            cancellationToken);

        if (!string.IsNullOrEmpty(submission.SubmittedByUserId))
        {
            await _notificationService.SendIngredientApprovedAsync(
                submission.SubmittedByUserId,
                submission.IngredientName,
                cancellationToken);
        }

        _logger.LogInformation("Admin {AdminId} approved ingredient submission {Id} ('{Name}')",
            adminId, id, submission.IngredientName);

        return Ok(MapToSubmissionResponse(submission));
    }

    /// <summary>
    /// Rejects an ingredient submission and notifies the submitter.
    /// POST /api/v1/admin/ingredients/submissions/{id}/reject
    /// </summary>
    [HttpPost("submissions/{id}/reject")]
    public async Task<IActionResult> RejectSubmission(
        string id,
        [FromBody] RejectSubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var submission = await _submissionsRepository.GetByIdAsync(id, "ingredient-submission", cancellationToken);
        if (submission is null) return NotFound();

        if (submission.SubmissionStatus != IngredientSubmissionStatus.Pending)
            return Conflict(new ProblemDetails
            {
                Title = "Already reviewed",
                Detail = $"Submission is already '{submission.SubmissionStatus}'.",
                Status = 409
            });

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        submission.SubmissionStatus = IngredientSubmissionStatus.Rejected;
        submission.ReviewedAt = DateTimeOffset.UtcNow;
        submission.ReviewedByAdminId = adminId;
        submission.RejectionReason = request?.Reason;

        await _submissionsRepository.UpdateAsync(submission, "ingredient-submission", cancellationToken);

        if (!string.IsNullOrEmpty(submission.SubmittedByUserId))
        {
            await _notificationService.SendIngredientRejectedAsync(
                submission.SubmittedByUserId,
                submission.IngredientName,
                request?.Reason,
                cancellationToken);
        }

        _logger.LogInformation("Admin {AdminId} rejected ingredient submission {Id} ('{Name}')",
            adminId, id, submission.IngredientName);

        return Ok(MapToSubmissionResponse(submission));
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static bool IsValidStatus(string status) =>
        status is "pending" or "approved" or "rejected";

    private static IngredientSubmissionStatus ParseStatus(string status) => status switch
    {
        "pending" => IngredientSubmissionStatus.Pending,
        "approved" => IngredientSubmissionStatus.Approved,
        "rejected" => IngredientSubmissionStatus.Rejected,
        _ => IngredientSubmissionStatus.Pending
    };

    private static IngredientSubmissionResponse MapToSubmissionResponse(IngredientSubmission s) => new()
    {
        Id = s.Id,
        IngredientName = s.IngredientName,
        Description = s.Description,
        Category = s.Category,
        Aliases = s.Aliases,
        SubmittedByUserId = s.SubmittedByUserId,
        SubmissionStatus = s.SubmissionStatus.ToString().ToLowerInvariant(),
        SubmittedAt = s.SubmittedAt,
        ReviewedAt = s.ReviewedAt,
        ReviewedByAdminId = s.ReviewedByAdminId,
        RejectionReason = s.RejectionReason
    };
}
