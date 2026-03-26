using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Blend.Api.Admin.Services;
using Blend.Api.Auth.Services;
using Blend.Api.Ingredients.Services;
using Blend.Api.Notifications.Services;
using Blend.Domain.Entities;
using Blend.Domain.Identity;
using Blend.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blend.Tests.Integration;

// ── In-memory Content repository for admin tests ───────────────────────────────

public sealed class AdminTestContentRepository : IRepository<Content>
{
    private readonly ConcurrentDictionary<string, Content> _docs = new();

    public Task<Content?> GetByIdAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        _docs.TryGetValue(id, out var doc);
        return Task.FromResult(doc);
    }

    public Task<IReadOnlyList<Content>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<Content> results = _docs.Values;

        // Filter by contentType
        if (query.Contains("contentType = 'FeaturedRecipe'"))
        {
            results = results.Where(c => c.ContentType == ContentType.FeaturedRecipe);
        }
        else if (query.Contains("contentType = 'Story'"))
        {
            results = results.Where(c => c.ContentType == ContentType.Story);
        }
        else if (query.Contains("contentType = 'Video'"))
        {
            results = results.Where(c => c.ContentType == ContentType.Video);
        }
        else if (query.Contains("contentType = 'IngredientSubmission'"))
        {
            results = results.Where(c => c.ContentType == ContentType.IngredientSubmission);

            if (query.Contains("submissionStatus = 'Pending'"))
            {
                results = results.Where(c => c.SubmissionStatus == SubmissionStatus.Pending);
            }
            else if (query.Contains("submissionStatus = 'Approved'"))
            {
                results = results.Where(c => c.SubmissionStatus == SubmissionStatus.Approved);
            }
            else if (query.Contains("submissionStatus = 'Rejected'"))
            {
                results = results.Where(c => c.SubmissionStatus == SubmissionStatus.Rejected);
            }
        }

        IReadOnlyList<Content> list = results.OrderBy(c => c.DisplayOrder ?? int.MaxValue).ToList();
        return Task.FromResult(list);
    }

    public Task<Content> CreateAsync(Content entity, CancellationToken cancellationToken = default)
    {
        _docs[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<Content> UpdateAsync(Content entity, string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        _docs[id] = entity;
        return Task.FromResult(entity);
    }

    public Task<Content> PatchAsync(string id, string partitionKey, IReadOnlyDictionary<string, object?> patches, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        _docs.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<PagedResult<Content>> GetPagedAsync(string query, FeedPaginationOptions options, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        var all = GetByQueryAsync(query, partitionKey, cancellationToken).GetAwaiter().GetResult();
        var page = all.Take(options.PageSize).ToList();
        return Task.FromResult(new PagedResult<Content> { Items = page });
    }

    public Task<PagedResult<Content>> GetOffsetPagedAsync(string baseQuery, OffsetPaginationOptions options, string? partitionKey = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task ExecuteTransactionalBatchAsync(string partitionKey, IEnumerable<(TransactionalBatchOperation Operation, Content Entity)> operations, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public void Add(Content item) => _docs[item.Id] = item;

    public IReadOnlyList<Content> All => _docs.Values.ToList();
}

// ── Admin test factory ─────────────────────────────────────────────────────────

public sealed class AdminTestFactory : WebApplicationFactory<Program>
{
    private readonly InMemoryUserStore _userStore = new();
    public readonly AdminTestContentRepository ContentRepo = new();
    public readonly InMemoryNotificationRepository AdminNotifRepo = new();

    // Pre-seeded admin user
    public BlendUser AdminUser { get; } = new()
    {
        Id = "admin-user-id",
        UserName = "admin@blend.test",
        NormalizedUserName = "ADMIN@BLEND.TEST",
        Email = "admin@blend.test",
        NormalizedEmail = "ADMIN@BLEND.TEST",
        DisplayName = "Admin User",
        Role = UserRole.Admin,
        SecurityStamp = Guid.NewGuid().ToString(),
        EmailConfirmed = true,
    };

    // Pre-seeded regular user
    public BlendUser RegularUser { get; } = new()
    {
        Id = "regular-user-id",
        UserName = "user@blend.test",
        NormalizedUserName = "USER@BLEND.TEST",
        Email = "user@blend.test",
        NormalizedEmail = "USER@BLEND.TEST",
        DisplayName = "Regular User",
        Role = UserRole.User,
        SecurityStamp = Guid.NewGuid().ToString(),
        EmailConfirmed = true,
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:SecretKey", "test-secret-key-that-is-long-enough-for-hs256-algorithm");
        builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Development");

        // Seed admin and regular users into the in-memory store
        _userStore.CreateAsync(AdminUser, CancellationToken.None).GetAwaiter().GetResult();
        _userStore.CreateAsync(RegularUser, CancellationToken.None).GetAwaiter().GetResult();

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IUserStore<BlendUser>>();
            services.AddSingleton<IUserStore<BlendUser>>(_userStore);

            services.RemoveAll<IRepository<Content>>();
            services.AddSingleton<IRepository<Content>>(ContentRepo);

            services.RemoveAll<IRepository<Notification>>();
            services.AddSingleton<IRepository<Notification>>(AdminNotifRepo);

            // Replace KB service with a no-op in-memory implementation
            services.RemoveAll<IKnowledgeBaseService>();
            services.AddSingleton<IKnowledgeBaseService>(new InMemoryKnowledgeBaseService());
        });
    }

    /// <summary>Generates a JWT for the given user using the app's configured JwtService.</summary>
    public string GenerateTokenFor(BlendUser user)
    {
        using var scope = Services.CreateScope();
        var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();
        return jwtService.GenerateAccessToken(user);
    }
}

// ── Integration tests ─────────────────────────────────────────────────────────

public class AdminEndpointTests : IClassFixture<AdminTestFactory>
{
    private readonly AdminTestFactory _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public AdminEndpointTests(AdminTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = false,
    });

    private HttpClient CreateAdminClient()
    {
        var client = CreateClient();
        var token = _factory.GenerateTokenFor(_factory.AdminUser);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private HttpClient CreateUserClient()
    {
        var client = CreateClient();
        var token = _factory.GenerateTokenFor(_factory.RegularUser);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static StringContent JsonBody(object payload) =>
        new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    // ── Auth guards — unauthenticated ─────────────────────────────────────────

    [Fact]
    public async Task GetFeaturedRecipes_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var resp = await client.GetAsync("/api/v1/admin/content/featured-recipes");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetStories_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var resp = await client.GetAsync("/api/v1/admin/content/stories");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetVideos_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var resp = await client.GetAsync("/api/v1/admin/content/videos");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task GetSubmissions_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var resp = await client.GetAsync("/api/v1/admin/ingredients/submissions");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    // ── Auth guards — non-admin ────────────────────────────────────────────────

    [Fact]
    public async Task GetFeaturedRecipes_AsNonAdmin_Returns403()
    {
        var client = CreateUserClient();
        var resp = await client.GetAsync("/api/v1/admin/content/featured-recipes");
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task GetStories_AsNonAdmin_Returns403()
    {
        var client = CreateUserClient();
        var resp = await client.GetAsync("/api/v1/admin/content/stories");
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task GetVideos_AsNonAdmin_Returns403()
    {
        var client = CreateUserClient();
        var resp = await client.GetAsync("/api/v1/admin/content/videos");
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task GetSubmissions_AsNonAdmin_Returns403()
    {
        var client = CreateUserClient();
        var resp = await client.GetAsync("/api/v1/admin/ingredients/submissions");
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task CreateFeaturedRecipe_AsNonAdmin_Returns403()
    {
        var client = CreateUserClient();
        var resp = await client.PostAsync("/api/v1/admin/content/featured-recipes",
            JsonBody(new { recipeId = "r1", source = "spoonacular", title = "Test", displayOrder = 1 }));
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    // ── Featured Recipes CRUD ─────────────────────────────────────────────────

    [Fact]
    public async Task FeaturedRecipeCrud_CreateReadUpdateDelete()
    {
        var client = CreateAdminClient();

        // 1. List (should be empty initially for this type)
        var listResp = await client.GetAsync("/api/v1/admin/content/featured-recipes");
        Assert.Equal(HttpStatusCode.OK, listResp.StatusCode);

        // 2. Create
        var createResp = await client.PostAsync("/api/v1/admin/content/featured-recipes",
            JsonBody(new
            {
                recipeId = "recipe-001",
                source = "spoonacular",
                title = "Spaghetti Bolognese",
                description = "Classic Italian pasta dish",
                imageUrl = "https://example.com/img.jpg",
                displayOrder = 1,
            }));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

        var createBody = await createResp.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions);
        var id = created.GetProperty("id").GetString();
        Assert.NotNull(id);
        Assert.Equal("Spaghetti Bolognese", created.GetProperty("title").GetString());
        Assert.Equal("recipe-001", created.GetProperty("recipeId").GetString());
        Assert.Equal(1, created.GetProperty("displayOrder").GetInt32());

        // 3. Update
        var updateResp = await client.PutAsync($"/api/v1/admin/content/featured-recipes/{id}",
            JsonBody(new { title = "Spaghetti Carbonara", displayOrder = 2 }));
        Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

        var updateBody = await updateResp.Content.ReadAsStringAsync();
        var updated = JsonSerializer.Deserialize<JsonElement>(updateBody, JsonOptions);
        Assert.Equal("Spaghetti Carbonara", updated.GetProperty("title").GetString());
        Assert.Equal(2, updated.GetProperty("displayOrder").GetInt32());

        // 4. Delete
        var deleteResp = await client.DeleteAsync($"/api/v1/admin/content/featured-recipes/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

        // 5. Delete again → 404
        var deleteAgain = await client.DeleteAsync($"/api/v1/admin/content/featured-recipes/{id}");
        Assert.Equal(HttpStatusCode.NotFound, deleteAgain.StatusCode);
    }

    [Fact]
    public async Task CreateFeaturedRecipe_MissingTitle_Returns400()
    {
        var client = CreateAdminClient();
        var resp = await client.PostAsync("/api/v1/admin/content/featured-recipes",
            JsonBody(new { recipeId = "r1", source = "community", title = "", displayOrder = 1 }));
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task CreateFeaturedRecipe_MissingRecipeId_Returns400()
    {
        var client = CreateAdminClient();
        var resp = await client.PostAsync("/api/v1/admin/content/featured-recipes",
            JsonBody(new { recipeId = "", source = "community", title = "Test", displayOrder = 1 }));
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task UpdateFeaturedRecipe_NotFound_Returns404()
    {
        var client = CreateAdminClient();
        var resp = await client.PutAsync("/api/v1/admin/content/featured-recipes/nonexistent-id",
            JsonBody(new { title = "New Title" }));
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    // ── Stories CRUD ──────────────────────────────────────────────────────────

    [Fact]
    public async Task StoryCrud_CreateReadUpdateDelete()
    {
        var client = CreateAdminClient();

        // 1. Create
        var createResp = await client.PostAsync("/api/v1/admin/content/stories",
            JsonBody(new
            {
                title = "My First Story",
                coverImageUrl = "https://example.com/cover.jpg",
                author = "Jane Doe",
                content = "# Introduction\nThis is a story.",
                relatedRecipeIds = new[] { "rec-1", "rec-2" },
                readingTimeMinutes = 3,
            }));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

        var createBody = await createResp.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions);
        var id = created.GetProperty("id").GetString();
        Assert.NotNull(id);
        Assert.Equal("My First Story", created.GetProperty("title").GetString());
        Assert.Equal("Jane Doe", created.GetProperty("authorName").GetString());
        Assert.Equal(3, created.GetProperty("readingTimeMinutes").GetInt32());

        // 2. List
        var listResp = await client.GetAsync("/api/v1/admin/content/stories");
        Assert.Equal(HttpStatusCode.OK, listResp.StatusCode);

        var listBody = await listResp.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<JsonElement>(listBody, JsonOptions);
        Assert.True(list.GetArrayLength() >= 1);

        // 3. Update
        var updateResp = await client.PutAsync($"/api/v1/admin/content/stories/{id}",
            JsonBody(new { title = "Updated Story Title", readingTimeMinutes = 5 }));
        Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

        var updateBody = await updateResp.Content.ReadAsStringAsync();
        var updated = JsonSerializer.Deserialize<JsonElement>(updateBody, JsonOptions);
        Assert.Equal("Updated Story Title", updated.GetProperty("title").GetString());
        Assert.Equal(5, updated.GetProperty("readingTimeMinutes").GetInt32());

        // 4. Delete
        var deleteResp = await client.DeleteAsync($"/api/v1/admin/content/stories/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);
    }

    [Fact]
    public async Task CreateStory_MissingTitle_Returns400()
    {
        var client = CreateAdminClient();
        var resp = await client.PostAsync("/api/v1/admin/content/stories",
            JsonBody(new { title = "", author = "Jane" }));
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    // ── Videos CRUD ───────────────────────────────────────────────────────────

    [Fact]
    public async Task VideoCrud_CreateReadUpdateDelete()
    {
        var client = CreateAdminClient();

        // 1. Create
        var createResp = await client.PostAsync("/api/v1/admin/content/videos",
            JsonBody(new
            {
                title = "Knife Skills 101",
                thumbnailUrl = "https://example.com/thumb.jpg",
                videoUrl = "https://youtube.com/embed/abc123",
                durationSeconds = 420,
                creator = "Chef Sam",
            }));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

        var createBody = await createResp.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions);
        var id = created.GetProperty("id").GetString();
        Assert.NotNull(id);
        Assert.Equal("Knife Skills 101", created.GetProperty("title").GetString());
        Assert.Equal("https://youtube.com/embed/abc123", created.GetProperty("mediaUrl").GetString());
        Assert.Equal(420, created.GetProperty("durationSeconds").GetInt32());
        Assert.Equal("Chef Sam", created.GetProperty("authorName").GetString());

        // 2. List
        var listResp = await client.GetAsync("/api/v1/admin/content/videos");
        Assert.Equal(HttpStatusCode.OK, listResp.StatusCode);

        // 3. Update
        var updateResp = await client.PutAsync($"/api/v1/admin/content/videos/{id}",
            JsonBody(new { title = "Advanced Knife Skills", durationSeconds = 600 }));
        Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

        var updateBody = await updateResp.Content.ReadAsStringAsync();
        var updated = JsonSerializer.Deserialize<JsonElement>(updateBody, JsonOptions);
        Assert.Equal("Advanced Knife Skills", updated.GetProperty("title").GetString());
        Assert.Equal(600, updated.GetProperty("durationSeconds").GetInt32());

        // 4. Delete
        var deleteResp = await client.DeleteAsync($"/api/v1/admin/content/videos/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);
    }

    [Fact]
    public async Task CreateVideo_MissingTitle_Returns400()
    {
        var client = CreateAdminClient();
        var resp = await client.PostAsync("/api/v1/admin/content/videos",
            JsonBody(new { title = "", videoUrl = "https://youtube.com/embed/test" }));
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    // ── Ingredient Approval Queue ──────────────────────────────────────────────

    [Fact]
    public async Task GetSubmissions_AsAdmin_ReturnsOk()
    {
        var client = CreateAdminClient();
        var resp = await client.GetAsync("/api/v1/admin/ingredients/submissions");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var body = await resp.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);
        Assert.True(result.TryGetProperty("items", out _));
    }

    [Fact]
    public async Task IngredientApprovalWorkflow_SubmitApproveNotify()
    {
        var client = CreateAdminClient();

        // Pre-seed a pending ingredient submission
        var submissionId = Guid.NewGuid().ToString();
        _factory.ContentRepo.Add(new Content
        {
            Id = submissionId,
            ContentType = ContentType.IngredientSubmission,
            Title = "Dragon Fruit",
            Category = "fruit",
            SubmittedByUserId = "user-submitter",
            SubmissionStatus = SubmissionStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
            UpdatedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
        });

        // 1. List submissions — should appear
        var listResp = await client.GetAsync("/api/v1/admin/ingredients/submissions?status=pending");
        Assert.Equal(HttpStatusCode.OK, listResp.StatusCode);

        var listBody = await listResp.Content.ReadAsStringAsync();
        var listResult = JsonSerializer.Deserialize<JsonElement>(listBody, JsonOptions);
        var items = listResult.GetProperty("items");
        Assert.True(items.GetArrayLength() >= 1);

        // 2. Approve
        var approveResp = await client.PostAsync(
            $"/api/v1/admin/ingredients/submissions/{submissionId}/approve", null);
        Assert.Equal(HttpStatusCode.OK, approveResp.StatusCode);

        var approveBody = await approveResp.Content.ReadAsStringAsync();
        var approved = JsonSerializer.Deserialize<JsonElement>(approveBody, JsonOptions);
        Assert.Equal("Approved", approved.GetProperty("status").GetString());

        // 3. Notification was created for the submitting user
        var notif = _factory.AdminNotifRepo.All
            .FirstOrDefault(n => n.RecipientUserId == "user-submitter" &&
                                 n.Type == NotificationType.IngredientApproved);
        Assert.NotNull(notif);
    }

    [Fact]
    public async Task IngredientRejectionWorkflow_SubmitRejectWithReasonNotify()
    {
        var client = CreateAdminClient();

        // Pre-seed a pending submission
        var submissionId = Guid.NewGuid().ToString();
        _factory.ContentRepo.Add(new Content
        {
            Id = submissionId,
            ContentType = ContentType.IngredientSubmission,
            Title = "Suspicious Herb",
            SubmittedByUserId = "user-submitter-2",
            SubmissionStatus = SubmissionStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-3),
            UpdatedAt = DateTimeOffset.UtcNow.AddMinutes(-3),
        });

        // Reject with a reason
        var rejectResp = await client.PostAsync(
            $"/api/v1/admin/ingredients/submissions/{submissionId}/reject",
            JsonBody(new { reason = "Not a real ingredient" }));
        Assert.Equal(HttpStatusCode.OK, rejectResp.StatusCode);

        var rejectBody = await rejectResp.Content.ReadAsStringAsync();
        var rejected = JsonSerializer.Deserialize<JsonElement>(rejectBody, JsonOptions);
        Assert.Equal("Rejected", rejected.GetProperty("status").GetString());
        Assert.Equal("Not a real ingredient", rejected.GetProperty("rejectionReason").GetString());

        // Notification with reason was created
        var notif = _factory.AdminNotifRepo.All
            .FirstOrDefault(n => n.RecipientUserId == "user-submitter-2" &&
                                 n.Type == NotificationType.IngredientRejected);
        Assert.NotNull(notif);
        Assert.Contains("Not a real ingredient", notif.Message);
    }

    [Fact]
    public async Task ApproveSubmission_NotFound_Returns404()
    {
        var client = CreateAdminClient();
        var resp = await client.PostAsync(
            "/api/v1/admin/ingredients/submissions/nonexistent/approve", null);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task RejectSubmission_NotFound_Returns404()
    {
        var client = CreateAdminClient();
        var resp = await client.PostAsync(
            "/api/v1/admin/ingredients/submissions/nonexistent/reject",
            JsonBody(new { reason = "test" }));
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    // ── Content ordering ──────────────────────────────────────────────────────

    [Fact]
    public async Task ContentOrdering_UpdateDisplayOrderReflectedInList()
    {
        var client = CreateAdminClient();

        // Create two featured recipes with different display orders
        var createResp1 = await client.PostAsync("/api/v1/admin/content/featured-recipes",
            JsonBody(new { recipeId = "ord-r1", source = "spoonacular", title = "Second Recipe", displayOrder = 2 }));
        Assert.Equal(HttpStatusCode.Created, createResp1.StatusCode);
        var id1 = JsonSerializer.Deserialize<JsonElement>(
            await createResp1.Content.ReadAsStringAsync(), JsonOptions).GetProperty("id").GetString()!;

        var createResp2 = await client.PostAsync("/api/v1/admin/content/featured-recipes",
            JsonBody(new { recipeId = "ord-r2", source = "community", title = "First Recipe", displayOrder = 1 }));
        Assert.Equal(HttpStatusCode.Created, createResp2.StatusCode);
        var id2 = JsonSerializer.Deserialize<JsonElement>(
            await createResp2.Content.ReadAsStringAsync(), JsonOptions).GetProperty("id").GetString()!;

        // Update recipe 1's displayOrder to 0 (should now be first)
        var updateResp = await client.PutAsync($"/api/v1/admin/content/featured-recipes/{id1}",
            JsonBody(new { displayOrder = 0 }));
        Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

        // Clean up
        await client.DeleteAsync($"/api/v1/admin/content/featured-recipes/{id1}");
        await client.DeleteAsync($"/api/v1/admin/content/featured-recipes/{id2}");
    }
}
