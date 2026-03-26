using System.Security.Claims;
using Blend.Api.Account.Models;
using Blend.Api.Account.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Account.Controllers;

/// <summary>
/// Handles account lifecycle management: deletion request and cancellation
/// (SETT-17 through SETT-24, REQ-61).
/// </summary>
[ApiController]
[Route("api/v1/users/me")]
[Authorize]
public sealed class AccountController : ControllerBase
{
    private readonly IAccountDeletionService? _deletionService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        ILogger<AccountController> logger,
        IAccountDeletionService? deletionService = null)
    {
        _logger = logger;
        _deletionService = deletionService;
    }

    // POST /api/v1/users/me/delete-request
    /// <summary>
    /// Initiates account deletion. Requires password re-authentication (SETT-17, SETT-22).
    /// The account is deactivated immediately; permanent deletion occurs after 30 days.
    /// </summary>
    [HttpPost("delete-request")]
    [ProducesResponseType(typeof(DeleteAccountResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RequestDeletion(
        [FromBody] DeleteAccountRequest request,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_deletionService is null)
        {
            return ServiceUnavailableProblem();
        }

        var (scheduledAt, result) = await _deletionService.RequestDeletionAsync(userId, request.Password, ct);

        return result switch
        {
            AccountDeletionOpResult.Success => Accepted(new DeleteAccountResponse(
                DeletionScheduledAt: scheduledAt!.Value,
                GracePeriodEndsAt: scheduledAt!.Value)),
            AccountDeletionOpResult.ReAuthRequired => Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Re-authentication required",
                detail: "Your current password is required to confirm account deletion."),
            AccountDeletionOpResult.AlreadyRequested => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: "An account deletion request is already pending."),
            AccountDeletionOpResult.NotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not found",
                detail: "User account not found."),
            _ => ServiceUnavailableProblem(),
        };
    }

    // POST /api/v1/users/me/cancel-deletion
    /// <summary>
    /// Cancels a pending deletion request and reactivates the account (SETT-23).
    /// Must be called within the 30-day grace period.
    /// </summary>
    [HttpPost("cancel-deletion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CancelDeletion(CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_deletionService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _deletionService.CancelDeletionAsync(userId, ct);

        return result switch
        {
            AccountDeletionOpResult.Success => NoContent(),
            AccountDeletionOpResult.NoPendingRequest => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not found",
                detail: "No pending deletion request found within the grace period."),
            AccountDeletionOpResult.NotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not found",
                detail: "User account not found."),
            _ => ServiceUnavailableProblem(),
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private IActionResult UnauthorizedProblem() =>
        Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized",
            detail: "User identity could not be resolved.");

    private IActionResult ServiceUnavailableProblem() =>
        Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Service unavailable",
            detail: "The account service is not available.");
}
