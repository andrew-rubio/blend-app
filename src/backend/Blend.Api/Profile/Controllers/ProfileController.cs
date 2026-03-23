using System.Security.Claims;
using Blend.Api.Profile.Models;
using Blend.Api.Profile.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Profile.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public sealed class ProfileController : ControllerBase
{
    private readonly IProfileService? _profileService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        ILogger<ProfileController> logger,
        IProfileService? profileService = null)
    {
        _logger = logger;
        _profileService = profileService;
    }

    // GET /api/v1/users/me/profile
    [HttpGet("me/profile")]
    [ProducesResponseType(typeof(MyProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_profileService is null)
        {
            return ServiceUnavailableProblem();
        }

        var profile = await _profileService.GetMyProfileAsync(userId, ct);
        if (profile is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found",
                detail: "User profile not found.");
        }

        return Ok(profile);
    }

    // PUT /api/v1/users/me/profile
    [HttpPut("me/profile")]
    [ProducesResponseType(typeof(MyProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_profileService is null)
        {
            return ServiceUnavailableProblem();
        }

        var (profile, result, errors) = await _profileService.UpdateMyProfileAsync(userId, request, ct);

        return result switch
        {
            ProfileOpResult.ValidationFailed => Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Validation failed", detail: string.Join(" ", errors ?? [])),
            ProfileOpResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Not found", detail: "User profile not found."),
            _ => Ok(profile),
        };
    }

    // GET /api/v1/users/{userId}/profile
    [HttpGet("{userId}/profile")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PublicProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetPublicProfile(string userId, CancellationToken ct)
    {
        if (_profileService is null)
        {
            return ServiceUnavailableProblem();
        }

        var profile = await _profileService.GetPublicProfileAsync(userId, ct);
        if (profile is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found",
                detail: "User profile not found.");
        }

        return Ok(profile);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private IActionResult UnauthorizedProblem() =>
        Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized",
            detail: "User identity could not be resolved.");

    private IActionResult ServiceUnavailableProblem() =>
        Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Service unavailable",
            detail: "The profile service is not available.");
}
