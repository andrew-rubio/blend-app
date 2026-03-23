using Blend.Api.Friends.Models;
using Blend.Domain.Repositories;

namespace Blend.Api.Friends.Services;

/// <summary>Service interface for the friends/connections system.</summary>
public interface IFriendsService
{
    /// <summary>Returns the current user's accepted friends with cursor-based pagination.</summary>
    Task<FriendsPageResponse> GetFriendsAsync(
        string userId,
        int pageSize,
        string? cursor,
        CancellationToken ct = default);

    /// <summary>Returns pending incoming friend requests for the current user.</summary>
    Task<FriendRequestsPageResponse> GetIncomingRequestsAsync(
        string userId,
        int pageSize,
        string? cursor,
        CancellationToken ct = default);

    /// <summary>Returns pending outgoing friend requests sent by the current user.</summary>
    Task<FriendRequestsPageResponse> GetOutgoingRequestsAsync(
        string userId,
        int pageSize,
        string? cursor,
        CancellationToken ct = default);

    /// <summary>
    /// Sends a friend request from <paramref name="userId"/> to <paramref name="targetUserId"/>.
    /// </summary>
    /// <returns>The new connection id and an operation result.</returns>
    Task<(string? ConnectionId, FriendsOpResult Result)> SendFriendRequestAsync(
        string userId,
        string targetUserId,
        CancellationToken ct = default);

    /// <summary>
    /// Accepts the friend request identified by <paramref name="requestId"/> on behalf of
    /// <paramref name="userId"/>.
    /// </summary>
    Task<FriendsOpResult> AcceptFriendRequestAsync(
        string userId,
        string requestId,
        CancellationToken ct = default);

    /// <summary>
    /// Declines the friend request identified by <paramref name="requestId"/> on behalf of
    /// <paramref name="userId"/>.
    /// </summary>
    Task<FriendsOpResult> DeclineFriendRequestAsync(
        string userId,
        string requestId,
        CancellationToken ct = default);

    /// <summary>Removes the friendship between <paramref name="userId"/> and <paramref name="friendUserId"/>.</summary>
    Task<FriendsOpResult> RemoveFriendAsync(
        string userId,
        string friendUserId,
        CancellationToken ct = default);

    /// <summary>
    /// Searches users by display name and returns matches with their connection status relative to
    /// <paramref name="userId"/>.
    /// </summary>
    Task<UserSearchPageResponse> SearchUsersAsync(
        string userId,
        string query,
        int pageSize,
        string? cursor,
        CancellationToken ct = default);
}
