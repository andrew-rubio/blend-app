using System.Text;
using Blend.Api.Friends.Models;
using Blend.Domain.Entities;
using Blend.Domain.Identity;
using Blend.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Blend.Api.Friends.Services;

/// <summary>
/// Implements the friends/connections system backed by the Cosmos DB 'connections' and
/// 'notifications' containers, following the Profile &amp; Social FRD
/// (PROF-01 through PROF-06, PROF-27 through PROF-28).
/// </summary>
public sealed class FriendsService : IFriendsService
{
    private const int DeclinedCooldownDays = 30;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 50;

    /// <summary>Number of days a declined request must wait before the same user can re-send.</summary>
    public const int CooldownDays = DeclinedCooldownDays;

    private readonly IRepository<Connection>? _connectionRepository;
    private readonly IRepository<Notification>? _notificationRepository;
    private readonly IRepository<BlendUser>? _userRepository;
    private readonly UserManager<BlendUser>? _userManager;
    private readonly ILogger<FriendsService> _logger;

    public FriendsService(
        ILogger<FriendsService> logger,
        IRepository<Connection>? connectionRepository = null,
        IRepository<Notification>? notificationRepository = null,
        UserManager<BlendUser>? userManager = null,
        IRepository<BlendUser>? userRepository = null)
    {
        _logger = logger;
        _connectionRepository = connectionRepository;
        _notificationRepository = notificationRepository;
        _userManager = userManager;
        _userRepository = userRepository;
    }

