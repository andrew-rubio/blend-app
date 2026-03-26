using Blend.Api.Notifications.Models;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;

namespace Blend.Api.Notifications.Services;

/// <summary>
/// Abstraction for creating and querying notifications stored in Cosmos DB.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Creates a single notification for a recipient user.
    /// </summary>
    Task<Notification> CreateNotificationAsync(
        string recipientUserId,
        NotificationType type,
        string title,
        string message,
        string? actionUrl = null,
        string? sourceUserId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Creates notifications for multiple recipients in parallel (e.g., admin announcements).
    /// </summary>
    Task CreateBatchNotificationsAsync(
        IReadOnlyList<string> recipientUserIds,
        NotificationType type,
        string title,
        string message,
        string? actionUrl = null,
        string? sourceUserId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Returns a paged list of notifications for the given user, sorted by createdAt descending.
    /// </summary>
    Task<NotificationsPageResponse> GetNotificationsAsync(
        string userId,
        int pageSize,
        string? cursor,
        bool unreadOnly,
        CancellationToken ct = default);

    /// <summary>Returns the count of unread notifications for the given user.</summary>
    Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default);

    /// <summary>Marks a single notification as read. Returns false if not found or not owned by the user.</summary>
    Task<bool> MarkAsReadAsync(string userId, string notificationId, CancellationToken ct = default);

    /// <summary>Marks all notifications for the given user as read.</summary>
    Task MarkAllAsReadAsync(string userId, CancellationToken ct = default);
}
