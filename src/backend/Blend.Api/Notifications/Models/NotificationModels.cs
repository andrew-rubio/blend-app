using Blend.Domain.Entities;
using Blend.Domain.Repositories;

namespace Blend.Api.Notifications.Models;

/// <summary>Response returned by the unread-count endpoint.</summary>
/// <param name="Count">Number of unread notifications for the current user.</param>
public sealed record UnreadCountResponse(int Count);

/// <summary>A single notification item returned by the list endpoint.</summary>
public sealed class NotificationResponse
{
    public string Id { get; init; } = string.Empty;
    public NotificationType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? ActionUrl { get; init; }
    public string? SourceUserId { get; init; }
    public bool Read { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Maps a domain <see cref="Notification"/> to the API response model.</summary>
    public static NotificationResponse FromEntity(Notification n) => new()
    {
        Id = n.Id,
        Type = n.Type,
        Title = n.Title,
        Message = n.Message,
        ActionUrl = n.ActionUrl,
        SourceUserId = n.SourceUserId,
        Read = n.Read,
        CreatedAt = n.CreatedAt,
    };
}

/// <summary>Cursor-paged response of notifications.</summary>
public sealed class NotificationsPageResponse
{
    public IReadOnlyList<NotificationResponse> Items { get; init; } = [];
    public string? NextCursor { get; init; }
    public bool HasMore { get; init; }
}
