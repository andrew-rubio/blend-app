using System.Security.Claims;
using Blend.Api.Notifications.Models;
using Blend.Api.Notifications.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Notifications.Controllers;

/// <summary>
/// Provides notification inbox endpoints for the authenticated user (PLAT-35 through PLAT-41).
/// </summary>
[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService? _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        ILogger<NotificationsController> logger,
        INotificationService? notificationService = null)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    // GET /api/v1/notifications
    /// <summary>Returns a paged list of notifications for the current user (PLAT-37).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(NotificationsPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cursor = null,
        [FromQuery] bool unreadOnly = false,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_notificationService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _notificationService.GetNotificationsAsync(userId, pageSize, cursor, unreadOnly, ct);
        return Ok(result);
    }

    // GET /api/v1/notifications/unread-count
    /// <summary>Returns the number of unread notifications for the current user (PLAT-35).</summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadCountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_notificationService is null)
        {
            return ServiceUnavailableProblem();
        }

        var count = await _notificationService.GetUnreadCountAsync(userId, ct);
        return Ok(new UnreadCountResponse(count));
    }

    // POST /api/v1/notifications/{id}/read
    /// <summary>Marks a single notification as read (PLAT-39).</summary>
    [HttpPost("{id}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> MarkAsRead(string id, CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_notificationService is null)
        {
            return ServiceUnavailableProblem();
        }

        var found = await _notificationService.MarkAsReadAsync(userId, id, ct);
        if (!found)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found",
                detail: "Notification not found.");
        }

        return NoContent();
    }

    // POST /api/v1/notifications/read-all
    /// <summary>Marks all notifications for the current user as read (PLAT-40).</summary>
    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_notificationService is null)
        {
            return ServiceUnavailableProblem();
        }

        await _notificationService.MarkAllAsReadAsync(userId, ct);
        return NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private IActionResult UnauthorizedProblem() =>
        Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized",
            detail: "User identity could not be resolved.");

    private IActionResult ServiceUnavailableProblem() =>
        Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Service unavailable",
            detail: "The notification service is not available.");
}
