using Blend.Api.Friends.Models;
using Blend.Api.Friends.Services;
using Blend.Domain.Entities;
using Blend.Domain.Identity;
using Blend.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Blend.Tests.Unit.Friends;

/// <summary>Unit tests for FriendsService business logic.</summary>
public class FriendsServiceTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Mock<IRepository<Connection>> MockConnectionRepo() => new();
    private static Mock<IRepository<Notification>> MockNotificationRepo() => new();

    private static Mock<UserManager<BlendUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<BlendUser>>();
        // IQueryableUserStore is required for .Users
        var queryableStore = store.As<IQueryableUserStore<BlendUser>>();
        queryableStore.Setup(s => s.Users).Returns(new List<BlendUser>().AsQueryable());
        return new Mock<UserManager<BlendUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static FriendsService CreateService(
        Mock<IRepository<Connection>>? connectionRepo = null,
        Mock<IRepository<Notification>>? notificationRepo = null,
        Mock<UserManager<BlendUser>>? userManager = null,
        Mock<IRepository<BlendUser>>? userRepo = null)
    {
        return new FriendsService(
            NullLogger<FriendsService>.Instance,
            connectionRepo?.Object,
            notificationRepo?.Object,
            userManager?.Object,
            userRepo?.Object);
    }

    private static Connection MakeConnection(
        string id,
        string userId,
        string friendUserId,
        ConnectionStatus status,
        string? initiatedBy = null,
        DateTimeOffset? updatedAt = null)
    {
        return new Connection
        {
            Id = id,
            UserId = userId,
            FriendUserId = friendUserId,
            Status = status,
            InitiatedBy = initiatedBy ?? userId,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow.AddDays(-1),
        };
    }

    // ── SendFriendRequestAsync — validation ───────────────────────────────────

    [Fact]
    public async Task SendFriendRequest_SelfRequest_ReturnsInvalidRequest()
    {
        var svc = CreateService();
        var (_, result) = await svc.SendFriendRequestAsync("user-1", "user-1");
        Assert.Equal(FriendsOpResult.InvalidRequest, result);
    }

    [Fact]
    public async Task SendFriendRequest_TargetNotFound_ReturnsNotFound()
    {
        var userMgr = MockUserManager();
        userMgr.Setup(m => m.FindByIdAsync("user-2")).ReturnsAsync((BlendUser?)null);

        var connRepo = MockConnectionRepo();
        connRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Connection>());

        var svc = CreateService(connRepo, userManager: userMgr);
        var (_, result) = await svc.SendFriendRequestAsync("user-1", "user-2");
        Assert.Equal(FriendsOpResult.NotFound, result);
    }

    [Fact]
    public async Task SendFriendRequest_AlreadyAccepted_ReturnsAlreadyExists()
    {
        var userMgr = MockUserManager();
        userMgr.Setup(m => m.FindByIdAsync("user-2"))
            .ReturnsAsync(new BlendUser { Id = "user-2", DisplayName = "User 2" });

        var existing = new List<Connection>
        {
            MakeConnection("conn-1", "user-1", "user-2", ConnectionStatus.Accepted),
        };

        var connRepo = MockConnectionRepo();
        connRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var svc = CreateService(connRepo, userManager: userMgr);
        var (_, result) = await svc.SendFriendRequestAsync("user-1", "user-2");
        Assert.Equal(FriendsOpResult.AlreadyExists, result);
    }

    [Fact]
    public async Task SendFriendRequest_AlreadyPending_ReturnsAlreadyExists()
    {
        var userMgr = MockUserManager();
        userMgr.Setup(m => m.FindByIdAsync("user-2"))
            .ReturnsAsync(new BlendUser { Id = "user-2", DisplayName = "User 2" });

        var existing = new List<Connection>
        {
            MakeConnection("conn-1", "user-1", "user-2", ConnectionStatus.Pending),
        };

        var connRepo = MockConnectionRepo();
        connRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var svc = CreateService(connRepo, userManager: userMgr);
        var (_, result) = await svc.SendFriendRequestAsync("user-1", "user-2");
        Assert.Equal(FriendsOpResult.AlreadyExists, result);
    }

    [Fact]
    public async Task SendFriendRequest_DeclinedWithinCooldown_ReturnsCooldownActive()
    {
        var userMgr = MockUserManager();
        userMgr.Setup(m => m.FindByIdAsync("user-2"))
            .ReturnsAsync(new BlendUser { Id = "user-2", DisplayName = "User 2" });

        // Declined only 10 days ago — still in cooldown
        var existing = new List<Connection>
        {
            MakeConnection("conn-1", "user-1", "user-2", ConnectionStatus.Declined,
                updatedAt: DateTimeOffset.UtcNow.AddDays(-10)),
        };

        var connRepo = MockConnectionRepo();
        connRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var svc = CreateService(connRepo, userManager: userMgr);
        var (_, result) = await svc.SendFriendRequestAsync("user-1", "user-2");
        Assert.Equal(FriendsOpResult.CooldownActive, result);
    }

    [Fact]
    public async Task SendFriendRequest_DeclinedCooldownExpired_ReturnsSuccess()
    {
        var userMgr = MockUserManager();
        userMgr.Setup(m => m.FindByIdAsync("user-2"))
            .ReturnsAsync(new BlendUser { Id = "user-2", DisplayName = "User 2" });

        // Declined 31 days ago — cooldown has expired
        var existing = new List<Connection>
        {
            MakeConnection("conn-1", "user-1", "user-2", ConnectionStatus.Declined,
                updatedAt: DateTimeOffset.UtcNow.AddDays(-31)),
        };

        var connRepo = MockConnectionRepo();
        connRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        connRepo.Setup(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        connRepo.Setup(r => r.CreateAsync(It.IsAny<Connection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Connection c, CancellationToken _) => c);

        var notifRepo = MockNotificationRepo();
        notifRepo.Setup(r => r.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification n, CancellationToken _) => n);

        var svc = CreateService(connRepo, notifRepo, userMgr);
        var (connectionId, result) = await svc.SendFriendRequestAsync("user-1", "user-2");
        Assert.Equal(FriendsOpResult.Success, result);
        Assert.NotNull(connectionId);
    }

    [Fact]
    public async Task SendFriendRequest_NoExistingConnection_CreatesMirroredDocuments()
    {
        var userMgr = MockUserManager();
        userMgr.Setup(m => m.FindByIdAsync("user-2"))
            .ReturnsAsync(new BlendUser { Id = "user-2", DisplayName = "User 2" });

        var connRepo = MockConnectionRepo();
        connRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Connection>());

        var createdDocs = new List<Connection>();
        connRepo.Setup(r => r.CreateAsync(It.IsAny<Connection>(), It.IsAny<CancellationToken>()))
            .Callback<Connection, CancellationToken>((c, _) => createdDocs.Add(c))
            .ReturnsAsync((Connection c, CancellationToken _) => c);

        var notifRepo = MockNotificationRepo();
        notifRepo.Setup(r => r.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification n, CancellationToken _) => n);

        var svc = CreateService(connRepo, notifRepo, userMgr);
        var (connectionId, result) = await svc.SendFriendRequestAsync("user-1", "user-2");

        Assert.Equal(FriendsOpResult.Success, result);
        Assert.NotNull(connectionId);
        // Two mirrored documents must be created
        Assert.Equal(2, createdDocs.Count);
        // Both docs share the same connection id
        Assert.All(createdDocs, d => Assert.Equal(connectionId, d.Id));
        // One doc per user partition
        Assert.Contains(createdDocs, d => d.UserId == "user-1" && d.FriendUserId == "user-2");
        Assert.Contains(createdDocs, d => d.UserId == "user-2" && d.FriendUserId == "user-1");
        // Both are pending, initiatedBy is the sender
        Assert.All(createdDocs, d => Assert.Equal(ConnectionStatus.Pending, d.Status));
        Assert.All(createdDocs, d => Assert.Equal("user-1", d.InitiatedBy));
    }

    [Fact]
    public async Task SendFriendRequest_ServiceUnavailable_WhenRepositoryIsNull()
    {
        // No repository injected
        var svc = CreateService();
        var (_, result) = await svc.SendFriendRequestAsync("user-1", "user-2");
        Assert.Equal(FriendsOpResult.ServiceUnavailable, result);
    }

    [Fact]
    public async Task SendFriendRequest_CreatesNotificationForTarget()
    {
        var userMgr = MockUserManager();
        userMgr.Setup(m => m.FindByIdAsync("user-2"))
            .ReturnsAsync(new BlendUser { Id = "user-2", DisplayName = "User 2" });

        var connRepo = MockConnectionRepo();
        connRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Connection>());
        connRepo.Setup(r => r.CreateAsync(It.IsAny<Connection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Connection c, CancellationToken _) => c);

        var createdNotifications = new List<Notification>();
        var notifRepo = MockNotificationRepo();
        notifRepo.Setup(r => r.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, _) => createdNotifications.Add(n))
            .ReturnsAsync((Notification n, CancellationToken _) => n);

        var svc = CreateService(connRepo, notifRepo, userMgr);
        await svc.SendFriendRequestAsync("user-1", "user-2");

        Assert.Single(createdNotifications);
        Assert.Equal("user-2", createdNotifications[0].RecipientUserId);
        Assert.Equal(NotificationType.FriendRequestReceived, createdNotifications[0].Type);
        Assert.Equal("user-1", createdNotifications[0].SourceUserId);
    }

    // ── AcceptFriendRequestAsync ───────────────────────────────────────────────

    [Fact]
    public async Task AcceptFriendRequest_NotFound_ReturnsNotFound()
    {
        var connRepo = MockConnectionRepo();
        connRepo.Setup(r => r.GetByIdAsync("req-1", "user-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Connection?)null);

        var svc = CreateService(connRepo);
        var result = await svc.AcceptFriendRequestAsync("user-2", "req-1");
        Assert.Equal(FriendsOpResult.NotFound, result);
    }

    [Fact]
    public async Task AcceptFriendRequest_OwnRequest_ReturnsForbidden()
    {
        var connRepo = MockConnectionRepo();
        var doc = MakeConnection("req-1", "user-1", "user-2", ConnectionStatus.Pending, initiatedBy: "user-1");
        connRepo.Setup(r => r.GetByIdAsync("req-1", "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(doc);

        var svc = CreateService(connRepo);
        var result = await svc.AcceptFriendRequestAsync("user-1", "req-1");
        Assert.Equal(FriendsOpResult.Forbidden, result);
    }

    [Fact]
    public async Task AcceptFriendRequest_NotPending_ReturnsInvalidRequest()
    {
        var connRepo = MockConnectionRepo();
        var doc = MakeConnection("req-1", "user-2", "user-1", ConnectionStatus.Accepted, initiatedBy: "user-1");
        connRepo.Setup(r => r.GetByIdAsync("req-1", "user-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(doc);

        var svc = CreateService(connRepo);
        var result = await svc.AcceptFriendRequestAsync("user-2", "req-1");
        Assert.Equal(FriendsOpResult.InvalidRequest, result);
    }

    [Fact]
    public async Task AcceptFriendRequest_Valid_UpdatesBothSidesAndCreatesNotification()
    {
        var connRepo = MockConnectionRepo();
        var notifRepo = MockNotificationRepo();

        // user-2's partition document
        var myDoc = MakeConnection("req-1", "user-2", "user-1", ConnectionStatus.Pending, initiatedBy: "user-1");
        connRepo.Setup(r => r.GetByIdAsync("req-1", "user-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(myDoc);

        // user-1's partition document (the initiator's mirror)
        var initiatorDoc = MakeConnection("req-1", "user-1", "user-2", ConnectionStatus.Pending, initiatedBy: "user-1");
        connRepo.Setup(r => r.GetByIdAsync("req-1", "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(initiatorDoc);

        var updatedDocs = new List<(string Id, string PartitionKey)>();
        connRepo.Setup(r => r.UpdateAsync(It.IsAny<Connection>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<Connection, string, string, CancellationToken>((conn, id, pk, _) => updatedDocs.Add((id, pk)))
            .ReturnsAsync((Connection c, string _, string __, CancellationToken ___) => c);

        var notifications = new List<Notification>();
        notifRepo.Setup(r => r.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, _) => notifications.Add(n))
            .ReturnsAsync((Notification n, CancellationToken _) => n);

        var svc = CreateService(connRepo, notifRepo);
        var result = await svc.AcceptFriendRequestAsync("user-2", "req-1");

        Assert.Equal(FriendsOpResult.Success, result);
        // Both sides updated
        Assert.Equal(2, updatedDocs.Count);
        Assert.Contains(updatedDocs, u => u.PartitionKey == "user-2");
        Assert.Contains(updatedDocs, u => u.PartitionKey == "user-1");
        // Notification sent to initiator
        Assert.Single(notifications);
        Assert.Equal("user-1", notifications[0].RecipientUserId);
        Assert.Equal(NotificationType.FriendRequestAccepted, notifications[0].Type);
    }

    // ── DeclineFriendRequestAsync ─────────────────────────────────────────────

    [Fact]
    public async Task DeclineFriendRequest_NotFound_ReturnsNotFound()
    {
        var connRepo = MockConnectionRepo();
        connRepo.Setup(r => r.GetByIdAsync("req-1", "user-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Connection?)null);

        var svc = CreateService(connRepo);
        var result = await svc.DeclineFriendRequestAsync("user-2", "req-1");
        Assert.Equal(FriendsOpResult.NotFound, result);
    }

    [Fact]
    public async Task DeclineFriendRequest_NotPending_ReturnsInvalidRequest()
    {
        var connRepo = MockConnectionRepo();
        var doc = MakeConnection("req-1", "user-2", "user-1", ConnectionStatus.Accepted, initiatedBy: "user-1");
        connRepo.Setup(r => r.GetByIdAsync("req-1", "user-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(doc);

        var svc = CreateService(connRepo);
        var result = await svc.DeclineFriendRequestAsync("user-2", "req-1");
        Assert.Equal(FriendsOpResult.InvalidRequest, result);
    }

    [Fact]
    public async Task DeclineFriendRequest_Valid_UpdatesBothSides()
    {
        var connRepo = MockConnectionRepo();

        var myDoc = MakeConnection("req-1", "user-2", "user-1", ConnectionStatus.Pending, initiatedBy: "user-1");
        connRepo.Setup(r => r.GetByIdAsync("req-1", "user-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(myDoc);

        var otherDoc = MakeConnection("req-1", "user-1", "user-2", ConnectionStatus.Pending, initiatedBy: "user-1");
        connRepo.Setup(r => r.GetByIdAsync("req-1", "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherDoc);

        var updatedConnections = new List<Connection>();
        connRepo.Setup(r => r.UpdateAsync(It.IsAny<Connection>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<Connection, string, string, CancellationToken>((c, _, __, ___) => updatedConnections.Add(c))
            .ReturnsAsync((Connection c, string _, string __, CancellationToken ___) => c);

        var svc = CreateService(connRepo);
        var result = await svc.DeclineFriendRequestAsync("user-2", "req-1");

        Assert.Equal(FriendsOpResult.Success, result);
        Assert.Equal(2, updatedConnections.Count);
        Assert.All(updatedConnections, c => Assert.Equal(ConnectionStatus.Declined, c.Status));
    }

    // ── RemoveFriendAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveFriend_NoAcceptedConnection_ReturnsNotFound()
    {
        var connRepo = MockConnectionRepo();
        connRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Connection>());

        var svc = CreateService(connRepo);
        var result = await svc.RemoveFriendAsync("user-1", "user-2");
        Assert.Equal(FriendsOpResult.NotFound, result);
    }

    [Fact]
    public async Task RemoveFriend_ExistingFriendship_DeletesBothDocuments()
    {
        var connRepo = MockConnectionRepo();
        var conn = MakeConnection("conn-1", "user-1", "user-2", ConnectionStatus.Accepted);
        connRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Connection> { conn });

        var deletedKeys = new List<(string Id, string Pk)>();
        connRepo.Setup(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((id, pk, _) => deletedKeys.Add((id, pk)))
            .Returns(Task.CompletedTask);

        var svc = CreateService(connRepo);
        var result = await svc.RemoveFriendAsync("user-1", "user-2");

        Assert.Equal(FriendsOpResult.Success, result);
        Assert.Equal(2, deletedKeys.Count);
        Assert.Contains(deletedKeys, d => d.Pk == "user-1");
        Assert.Contains(deletedKeys, d => d.Pk == "user-2");
    }

    // ── GetFriendsAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetFriendsAsync_RepositoryUnavailable_ReturnsEmptyList()
    {
        var svc = CreateService(); // no repo
        var result = await svc.GetFriendsAsync("user-1", 20, null);
        Assert.Empty(result.Items);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    public async Task GetFriendsAsync_WithAcceptedConnections_ReturnsFriendProfiles()
    {
        var connRepo = MockConnectionRepo();
        var userMgr = MockUserManager();

        var conn = MakeConnection("conn-1", "user-1", "user-2", ConnectionStatus.Accepted);
        connRepo.Setup(r => r.GetPagedAsync(It.IsAny<string>(), It.IsAny<FeedPaginationOptions>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Connection> { Items = new List<Connection> { conn } });

        userMgr.Setup(m => m.FindByIdAsync("user-2"))
            .ReturnsAsync(new BlendUser { Id = "user-2", DisplayName = "Friend User", RecipeCount = 5 });

        var svc = CreateService(connRepo, userManager: userMgr);
        var result = await svc.GetFriendsAsync("user-1", 20, null);

        Assert.Single(result.Items);
        Assert.Equal("user-2", result.Items[0].UserId);
        Assert.Equal("Friend User", result.Items[0].DisplayName);
        Assert.Equal(5, result.Items[0].RecipeCount);
    }

    // ── GetIncomingRequestsAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetIncomingRequestsAsync_RepositoryUnavailable_ReturnsEmptyList()
    {
        var svc = CreateService();
        var result = await svc.GetIncomingRequestsAsync("user-1", 20, null);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetIncomingRequestsAsync_WithPendingRequests_ReturnsRequests()
    {
        var connRepo = MockConnectionRepo();
        var userMgr = MockUserManager();

        var conn = MakeConnection("req-1", "user-1", "user-2", ConnectionStatus.Pending, initiatedBy: "user-2");
        connRepo.Setup(r => r.GetPagedAsync(It.IsAny<string>(), It.IsAny<FeedPaginationOptions>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Connection> { Items = new List<Connection> { conn } });

        userMgr.Setup(m => m.FindByIdAsync("user-2"))
            .ReturnsAsync(new BlendUser { Id = "user-2", DisplayName = "Sender" });

        var svc = CreateService(connRepo, userManager: userMgr);
        var result = await svc.GetIncomingRequestsAsync("user-1", 20, null);

        Assert.Single(result.Items);
        Assert.Equal("req-1", result.Items[0].RequestId);
        Assert.Equal("user-2", result.Items[0].UserId);
    }

    // ── GetOutgoingRequestsAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetOutgoingRequestsAsync_RepositoryUnavailable_ReturnsEmptyList()
    {
        var svc = CreateService();
        var result = await svc.GetOutgoingRequestsAsync("user-1", 20, null);
        Assert.Empty(result.Items);
    }

    // ── SearchUsersAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task SearchUsersAsync_UserRepoUnavailable_ReturnsEmptyList()
    {
        var svc = CreateService(); // no userRepo
        var result = await svc.SearchUsersAsync("user-1", "Alice", 20, null);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task SearchUsersAsync_ExcludesCurrentUser()
    {
        var userRepo = new Mock<IRepository<BlendUser>>();
        userRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BlendUser>
            {
                new() { Id = "user-2", DisplayName = "Alice Other" },
            });

        var connRepo = MockConnectionRepo();
        connRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Connection>());

        var svc = CreateService(connRepo, userRepo: userRepo);
        var result = await svc.SearchUsersAsync("user-1", "Alice", 20, null);

        // Query excludes current user via SQL — returned results should not include user-1
        Assert.DoesNotContain(result.Items, u => u.UserId == "user-1");
    }

    [Fact]
    public async Task SearchUsersAsync_WithExistingConnection_ReturnsCorrectStatus()
    {
        var userRepo = new Mock<IRepository<BlendUser>>();
        userRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BlendUser>
            {
                new() { Id = "user-2", DisplayName = "Bob" },
            });

        var connRepo = MockConnectionRepo();
        var conn = MakeConnection("conn-1", "user-1", "user-2", ConnectionStatus.Accepted);
        connRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Connection> { conn });

        var svc = CreateService(connRepo, userRepo: userRepo);
        var result = await svc.SearchUsersAsync("user-1", "bob", 20, null);

        Assert.Single(result.Items);
        Assert.Equal("accepted", result.Items[0].ConnectionStatus);
    }

    [Fact]
    public async Task SearchUsersAsync_PendingConnection_ReturnsPendingStatus()
    {
        var userRepo = new Mock<IRepository<BlendUser>>();
        userRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BlendUser>
            {
                new() { Id = "user-2", DisplayName = "Carol" },
            });

        var connRepo = MockConnectionRepo();
        var conn = MakeConnection("conn-1", "user-1", "user-2", ConnectionStatus.Pending);
        connRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Connection> { conn });

        var svc = CreateService(connRepo, userRepo: userRepo);
        var result = await svc.SearchUsersAsync("user-1", "carol", 20, null);

        Assert.Single(result.Items);
        Assert.Equal("pending", result.Items[0].ConnectionStatus);
    }

    [Fact]
    public async Task SearchUsersAsync_NoExistingConnection_ReturnsNoneStatus()
    {
        var userRepo = new Mock<IRepository<BlendUser>>();
        userRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BlendUser>
            {
                new() { Id = "user-2", DisplayName = "Dan" },
            });

        var connRepo = MockConnectionRepo();
        connRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Connection>());

        var svc = CreateService(connRepo, userRepo: userRepo);
        var result = await svc.SearchUsersAsync("user-1", "dan", 20, null);

        Assert.Single(result.Items);
        Assert.Equal("none", result.Items[0].ConnectionStatus);
    }

    // ── Cooldown boundary tests ────────────────────────────────────────────────

    [Fact]
    public async Task SendFriendRequest_DeclinedExactly30DaysAgo_StillInCooldown()
    {
        var userMgr = MockUserManager();
        userMgr.Setup(m => m.FindByIdAsync("user-2"))
            .ReturnsAsync(new BlendUser { Id = "user-2", DisplayName = "User 2" });

        // Declined 29 days ago — clearly still in cooldown (must wait 30+ days)
        var existing = new List<Connection>
        {
            MakeConnection("conn-1", "user-1", "user-2", ConnectionStatus.Declined,
                updatedAt: DateTimeOffset.UtcNow.AddDays(-29)),
        };

        var connRepo = MockConnectionRepo();
        connRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var svc = CreateService(connRepo, userManager: userMgr);
        var (_, result) = await svc.SendFriendRequestAsync("user-1", "user-2");
        Assert.Equal(FriendsOpResult.CooldownActive, result);
    }

    [Fact]
    public async Task SendFriendRequest_DeclinedMoreThan30DaysAgo_AllowsReSend()
    {
        var userMgr = MockUserManager();
        userMgr.Setup(m => m.FindByIdAsync("user-2"))
            .ReturnsAsync(new BlendUser { Id = "user-2", DisplayName = "User 2" });

        // Declined 31 days ago — cooldown has definitely passed
        var existing = new List<Connection>
        {
            MakeConnection("conn-1", "user-1", "user-2", ConnectionStatus.Declined,
                updatedAt: DateTimeOffset.UtcNow.AddDays(-31)),
        };

        var connRepo = MockConnectionRepo();
        connRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        connRepo.Setup(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        connRepo.Setup(r => r.CreateAsync(It.IsAny<Connection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Connection c, CancellationToken _) => c);

        var notifRepo = MockNotificationRepo();
        notifRepo.Setup(r => r.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification n, CancellationToken _) => n);

        var svc = CreateService(connRepo, notifRepo, userMgr);
        var (_, result) = await svc.SendFriendRequestAsync("user-1", "user-2");
        Assert.Equal(FriendsOpResult.Success, result);
    }
}
