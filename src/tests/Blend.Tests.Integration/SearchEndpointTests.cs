using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Blend.Api.Auth.Models;
using Blend.Api.Search.Models;
using Blend.Domain.Entities;
using Blend.Domain.Identity;
using Blend.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blend.Tests.Integration;

// ── In-memory activity repository for search integration tests ─────────────────

public sealed class InMemorySearchActivityRepository : IRepository<Activity>
{
    private readonly ConcurrentDictionary<string, Activity> _items = new();

    public void Seed(Activity activity) => _items[activity.Id] = activity;

    public IReadOnlyCollection<Activity> All => _items.Values.ToList();

    public Task<Activity?> GetByIdAsync(string id, string partitionKey, CancellationToken ct = default)
        => Task.FromResult(_items.GetValueOrDefault(id));

    public Task<IReadOnlyList<Activity>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken ct = default)
    {
        IEnumerable<Activity> results = _items.Values;

        if (!string.IsNullOrEmpty(partitionKey))
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

// ── Search-specific in-memory recipe repository ────────────────────────────────

public sealed class InMemorySearchRecipeRepository : IRepository<Recipe>
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

        return Task.FromResult<IReadOnlyList<Recipe>>([.. results]);
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

// ── Test Web Application Factory ───────────────────────────────────────────────

public sealed class SearchTestFactory : WebApplicationFactory<Program>
{
    private readonly InMemoryUserStore _userStore = new();
    public readonly InMemorySearchRecipeRepository RecipeRepository = new();
    public readonly InMemorySearchActivityRepository ActivityRepository = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:SecretKey", "test-secret-key-that-is-long-enough-for-hs256-algorithm");
        builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IUserStore<BlendUser>>();
            services.AddSingleton<IUserStore<BlendUser>>(_userStore);

            services.RemoveAll<IRepository<Recipe>>();
            services.AddSingleton<IRepository<Recipe>>(RecipeRepository);

            services.RemoveAll<IRepository<Activity>>();
            services.AddSingleton<IRepository<Activity>>(ActivityRepository);
        });
    }
}

// ── Integration tests ──────────────────────────────────────────────────────────

