using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Blend.Api.Auth.Models;
using Blend.Api.Home.Models;
using Blend.Domain.Entities;
using Blend.Domain.Identity;
using Blend.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blend.Tests.Integration;

// ── In-memory content repository for home integration tests ───────────────────

public sealed class InMemoryHomeContentRepository : IRepository<Content>
{
    private readonly ConcurrentDictionary<string, Content> _items = new();

    public void Seed(Content content) => _items[content.Id] = content;

    public Task<Content?> GetByIdAsync(string id, string partitionKey, CancellationToken ct = default)
        => Task.FromResult(_items.GetValueOrDefault(id));

    public Task<IReadOnlyList<Content>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken ct = default)
    {
        IEnumerable<Content> results = _items.Values;

        if (partitionKey is not null)
        {
            results = results.Where(c => c.ContentType.ToString() == partitionKey);
        }

        if (query.Contains("isPublished = true"))
        {
            results = results.Where(c => c.IsPublished);
        }

        return Task.FromResult<IReadOnlyList<Content>>([.. results]);
    }

    public Task<Content> CreateAsync(Content entity, CancellationToken ct = default)
    {
        _items[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<Content> UpdateAsync(Content entity, string id, string partitionKey, CancellationToken ct = default)
    {
        _items[id] = entity;
        return Task.FromResult(entity);
    }

    public Task<Content> PatchAsync(string id, string partitionKey, IReadOnlyDictionary<string, object?> patches, CancellationToken ct = default)
        => Task.FromResult(_items[id]);

    public Task DeleteAsync(string id, string partitionKey, CancellationToken ct = default)
    {
        _items.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<PagedResult<Content>> GetPagedAsync(string query, FeedPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<Content> { Items = [.. _items.Values] });

    public Task<PagedResult<Content>> GetOffsetPagedAsync(string baseQuery, OffsetPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<Content> { Items = [.. _items.Values] });

    public Task ExecuteTransactionalBatchAsync(string partitionKey, IEnumerable<(TransactionalBatchOperation Operation, Content Entity)> operations, CancellationToken ct = default)
        => Task.CompletedTask;
}

// ── In-memory recipe repository for home integration tests ────────────────────

public sealed class InMemoryHomeIntegrationRecipeRepository : IRepository<Recipe>
{
    private readonly ConcurrentDictionary<string, Recipe> _items = new();

    public void Seed(Recipe recipe) => _items[recipe.Id] = recipe;

    public Task<Recipe?> GetByIdAsync(string id, string partitionKey, CancellationToken ct = default)
        => Task.FromResult(_items.GetValueOrDefault(id));

    public Task<IReadOnlyList<Recipe>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken ct = default)
    {
        IEnumerable<Recipe> results = _items.Values;

        if (query.Contains("c.isPublic = true"))
        {
            results = results.Where(r => r.IsPublic);
        }

        return Task.FromResult<IReadOnlyList<Recipe>>([.. results.OrderByDescending(r => r.CreatedAt)]);
    }

    public Task<Recipe> CreateAsync(Recipe entity, CancellationToken ct = default)
    {
        _items[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<Recipe> UpdateAsync(Recipe entity, string id, string partitionKey, CancellationToken ct = default)
    {
        _items[id] = entity;
        return Task.FromResult(entity);
    }

    public Task<Recipe> PatchAsync(string id, string partitionKey, IReadOnlyDictionary<string, object?> patches, CancellationToken ct = default)
        => Task.FromResult(_items[id]);

    public Task DeleteAsync(string id, string partitionKey, CancellationToken ct = default)
    {
        _items.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<PagedResult<Recipe>> GetPagedAsync(string query, FeedPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<Recipe> { Items = [.. _items.Values] });

    public Task<PagedResult<Recipe>> GetOffsetPagedAsync(string baseQuery, OffsetPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<Recipe> { Items = [.. _items.Values] });

    public Task ExecuteTransactionalBatchAsync(string partitionKey, IEnumerable<(TransactionalBatchOperation Operation, Recipe Entity)> operations, CancellationToken ct = default)
        => Task.CompletedTask;
}

// ── In-memory activity repository for home integration tests ──────────────────

public sealed class InMemoryHomeIntegrationActivityRepository : IRepository<Activity>
{
    private readonly ConcurrentDictionary<string, Activity> _items = new();

    public void Seed(Activity activity) => _items[activity.Id] = activity;

    public Task<Activity?> GetByIdAsync(string id, string partitionKey, CancellationToken ct = default)
        => Task.FromResult(_items.GetValueOrDefault(id));

    public Task<IReadOnlyList<Activity>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken ct = default)
    {
        IEnumerable<Activity> results = _items.Values;

        if (partitionKey is not null)
        {
            results = results.Where(a => a.UserId == partitionKey);
        }

        if (query.Contains("c.type = 'Viewed'"))
        {
            results = results.Where(a => a.Type == ActivityType.Viewed);
        }

        return Task.FromResult<IReadOnlyList<Activity>>([.. results.OrderByDescending(a => a.Timestamp)]);
    }

    public Task<Activity> CreateAsync(Activity entity, CancellationToken ct = default)
    {
        _items[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<Activity> UpdateAsync(Activity entity, string id, string partitionKey, CancellationToken ct = default)
    {
        _items[id] = entity;
        return Task.FromResult(entity);
    }

    public Task<Activity> PatchAsync(string id, string partitionKey, IReadOnlyDictionary<string, object?> patches, CancellationToken ct = default)
        => Task.FromResult(_items[id]);

    public Task DeleteAsync(string id, string partitionKey, CancellationToken ct = default)
    {
        _items.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<PagedResult<Activity>> GetPagedAsync(string query, FeedPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<Activity> { Items = [.. _items.Values] });

    public Task<PagedResult<Activity>> GetOffsetPagedAsync(string baseQuery, OffsetPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<Activity> { Items = [.. _items.Values] });

    public Task ExecuteTransactionalBatchAsync(string partitionKey, IEnumerable<(TransactionalBatchOperation Operation, Activity Entity)> operations, CancellationToken ct = default)
        => Task.CompletedTask;
}

// ── Test Web Application Factory ───────────────────────────────────────────────

public sealed class HomeTestFactory : WebApplicationFactory<Program>
{
    private readonly InMemoryUserStore _userStore = new();
    public readonly InMemoryHomeContentRepository ContentRepository = new();
    public readonly InMemoryHomeIntegrationRecipeRepository RecipeRepository = new();
    public readonly InMemoryHomeIntegrationActivityRepository ActivityRepository = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:SecretKey", "test-secret-key-that-is-long-enough-for-hs256-algorithm");
        builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IUserStore<BlendUser>>();
            services.AddSingleton<IUserStore<BlendUser>>(_userStore);

            services.RemoveAll<IRepository<Content>>();
            services.AddSingleton<IRepository<Content>>(ContentRepository);

            services.RemoveAll<IRepository<Recipe>>();
            services.AddSingleton<IRepository<Recipe>>(RecipeRepository);

            // Register the activity repo for Home (separate from Search's activity repo)
            services.RemoveAll<IRepository<Activity>>();
            services.AddSingleton<IRepository<Activity>>(ActivityRepository);
        });
    }
}

// ── Integration tests ──────────────────────────────────────────────────────────

public class HomeEndpointTests : IClassFixture<HomeTestFactory>
{
    private readonly HomeTestFactory _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public HomeEndpointTests(HomeTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = false,
    });

    private static string UniqueEmail() => $"home-{Guid.NewGuid():N}@example.com";

    private static StringContent JsonBody(object payload) =>
        new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    private async Task<(HttpClient Client, string UserId)> RegisterAndAuthenticateAsync()
    {
        var client = CreateClient();
        var email = UniqueEmail();
        var registerResponse = await client.PostAsync("/api/v1/auth/register",
            JsonBody(new { displayName = "HomeUser", email, password = "ValidPass1!" }));
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var authBody = await registerResponse.Content.ReadAsStringAsync();
        var auth = JsonSerializer.Deserialize<AuthResponse>(authBody, JsonOptions);
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

    // 1. GET /api/v1/home without auth → 200 (anonymous allowed)
    [Fact]
    public async Task GetHome_Anonymous_Returns200()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/home");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 2. GET /api/v1/home returns valid JSON with all four sections
    [Fact]
    public async Task GetHome_ReturnsValidResponseShape()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/home");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);

        Assert.True(doc.RootElement.TryGetProperty("search", out _));
        Assert.True(doc.RootElement.TryGetProperty("featured", out _));
        Assert.True(doc.RootElement.TryGetProperty("community", out _));
        Assert.True(doc.RootElement.TryGetProperty("recentlyViewed", out _));
    }

    // 3. GET /api/v1/home — search section has a non-empty placeholder
    [Fact]
    public async Task GetHome_SearchSection_HasPlaceholder()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/home");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var homeResponse = JsonSerializer.Deserialize<HomeResponse>(body, JsonOptions);
        Assert.NotNull(homeResponse);
        Assert.False(string.IsNullOrWhiteSpace(homeResponse.Search.Placeholder));
    }

    // 4. GET /api/v1/home — featured section contains recipes, stories, and videos keys
    [Fact]
    public async Task GetHome_FeaturedSection_ContainsAllSubsections()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/home");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        var featured = doc.RootElement.GetProperty("featured");

        Assert.True(featured.TryGetProperty("recipes", out _));
        Assert.True(featured.TryGetProperty("stories", out _));
        Assert.True(featured.TryGetProperty("videos", out _));
    }

    // 5. GET /api/v1/home — published featured recipe appears in response
    [Fact]
    public async Task GetHome_PublishedFeaturedRecipe_AppearsInResponse()
    {
        var contentId = $"fr-{Guid.NewGuid():N}";
        _factory.ContentRepository.Seed(new Content
        {
            Id = contentId,
            ContentType = ContentType.FeaturedRecipe,
            Title = "Featured Pasta",
            IsPublished = true,
            ThumbnailUrl = "https://img/pasta.jpg",
            AuthorName = "Chef Test",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/home");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var homeResponse = JsonSerializer.Deserialize<HomeResponse>(body, JsonOptions);
        Assert.NotNull(homeResponse);

        var recipe = homeResponse.Featured.Recipes.FirstOrDefault(r => r.Id == contentId);
        Assert.NotNull(recipe);
        Assert.Equal("Featured Pasta", recipe.Title);
        Assert.Equal("https://img/pasta.jpg", recipe.ImageUrl);
        Assert.Equal("Chef Test", recipe.Attribution);
    }

    // 6. GET /api/v1/home — published story appears in response
    [Fact]
    public async Task GetHome_PublishedStory_AppearsInResponse()
    {
        var contentId = $"st-{Guid.NewGuid():N}";
        _factory.ContentRepository.Seed(new Content
        {
            Id = contentId,
            ContentType = ContentType.Story,
            Title = "Sourdough Story",
            IsPublished = true,
            Body = "A fascinating story about sourdough bread.",
            ThumbnailUrl = "https://img/story.jpg",
            AuthorName = "Writer Test",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/home");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var homeResponse = JsonSerializer.Deserialize<HomeResponse>(body, JsonOptions);
        Assert.NotNull(homeResponse);

        var story = homeResponse.Featured.Stories.FirstOrDefault(s => s.Id == contentId);
        Assert.NotNull(story);
        Assert.Equal("Sourdough Story", story.Title);
        Assert.Equal("Writer Test", story.Author);
    }

    // 7. GET /api/v1/home — published video appears in response
    [Fact]
    public async Task GetHome_PublishedVideo_AppearsInResponse()
    {
        var contentId = $"vid-{Guid.NewGuid():N}";
        _factory.ContentRepository.Seed(new Content
        {
            Id = contentId,
            ContentType = ContentType.Video,
            Title = "Quick Stir Fry Video",
            IsPublished = true,
            ThumbnailUrl = "https://img/vid-thumb.jpg",
            MediaUrl = "https://embed.video/123",
            AuthorName = "Creator Test",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/home");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var homeResponse = JsonSerializer.Deserialize<HomeResponse>(body, JsonOptions);
        Assert.NotNull(homeResponse);

        var video = homeResponse.Featured.Videos.FirstOrDefault(v => v.Id == contentId);
        Assert.NotNull(video);
        Assert.Equal("Quick Stir Fry Video", video.Title);
        Assert.Equal("https://embed.video/123", video.VideoUrl);
        Assert.Equal("Creator Test", video.Creator);
    }

    // 8. GET /api/v1/home — unpublished content not returned
    [Fact]
    public async Task GetHome_UnpublishedContent_NotIncluded()
    {
        var contentId = $"draft-{Guid.NewGuid():N}";
        _factory.ContentRepository.Seed(new Content
        {
            Id = contentId,
            ContentType = ContentType.FeaturedRecipe,
            Title = "Draft Recipe",
            IsPublished = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/home");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var homeResponse = JsonSerializer.Deserialize<HomeResponse>(body, JsonOptions);
        Assert.NotNull(homeResponse);

        Assert.DoesNotContain(homeResponse.Featured.Recipes, r => r.Id == contentId);
    }

    // 9. GET /api/v1/home — public community recipe appears in community section
    [Fact]
    public async Task GetHome_PublicCommunityRecipe_AppearsInCommunitySection()
    {
        var recipeId = $"cr-{Guid.NewGuid():N}";
        _factory.RecipeRepository.Seed(new Recipe
        {
            Id = recipeId,
            AuthorId = "author-home",
            Title = "Community Tacos",
            IsPublic = true,
            LikeCount = 10,
            CuisineType = "Mexican",
            Tags = [],
            Ingredients = [],
            Directions = [],
            Photos = [],
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/home");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var homeResponse = JsonSerializer.Deserialize<HomeResponse>(body, JsonOptions);
        Assert.NotNull(homeResponse);

        var recipe = homeResponse.Community.Recipes.FirstOrDefault(r => r.Id == recipeId);
        Assert.NotNull(recipe);
        Assert.Equal("Community Tacos", recipe.Title);
        Assert.Equal("Mexican", recipe.CuisineType);
        Assert.Equal(10, recipe.LikeCount);
    }

    // 10. GET /api/v1/home — private community recipe not included
    [Fact]
    public async Task GetHome_PrivateCommunityRecipe_NotIncluded()
    {
        var recipeId = $"priv-{Guid.NewGuid():N}";
        _factory.RecipeRepository.Seed(new Recipe
        {
            Id = recipeId,
            AuthorId = "author-home",
            Title = "Secret Recipe",
            IsPublic = false,
            Tags = [],
            Ingredients = [],
            Directions = [],
            Photos = [],
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/home");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var homeResponse = JsonSerializer.Deserialize<HomeResponse>(body, JsonOptions);
        Assert.NotNull(homeResponse);

        Assert.DoesNotContain(homeResponse.Community.Recipes, r => r.Id == recipeId);
    }

    // 11. GET /api/v1/home — guest user receives empty recentlyViewed
    [Fact]
    public async Task GetHome_GuestUser_RecentlyViewedIsEmpty()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/home");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var homeResponse = JsonSerializer.Deserialize<HomeResponse>(body, JsonOptions);
        Assert.NotNull(homeResponse);

        Assert.Empty(homeResponse.RecentlyViewed.Recipes);
    }

    // 12. GET /api/v1/home — authenticated user receives recently viewed entries
    [Fact]
    public async Task GetHome_AuthenticatedUser_ReturnsRecentlyViewed()
    {
        var (client, userId) = await RegisterAndAuthenticateAsync();

        _factory.ActivityRepository.Seed(new Activity
        {
            Id = $"{userId}:Viewed:recipe-home-test",
            UserId = userId,
            Type = ActivityType.Viewed,
            ReferenceId = "recipe-home-test",
            ReferenceType = "Recipe",
            Timestamp = DateTimeOffset.UtcNow,
        });

        var response = await client.GetAsync("/api/v1/home");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var homeResponse = JsonSerializer.Deserialize<HomeResponse>(body, JsonOptions);
        Assert.NotNull(homeResponse);

        Assert.Contains(homeResponse.RecentlyViewed.Recipes, r => r.RecipeId == "recipe-home-test");
    }

    // 13. GET /api/v1/home — recently viewed has recipeId, referenceType, viewedAt fields
    [Fact]
    public async Task GetHome_RecentlyViewed_HasRequiredFields()
    {
        var (client, userId) = await RegisterAndAuthenticateAsync();

        var timestamp = DateTimeOffset.UtcNow.AddMinutes(-5);
        _factory.ActivityRepository.Seed(new Activity
        {
            Id = $"{userId}:Viewed:fields-test-recipe",
            UserId = userId,
            Type = ActivityType.Viewed,
            ReferenceId = "fields-test-recipe",
            ReferenceType = "SpoonacularRecipe",
            Timestamp = timestamp,
        });

        var response = await client.GetAsync("/api/v1/home");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        var recentlyViewed = doc.RootElement.GetProperty("recentlyViewed").GetProperty("recipes");

        var entry = recentlyViewed.EnumerateArray()
            .FirstOrDefault(e => e.GetProperty("recipeId").GetString() == "fields-test-recipe");

        Assert.True(entry.ValueKind != JsonValueKind.Undefined);
        Assert.Equal("SpoonacularRecipe", entry.GetProperty("referenceType").GetString());
        Assert.True(entry.TryGetProperty("viewedAt", out _));
    }

    // 14. GET /api/v1/home — empty sections return gracefully (no errors)
    [Fact]
    public async Task GetHome_AllSectionsEmpty_ReturnsSuccessfully()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/home");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var homeResponse = JsonSerializer.Deserialize<HomeResponse>(body, JsonOptions);
        Assert.NotNull(homeResponse);

        // All sections exist but may be empty — no errors thrown
        Assert.NotNull(homeResponse.Featured.Recipes);
        Assert.NotNull(homeResponse.Featured.Stories);
        Assert.NotNull(homeResponse.Featured.Videos);
        Assert.NotNull(homeResponse.Community.Recipes);
        Assert.NotNull(homeResponse.RecentlyViewed.Recipes);
    }
}
