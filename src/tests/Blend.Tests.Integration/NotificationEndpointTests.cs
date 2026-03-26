using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Blend.Api.Auth.Models;
using Blend.Api.Notifications.Models;
using Blend.Api.Settings.Models;
using Blend.Domain.Entities;
using Blend.Domain.Identity;
using Blend.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blend.Tests.Integration;

// ── In-memory notification repository with full patch support ─────────────────

public sealed class InMemoryNotificationRepositoryFull : IRepository<Notification>
{
    private readonly ConcurrentDictionary<string, Notification> _docs = new();

    public Task<Notification?> GetByIdAsync(string id, string partitionKey, CancellationToken ct = default)
    {
        _docs.TryGetValue(id, out var doc);
        return Task.FromResult(doc);
    }

    public Task<IReadOnlyList<Notification>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken ct = default)
    {
        IEnumerable<Notification> results = _docs.Values;

        if (partitionKey is not null)
        {
            results = results.Where(n => n.RecipientUserId == partitionKey);
        }

        if (query.Contains("n.read = false"))
        {
            results = results.Where(n => !n.Read);
        }

        IReadOnlyList<Notification> list = results.OrderByDescending(n => n.CreatedAt).ToList();
        return Task.FromResult(list);
    }

    public Task<Notification> CreateAsync(Notification entity, CancellationToken ct = default)
    {
        _docs[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<Notification> UpdateAsync(Notification entity, string id, string partitionKey, CancellationToken ct = default)
    {
        _docs[id] = entity;
        return Task.FromResult(entity);
    }

    public Task<Notification> PatchAsync(string id, string partitionKey, IReadOnlyDictionary<string, object?> patches, CancellationToken ct = default)
    {
        if (!_docs.TryGetValue(id, out var existing))
        {
            throw new KeyNotFoundException($"Notification {id} not found.");
        }

        var read = existing.Read;
        foreach (var (path, value) in patches)
        {
            if (path == "/read" && value is bool b)
            {
                read = b;
            }
        }

        var updated = new Notification
        {
            Id = existing.Id,
            RecipientUserId = existing.RecipientUserId,
            Type = existing.Type,
            Title = existing.Title,
            Message = existing.Message,
            ActionUrl = existing.ActionUrl,
            SourceUserId = existing.SourceUserId,
            Read = read,
            CreatedAt = existing.CreatedAt,
            Ttl = existing.Ttl,
        };

        _docs[id] = updated;
        return Task.FromResult(updated);
    }

    public Task DeleteAsync(string id, string partitionKey, CancellationToken ct = default)
    {
        _docs.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<PagedResult<Notification>> GetPagedAsync(string query, FeedPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
    {
        var result = GetByQueryAsync(query, partitionKey, ct).GetAwaiter().GetResult();
        var page = result.Take(options.PageSize).ToList();
        return Task.FromResult(new PagedResult<Notification> { Items = page });
    }

    public Task<PagedResult<Notification>> GetOffsetPagedAsync(string baseQuery, OffsetPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task ExecuteTransactionalBatchAsync(string partitionKey, IEnumerable<(TransactionalBatchOperation Operation, Notification Entity)> operations, CancellationToken ct = default)
        => throw new NotImplementedException();

    public void Seed(Notification notification) => _docs[notification.Id] = notification;
    public IReadOnlyList<Notification> All => _docs.Values.ToList();
}

// ── In-memory content repository ──────────────────────────────────────────────

public sealed class InMemoryContentRepository : IRepository<Content>
{
    private readonly ConcurrentDictionary<string, Content> _docs = new();

    public Task<Content?> GetByIdAsync(string id, string partitionKey, CancellationToken ct = default)
    {
        _docs.TryGetValue(id, out var doc);
        return Task.FromResult(doc);
    }

    public Task<IReadOnlyList<Content>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken ct = default)
    {
        IEnumerable<Content> results = _docs.Values;

        if (query.Contains("IngredientSubmission"))
        {
            results = results.Where(c => c.ContentType == ContentType.IngredientSubmission);
        }

        if (query.Contains("submittedByUserId = '"))
        {
            var start = query.IndexOf("submittedByUserId = '") + "submittedByUserId = '".Length;
            var end = query.IndexOf("'", start);
            if (end > start)
            {
                var userId = query[start..end];
                results = results.Where(c => c.SubmittedByUserId == userId);
            }
        }

        IReadOnlyList<Content> list = results.OrderByDescending(c => c.CreatedAt).ToList();
        return Task.FromResult(list);
    }

    public Task<Content> CreateAsync(Content entity, CancellationToken ct = default)
    {
        _docs[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<Content> UpdateAsync(Content entity, string id, string partitionKey, CancellationToken ct = default)
    {
        _docs[id] = entity;
        return Task.FromResult(entity);
    }

    public Task<Content> PatchAsync(string id, string partitionKey, IReadOnlyDictionary<string, object?> patches, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task DeleteAsync(string id, string partitionKey, CancellationToken ct = default)
    {
        _docs.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<PagedResult<Content>> GetPagedAsync(string query, FeedPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<PagedResult<Content>> GetOffsetPagedAsync(string baseQuery, OffsetPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task ExecuteTransactionalBatchAsync(string partitionKey, IEnumerable<(TransactionalBatchOperation Operation, Content Entity)> operations, CancellationToken ct = default)
        => throw new NotImplementedException();
}

// ── Test Web Application Factory ──────────────────────────────────────────────

public sealed class NotificationsTestFactory : WebApplicationFactory<Program>
{
    public readonly InMemoryNotificationRepositoryFull NotificationRepo = new();
    public readonly InMemoryUserRepository UserRepository = new();
    public readonly InMemoryContentRepository ContentRepo = new();
    private readonly InMemoryUserStore _userStore = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:SecretKey", "test-secret-key-that-is-long-enough-for-hs256-algorithm");
        builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IUserStore<BlendUser>>();
            services.AddSingleton<IUserStore<BlendUser>>(_userStore);

            services.RemoveAll<IRepository<Notification>>();
            services.AddSingleton<IRepository<Notification>>(NotificationRepo);

            services.RemoveAll<IRepository<User>>();
            services.AddSingleton<IRepository<User>>(UserRepository);

            services.RemoveAll<IRepository<Content>>();
            services.AddSingleton<IRepository<Content>>(ContentRepo);
        });
    }
}

// ── Integration tests ─────────────────────────────────────────────────────────

public class NotificationEndpointTests : IClassFixture<NotificationsTestFactory>
{
    private readonly NotificationsTestFactory _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public NotificationEndpointTests(NotificationsTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = false,
    });

    private static string UniqueEmail() => $"notif-{Guid.NewGuid():N}@example.com";

    private static StringContent JsonBody(object payload) =>
        new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    private static string ExtractUserIdFromJwt(string token)
    {
        var parts = token.Split('.');
        var payload = parts[1];
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("sub").GetString()!;
    }

    private async Task<(HttpClient Client, string UserId)> RegisterAndAuthenticateAsync()
    {
        var client = CreateClient();
        var email = UniqueEmail();
        var registerResponse = await client.PostAsync("/api/v1/auth/register",
            JsonBody(new { displayName = "NotifUser", email, password = "ValidPass1!" }));

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authBody = await registerResponse.Content.ReadAsStringAsync();
        var auth = JsonSerializer.Deserialize<AuthResponse>(authBody, JsonOptions);
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var userId = ExtractUserIdFromJwt(auth.AccessToken);

        // Seed the User document
        await _factory.UserRepository.CreateAsync(new User
        {
            Id = userId,
            Email = email,
            DisplayName = "NotifUser",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        return (client, userId);
    }

    // ── Unauthenticated access ─────────────────────────────────────────────────

    [Fact]
    public async Task GetNotifications_WhenUnauthenticated_Returns401()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/notifications");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUnreadCount_WhenUnauthenticated_Returns401()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/notifications/unread-count");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── GET /api/v1/notifications ─────────────────────────────────────────────

    [Fact]
    public async Task GetNotifications_ReturnsEmptyListWhenNoNotifications()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.GetAsync("/api/v1/notifications");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var page = JsonSerializer.Deserialize<NotificationsPageResponse>(body, JsonOptions);
        Assert.NotNull(page);
        Assert.Empty(page.Items);
    }

    [Fact]
    public async Task GetNotifications_ReturnsUserNotifications()
    {
        var (client, userId) = await RegisterAndAuthenticateAsync();

        _factory.NotificationRepo.Seed(new Notification
        {
            Id = Guid.NewGuid().ToString(),
            RecipientUserId = userId,
            Type = NotificationType.System,
            Title = "Welcome",
            Message = "Welcome to Blend!",
            Read = false,
            CreatedAt = DateTimeOffset.UtcNow,
            Ttl = 7776000,
        });

        var response = await client.GetAsync("/api/v1/notifications");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var page = JsonSerializer.Deserialize<NotificationsPageResponse>(body, JsonOptions);
        Assert.NotNull(page);
        Assert.NotEmpty(page.Items);
        Assert.Equal("Welcome", page.Items[0].Title);
    }

    // ── GET /api/v1/notifications/unread-count ────────────────────────────────

    [Fact]
    public async Task GetUnreadCount_ReturnsZeroWhenNoNotifications()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.GetAsync("/api/v1/notifications/unread-count");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UnreadCountResponse>(body, JsonOptions);
        Assert.NotNull(result);
        Assert.Equal(0, result.Count);
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsCorrectCount()
    {
        var (client, userId) = await RegisterAndAuthenticateAsync();

        for (var i = 0; i < 3; i++)
        {
            _factory.NotificationRepo.Seed(new Notification
            {
                Id = Guid.NewGuid().ToString(),
                RecipientUserId = userId,
                Type = NotificationType.System,
                Title = $"Notification {i}",
                Message = $"Message {i}",
                Read = false,
                CreatedAt = DateTimeOffset.UtcNow,
                Ttl = 7776000,
            });
        }

        var response = await client.GetAsync("/api/v1/notifications/unread-count");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UnreadCountResponse>(body, JsonOptions);
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    // ── POST /api/v1/notifications/{id}/read ─────────────────────────────────

    [Fact]
    public async Task MarkAsRead_WhenNotFound_Returns404()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PostAsync("/api/v1/notifications/nonexistent-id/read", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MarkAsRead_WhenFound_Returns204()
    {
        var (client, userId) = await RegisterAndAuthenticateAsync();
        var notifId = Guid.NewGuid().ToString();

        _factory.NotificationRepo.Seed(new Notification
        {
            Id = notifId,
            RecipientUserId = userId,
            Type = NotificationType.System,
            Title = "T",
            Message = "M",
            Read = false,
            CreatedAt = DateTimeOffset.UtcNow,
            Ttl = 7776000,
        });

        var response = await client.PostAsync($"/api/v1/notifications/{notifId}/read", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // ── POST /api/v1/notifications/read-all ──────────────────────────────────

    [Fact]
    public async Task MarkAllAsRead_Returns204()
    {
        var (client, userId) = await RegisterAndAuthenticateAsync();

        _factory.NotificationRepo.Seed(new Notification
        {
            Id = Guid.NewGuid().ToString(),
            RecipientUserId = userId,
            Type = NotificationType.System,
            Title = "T",
            Message = "M",
            Read = false,
            CreatedAt = DateTimeOffset.UtcNow,
            Ttl = 7776000,
        });

        var response = await client.PostAsync("/api/v1/notifications/read-all", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}

// ── Settings endpoint tests ───────────────────────────────────────────────────

public class SettingsEndpointTests : IClassFixture<NotificationsTestFactory>
{
    private readonly NotificationsTestFactory _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public SettingsEndpointTests(NotificationsTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = false,
    });

    private static string UniqueEmail() => $"settings-{Guid.NewGuid():N}@example.com";

    private static StringContent JsonBody(object payload) =>
        new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    private static string ExtractUserIdFromJwt(string token)
    {
        var parts = token.Split('.');
        var payload = parts[1];
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("sub").GetString()!;
    }

    private async Task<(HttpClient Client, string UserId)> RegisterAndAuthenticateAsync()
    {
        var client = CreateClient();
        var email = UniqueEmail();
        var registerResponse = await client.PostAsync("/api/v1/auth/register",
            JsonBody(new { displayName = "SettingsUser", email, password = "ValidPass1!" }));

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authBody = await registerResponse.Content.ReadAsStringAsync();
        var auth = JsonSerializer.Deserialize<AuthResponse>(authBody, JsonOptions);
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var userId = ExtractUserIdFromJwt(auth.AccessToken);

        await _factory.UserRepository.CreateAsync(new User
        {
            Id = userId,
            Email = email,
            DisplayName = "SettingsUser",
            Settings = new AppSettings
            {
                UnitSystem = MeasurementUnit.Metric,
                Theme = ThemePreference.System,
                Notifications = new NotificationPreferences(),
            },
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        return (client, userId);
    }

    // ── GET /api/v1/settings ─────────────────────────────────────────────────

    [Fact]
    public async Task GetSettings_WhenUnauthenticated_Returns401()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/settings");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSettings_ReturnsDefaultSettings()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.GetAsync("/api/v1/settings");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var settings = JsonSerializer.Deserialize<AppSettingsResponse>(body, JsonOptions);
        Assert.NotNull(settings);
        Assert.Equal(MeasurementUnit.Metric, settings.UnitSystem);
    }

    // ── PUT /api/v1/settings ─────────────────────────────────────────────────

    [Fact]
    public async Task PutSettings_UpdatesUnitSystem()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PutAsync("/api/v1/settings",
            JsonBody(new { unitSystem = "Imperial" }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var settings = JsonSerializer.Deserialize<AppSettingsResponse>(body, JsonOptions);
        Assert.NotNull(settings);
        Assert.Equal(MeasurementUnit.Imperial, settings.UnitSystem);
    }

    [Fact]
    public async Task PutSettings_UpdatesTheme()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PutAsync("/api/v1/settings",
            JsonBody(new { theme = "Dark" }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var settings = JsonSerializer.Deserialize<AppSettingsResponse>(body, JsonOptions);
        Assert.NotNull(settings);
        Assert.Equal(ThemePreference.Dark, settings.Theme);
    }
}

// ── Ingredient submission endpoint tests ─────────────────────────────────────

public class IngredientSubmissionEndpointTests : IClassFixture<NotificationsTestFactory>
{
    private readonly NotificationsTestFactory _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public IngredientSubmissionEndpointTests(NotificationsTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = false,
    });

    private static string UniqueEmail() => $"ingr-{Guid.NewGuid():N}@example.com";

    private static StringContent JsonBody(object payload) =>
        new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    private async Task<HttpClient> RegisterAndAuthenticateAsync()
    {
        var client = CreateClient();
        var email = UniqueEmail();
        var registerResponse = await client.PostAsync("/api/v1/auth/register",
            JsonBody(new { displayName = "IngrUser", email, password = "ValidPass1!" }));

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var auth = JsonSerializer.Deserialize<AuthResponse>(
            await registerResponse.Content.ReadAsStringAsync(), JsonOptions);
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        return client;
    }

    // ── POST /api/v1/ingredients/submissions ─────────────────────────────────

    [Fact]
    public async Task SubmitIngredient_WhenUnauthenticated_Returns401()
    {
        var client = CreateClient();
        var response = await client.PostAsync("/api/v1/ingredients/submissions",
            JsonBody(new { name = "Truffle" }));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SubmitIngredient_WithValidPayload_Returns201()
    {
        var client = await RegisterAndAuthenticateAsync();

        var response = await client.PostAsync("/api/v1/ingredients/submissions",
            JsonBody(new { name = "Black Truffle", category = "Fungi", description = "A luxury ingredient." }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);
        Assert.Equal("Black Truffle", result.GetProperty("name").GetString());
        Assert.Equal("Pending", result.GetProperty("status").GetString());
    }

    [Fact]
    public async Task SubmitIngredient_WithMissingName_Returns400()
    {
        var client = await RegisterAndAuthenticateAsync();

        var response = await client.PostAsync("/api/v1/ingredients/submissions",
            JsonBody(new { name = "" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── GET /api/v1/ingredients/submissions/mine ──────────────────────────────

    [Fact]
    public async Task GetMySubmissions_WhenUnauthenticated_Returns401()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/ingredients/submissions/mine");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMySubmissions_ReturnsSubmissionsForCurrentUser()
    {
        var client = await RegisterAndAuthenticateAsync();

        // Submit an ingredient first
        var submitResponse = await client.PostAsync("/api/v1/ingredients/submissions",
            JsonBody(new { name = "Saffron", category = "Spices" }));
        Assert.Equal(HttpStatusCode.Created, submitResponse.StatusCode);

        // Then query own submissions
        var listResponse = await client.GetAsync("/api/v1/ingredients/submissions/mine");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var body = await listResponse.Content.ReadAsStringAsync();
        var submissions = JsonSerializer.Deserialize<List<JsonElement>>(body, JsonOptions);
        Assert.NotNull(submissions);
        Assert.Contains(submissions, s => s.GetProperty("name").GetString() == "Saffron");
    }
}

// ── Account deletion endpoint tests ──────────────────────────────────────────

public class AccountDeletionEndpointTests : IClassFixture<NotificationsTestFactory>
{
    private readonly NotificationsTestFactory _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public AccountDeletionEndpointTests(NotificationsTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = false,
    });

    private static string UniqueEmail() => $"acct-{Guid.NewGuid():N}@example.com";

    private static StringContent JsonBody(object payload) =>
        new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    private static string ExtractUserIdFromJwt(string token)
    {
        var parts = token.Split('.');
        var payload = parts[1];
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("sub").GetString()!;
    }

    private async Task<(HttpClient Client, string UserId, string Email)> RegisterAndAuthenticateAsync()
    {
        var client = CreateClient();
        var email = UniqueEmail();
        const string password = "ValidPass1!";

        var registerResponse = await client.PostAsync("/api/v1/auth/register",
            JsonBody(new { displayName = "AcctUser", email, password }));

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var auth = JsonSerializer.Deserialize<AuthResponse>(
            await registerResponse.Content.ReadAsStringAsync(), JsonOptions);
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var userId = ExtractUserIdFromJwt(auth.AccessToken);

        await _factory.UserRepository.CreateAsync(new User
        {
            Id = userId,
            Email = email,
            DisplayName = "AcctUser",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        return (client, userId, email);
    }

    // ── POST /api/v1/users/me/delete-request ─────────────────────────────────

    [Fact]
    public async Task RequestDeletion_WhenUnauthenticated_Returns401()
    {
        var client = CreateClient();
        var response = await client.PostAsync("/api/v1/users/me/delete-request",
            JsonBody(new { password = "password" }));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RequestDeletion_WithCorrectPassword_Returns202()
    {
        var (client, _, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PostAsync("/api/v1/users/me/delete-request",
            JsonBody(new { password = "ValidPass1!" }));

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task RequestDeletion_WithWrongPassword_Returns401()
    {
        var (client, _, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PostAsync("/api/v1/users/me/delete-request",
            JsonBody(new { password = "WrongPassword!" }));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RequestDeletion_WhenAlreadyRequested_Returns409()
    {
        var (client, userId, _) = await RegisterAndAuthenticateAsync();

        // First request succeeds
        var firstResponse = await client.PostAsync("/api/v1/users/me/delete-request",
            JsonBody(new { password = "ValidPass1!" }));
        Assert.Equal(HttpStatusCode.Accepted, firstResponse.StatusCode);

        // Second request should conflict — user document is now marked as deletion-requested
        var secondResponse = await client.PostAsync("/api/v1/users/me/delete-request",
            JsonBody(new { password = "ValidPass1!" }));
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    // ── POST /api/v1/users/me/cancel-deletion ────────────────────────────────

    [Fact]
    public async Task CancelDeletion_WhenNoPendingRequest_Returns404()
    {
        var (client, _, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PostAsync("/api/v1/users/me/cancel-deletion", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CancelDeletion_AfterRequestingDeletion_Returns204()
    {
        var (client, _, _) = await RegisterAndAuthenticateAsync();

        // Request deletion
        var deleteResponse = await client.PostAsync("/api/v1/users/me/delete-request",
            JsonBody(new { password = "ValidPass1!" }));
        Assert.Equal(HttpStatusCode.Accepted, deleteResponse.StatusCode);

        // Cancel it
        var cancelResponse = await client.PostAsync("/api/v1/users/me/cancel-deletion", null);
        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);
    }
}
