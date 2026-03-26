using Blend.Api.Notifications.Models;
using Blend.Api.Notifications.Services;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Blend.Tests.Unit.Notifications;

/// <summary>Unit tests for <see cref="NotificationService"/>.</summary>
public class NotificationServiceTests
{
    private static NotificationService CreateService(IRepository<Notification>? repo = null) =>
        new(NullLogger<NotificationService>.Instance, repo);

    private static Mock<IRepository<Notification>> CreateRepoMock() => new();

    // ── CreateNotificationAsync ───────────────────────────────────────────────

    [Fact]
    public async Task CreateNotification_WhenRepoNull_ReturnsUnsavedNotification()
    {
        var svc = CreateService();

        var result = await svc.CreateNotificationAsync(
            "user-1", NotificationType.System, "Title", "Hello");

        Assert.Equal("user-1", result.RecipientUserId);
        Assert.Equal(NotificationType.System, result.Type);
        Assert.Equal("Hello", result.Message);
        Assert.Equal(7776000, result.Ttl);
        Assert.False(result.Read);
    }

    [Fact]
    public async Task CreateNotification_WhenRepoAvailable_CallsCreateAsync()
    {
        var mock = CreateRepoMock();
        mock.Setup(r => r.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification n, CancellationToken _) => n);

        var svc = CreateService(mock.Object);

        await svc.CreateNotificationAsync(
            "user-2", NotificationType.FriendRequestReceived, "Friend request", "Alice wants to connect",
            actionUrl: "/friends", sourceUserId: "alice");

        mock.Verify(r => r.CreateAsync(
            It.Is<Notification>(n =>
                n.RecipientUserId == "user-2" &&
                n.Type == NotificationType.FriendRequestReceived &&
                n.SourceUserId == "alice" &&
                n.ActionUrl == "/friends" &&
                n.Ttl == 7776000),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateNotification_SetsTtlTo90Days()
    {
        var mock = CreateRepoMock();
        Notification? captured = null;
        mock.Setup(r => r.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, _) => captured = n)
            .ReturnsAsync((Notification n, CancellationToken _) => n);

        var svc = CreateService(mock.Object);
        await svc.CreateNotificationAsync("u1", NotificationType.System, "T", "M");

        Assert.NotNull(captured);
        Assert.Equal(7776000, captured.Ttl);
    }

    // ── CreateBatchNotificationsAsync ─────────────────────────────────────────

    [Fact]
    public async Task CreateBatchNotifications_WhenRepoNull_DoesNotThrow()
    {
        var svc = CreateService();
        // Should complete without exception
        await svc.CreateBatchNotificationsAsync(
            ["user-1", "user-2"], NotificationType.System, "T", "Announcement");
    }

    [Fact]
    public async Task CreateBatchNotifications_CreatesOneNotificationPerRecipient()
    {
        var mock = CreateRepoMock();
        var created = new List<Notification>();
        mock.Setup(r => r.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, _) => created.Add(n))
            .ReturnsAsync((Notification n, CancellationToken _) => n);

        var svc = CreateService(mock.Object);
        var recipients = new[] { "user-1", "user-2", "user-3" };
        await svc.CreateBatchNotificationsAsync(recipients, NotificationType.System, "T", "Batch");

        Assert.Equal(3, created.Count);
        Assert.Contains(created, n => n.RecipientUserId == "user-1");
        Assert.Contains(created, n => n.RecipientUserId == "user-2");
        Assert.Contains(created, n => n.RecipientUserId == "user-3");
    }

    // ── GetUnreadCountAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetUnreadCount_WhenRepoNull_ReturnsZero()
    {
        var svc = CreateService();
        var count = await svc.GetUnreadCountAsync("user-1");
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsCountFromRepo()
    {
        var mock = CreateRepoMock();
        var unreadNotifications = new List<Notification>
        {
            new() { Id = "n1", RecipientUserId = "user-1", Read = false, Title = "T", Message = "M", CreatedAt = DateTimeOffset.UtcNow },
            new() { Id = "n2", RecipientUserId = "user-1", Read = false, Title = "T", Message = "M", CreatedAt = DateTimeOffset.UtcNow },
        };

        mock.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Notification>)unreadNotifications);

