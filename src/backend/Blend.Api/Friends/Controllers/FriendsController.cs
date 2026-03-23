using System.Security.Claims;
using Blend.Api.Friends.Models;
using Blend.Api.Friends.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Friends.Controllers;

[ApiController]
[Authorize]
public sealed class FriendsController : ControllerBase
{
    private readonly IFriendsService? _friendsService;
    private readonly ILogger<FriendsController> _logger;

    public FriendsController(
        ILogger<FriendsController> logger,
        IFriendsService? friendsService = null)
    {
        _logger = logger;
        _friendsService = friendsService;
    }

    // GET /api/v1/friends
    [HttpGet("api/v1/friends")]
    [ProducesResponseType(typeof(FriendsPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetFriends(
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cursor = null,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_friendsService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _friendsService.GetFriendsAsync(userId, pageSize, cursor, ct);
        return Ok(result);
    }

    // GET /api/v1/friends/requests/incoming
    [HttpGet("api/v1/friends/requests/incoming")]
    [ProducesResponseType(typeof(FriendRequestsPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetIncomingRequests(
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cursor = null,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_friendsService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _friendsService.GetIncomingRequestsAsync(userId, pageSize, cursor, ct);
        return Ok(result);
    }

    // GET /api/v1/friends/requests/outgoing
    [HttpGet("api/v1/friends/requests/outgoing")]
    [ProducesResponseType(typeof(FriendRequestsPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetOutgoingRequests(
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cursor = null,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_friendsService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _friendsService.GetOutgoingRequestsAsync(userId, pageSize, cursor, ct);
        return Ok(result);
    }

    // POST /api/v1/friends/requests
    [HttpPost("api/v1/friends/requests")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SendFriendRequest(
        [FromBody] SendFriendRequestBody request,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_friendsService is null)
        {
            return ServiceUnavailableProblem();
        }

        if (string.IsNullOrWhiteSpace(request.TargetUserId))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Bad request",
                detail: "targetUserId is required.");
        }

        var (connectionId, result) = await _friendsService.SendFriendRequestAsync(userId, request.TargetUserId, ct);

        return result switch
        {
            FriendsOpResult.Success => CreatedAtAction(
                nameof(GetIncomingRequests),
                new { },
                new { connectionId }),
            FriendsOpResult.InvalidRequest => Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Bad request", detail: "You cannot send a friend request to yourself."),
            FriendsOpResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Not found", detail: "Target user not found."),
            FriendsOpResult.AlreadyExists => Problem(statusCode: StatusCodes.Status409Conflict,
                title: "Conflict", detail: "A connection with this user already exists."),
            FriendsOpResult.CooldownActive => Problem(statusCode: StatusCodes.Status409Conflict,
                title: "Conflict", detail: $"You must wait {30} days after a declined request before sending another."),
            _ => ServiceUnavailableProblem(),
        };
    }

    // POST /api/v1/friends/requests/{requestId}/accept
    [HttpPost("api/v1/friends/requests/{requestId}/accept")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> AcceptFriendRequest(string requestId, CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_friendsService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _friendsService.AcceptFriendRequestAsync(userId, requestId, ct);

        return result switch
        {
            FriendsOpResult.Success => NoContent(),
            FriendsOpResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Not found", detail: "Friend request not found."),
            FriendsOpResult.Forbidden => Problem(statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden", detail: "You cannot accept your own friend request."),
            FriendsOpResult.InvalidRequest => Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Bad request", detail: "This request is not in a pending state."),
            _ => ServiceUnavailableProblem(),
        };
    }

    // POST /api/v1/friends/requests/{requestId}/decline
    [HttpPost("api/v1/friends/requests/{requestId}/decline")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> DeclineFriendRequest(string requestId, CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_friendsService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _friendsService.DeclineFriendRequestAsync(userId, requestId, ct);

        return result switch
        {
            FriendsOpResult.Success => NoContent(),
            FriendsOpResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Not found", detail: "Friend request not found."),
            FriendsOpResult.InvalidRequest => Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Bad request", detail: "This request is not in a pending state."),
            _ => ServiceUnavailableProblem(),
        };
    }

    // DELETE /api/v1/friends/{friendUserId}
    [HttpDelete("api/v1/friends/{friendUserId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RemoveFriend(string friendUserId, CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_friendsService is null)
        {
            return ServiceUnavailableProblem();
        }

        var result = await _friendsService.RemoveFriendAsync(userId, friendUserId, ct);

        return result switch
        {
            FriendsOpResult.Success => NoContent(),
            FriendsOpResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Not found", detail: "Friend not found."),
            _ => ServiceUnavailableProblem(),
        };
    }

    // GET /api/v1/users/search?q={query}
    [HttpGet("api/v1/users/search")]
    [ProducesResponseType(typeof(UserSearchPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SearchUsers(
        [FromQuery] string? q,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cursor = null,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return UnauthorizedProblem();
        }

        if (_friendsService is null)
        {
            return ServiceUnavailableProblem();
        }

        if (string.IsNullOrWhiteSpace(q))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Bad request",
                detail: "Query parameter 'q' is required.");
        }

        var result = await _friendsService.SearchUsersAsync(userId, q, pageSize, cursor, ct);
        return Ok(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private IActionResult UnauthorizedProblem() =>
        Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized",
            detail: "User identity could not be resolved.");

    private IActionResult ServiceUnavailableProblem() =>
        Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Service unavailable",
            detail: "The friends service is not available.");
}
