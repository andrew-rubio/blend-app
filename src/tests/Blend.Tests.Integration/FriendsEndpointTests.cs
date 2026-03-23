using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Blend.Api.Auth.Models;
using Blend.Domain.Entities;
using Blend.Domain.Identity;
using Blend.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blend.Tests.Integration;

// ── In-memory connection repository ──────────────────────────────────────────

public sealed class InMemoryConnectionRepository : IRepository<Connection>
{
    private readonly ConcurrentDictionary<string, Connection> _docs = new();

    private static string Key(string id, string partitionKey) => $"{partitionKey}:{id}";

    public Task<Connection?> GetByIdAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        _docs.TryGetValue(Key(id, partitionKey), out var doc);
        return Task.FromResult(doc);
    }

    public Task<IReadOnlyList<Connection>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        // Simple in-memory query simulation — filter by userId and status
        IEnumerable<Connection> results = _docs.Values;
        if (partitionKey is not null)
        {
            results = results.Where(c => c.UserId == partitionKey);
        }

        // Parse basic WHERE conditions from query
        if (query.Contains("c.status = 'Accepted'"))
        {
            results = results.Where(c => c.Status == ConnectionStatus.Accepted);
        }
        else if (query.Contains("c.status = 'Pending'") && query.Contains("c.initiatedBy !="))
        {
            // incoming: pending AND initiatedBy != userId
            results = results.Where(c => c.Status == ConnectionStatus.Pending && c.InitiatedBy != partitionKey);
        }
        else if (query.Contains("c.status = 'Pending'") && query.Contains("c.initiatedBy ="))
        {
            // outgoing: pending AND initiatedBy = userId
            results = results.Where(c => c.Status == ConnectionStatus.Pending && c.InitiatedBy == partitionKey);
        }
        else if (query.Contains("c.status != 'Declined'"))
        {
            results = results.Where(c => c.Status != ConnectionStatus.Declined);
        }

        // Filter by friendUserId if present
        if (query.Contains("c.friendUserId = '"))
        {
            var start = query.IndexOf("c.friendUserId = '") + "c.friendUserId = '".Length;
            var end = query.IndexOf("'", start);
            if (end > start)
            {
                var friendId = query[start..end];
                results = results.Where(c => c.FriendUserId == friendId);
            }
        }

        IReadOnlyList<Connection> list = results.OrderByDescending(c => c.CreatedAt).ToList();
        return Task.FromResult(list);
    }

    public Task<Connection> CreateAsync(Connection entity, CancellationToken cancellationToken = default)
    {
        _docs[Key(entity.Id, entity.UserId)] = entity;
        return Task.FromResult(entity);
    }

    public Task<Connection> UpdateAsync(Connection entity, string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        _docs[Key(id, partitionKey)] = entity;
        return Task.FromResult(entity);
    }

    public Task<Connection> PatchAsync(string id, string partitionKey, IReadOnlyDictionary<string, object?> patches, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        _docs.TryRemove(Key(id, partitionKey), out _);
        return Task.CompletedTask;
    }

    public Task<PagedResult<Connection>> GetPagedAsync(string query, FeedPaginationOptions options, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        var all = GetByQueryAsync(query, partitionKey, cancellationToken).GetAwaiter().GetResult();
        var page = all.Take(options.PageSize).ToList();
        return Task.FromResult(new PagedResult<Connection> { Items = page });
    }

    public Task<PagedResult<Connection>> GetOffsetPagedAsync(string baseQuery, OffsetPaginationOptions options, string? partitionKey = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task ExecuteTransactionalBatchAsync(string partitionKey, IEnumerable<(TransactionalBatchOperation Operation, Connection Entity)> operations, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}

// ── In-memory notification repository ────────────────────────────────────────

public sealed class InMemoryNotificationRepository : IRepository<Notification>
{
    private readonly ConcurrentDictionary<string, Notification> _docs = new();

    public Task<Notification?> GetByIdAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        _docs.TryGetValue(id, out var doc);
        return Task.FromResult(doc);
    }

    public Task<IReadOnlyList<Notification>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Notification>>(_docs.Values.ToList());

    public Task<Notification> CreateAsync(Notification entity, CancellationToken cancellationToken = default)
    {
        _docs[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<Notification> UpdateAsync(Notification entity, string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        _docs[id] = entity;
        return Task.FromResult(entity);
    }

    public Task<Notification> PatchAsync(string id, string partitionKey, IReadOnlyDictionary<string, object?> patches, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        _docs.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<PagedResult<Notification>> GetPagedAsync(string query, FeedPaginationOptions options, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        var page = _docs.Values.Take(options.PageSize).ToList();
        return Task.FromResult(new PagedResult<Notification> { Items = page });
    }

    public Task<PagedResult<Notification>> GetOffsetPagedAsync(string baseQuery, OffsetPaginationOptions options, string? partitionKey = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task ExecuteTransactionalBatchAsync(string partitionKey, IEnumerable<(TransactionalBatchOperation Operation, Notification Entity)> operations, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public IReadOnlyList<Notification> All => _docs.Values.ToList();
}

// ── In-memory BlendUser repository (for user search) ─────────────────────────

public sealed class InMemoryBlendUserRepository : IRepository<BlendUser>
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, BlendUser> _users = new();

    public void AddUser(BlendUser user) => _users[user.Id] = user;

    public Task<BlendUser?> GetByIdAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<IReadOnlyList<BlendUser>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<BlendUser> results = _users.Values;

        // Extract CONTAINS filter for displayName
        if (query.Contains("CONTAINS(LOWER(c.displayName)"))
        {
            var start = query.IndexOf("CONTAINS(LOWER(c.displayName), '") + "CONTAINS(LOWER(c.displayName), '".Length;
            var end = query.IndexOf("'", start);
            if (end > start)
            {
                var searchTerm = query[start..end];
                results = results.Where(u => u.DisplayName.ToLowerInvariant().Contains(searchTerm));
            }
        }

        // Exclude current user
        if (query.Contains("c.id != '"))
        {
            var start = query.IndexOf("c.id != '") + "c.id != '".Length;
            var end = query.IndexOf("'", start);
            if (end > start)
            {
                var excludeId = query[start..end];
                results = results.Where(u => u.Id != excludeId);
            }
        }

        IReadOnlyList<BlendUser> list = results.OrderBy(u => u.DisplayName).ToList();
        return Task.FromResult(list);
    }

    public Task<BlendUser> CreateAsync(BlendUser entity, CancellationToken cancellationToken = default)
    {
        _users[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<BlendUser> UpdateAsync(BlendUser entity, string id, string partitionKey, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<BlendUser> PatchAsync(string id, string partitionKey, IReadOnlyDictionary<string, object?> patches, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<PagedResult<BlendUser>> GetPagedAsync(string query, FeedPaginationOptions options, string? partitionKey = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<PagedResult<BlendUser>> GetOffsetPagedAsync(string baseQuery, OffsetPaginationOptions options, string? partitionKey = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task ExecuteTransactionalBatchAsync(string partitionKey, IEnumerable<(TransactionalBatchOperation Operation, BlendUser Entity)> operations, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}

// ── Test factory ──────────────────────────────────────────────────────────────

public sealed class FriendsTestFactory : WebApplicationFactory<Program>
{
    private readonly InMemoryUserStore _userStore = new();
    public readonly InMemoryConnectionRepository ConnectionRepo = new();
    public readonly InMemoryNotificationRepository NotificationRepo = new();
    public readonly InMemoryBlendUserRepository UserRepo = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:SecretKey", "test-secret-key-that-is-long-enough-for-hs256-algorithm");
        builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Development");

        builder.ConfigureServices(services =>
        {
            // Replace user store — intercept CreateAsync to also add to UserRepo
            services.RemoveAll<IUserStore<BlendUser>>();
            services.AddSingleton<IUserStore<BlendUser>>(new SyncingUserStore(_userStore, UserRepo));

            // Replace Cosmos repos with in-memory implementations
            services.RemoveAll<IRepository<Connection>>();
            services.AddSingleton<IRepository<Connection>>(ConnectionRepo);

            services.RemoveAll<IRepository<Notification>>();
            services.AddSingleton<IRepository<Notification>>(NotificationRepo);

            services.RemoveAll<IRepository<BlendUser>>();
            services.AddSingleton<IRepository<BlendUser>>(UserRepo);
        });
    }

    /// <summary>Wraps InMemoryUserStore and also writes users to the BlendUser search repo.</summary>
    private sealed class SyncingUserStore :
        IUserStore<BlendUser>,
        IUserPasswordStore<BlendUser>,
        IUserEmailStore<BlendUser>,
        IUserLoginStore<BlendUser>,
        IUserRoleStore<BlendUser>,
        IUserSecurityStampStore<BlendUser>
    {
        private readonly InMemoryUserStore _inner;
        private readonly InMemoryBlendUserRepository _userRepo;

        public SyncingUserStore(InMemoryUserStore inner, InMemoryBlendUserRepository userRepo)
        {
            _inner = inner;
            _userRepo = userRepo;
        }

        public void Dispose() { }

        public async Task<IdentityResult> CreateAsync(BlendUser user, CancellationToken ct)
        {
            var result = await _inner.CreateAsync(user, ct);
            if (result.Succeeded)
            {
                _userRepo.AddUser(user);
            }
            return result;
        }

        public Task<IdentityResult> UpdateAsync(BlendUser user, CancellationToken ct)
        {
            _userRepo.AddUser(user);
            return _inner.UpdateAsync(user, ct);
        }

        public Task<IdentityResult> DeleteAsync(BlendUser user, CancellationToken ct) => _inner.DeleteAsync(user, ct);
        public Task<BlendUser?> FindByIdAsync(string userId, CancellationToken ct) => _inner.FindByIdAsync(userId, ct);
        public Task<BlendUser?> FindByNameAsync(string normalizedUserName, CancellationToken ct) => _inner.FindByNameAsync(normalizedUserName, ct);
        public Task<string> GetUserIdAsync(BlendUser user, CancellationToken ct) => _inner.GetUserIdAsync(user, ct);
        public Task<string?> GetUserNameAsync(BlendUser user, CancellationToken ct) => _inner.GetUserNameAsync(user, ct);
        public Task SetUserNameAsync(BlendUser user, string? userName, CancellationToken ct) => _inner.SetUserNameAsync(user, userName, ct);
        public Task<string?> GetNormalizedUserNameAsync(BlendUser user, CancellationToken ct) => _inner.GetNormalizedUserNameAsync(user, ct);
        public Task SetNormalizedUserNameAsync(BlendUser user, string? normalizedName, CancellationToken ct) => _inner.SetNormalizedUserNameAsync(user, normalizedName, ct);
        public Task SetPasswordHashAsync(BlendUser user, string? passwordHash, CancellationToken ct) => _inner.SetPasswordHashAsync(user, passwordHash, ct);
        public Task<string?> GetPasswordHashAsync(BlendUser user, CancellationToken ct) => _inner.GetPasswordHashAsync(user, ct);
        public Task<bool> HasPasswordAsync(BlendUser user, CancellationToken ct) => _inner.HasPasswordAsync(user, ct);
        public Task SetEmailAsync(BlendUser user, string? email, CancellationToken ct) => _inner.SetEmailAsync(user, email, ct);
        public Task<string?> GetEmailAsync(BlendUser user, CancellationToken ct) => _inner.GetEmailAsync(user, ct);
        public Task<bool> GetEmailConfirmedAsync(BlendUser user, CancellationToken ct) => _inner.GetEmailConfirmedAsync(user, ct);
        public Task SetEmailConfirmedAsync(BlendUser user, bool confirmed, CancellationToken ct) => _inner.SetEmailConfirmedAsync(user, confirmed, ct);
        public Task<BlendUser?> FindByEmailAsync(string normalizedEmail, CancellationToken ct) => _inner.FindByEmailAsync(normalizedEmail, ct);
        public Task<string?> GetNormalizedEmailAsync(BlendUser user, CancellationToken ct) => _inner.GetNormalizedEmailAsync(user, ct);
        public Task SetNormalizedEmailAsync(BlendUser user, string? normalizedEmail, CancellationToken ct) => _inner.SetNormalizedEmailAsync(user, normalizedEmail, ct);
        public Task AddLoginAsync(BlendUser user, UserLoginInfo login, CancellationToken ct) => _inner.AddLoginAsync(user, login, ct);
        public Task RemoveLoginAsync(BlendUser user, string loginProvider, string providerKey, CancellationToken ct) => _inner.RemoveLoginAsync(user, loginProvider, providerKey, ct);
        public Task<IList<UserLoginInfo>> GetLoginsAsync(BlendUser user, CancellationToken ct) => _inner.GetLoginsAsync(user, ct);
        public Task<BlendUser?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken ct) => _inner.FindByLoginAsync(loginProvider, providerKey, ct);
        public Task AddToRoleAsync(BlendUser user, string roleName, CancellationToken ct) => _inner.AddToRoleAsync(user, roleName, ct);
        public Task RemoveFromRoleAsync(BlendUser user, string roleName, CancellationToken ct) => _inner.RemoveFromRoleAsync(user, roleName, ct);
        public Task<IList<string>> GetRolesAsync(BlendUser user, CancellationToken ct) => _inner.GetRolesAsync(user, ct);
        public Task<bool> IsInRoleAsync(BlendUser user, string roleName, CancellationToken ct) => _inner.IsInRoleAsync(user, roleName, ct);
        public Task<IList<BlendUser>> GetUsersInRoleAsync(string roleName, CancellationToken ct) => _inner.GetUsersInRoleAsync(roleName, ct);
        public Task SetSecurityStampAsync(BlendUser user, string stamp, CancellationToken ct) => _inner.SetSecurityStampAsync(user, stamp, ct);
        public Task<string?> GetSecurityStampAsync(BlendUser user, CancellationToken ct) => _inner.GetSecurityStampAsync(user, ct);
    }
}

// ── Integration tests ─────────────────────────────────────────────────────────

public class FriendsEndpointTests : IClassFixture<FriendsTestFactory>
{
    private readonly FriendsTestFactory _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public FriendsEndpointTests(FriendsTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = false,
    });

    private static string UniqueEmail() => $"friends-{Guid.NewGuid():N}@example.com";

    private static StringContent JsonBody(object payload) =>
        new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    private async Task<(HttpClient Client, string UserId)> RegisterAndAuthAsync(
        string displayName)
    {
        var client = CreateClient();
        var email = UniqueEmail();
        var resp = await client.PostAsync("/api/v1/auth/register",
            JsonBody(new { displayName, email, password = "ValidPass1!" }));
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

        var body = await resp.Content.ReadAsStringAsync();
        var auth = JsonSerializer.Deserialize<AuthResponse>(body, JsonOptions);
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var userId = ExtractUserIdFromJwt(auth.AccessToken);
        return (client, userId);
    }

    private static string ExtractUserIdFromJwt(string token)
    {
        var parts = token.Split('.');
        var payload = parts[1];
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("sub").GetString()!;
    }

    // ── Auth guards ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetFriends_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var resp = await client.GetAsync("/api/v1/friends");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetIncomingRequests_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var resp = await client.GetAsync("/api/v1/friends/requests/incoming");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetOutgoingRequests_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var resp = await client.GetAsync("/api/v1/friends/requests/outgoing");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task SendFriendRequest_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var resp = await client.PostAsync("/api/v1/friends/requests",
            JsonBody(new { targetUserId = "some-user" }));
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task AcceptFriendRequest_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var resp = await client.PostAsync("/api/v1/friends/requests/req-1/accept", null);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task DeclineFriendRequest_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var resp = await client.PostAsync("/api/v1/friends/requests/req-1/decline", null);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task RemoveFriend_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var resp = await client.DeleteAsync("/api/v1/friends/some-user");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task SearchUsers_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var resp = await client.GetAsync("/api/v1/users/search?q=alice");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Validation ────────────────────────────────────────────────────────────

    [Fact]
    public async Task SendFriendRequest_MissingTargetUserId_Returns400()
    {
        var (client, _) = await RegisterAndAuthAsync("SenderUser");
        var resp = await client.PostAsync("/api/v1/friends/requests",
            JsonBody(new { targetUserId = "" }));
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task SearchUsers_MissingQuery_Returns400()
    {
        var (client, _) = await RegisterAndAuthAsync("SearchUser");
        var resp = await client.GetAsync("/api/v1/users/search");
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    // ── Full friend request lifecycle ─────────────────────────────────────────

    [Fact]
    public async Task FriendLifecycle_SendAcceptListRemove()
    {
        var (senderClient, senderId) = await RegisterAndAuthAsync("LifecycleSender");
        var (receiverClient, receiverId) = await RegisterAndAuthAsync("LifecycleReceiver");

        // 1. Sender cannot send to themselves
        var selfResp = await senderClient.PostAsync("/api/v1/friends/requests",
            JsonBody(new { targetUserId = senderId }));
        Assert.Equal(HttpStatusCode.BadRequest, selfResp.StatusCode);

        // 2. Send friend request
        var sendResp = await senderClient.PostAsync("/api/v1/friends/requests",
            JsonBody(new { targetUserId = receiverId }));
        Assert.Equal(HttpStatusCode.Created, sendResp.StatusCode);

        var sendBody = await sendResp.Content.ReadAsStringAsync();
        var sendJson = JsonSerializer.Deserialize<JsonElement>(sendBody, JsonOptions);
        var connectionId = sendJson.GetProperty("connectionId").GetString();
        Assert.NotNull(connectionId);

        // 3. Duplicate request returns 409
        var dupResp = await senderClient.PostAsync("/api/v1/friends/requests",
            JsonBody(new { targetUserId = receiverId }));
        Assert.Equal(HttpStatusCode.Conflict, dupResp.StatusCode);

        // 4. Sender sees it in outgoing requests
        var outgoingResp = await senderClient.GetAsync("/api/v1/friends/requests/outgoing");
        Assert.Equal(HttpStatusCode.OK, outgoingResp.StatusCode);
        var outgoingBody = await outgoingResp.Content.ReadAsStringAsync();
        var outgoing = JsonSerializer.Deserialize<JsonElement>(outgoingBody, JsonOptions);
        Assert.True(outgoing.GetProperty("items").GetArrayLength() >= 1);

        // 5. Receiver sees it in incoming requests
        var incomingResp = await receiverClient.GetAsync("/api/v1/friends/requests/incoming");
        Assert.Equal(HttpStatusCode.OK, incomingResp.StatusCode);
        var incomingBody = await incomingResp.Content.ReadAsStringAsync();
        var incoming = JsonSerializer.Deserialize<JsonElement>(incomingBody, JsonOptions);
        Assert.True(incoming.GetProperty("items").GetArrayLength() >= 1);

        // 6. Receiver accepts
        var acceptResp = await receiverClient.PostAsync(
            $"/api/v1/friends/requests/{connectionId}/accept", null);
        Assert.Equal(HttpStatusCode.NoContent, acceptResp.StatusCode);

        // 7. Sender's friend list now includes receiver
        var friendsResp = await senderClient.GetAsync("/api/v1/friends");
        Assert.Equal(HttpStatusCode.OK, friendsResp.StatusCode);
        var friendsBody = await friendsResp.Content.ReadAsStringAsync();
        var friends = JsonSerializer.Deserialize<JsonElement>(friendsBody, JsonOptions);
        var friendItems = friends.GetProperty("items").EnumerateArray().ToList();
        Assert.Contains(friendItems, f => f.GetProperty("userId").GetString() == receiverId);

        // 8. Notification created for the sender (friendRequestAccepted)
        var acceptedNotif = _factory.NotificationRepo.All
            .FirstOrDefault(n => n.Type == NotificationType.FriendRequestAccepted && n.RecipientUserId == senderId);
        Assert.NotNull(acceptedNotif);

        // 9. Remove friend
        var removeResp = await senderClient.DeleteAsync($"/api/v1/friends/{receiverId}");
        Assert.Equal(HttpStatusCode.NoContent, removeResp.StatusCode);

        // 10. Friend list is now empty
        friendsResp = await senderClient.GetAsync("/api/v1/friends");
        friendsBody = await friendsResp.Content.ReadAsStringAsync();
        friends = JsonSerializer.Deserialize<JsonElement>(friendsBody, JsonOptions);
        var remainingFriends = friends.GetProperty("items").EnumerateArray()
            .Where(f => f.GetProperty("userId").GetString() == receiverId)
            .ToList();
        Assert.Empty(remainingFriends);
    }

    // ── Decline and re-send cooldown ──────────────────────────────────────────

    [Fact]
    public async Task FriendLifecycle_DeclineAndCooldown()
    {
        var (senderClient, senderId) = await RegisterAndAuthAsync("DeclineSender");
        var (receiverClient, receiverId) = await RegisterAndAuthAsync("DeclineReceiver");

        // Send a request
        var sendResp = await senderClient.PostAsync("/api/v1/friends/requests",
            JsonBody(new { targetUserId = receiverId }));
        Assert.Equal(HttpStatusCode.Created, sendResp.StatusCode);

        var sendBody = await sendResp.Content.ReadAsStringAsync();
        var sendJson = JsonSerializer.Deserialize<JsonElement>(sendBody, JsonOptions);
        var connectionId = sendJson.GetProperty("connectionId").GetString()!;

        // Receiver declines
        var declineResp = await receiverClient.PostAsync(
            $"/api/v1/friends/requests/{connectionId}/decline", null);
        Assert.Equal(HttpStatusCode.NoContent, declineResp.StatusCode);

        // Sender tries to re-send immediately → 409 Conflict (cooldown)
        var reSendResp = await senderClient.PostAsync("/api/v1/friends/requests",
            JsonBody(new { targetUserId = receiverId }));
        Assert.Equal(HttpStatusCode.Conflict, reSendResp.StatusCode);
    }

    // ── Remove non-existing friend ────────────────────────────────────────────

    [Fact]
    public async Task RemoveFriend_NonExistingFriend_Returns404()
    {
        var (client, _) = await RegisterAndAuthAsync("RemoveUser");
        var resp = await client.DeleteAsync($"/api/v1/friends/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    // ── Accept own request ─────────────────────────────────────────────────────

    [Fact]
    public async Task AcceptFriendRequest_OwnRequest_Returns403()
    {
        var (senderClient, senderId) = await RegisterAndAuthAsync("OwnAcceptSender");
        var (_, receiverId) = await RegisterAndAuthAsync("OwnAcceptReceiver");

        var sendResp = await senderClient.PostAsync("/api/v1/friends/requests",
            JsonBody(new { targetUserId = receiverId }));
        Assert.Equal(HttpStatusCode.Created, sendResp.StatusCode);

        var sendBody = await sendResp.Content.ReadAsStringAsync();
        var sendJson = JsonSerializer.Deserialize<JsonElement>(sendBody, JsonOptions);
        var connectionId = sendJson.GetProperty("connectionId").GetString()!;

        // Sender tries to accept their own request → 403
        var acceptResp = await senderClient.PostAsync(
            $"/api/v1/friends/requests/{connectionId}/accept", null);
        Assert.Equal(HttpStatusCode.Forbidden, acceptResp.StatusCode);
    }

    // ── User search ───────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchUsers_ReturnsMatchingUsers_WithConnectionStatus()
    {
        var (searcherClient, searcherId) = await RegisterAndAuthAsync("SearcherAlpha");
        var (_, targetId) = await RegisterAndAuthAsync("TargetAlpha");

        // Search without any connection
        var searchResp = await searcherClient.GetAsync("/api/v1/users/search?q=TargetAlpha");
        Assert.Equal(HttpStatusCode.OK, searchResp.StatusCode);

        var searchBody = await searchResp.Content.ReadAsStringAsync();
        var searchResult = JsonSerializer.Deserialize<JsonElement>(searchBody, JsonOptions);
        var items = searchResult.GetProperty("items").EnumerateArray().ToList();
        var target = items.FirstOrDefault(u => u.GetProperty("userId").GetString() == targetId);
        Assert.NotNull(target.ValueKind == JsonValueKind.Undefined ? null : (object)"found");
        if (target.ValueKind != JsonValueKind.Undefined)
        {
            Assert.Equal("none", target.GetProperty("connectionStatus").GetString());
        }
    }

    // ── Friend request to non-existing user ───────────────────────────────────

    [Fact]
    public async Task SendFriendRequest_ToNonExistingUser_Returns404()
    {
        var (client, _) = await RegisterAndAuthAsync("SenderForNotFound");
        var resp = await client.PostAsync("/api/v1/friends/requests",
            JsonBody(new { targetUserId = Guid.NewGuid().ToString() }));
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    // ── Notification on friend request sent ───────────────────────────────────

    [Fact]
    public async Task SendFriendRequest_CreatesNotificationForRecipient()
    {
        var (senderClient, senderId) = await RegisterAndAuthAsync("NotifSender");
        var (_, receiverId) = await RegisterAndAuthAsync("NotifReceiver");

        var notifCountBefore = _factory.NotificationRepo.All
            .Count(n => n.Type == NotificationType.FriendRequestReceived && n.RecipientUserId == receiverId);

        var sendResp = await senderClient.PostAsync("/api/v1/friends/requests",
            JsonBody(new { targetUserId = receiverId }));
        Assert.Equal(HttpStatusCode.Created, sendResp.StatusCode);

        var notifCountAfter = _factory.NotificationRepo.All
            .Count(n => n.Type == NotificationType.FriendRequestReceived && n.RecipientUserId == receiverId);

        Assert.Equal(notifCountBefore + 1, notifCountAfter);
    }
}