        var svc = CreateService(mock.Object);
        var count = await svc.GetUnreadCountAsync("user-1");

        Assert.Equal(2, count);
    }

    // ── MarkAsReadAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task MarkAsRead_WhenRepoNull_ReturnsFalse()
    {
        var svc = CreateService();
        var result = await svc.MarkAsReadAsync("user-1", "notif-1");
        Assert.False(result);
    }

    [Fact]
    public async Task MarkAsRead_WhenNotificationNotFound_ReturnsFalse()
    {
        var mock = CreateRepoMock();
        mock.Setup(r => r.GetByIdAsync("notif-1", "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);

        var svc = CreateService(mock.Object);
        var result = await svc.MarkAsReadAsync("user-1", "notif-1");

        Assert.False(result);
    }

    [Fact]
    public async Task MarkAsRead_WhenNotificationAlreadyRead_ReturnsTrueWithoutPatch()
    {
        var mock = CreateRepoMock();
        var notification = new Notification
        {
            Id = "notif-1",
            RecipientUserId = "user-1",
            Read = true,
            Title = "T",
            Message = "M",
            CreatedAt = DateTimeOffset.UtcNow,
        };
        mock.Setup(r => r.GetByIdAsync("notif-1", "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var svc = CreateService(mock.Object);
        var result = await svc.MarkAsReadAsync("user-1", "notif-1");

        Assert.True(result);
        mock.Verify(r => r.PatchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkAsRead_WhenNotificationUnread_PatchesAndReturnsTrue()
    {
        var mock = CreateRepoMock();
        var notification = new Notification
        {
            Id = "notif-1",
            RecipientUserId = "user-1",
            Read = false,
            Title = "T",
            Message = "M",
            CreatedAt = DateTimeOffset.UtcNow,
        };
        mock.Setup(r => r.GetByIdAsync("notif-1", "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);
        mock.Setup(r => r.PatchAsync("notif-1", "user-1", It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Notification
            {
                Id = "notif-1",
                RecipientUserId = "user-1",
                Read = true,
                Title = "T",
                Message = "M",
                CreatedAt = notification.CreatedAt,
            });

        var svc = CreateService(mock.Object);
        var result = await svc.MarkAsReadAsync("user-1", "notif-1");

        Assert.True(result);
        mock.Verify(r => r.PatchAsync("notif-1", "user-1",
            It.Is<IReadOnlyDictionary<string, object?>>(d => d.ContainsKey("/read")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── MarkAllAsReadAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task MarkAllAsRead_WhenRepoNull_DoesNotThrow()
    {
        var svc = CreateService();
        await svc.MarkAllAsReadAsync("user-1");
    }

    [Fact]
    public async Task MarkAllAsRead_PatchesAllUnreadNotifications()
    {
        var mock = CreateRepoMock();
        var unread = new List<Notification>
        {
            new() { Id = "n1", RecipientUserId = "user-1", Read = false, Title = "T", Message = "M", CreatedAt = DateTimeOffset.UtcNow },
            new() { Id = "n2", RecipientUserId = "user-1", Read = false, Title = "T", Message = "M", CreatedAt = DateTimeOffset.UtcNow },
        };
        mock.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Notification>)unread);
        mock.Setup(r => r.PatchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Notification());

        var svc = CreateService(mock.Object);
        await svc.MarkAllAsReadAsync("user-1");

        mock.Verify(r => r.PatchAsync(It.IsAny<string>(), "user-1",
            It.Is<IReadOnlyDictionary<string, object?>>(d => d.ContainsKey("/read")),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    // ── GetNotificationsAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetNotifications_WhenRepoNull_ReturnsEmptyPage()
    {
        var svc = CreateService();
        var result = await svc.GetNotificationsAsync("user-1", 20, null, false);

        Assert.Empty(result.Items);
        Assert.Null(result.NextCursor);
        Assert.False(result.HasMore);
    }
}