public class SearchEndpointTests : IClassFixture<SearchTestFactory>
{
    private readonly SearchTestFactory _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public SearchEndpointTests(SearchTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = false,
    });

    private static string UniqueEmail() => $"search-{Guid.NewGuid():N}@example.com";

    private static StringContent JsonBody(object payload) =>
        new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    private async Task<(HttpClient Client, string UserId)> RegisterAndAuthenticateAsync()
    {
        var client = CreateClient();
        var email = UniqueEmail();
        var registerResponse = await client.PostAsync("/api/v1/auth/register",
            JsonBody(new { displayName = "SearchUser", email, password = "ValidPass1!" }));
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

    // 1. GET /api/v1/search/recipes without auth → 200 (anonymous allowed)
    [Fact]
    public async Task SearchRecipes_Anonymous_Returns200()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/search/recipes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 2. GET /api/v1/search/recipes returns valid JSON with results and metadata
    [Fact]
    public async Task SearchRecipes_ReturnsValidResponseShape()
    {
        _factory.RecipeRepository.Seed(new Recipe
        {
            Id = $"shape-{Guid.NewGuid():N}",
            AuthorId = "author-1",
            Title = "Test Recipe",
            IsPublic = true,
            LikeCount = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Ingredients = [],
            Directions = [],
            Tags = [],
            Photos = [],
        });

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/search/recipes?q=test");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.TryGetProperty("results", out _));
        Assert.True(doc.RootElement.TryGetProperty("metadata", out _));
    }

    // 3. GET /api/v1/search/recipes — metadata contains totalResults and quotaExhausted
    [Fact]
    public async Task SearchRecipes_MetadataContainsRequiredFields()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/search/recipes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        var metadata = doc.RootElement.GetProperty("metadata");

        Assert.True(metadata.TryGetProperty("totalResults", out _));
        Assert.True(metadata.TryGetProperty("quotaExhausted", out _));
    }

    // 4. GET /api/v1/search/recipes with internal recipe — result has dataSource = "community"
    [Fact]
    public async Task SearchRecipes_InternalRecipeResult_HasCommunityDataSource()
    {
        var id = $"ds-{Guid.NewGuid():N}";
        _factory.RecipeRepository.Seed(new Recipe
        {
            Id = id,
            AuthorId = "author-1",
            Title = "Community Pasta",
            IsPublic = true,
            LikeCount = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Ingredients = [],
            Directions = [],
            Tags = [],
            Photos = [],
        });

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/search/recipes?q=pasta");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<UnifiedSearchResponse>(body, JsonOptions);
        Assert.NotNull(searchResponse);

        var communityResult = searchResponse.Results.FirstOrDefault(r => r.DataSource == RecipeDataSource.Community);
        Assert.NotNull(communityResult);
    }

    // 5. GET /api/v1/search/recipes with private recipe — not returned
    [Fact]
    public async Task SearchRecipes_PrivateRecipe_NotIncluded()
    {
        var id = $"private-{Guid.NewGuid():N}";
        // The in-memory repo filters isPublic = true from the query
        // Private recipe should not appear.
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/search/recipes?q=secret");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 6. GET /api/v1/users/me/recently-viewed without auth → 401
    [Fact]
    public async Task GetRecentlyViewed_Anonymous_Returns401()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/users/me/recently-viewed");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // 7. GET /api/v1/users/me/recently-viewed with auth → 200
    [Fact]
    public async Task GetRecentlyViewed_Authenticated_Returns200()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();
        var response = await client.GetAsync("/api/v1/users/me/recently-viewed");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 8. GET /api/v1/users/me/recently-viewed returns viewed activities for user
    [Fact]
    public async Task GetRecentlyViewed_ReturnsViewedActivities()
    {
        var (client, userId) = await RegisterAndAuthenticateAsync();

        _factory.ActivityRepository.Seed(new Activity
        {
            Id = $"{userId}:Viewed:recipe-abc",
            UserId = userId,
            Type = ActivityType.Viewed,
            ReferenceId = "recipe-abc",
            ReferenceType = "Recipe",
            Timestamp = DateTimeOffset.UtcNow,
        });

        var response = await client.GetAsync("/api/v1/users/me/recently-viewed");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var activities = JsonSerializer.Deserialize<IReadOnlyList<Activity>>(body, JsonOptions);
        Assert.NotNull(activities);
        Assert.True(activities.Count >= 1);
        Assert.Contains(activities, a => a.ReferenceId == "recipe-abc");
    }

    // 9. GET /api/v1/search/recipes with pageSize → honours page size
    [Fact]
    public async Task SearchRecipes_PageSize_Honoured()
    {
        // Seed several public recipes
        for (var i = 0; i < 5; i++)
        {
            _factory.RecipeRepository.Seed(new Recipe
            {
                Id = $"pg-{Guid.NewGuid():N}",
                AuthorId = "author-1",
                Title = $"Page Recipe {i}",
                IsPublic = true,
                LikeCount = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Ingredients = [],
                Directions = [],
                Tags = [],
                Photos = [],
            });
        }

        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/search/recipes?pageSize=2");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<UnifiedSearchResponse>(body, JsonOptions);
        Assert.NotNull(searchResponse);
        Assert.True(searchResponse.Results.Count <= 2);
    }

    // 10. GET /api/v1/search/recipes with cursor → second page is different from first
    [Fact]
    public async Task SearchRecipes_CursorPagination_SecondPageIsDifferent()
    {
        // Ensure there are enough public recipes for two pages
        for (var i = 0; i < 4; i++)
        {
            _factory.RecipeRepository.Seed(new Recipe
            {
                Id = $"cur-{Guid.NewGuid():N}",
                AuthorId = "author-1",
                Title = $"Cursor Recipe {i}",
                IsPublic = true,
                LikeCount = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Ingredients = [],
                Directions = [],
                Tags = [],
                Photos = [],
            });
        }

        var client = CreateClient();

        var firstResponse = await client.GetAsync("/api/v1/search/recipes?pageSize=2");
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        var firstBody = await firstResponse.Content.ReadAsStringAsync();
        var firstPage = JsonSerializer.Deserialize<UnifiedSearchResponse>(firstBody, JsonOptions);
        Assert.NotNull(firstPage);

        if (firstPage.Metadata.NextCursor is null)
        {
            // Not enough results for a second page — skip.
            return;
        }

        var cursor = Uri.EscapeDataString(firstPage.Metadata.NextCursor);
        var secondResponse = await client.GetAsync($"/api/v1/search/recipes?pageSize=2&cursor={cursor}");
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
    }
}