    // ── List friends ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<FriendsPageResponse> GetFriendsAsync(
        string userId,
        int pageSize,
        string? cursor,
        CancellationToken ct = default)
    {
        if (_connectionRepository is null)
        {
            _logger.LogWarning("Connection repository unavailable; returning empty friends list for user {UserId}.", userId);
            return new FriendsPageResponse();
        }

        var clampedSize = Math.Clamp(pageSize, 1, MaxPageSize);
        var safeUserId = Sanitize(userId);

        var query = $"SELECT * FROM c WHERE c.userId = '{safeUserId}' AND c.status = 'Accepted' ORDER BY c.updatedAt DESC";

        var paged = await _connectionRepository.GetPagedAsync(
            query,
            new FeedPaginationOptions { PageSize = clampedSize, ContinuationToken = cursor },
            partitionKey: userId,
            ct);

        var friends = new List<FriendResponse>();
        foreach (var connection in paged.Items)
        {
            var friendProfile = await GetUserProfileAsync(connection.FriendUserId, ct);
            if (friendProfile is null)
            {
                continue;
            }

            friends.Add(new FriendResponse
            {
                UserId = connection.FriendUserId,
                DisplayName = friendProfile.DisplayName,
                AvatarUrl = friendProfile.ProfilePhotoUrl,
                RecipeCount = friendProfile.RecipeCount,
                ConnectedAt = connection.UpdatedAt,
            });
        }

        return new FriendsPageResponse
        {
            Items = friends,
            NextCursor = paged.ContinuationToken,
        };
    }

    // ── Incoming requests ─────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<FriendRequestsPageResponse> GetIncomingRequestsAsync(
        string userId,
        int pageSize,
        string? cursor,
        CancellationToken ct = default)
    {
        if (_connectionRepository is null)
        {
            _logger.LogWarning("Connection repository unavailable; returning empty incoming requests for user {UserId}.", userId);
            return new FriendRequestsPageResponse();
        }

        var clampedSize = Math.Clamp(pageSize, 1, MaxPageSize);
        var safeUserId = Sanitize(userId);

        var query =
            $"SELECT * FROM c WHERE c.userId = '{safeUserId}' " +
            $"AND c.status = 'Pending' " +
            $"AND c.initiatedBy != '{safeUserId}' " +
            "ORDER BY c.createdAt DESC";

        var paged = await _connectionRepository.GetPagedAsync(
            query,
            new FeedPaginationOptions { PageSize = clampedSize, ContinuationToken = cursor },
            partitionKey: userId,
            ct);

        return await MapToRequestsPageAsync(paged, ct);
    }

    // ── Outgoing requests ─────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<FriendRequestsPageResponse> GetOutgoingRequestsAsync(
        string userId,
        int pageSize,
        string? cursor,
        CancellationToken ct = default)
    {
        if (_connectionRepository is null)
        {
            _logger.LogWarning("Connection repository unavailable; returning empty outgoing requests for user {UserId}.", userId);
            return new FriendRequestsPageResponse();
        }

        var clampedSize = Math.Clamp(pageSize, 1, MaxPageSize);
        var safeUserId = Sanitize(userId);

        var query =
            $"SELECT * FROM c WHERE c.userId = '{safeUserId}' " +
            $"AND c.status = 'Pending' " +
            $"AND c.initiatedBy = '{safeUserId}' " +
            "ORDER BY c.createdAt DESC";

        var paged = await _connectionRepository.GetPagedAsync(
            query,
            new FeedPaginationOptions { PageSize = clampedSize, ContinuationToken = cursor },
            partitionKey: userId,
            ct);

        return await MapToRequestsPageAsync(paged, ct);
    }

    // ── Send request ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<(string? ConnectionId, FriendsOpResult Result)> SendFriendRequestAsync(
        string userId,
        string targetUserId,
        CancellationToken ct = default)
    {
        // Self-request guard — checked before repository availability
        if (string.Equals(userId, targetUserId, StringComparison.OrdinalIgnoreCase))
        {
            return (null, FriendsOpResult.InvalidRequest);
        }

        if (_connectionRepository is null)
        {
            _logger.LogWarning("Connection repository unavailable; cannot send friend request.");
            return (null, FriendsOpResult.ServiceUnavailable);
        }

        // Target user must exist
        if (_userManager is not null)
        {
            var target = await _userManager.FindByIdAsync(targetUserId);
            if (target is null)
            {
                return (null, FriendsOpResult.NotFound);
            }
        }

        // Check for an existing connection in the current user's partition
        var safeUserId = Sanitize(userId);
        var safeTargetId = Sanitize(targetUserId);
        var existingQuery =
            $"SELECT * FROM c WHERE c.userId = '{safeUserId}' AND c.friendUserId = '{safeTargetId}'";

        var existing = await _connectionRepository.GetByQueryAsync(existingQuery, partitionKey: userId, ct);
        if (existing.Count > 0)
        {
            var conn = existing[0];
            if (conn.Status == ConnectionStatus.Accepted)
            {
                return (null, FriendsOpResult.AlreadyExists);
            }

            if (conn.Status == ConnectionStatus.Pending)
            {
                return (null, FriendsOpResult.AlreadyExists);
            }

            if (conn.Status == ConnectionStatus.Declined)
            {
                // 30-day cooldown: the full 30 days must have passed after the decline
                var cooldownEnd = conn.UpdatedAt.AddDays(DeclinedCooldownDays);
                if (DateTimeOffset.UtcNow <= cooldownEnd)
                {
                    return (null, FriendsOpResult.CooldownActive);
                }

                // Cooldown expired — delete the stale declined documents before re-sending
                await DeleteBothSidesAsync(conn.Id, userId, targetUserId, ct);
            }
        }

        // Create mirrored connection documents
        var connectionId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;

        var initiatorDoc = new Connection
        {
            Id = connectionId,
            UserId = userId,
            FriendUserId = targetUserId,
            Status = ConnectionStatus.Pending,
            InitiatedBy = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var targetDoc = new Connection
        {
            Id = connectionId,
            UserId = targetUserId,
            FriendUserId = userId,
            Status = ConnectionStatus.Pending,
            InitiatedBy = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        try
        {
            await _connectionRepository.CreateAsync(initiatorDoc, ct);
            await _connectionRepository.CreateAsync(targetDoc, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create connection documents for {UserId} → {TargetUserId}.", userId, targetUserId);
            return (null, FriendsOpResult.ServiceUnavailable);
        }

        // Notify the target user
        await CreateNotificationAsync(
            recipientUserId: targetUserId,
            type: NotificationType.FriendRequestReceived,
            sourceUserId: userId,
            referenceId: connectionId,
            message: "sent you a friend request.",
            ct);

        return (connectionId, FriendsOpResult.Success);
    }

    // ── Accept request ────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<FriendsOpResult> AcceptFriendRequestAsync(
        string userId,
        string requestId,
        CancellationToken ct = default)
    {
        if (_connectionRepository is null)
        {
            _logger.LogWarning("Connection repository unavailable; cannot accept friend request.");
            return FriendsOpResult.ServiceUnavailable;
        }

        // Fetch the connection doc in the current user's partition
        var myDoc = await _connectionRepository.GetByIdAsync(requestId, userId, ct);
        if (myDoc is null)
        {
            return FriendsOpResult.NotFound;
        }

        if (myDoc.Status != ConnectionStatus.Pending)
        {
            return FriendsOpResult.InvalidRequest;
        }

        // Only the recipient (not the initiator) can accept
        if (string.Equals(myDoc.InitiatedBy, userId, StringComparison.OrdinalIgnoreCase))
        {
            return FriendsOpResult.Forbidden;
        }

        var now = DateTimeOffset.UtcNow;

        // Update my side
        var updatedMy = new Connection
        {
            Id = myDoc.Id,
            UserId = myDoc.UserId,
            FriendUserId = myDoc.FriendUserId,
            Status = ConnectionStatus.Accepted,
            InitiatedBy = myDoc.InitiatedBy,
            CreatedAt = myDoc.CreatedAt,
            UpdatedAt = now,
        };

        await _connectionRepository.UpdateAsync(updatedMy, requestId, userId, ct);

        // Update the initiator's mirror
        var initiatorId = myDoc.InitiatedBy;
        var initiatorDoc = await _connectionRepository.GetByIdAsync(requestId, initiatorId, ct);
        if (initiatorDoc is not null)
        {
            var updatedInitiator = new Connection
            {
                Id = initiatorDoc.Id,
                UserId = initiatorDoc.UserId,
                FriendUserId = initiatorDoc.FriendUserId,
                Status = ConnectionStatus.Accepted,
                InitiatedBy = initiatorDoc.InitiatedBy,
                CreatedAt = initiatorDoc.CreatedAt,
                UpdatedAt = now,
            };

            await _connectionRepository.UpdateAsync(updatedInitiator, requestId, initiatorId, ct);
        }

        // Notify the original sender that their request was accepted
        await CreateNotificationAsync(
            recipientUserId: initiatorId,
            type: NotificationType.FriendRequestAccepted,
            sourceUserId: userId,
            referenceId: requestId,
            message: "accepted your friend request.",
            ct);

        return FriendsOpResult.Success;
    }

    // ── Decline request ───────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<FriendsOpResult> DeclineFriendRequestAsync(
        string userId,
        string requestId,
        CancellationToken ct = default)
    {
        if (_connectionRepository is null)
        {
            _logger.LogWarning("Connection repository unavailable; cannot decline friend request.");
            return FriendsOpResult.ServiceUnavailable;
        }

        var myDoc = await _connectionRepository.GetByIdAsync(requestId, userId, ct);
        if (myDoc is null)
        {
            return FriendsOpResult.NotFound;
        }

        if (myDoc.Status != ConnectionStatus.Pending)
        {
            return FriendsOpResult.InvalidRequest;
        }

        var now = DateTimeOffset.UtcNow;

        // Update current user's doc to Declined
        var updatedMy = new Connection
        {
            Id = myDoc.Id,
            UserId = myDoc.UserId,
            FriendUserId = myDoc.FriendUserId,
            Status = ConnectionStatus.Declined,
            InitiatedBy = myDoc.InitiatedBy,
            CreatedAt = myDoc.CreatedAt,
            UpdatedAt = now,
        };
        await _connectionRepository.UpdateAsync(updatedMy, requestId, userId, ct);

        // Also update the mirror (the other party's doc) to Declined
        var otherUserId = myDoc.FriendUserId;
        var otherDoc = await _connectionRepository.GetByIdAsync(requestId, otherUserId, ct);
        if (otherDoc is not null)
        {
            var updatedOther = new Connection
            {
                Id = otherDoc.Id,
                UserId = otherDoc.UserId,
                FriendUserId = otherDoc.FriendUserId,
                Status = ConnectionStatus.Declined,
                InitiatedBy = otherDoc.InitiatedBy,
                CreatedAt = otherDoc.CreatedAt,
                UpdatedAt = now,
            };
            await _connectionRepository.UpdateAsync(updatedOther, requestId, otherUserId, ct);
        }

        return FriendsOpResult.Success;
    }

    // ── Remove friend ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<FriendsOpResult> RemoveFriendAsync(
        string userId,
        string friendUserId,
        CancellationToken ct = default)
    {
        if (_connectionRepository is null)
        {
            _logger.LogWarning("Connection repository unavailable; cannot remove friend.");
            return FriendsOpResult.ServiceUnavailable;
        }

        var safeUserId = Sanitize(userId);
        var safeFriendId = Sanitize(friendUserId);

        var query =
            $"SELECT * FROM c WHERE c.userId = '{safeUserId}' " +
            $"AND c.friendUserId = '{safeFriendId}' " +
            "AND c.status = 'Accepted'";

        var connections = await _connectionRepository.GetByQueryAsync(query, partitionKey: userId, ct);
        if (connections.Count == 0)
        {
            return FriendsOpResult.NotFound;
        }

        var connection = connections[0];
        await DeleteBothSidesAsync(connection.Id, userId, friendUserId, ct);

        return FriendsOpResult.Success;
    }

    // ── User search ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<UserSearchPageResponse> SearchUsersAsync(
        string userId,
        string query,
        int pageSize,
        string? cursor,
        CancellationToken ct = default)
    {
        if (_userRepository is null)
        {
            _logger.LogWarning("User repository unavailable; cannot search users.");
            return new UserSearchPageResponse();
        }

        var clampedSize = Math.Clamp(pageSize, 1, MaxPageSize);
        var cursorOffset = DecodeCursor(cursor);
        var safeQuery = Sanitize(query.Trim().ToLowerInvariant());

        // Query users whose displayName contains the search term (case-insensitive)
        var userQuery =
            $"SELECT * FROM c WHERE CONTAINS(LOWER(c.displayName), '{safeQuery}', true) " +
            $"AND c.id != '{Sanitize(userId)}' " +
            "ORDER BY c.displayName " +
            $"OFFSET {cursorOffset} LIMIT {clampedSize + 1}";

        IReadOnlyList<BlendUser> users;
        try
        {
            users = await _userRepository.GetByQueryAsync(userQuery, partitionKey: null, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "User search query failed; returning empty results.");
            return new UserSearchPageResponse();
        }

        var hasMore = users.Count > clampedSize;
        var pageUsers = hasMore ? users.Take(clampedSize).ToList() : users.ToList();

        // Fetch connections to determine status
        var connectionStatuses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (_connectionRepository is not null)
        {
            var safeUserId = Sanitize(userId);
            var connQuery = $"SELECT * FROM c WHERE c.userId = '{safeUserId}' AND c.status != 'Declined'";
            try
            {
                var connections = await _connectionRepository.GetByQueryAsync(connQuery, partitionKey: userId, ct);
                foreach (var conn in connections)
                {
                    connectionStatuses[conn.FriendUserId] = conn.Status == ConnectionStatus.Accepted ? "accepted" : "pending";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch connections for user {UserId} during search.", userId);
            }
        }

        var results = pageUsers.Select(u => new UserSearchResult
        {
            UserId = u.Id,
            DisplayName = u.DisplayName,
            AvatarUrl = u.ProfilePhotoUrl,
            RecipeCount = u.RecipeCount,
            ConnectionStatus = connectionStatuses.TryGetValue(u.Id, out var status) ? status : "none",
        }).ToList();

        var nextOffset = cursorOffset + pageUsers.Count;
        var nextCursor = hasMore ? EncodeCursor(nextOffset) : null;

        return new UserSearchPageResponse
        {
            Items = results,
            NextCursor = nextCursor,
        };
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<BlendUser?> GetUserProfileAsync(string userId, CancellationToken ct)
    {
        if (_userManager is null)
        {
            return null;
        }

        return await _userManager.FindByIdAsync(userId);
    }

    private async Task DeleteBothSidesAsync(
        string connectionId,
        string userAId,
        string userBId,
        CancellationToken ct)
    {
        try
        {
            await _connectionRepository!.DeleteAsync(connectionId, userAId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete connection {Id} from partition {UserId}.", connectionId, userAId);
        }

        try
        {
            await _connectionRepository!.DeleteAsync(connectionId, userBId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete connection {Id} from partition {UserId}.", connectionId, userBId);
        }
    }

    private async Task CreateNotificationAsync(
        string recipientUserId,
        NotificationType type,
        string sourceUserId,
        string referenceId,
        string message,
        CancellationToken ct)
    {
        if (_notificationRepository is null)
        {
            return;
        }

        var notification = new Notification
        {
            Id = Guid.NewGuid().ToString(),
            RecipientUserId = recipientUserId,
            Type = type,
            SourceUserId = sourceUserId,
            ReferenceId = referenceId,
            Message = message,
            Read = false,
            CreatedAt = DateTimeOffset.UtcNow,
            Ttl = -1,
        };

        try
        {
            await _notificationRepository.CreateAsync(notification, ct);
        }
        catch (Exception ex)
        {
            // Non-fatal — notifications are best-effort
            _logger.LogWarning(ex, "Failed to create {Type} notification for user {UserId}.", type, recipientUserId);
        }
    }

    private async Task<FriendRequestsPageResponse> MapToRequestsPageAsync(
        PagedResult<Connection> paged,
        CancellationToken ct)
    {
        var requests = new List<FriendRequestResponse>();
        foreach (var connection in paged.Items)
        {
            var otherUserId = connection.FriendUserId;
            var profile = await GetUserProfileAsync(otherUserId, ct);
            if (profile is null)
            {
                continue;
            }

            requests.Add(new FriendRequestResponse
            {
                RequestId = connection.Id,
                UserId = otherUserId,
                DisplayName = profile.DisplayName,
                AvatarUrl = profile.ProfilePhotoUrl,
                SentAt = connection.CreatedAt,
            });
        }

        return new FriendRequestsPageResponse
        {
            Items = requests,
            NextCursor = paged.ContinuationToken,
        };
    }

    /// <summary>Strips single quotes from a string to prevent Cosmos SQL injection.</summary>
    private static string Sanitize(string value) => value.Replace("'", string.Empty);

    private static string EncodeCursor(int offset) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(offset.ToString()));

    private static int DecodeCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return 0;
        }

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            return int.TryParse(decoded, out var offset) ? Math.Max(0, offset) : 0;
        }
        catch
        {
            return 0;
        }
    }
}
