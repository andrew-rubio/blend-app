using Blend.Api.Notifications.Models;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Blend.Api.Notifications.Services;

/// <summary>
/// Implements notification creation and querying backed by the Cosmos DB
/// <c>notifications</c> container, per PLAT-35 through PLAT-41.
/// </summary>
public sealed class NotificationService : INotificationService
{
    /// <summary>90 days in seconds — matches the container-level TTL.</summary>
    private const int TtlSeconds = 7776000;

    private readonly IRepository<Notification>? _repo;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        ILogger<NotificationService> logger,
        IRepository<Notification>? repo = null)
    {
        _logger = logger;
        _repo = repo;
    }

    /// <inheritdoc />
    public async Task<Notification> CreateNotificationAsync(
        string recipientUserId,
        NotificationType type,
        string title,
        string message,
        string? actionUrl = null,
        string? sourceUserId = null,
        CancellationToken ct = default)
    {
        if (_repo is null)
        {
            _logger.LogWarning("Notification repository is not available; skipping notification creation for {UserId}.", recipientUserId);
            return new Notification
            {
                Id = Guid.NewGuid().ToString(),
                RecipientUserId = recipientUserId,
                Type = type,
                Title = title,
                Message = message,
                ActionUrl = actionUrl,
                SourceUserId = sourceUserId,
                Read = false,
                CreatedAt = DateTimeOffset.UtcNow,
                Ttl = TtlSeconds,
            };
        }

        var notification = new Notification
        {
            Id = Guid.NewGuid().ToString(),
            RecipientUserId = recipientUserId,
            Type = type,
            Title = title,
            Message = message,
            ActionUrl = actionUrl,
            SourceUserId = sourceUserId,
            Read = false,
            CreatedAt = DateTimeOffset.UtcNow,
            Ttl = TtlSeconds,
        };

        return await _repo.CreateAsync(notification, ct);
    }

    /// <inheritdoc />
    public async Task CreateBatchNotificationsAsync(
        IReadOnlyList<string> recipientUserIds,
        NotificationType type,
        string title,
        string message,
        string? actionUrl = null,
        string? sourceUserId = null,
        CancellationToken ct = default)
    {
        if (_repo is null)
        {
            _logger.LogWarning("Notification repository is not available; skipping batch notification for {Count} users.", recipientUserIds.Count);
            return;
        }

        var tasks = recipientUserIds.Select(userId =>
            CreateNotificationAsync(userId, type, title, message, actionUrl, sourceUserId, ct));

        await Task.WhenAll(tasks);
    }

    /// <inheritdoc />
    public async Task<NotificationsPageResponse> GetNotificationsAsync(
        string userId,
        int pageSize,
        string? cursor,
        bool unreadOnly,
        CancellationToken ct = default)
    {
        if (_repo is null)
        {
            return new NotificationsPageResponse();
        }

        var clampedSize = Math.Clamp(pageSize, 1, 100);

        var whereClause = unreadOnly
            ? "WHERE n.recipientUserId = @userId AND n.read = false"
            : "WHERE n.recipientUserId = @userId";

        var query = $"SELECT * FROM n {whereClause} ORDER BY n.createdAt DESC";

        var options = new FeedPaginationOptions
        {
            PageSize = clampedSize,
            ContinuationToken = cursor,
        };

        var page = await _repo.GetPagedAsync(query, options, userId, ct);

        return new NotificationsPageResponse
        {
            Items = page.Items.Select(NotificationResponse.FromEntity).ToList(),
            NextCursor = page.ContinuationToken,
            HasMore = page.HasNextPage,
        };
    }

    /// <inheritdoc />
    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default)
    {
        if (_repo is null)
        {
            return 0;
        }

        var query = $"SELECT VALUE COUNT(1) FROM n WHERE n.recipientUserId = @userId AND n.read = false";
        var results = await _repo.GetByQueryAsync(query, userId, ct);

        // The query returns COUNT(1) as a scalar — but since IRepository<T> returns T (Notification),
        // we fall back to counting the list result for the unread notifications query.
        var countQuery = "SELECT * FROM n WHERE n.recipientUserId = @userId AND n.read = false";
        var notifications = await _repo.GetByQueryAsync(countQuery, userId, ct);
        return notifications.Count;
    }

    /// <inheritdoc />
    public async Task<bool> MarkAsReadAsync(string userId, string notificationId, CancellationToken ct = default)
    {
        if (_repo is null)
        {
            return false;
        }

        var notification = await _repo.GetByIdAsync(notificationId, userId, ct);
        if (notification is null)
        {
            return false;
        }

        if (notification.Read)
        {
            return true;
        }

        var patches = new Dictionary<string, object?>
        {
            ["/read"] = true,
        };

        await _repo.PatchAsync(notificationId, userId, patches, ct);
        return true;
    }

    /// <inheritdoc />
    public async Task MarkAllAsReadAsync(string userId, CancellationToken ct = default)
    {
        if (_repo is null)
        {
            return;
        }

        var query = "SELECT * FROM n WHERE n.recipientUserId = @userId AND n.read = false";
        var unread = await _repo.GetByQueryAsync(query, userId, ct);

        var patches = new Dictionary<string, object?>
        {
            ["/read"] = true,
        };

        var tasks = unread.Select(n => _repo.PatchAsync(n.Id, userId, patches, ct));
        await Task.WhenAll(tasks);
    }
}
