using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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

// ── In-memory repositories ────────────────────────────────────────────────────

public sealed class InMemoryRecipeRepository : IRepository<Recipe>
{
    private static readonly Regex IdQueryPattern = new(@"c\.id = '([^']+)'", RegexOptions.Compiled);

    private readonly ConcurrentDictionary<string, Recipe> _items = new();

    public void Seed(Recipe recipe) => _items[recipe.Id] = recipe;

    public Task<Recipe?> GetByIdAsync(string id, string partitionKey, CancellationToken ct = default)
        => Task.FromResult(_items.GetValueOrDefault(id));

    public Task<IReadOnlyList<Recipe>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken ct = default)
    {
        IEnumerable<Recipe> results = _items.Values;

        var idMatch = IdQueryPattern.Match(query);
        if (idMatch.Success)
        {
            var id = idMatch.Groups[1].Value;
            results = results.Where(r => r.Id == id);
        }
        else if (!string.IsNullOrEmpty(partitionKey))
        {
            results = results.Where(r => r.AuthorId == partitionKey);
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
    {
        if (!_items.TryGetValue(id, out var existing))
            throw new KeyNotFoundException($"Recipe {id} not found.");

        var isPublic = existing.IsPublic;
        var updatedAt = existing.UpdatedAt;
        var likeCount = existing.LikeCount;

        foreach (var (path, value) in patches)
        {
            switch (path)
            {
                case "/isPublic":
                    isPublic = value is bool b ? b : isPublic;
                    break;
                case "/updatedAt":
                    updatedAt = value is DateTimeOffset dto ? dto : updatedAt;
                    break;
                case "/likeCount":
                    likeCount = value switch
                    {
                        int i => i,
                        long l => (int)l,
                        _ => likeCount,
                    };
                    break;
            }
        }

        var updated = new Recipe
        {
            Id = existing.Id,
            AuthorId = existing.AuthorId,
            Title = existing.Title,
            Description = existing.Description,
            Ingredients = existing.Ingredients,
            Directions = existing.Directions,
            PrepTime = existing.PrepTime,
            CookTime = existing.CookTime,
            Servings = existing.Servings,
            CuisineType = existing.CuisineType,
            DishType = existing.DishType,
            Tags = existing.Tags,
            FeaturedPhotoUrl = existing.FeaturedPhotoUrl,
            Photos = existing.Photos,
            IsPublic = isPublic,
            LikeCount = likeCount,
            ViewCount = existing.ViewCount,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = updatedAt,
        };

        _items[id] = updated;
        return Task.FromResult(updated);
    }

    public Task DeleteAsync(string id, string partitionKey, CancellationToken ct = default)
    {
        _items.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<PagedResult<Recipe>> GetPagedAsync(string query, FeedPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
    {
        IEnumerable<Recipe> results = _items.Values;
        if (!string.IsNullOrEmpty(partitionKey))
            results = results.Where(r => r.AuthorId == partitionKey);

        if (query.Contains("c.isPublic = true"))
            results = results.Where(r => r.IsPublic);

        return Task.FromResult(new PagedResult<Recipe> { Items = [.. results] });
    }

    public Task<PagedResult<Recipe>> GetOffsetPagedAsync(string baseQuery, OffsetPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<Recipe> { Items = [.. _items.Values] });

    public Task ExecuteTransactionalBatchAsync(string partitionKey, IEnumerable<(TransactionalBatchOperation Operation, Recipe Entity)> operations, CancellationToken ct = default)
        => Task.CompletedTask;
}

public sealed class InMemoryActivityRepository : IRepository<Activity>
{
    private static readonly Regex IdQueryPattern = new(@"c\.id = '([^']+)'", RegexOptions.Compiled);
    private static readonly Regex RefIdQueryPattern = new(@"c\.referenceId = '([^']+)'", RegexOptions.Compiled);
    private static readonly Regex UserIdQueryPattern = new(@"c\.userId = '([^']+)'", RegexOptions.Compiled);

    private readonly ConcurrentDictionary<string, Activity> _items = new();

    public void Seed(Activity activity) => _items[activity.Id] = activity;

    public Task<Activity?> GetByIdAsync(string id, string partitionKey, CancellationToken ct = default)
        => Task.FromResult(_items.GetValueOrDefault(id));

    public Task<IReadOnlyList<Activity>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken ct = default)
    {
        IEnumerable<Activity> results = _items.Values;

        if (!string.IsNullOrEmpty(partitionKey))
            results = results.Where(a => a.UserId == partitionKey);

        var idMatch = IdQueryPattern.Match(query);
        if (idMatch.Success)
        {
            var id = idMatch.Groups[1].Value;
            results = results.Where(a => a.Id == id);
        }

        var refIdMatch = RefIdQueryPattern.Match(query);
        if (refIdMatch.Success)
        {
            var refId = refIdMatch.Groups[1].Value;
            results = results.Where(a => a.ReferenceId == refId);
        }

        var userIdMatch = UserIdQueryPattern.Match(query);
        if (userIdMatch.Success)
        {
            var userId = userIdMatch.Groups[1].Value;
            results = results.Where(a => a.UserId == userId);
        }

        if (query.Contains("c.type = 'Liked'"))
            results = results.Where(a => a.Type == ActivityType.Liked);

        if (query.Contains("c.referenceType = 'Recipe'"))
            results = results.Where(a => a.ReferenceType == "Recipe");

        return Task.FromResult<IReadOnlyList<Activity>>([.. results]);
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
    {
        if (!_items.TryGetValue(id, out var existing))
            throw new KeyNotFoundException($"Activity {id} not found.");
        return Task.FromResult(existing);
    }

    public Task DeleteAsync(string id, string partitionKey, CancellationToken ct = default)
    {
        _items.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<PagedResult<Activity>> GetPagedAsync(string query, FeedPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
    {
        IEnumerable<Activity> results = _items.Values;

        if (!string.IsNullOrEmpty(partitionKey))
            results = results.Where(a => a.UserId == partitionKey);

        if (query.Contains("c.type = 'Liked'"))
            results = results.Where(a => a.Type == ActivityType.Liked);

        if (query.Contains("c.referenceType = 'Recipe'"))
            results = results.Where(a => a.ReferenceType == "Recipe");

        var userIdMatch = UserIdQueryPattern.Match(query);
        if (userIdMatch.Success)
        {
            var userId = userIdMatch.Groups[1].Value;
            results = results.Where(a => a.UserId == userId);
        }

        return Task.FromResult(new PagedResult<Activity> { Items = [.. results] });
    }

    public Task<PagedResult<Activity>> GetOffsetPagedAsync(string baseQuery, OffsetPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<Activity> { Items = [.. _items.Values] });

    public Task ExecuteTransactionalBatchAsync(string partitionKey, IEnumerable<(TransactionalBatchOperation Operation, Activity Entity)> operations, CancellationToken ct = default)
        => Task.CompletedTask;
}

// ── Test Factory ──────────────────────────────────────────────────────────────

public sealed class RecipesTestFactory : WebApplicationFactory<Program>
{
    private readonly InMemoryUserStore _userStore = new();
    public readonly InMemoryRecipeRepository RecipeRepository = new();
    public readonly InMemoryActivityRepository ActivityRepository = new();

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

// ── Integration Tests ─────────────────────────────────────────────────────────

public class RecipeEndpointTests : IClassFixture<RecipesTestFactory>
{
    private readonly RecipesTestFactory _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public RecipeEndpointTests(RecipesTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = false,
    });

    private static string UniqueEmail() => $"recipe-{Guid.NewGuid():N}@example.com";

    private static StringContent JsonBody(object payload) =>
        new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    private async Task<(HttpClient Client, string UserId)> RegisterAndAuthenticateAsync()
    {
        var client = CreateClient();
        var email = UniqueEmail();
        var registerResponse = await client.PostAsync("/api/v1/auth/register",
            JsonBody(new { displayName = "RecipeUser", email, password = "ValidPass1!" }));
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

    private static object ValidRecipeBody(bool isPublic = false) => new
    {
        title = "Chocolate Cake",
        description = "A delicious cake",
        ingredients = isPublic
            ? new[] { new { quantity = 2.0, unit = "cups", ingredientName = "Flour", ingredientId = (string?)null } }
            : Array.Empty<object>(),
        directions = isPublic
            ? new[] { new { stepNumber = 1, text = "Mix flour", mediaUrl = (string?)null } }
            : Array.Empty<object>(),
        prepTime = 15,
        cookTime = 30,
        servings = 8,
        cuisineType = (string?)null,
        dishType = (string?)null,
        tags = Array.Empty<string>(),
        featuredPhotoUrl = (string?)null,
        photos = Array.Empty<string>(),
        isPublic,
    };

    // 1. POST /api/v1/recipes without auth → 401
    [Fact]
    public async Task CreateRecipe_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var response = await client.PostAsync("/api/v1/recipes", JsonBody(ValidRecipeBody()));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // 2. POST /api/v1/recipes with valid data → 201
    [Fact]
    public async Task CreateRecipe_WithValidData_Returns201()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();
        var response = await client.PostAsync("/api/v1/recipes", JsonBody(ValidRecipeBody()));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // 3. POST /api/v1/recipes with empty title → 400
    [Fact]
    public async Task CreateRecipe_EmptyTitle_Returns400()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();
        var body = new
        {
            title = "",
            ingredients = Array.Empty<object>(),
            directions = Array.Empty<object>(),
            tags = Array.Empty<string>(),
            photos = Array.Empty<string>(),
            isPublic = false,
            prepTime = 0,
            cookTime = 0,
            servings = 0,
        };
        var response = await client.PostAsync("/api/v1/recipes", JsonBody(body));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // 4. GET /api/v1/recipes/{id} for public recipe → 200
    [Fact]
    public async Task GetRecipe_PublicRecipe_Returns200()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();
        var createResp = await client.PostAsync("/api/v1/recipes", JsonBody(ValidRecipeBody(isPublic: true)));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = JsonSerializer.Deserialize<JsonElement>(await createResp.Content.ReadAsStringAsync(), JsonOptions);
        var id = created.GetProperty("id").GetString()!;

        var anonClient = CreateClient();
        var response = await anonClient.GetAsync($"/api/v1/recipes/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 5. GET /api/v1/recipes/{id} for private recipe as non-owner → 404
    [Fact]
    public async Task GetRecipe_PrivateRecipe_NonOwner_Returns404()
    {
        var (ownerClient, _) = await RegisterAndAuthenticateAsync();
        var createResp = await ownerClient.PostAsync("/api/v1/recipes", JsonBody(ValidRecipeBody(isPublic: false)));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = JsonSerializer.Deserialize<JsonElement>(await createResp.Content.ReadAsStringAsync(), JsonOptions);
        var id = created.GetProperty("id").GetString()!;

        var (otherClient, _) = await RegisterAndAuthenticateAsync();
        var response = await otherClient.GetAsync($"/api/v1/recipes/{id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // 6. GET /api/v1/recipes/{id} for non-existent recipe → 404
    [Fact]
    public async Task GetRecipe_NonExistent_Returns404()
    {
        var client = CreateClient();
        var response = await client.GetAsync($"/api/v1/recipes/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // 7. PUT /api/v1/recipes/{id} as owner → 200
    [Fact]
    public async Task UpdateRecipe_AsOwner_Returns200()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();
        var createResp = await client.PostAsync("/api/v1/recipes", JsonBody(ValidRecipeBody()));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = JsonSerializer.Deserialize<JsonElement>(await createResp.Content.ReadAsStringAsync(), JsonOptions);
        var id = created.GetProperty("id").GetString()!;

        var response = await client.PutAsync($"/api/v1/recipes/{id}", JsonBody(new
        {
            title = "Updated Title",
            ingredients = Array.Empty<object>(),
            directions = Array.Empty<object>(),
            tags = Array.Empty<string>(),
            photos = Array.Empty<string>(),
            isPublic = false,
            prepTime = 0,
            cookTime = 0,
            servings = 0,
        }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 8. PUT /api/v1/recipes/{id} as non-owner → 403
    [Fact]
    public async Task UpdateRecipe_AsNonOwner_Returns403()
    {
        var (ownerClient, _) = await RegisterAndAuthenticateAsync();
        var createResp = await ownerClient.PostAsync("/api/v1/recipes", JsonBody(ValidRecipeBody()));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = JsonSerializer.Deserialize<JsonElement>(await createResp.Content.ReadAsStringAsync(), JsonOptions);
        var id = created.GetProperty("id").GetString()!;

        var (otherClient, _) = await RegisterAndAuthenticateAsync();
        var response = await otherClient.PutAsync($"/api/v1/recipes/{id}", JsonBody(new
        {
            title = "Hijacked",
            ingredients = Array.Empty<object>(),
            directions = Array.Empty<object>(),
            tags = Array.Empty<string>(),
            photos = Array.Empty<string>(),
            isPublic = false,
            prepTime = 0,
            cookTime = 0,
            servings = 0,
        }));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // 9. DELETE /api/v1/recipes/{id} as owner → 204
    [Fact]
    public async Task DeleteRecipe_AsOwner_Returns204()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();
        var createResp = await client.PostAsync("/api/v1/recipes", JsonBody(ValidRecipeBody()));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = JsonSerializer.Deserialize<JsonElement>(await createResp.Content.ReadAsStringAsync(), JsonOptions);
        var id = created.GetProperty("id").GetString()!;

        var response = await client.DeleteAsync($"/api/v1/recipes/{id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // 10. DELETE /api/v1/recipes/{id} as non-owner → 403
    [Fact]
    public async Task DeleteRecipe_AsNonOwner_Returns403()
    {
        var (ownerClient, _) = await RegisterAndAuthenticateAsync();
        var createResp = await ownerClient.PostAsync("/api/v1/recipes", JsonBody(ValidRecipeBody()));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = JsonSerializer.Deserialize<JsonElement>(await createResp.Content.ReadAsStringAsync(), JsonOptions);
        var id = created.GetProperty("id").GetString()!;

        var (otherClient, _) = await RegisterAndAuthenticateAsync();
        var response = await otherClient.DeleteAsync($"/api/v1/recipes/{id}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // 11. PATCH /api/v1/recipes/{id}/visibility as owner → 200
    [Fact]
    public async Task SetVisibility_AsOwner_Returns200()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();
        var createResp = await client.PostAsync("/api/v1/recipes", JsonBody(ValidRecipeBody()));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = JsonSerializer.Deserialize<JsonElement>(await createResp.Content.ReadAsStringAsync(), JsonOptions);
        var id = created.GetProperty("id").GetString()!;

        var response = await client.PatchAsync($"/api/v1/recipes/{id}/visibility",
            JsonBody(new { isPublic = true }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 12. POST /api/v1/recipes/{id}/like → 200
    [Fact]
    public async Task LikeRecipe_Returns200()
    {
        var (ownerClient, _) = await RegisterAndAuthenticateAsync();
        var createResp = await ownerClient.PostAsync("/api/v1/recipes", JsonBody(ValidRecipeBody(isPublic: true)));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = JsonSerializer.Deserialize<JsonElement>(await createResp.Content.ReadAsStringAsync(), JsonOptions);
        var id = created.GetProperty("id").GetString()!;

        var (likerClient, _) = await RegisterAndAuthenticateAsync();
        var response = await likerClient.PostAsync($"/api/v1/recipes/{id}/like", new StringContent(""));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 13. POST /api/v1/recipes/{id}/like twice → 409
    [Fact]
    public async Task LikeRecipe_Twice_Returns409()
    {
        var (ownerClient, _) = await RegisterAndAuthenticateAsync();
        var createResp = await ownerClient.PostAsync("/api/v1/recipes", JsonBody(ValidRecipeBody(isPublic: true)));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = JsonSerializer.Deserialize<JsonElement>(await createResp.Content.ReadAsStringAsync(), JsonOptions);
        var id = created.GetProperty("id").GetString()!;

        var (likerClient, _) = await RegisterAndAuthenticateAsync();
        await likerClient.PostAsync($"/api/v1/recipes/{id}/like", new StringContent(""));
        var response = await likerClient.PostAsync($"/api/v1/recipes/{id}/like", new StringContent(""));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // 14. DELETE /api/v1/recipes/{id}/like → 204
    [Fact]
    public async Task UnlikeRecipe_Returns204()
    {
        var (ownerClient, _) = await RegisterAndAuthenticateAsync();
        var createResp = await ownerClient.PostAsync("/api/v1/recipes", JsonBody(ValidRecipeBody(isPublic: true)));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = JsonSerializer.Deserialize<JsonElement>(await createResp.Content.ReadAsStringAsync(), JsonOptions);
        var id = created.GetProperty("id").GetString()!;

        var (likerClient, _) = await RegisterAndAuthenticateAsync();
        await likerClient.PostAsync($"/api/v1/recipes/{id}/like", new StringContent(""));
        var response = await likerClient.DeleteAsync($"/api/v1/recipes/{id}/like");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // 15. GET /api/v1/users/me/recipes returns own recipes including private
    [Fact]
    public async Task GetMyRecipes_ReturnsOwnRecipesIncludingPrivate()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();
        await client.PostAsync("/api/v1/recipes", JsonBody(ValidRecipeBody(isPublic: false)));
        await client.PostAsync("/api/v1/recipes", JsonBody(ValidRecipeBody(isPublic: true)));

        var response = await client.GetAsync("/api/v1/users/me/recipes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);
        var items = result.GetProperty("items");
        Assert.True(items.GetArrayLength() >= 2);
    }

    // 16. GET /api/v1/users/{userId}/recipes returns only public recipes
    [Fact]
    public async Task GetUserRecipes_ReturnsOnlyPublicRecipes()
    {
        var (ownerClient, ownerId) = await RegisterAndAuthenticateAsync();
        await ownerClient.PostAsync("/api/v1/recipes", JsonBody(ValidRecipeBody(isPublic: false)));
        await ownerClient.PostAsync("/api/v1/recipes", JsonBody(ValidRecipeBody(isPublic: true)));

        var (viewerClient, _) = await RegisterAndAuthenticateAsync();
        var response = await viewerClient.GetAsync($"/api/v1/users/{ownerId}/recipes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);
        var items = result.GetProperty("items");
        foreach (var item in items.EnumerateArray())
        {
            Assert.True(item.GetProperty("isPublic").GetBoolean());
        }
    }

    // 17. GET /api/v1/users/me/liked-recipes returns liked recipes
    [Fact]
    public async Task GetMyLikedRecipes_ReturnsLikedRecipes()
    {
        var (ownerClient, _) = await RegisterAndAuthenticateAsync();
        var createResp = await ownerClient.PostAsync("/api/v1/recipes", JsonBody(ValidRecipeBody(isPublic: true)));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = JsonSerializer.Deserialize<JsonElement>(await createResp.Content.ReadAsStringAsync(), JsonOptions);
        var id = created.GetProperty("id").GetString()!;

        var (likerClient, _) = await RegisterAndAuthenticateAsync();
        await likerClient.PostAsync($"/api/v1/recipes/{id}/like", new StringContent(""));

        var response = await likerClient.GetAsync("/api/v1/users/me/liked-recipes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);
        var items = result.GetProperty("items");
        Assert.True(items.GetArrayLength() >= 1);
    }
}
