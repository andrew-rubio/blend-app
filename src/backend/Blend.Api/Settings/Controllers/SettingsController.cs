using System.Security.Claims;
using Blend.Api.Settings.Models;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Settings.Controllers;

/// <summary>
/// Manages the authenticated user's app settings (SETT-01 through SETT-16).
/// </summary>
[ApiController]
[Route("api/v1/settings")]
[Authorize]
public sealed class SettingsController : ControllerBase
{
    private readonly IRepository<User>? _userRepository;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(
        ILogger<SettingsController> logger,
        IRepository<User>? userRepository = null)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    // GET /api/v1/settings
    /// <summary>Returns the current user's app settings.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(AppSettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_userRepository is null)
        {
            return ServiceUnavailableProblem();
        }

        var user = await _userRepository.GetByIdAsync(userId, userId, ct);
        if (user is null)
        {
            return NotFoundProblem(userId);
        }

        // Prefer the Settings sub-document, but ensure UnitSystem is consistent with the legacy
        // MeasurementUnit field so existing users who updated via /preferences see the correct value.
        var effectiveSettings = new AppSettings
        {
            UnitSystem = user.MeasurementUnit,
            Theme = user.Settings.Theme,
            Notifications = user.Settings.Notifications,
        };

        return Ok(AppSettingsResponse.FromEntity(effectiveSettings));
    }

    // PUT /api/v1/settings
    /// <summary>Replaces the current user's app settings.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(AppSettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdateSettings(
        [FromBody] UpdateSettingsRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_userRepository is null)
        {
            return ServiceUnavailableProblem();
        }

        var user = await _userRepository.GetByIdAsync(userId, userId, ct);
        if (user is null)
        {
            return NotFoundProblem(userId);
        }

        var existing = user.Settings;

        var newNotificationPrefs = new NotificationPreferences
        {
            FriendRequests = request.Notifications?.FriendRequests ?? existing.Notifications.FriendRequests,
            RecipeLikes = request.Notifications?.RecipeLikes ?? existing.Notifications.RecipeLikes,
            RecipePublished = request.Notifications?.RecipePublished ?? existing.Notifications.RecipePublished,
            SystemAnnouncements = request.Notifications?.SystemAnnouncements ?? existing.Notifications.SystemAnnouncements,
        };

        var newSettings = new AppSettings
        {
            UnitSystem = request.UnitSystem ?? existing.UnitSystem,
            Theme = request.Theme ?? existing.Theme,
            Notifications = newNotificationPrefs,
        };

        var updated = await _userRepository.UpdateAsync(
            new User
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                ProfilePhotoUrl = user.ProfilePhotoUrl,
                PasswordHashRef = user.PasswordHashRef,
                Preferences = user.Preferences,
                MeasurementUnit = newSettings.UnitSystem,
                Settings = newSettings,
                CreatedAt = user.CreatedAt,
                UpdatedAt = DateTimeOffset.UtcNow,
                UnreadNotificationCount = user.UnreadNotificationCount,
                Role = user.Role,
                DeletionRequestedAt = user.DeletionRequestedAt,
                IsDeactivated = user.IsDeactivated,
            },
            userId,
            userId,
            ct);

        return Ok(AppSettingsResponse.FromEntity(updated.Settings));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private IActionResult UnauthorizedProblem() =>
        Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized",
            detail: "User identity could not be resolved.");

    private IActionResult ServiceUnavailableProblem() =>
        Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Service unavailable",
            detail: "The user data store is not available.");

    private IActionResult NotFoundProblem(string userId)
    {
        _logger.LogWarning("User {UserId} not found when accessing settings.", userId);
        return Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found",
            detail: "User profile not found.");
    }
}
