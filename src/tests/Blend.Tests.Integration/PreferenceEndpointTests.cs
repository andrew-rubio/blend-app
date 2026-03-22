using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

// ── In-memory user repository for preferences tests ───────────────────────────

public sealed class InMemoryUserRepository : IRepository<User>
{
    private readonly ConcurrentDictionary<string, User> _users = new();

    public void Seed(User user) => _users[user.Id] = user;

    public Task<User?> GetByIdAsync(string id, string partitionKey, CancellationToken ct = default)
        => Task.FromResult(_users.GetValueOrDefault(id));

    public Task<IReadOnlyList<User>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<User>>([.. _users.Values]);

    public Task<User> CreateAsync(User entity, CancellationToken ct = default)
    {
        _users[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<User> UpdateAsync(User entity, string id, string partitionKey, CancellationToken ct = default)
    {
        _users[id] = entity;
        return Task.FromResult(entity);
    }

    public Task<User> PatchAsync(string id, string partitionKey, IReadOnlyDictionary<string, object?> patches, CancellationToken ct = default)
    {
        if (!_users.TryGetValue(id, out var existing))
        {
            throw new KeyNotFoundException($"User {id} not found.");
        }

        // Apply patches manually — supports only the preference sub-paths used by the controller
        var favCuisines = existing.Preferences.FavoriteCuisines;
        var favDishTypes = existing.Preferences.FavoriteDishTypes;
        var diets = existing.Preferences.Diets;
        var intolerances = existing.Preferences.Intolerances;
        var dislikedIds = existing.Preferences.DislikedIngredientIds;
        var updatedAt = existing.UpdatedAt;

        foreach (var (path, value) in patches)
        {
            switch (path)
            {
                case "/preferences/favoriteCuisines":
                    favCuisines = ToStringList(value);
                    break;
                case "/preferences/favoriteDishTypes":
                    favDishTypes = ToStringList(value);
                    break;
                case "/preferences/diets":
                    diets = ToStringList(value);
                    break;
                case "/preferences/intolerances":
                    intolerances = ToStringList(value);
                    break;
                case "/preferences/dislikedIngredientIds":
                    dislikedIds = ToStringList(value);
                    break;
                case "/updatedAt":
                    updatedAt = value is DateTimeOffset dto ? dto : updatedAt;
                    break;
            }
        }

        var updated = new User
        {
            Id = existing.Id,
            Email = existing.Email,
            DisplayName = existing.DisplayName,
            ProfilePhotoUrl = existing.ProfilePhotoUrl,
            PasswordHashRef = existing.PasswordHashRef,
            Preferences = new UserPreferences
            {
                FavoriteCuisines = favCuisines,
                FavoriteDishTypes = favDishTypes,
                Diets = diets,
                Intolerances = intolerances,
                DislikedIngredientIds = dislikedIds,
            },
            MeasurementUnit = existing.MeasurementUnit,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = updatedAt,
            UnreadNotificationCount = existing.UnreadNotificationCount,
            Role = existing.Role,
        };

        _users[id] = updated;
        return Task.FromResult(updated);
    }

    public Task DeleteAsync(string id, string partitionKey, CancellationToken ct = default)
    {
        _users.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<PagedResult<User>> GetPagedAsync(string query, FeedPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<User> { Items = [.. _users.Values] });

    public Task<PagedResult<User>> GetOffsetPagedAsync(string baseQuery, OffsetPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<User> { Items = [.. _users.Values] });

    public Task ExecuteTransactionalBatchAsync(string partitionKey, IEnumerable<(TransactionalBatchOperation Operation, User Entity)> operations, CancellationToken ct = default)
        => Task.CompletedTask;

    private static IReadOnlyList<string> ToStringList(object? value)
    {
        if (value is IReadOnlyList<string> list)
        {
            return list;
        }

        if (value is IEnumerable<string> enumerable)
        {
            return [.. enumerable];
        }

        return [];
    }
}

// ── Test Web Application Factory ──────────────────────────────────────────────

public sealed class PreferencesTestFactory : WebApplicationFactory<Program>
{
    private readonly InMemoryUserRepository _userRepository = new();
    private readonly InMemoryUserStore _userStore = new();

    public IRepository<User> UserRepository => _userRepository;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:SecretKey", "test-secret-key-that-is-long-enough-for-hs256-algorithm");
        builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Development");

        builder.ConfigureServices(services =>
        {
            // Replace CosmosUserStore with in-memory for auth
            services.RemoveAll<IUserStore<BlendUser>>();
            services.AddSingleton<IUserStore<BlendUser>>(_userStore);

            // Replace IRepository<User> with in-memory for preferences
            services.RemoveAll<IRepository<User>>();
            services.AddSingleton<IRepository<User>>(_userRepository);
        });
    }
}

// ── Integration tests ─────────────────────────────────────────────────────────

public class PreferenceEndpointTests : IClassFixture<PreferencesTestFactory>
{
    private readonly PreferencesTestFactory _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public PreferenceEndpointTests(PreferencesTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = false,
    });

    private static string UniqueEmail() => $"pref-{Guid.NewGuid():N}@example.com";

    private static StringContent JsonBody(object payload) =>
        new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    /// <summary>Registers a user and returns an authenticated HttpClient along with the user ID.</summary>
    private async Task<(HttpClient Client, string UserId)> RegisterAndAuthenticateAsync()
    {
        var client = CreateClient();
        var email = UniqueEmail();
        var registerResponse = await client.PostAsync("/api/v1/auth/register",
            JsonBody(new { displayName = "PrefUser", email, password = "ValidPass1!" }));

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var authBody = await registerResponse.Content.ReadAsStringAsync();
        var auth = JsonSerializer.Deserialize<AuthResponse>(authBody, JsonOptions);
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // Extract userId from JWT
        var userId = ExtractUserIdFromJwt(auth.AccessToken);

        // Seed the User document in the in-memory repository so preference operations work
        await _factory.UserRepository.CreateAsync(new User
        {
            Id = userId,
            Email = email,
            DisplayName = "PrefUser",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        return (client, userId);
    }

    private static string ExtractUserIdFromJwt(string token)
    {
        var parts = token.Split('.');
        var payload = parts[1];
        // Pad base64 if needed
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("sub").GetString()!;
    }

    // ── Predefined list endpoints (public) ────────────────────────────────────

    [Fact]
    public async Task GetCuisines_ReturnsNonEmptyList()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/preferences/cuisines");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(list);
        Assert.NotEmpty(list);
        Assert.Contains("Italian", list);
    }

    [Fact]
    public async Task GetDishTypes_ReturnsNonEmptyList()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/preferences/dish-types");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(list);
        Assert.NotEmpty(list);
    }

    [Fact]
    public async Task GetDiets_ReturnsNonEmptyList()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/preferences/diets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(list);
        Assert.NotEmpty(list);
        Assert.Contains("vegetarian", list);
    }

    [Fact]
    public async Task GetIntolerances_ReturnsNonEmptyList()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/preferences/intolerances");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(list);
        Assert.NotEmpty(list);
        Assert.Contains("gluten", list);
    }

    [Fact]
    public async Task GetCuisines_DoesNotRequireAuthentication()
    {
        var client = CreateClient(); // no auth header
        var response = await client.GetAsync("/api/v1/preferences/cuisines");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── GET /api/v1/users/me/preferences ─────────────────────────────────────

    [Fact]
    public async Task GetPreferences_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/users/me/preferences");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPreferences_WithAuth_ReturnsDefaultPreferences()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.GetAsync("/api/v1/users/me/preferences");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("favoriteCuisines", body);
    }

    // ── PUT /api/v1/users/me/preferences ─────────────────────────────────────

    [Fact]
    public async Task PutPreferences_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var body = JsonBody(new
        {
            favoriteCuisines = new[] { "Italian" },
            favoriteDishTypes = Array.Empty<string>(),
            diets = Array.Empty<string>(),
            intolerances = Array.Empty<string>(),
            dislikedIngredientIds = Array.Empty<string>(),
        });

        var response = await client.PutAsync("/api/v1/users/me/preferences", body);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PutPreferences_WithValidData_Returns200WithUpdatedPreferences()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PutAsync("/api/v1/users/me/preferences", JsonBody(new
        {
            favoriteCuisines = new[] { "Italian", "Japanese" },
            favoriteDishTypes = new[] { "main course" },
            diets = new[] { "vegetarian" },
            intolerances = new[] { "gluten" },
            dislikedIngredientIds = new[] { "ing-1" },
        }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Italian", body);
        Assert.Contains("vegetarian", body);
        Assert.Contains("gluten", body);
    }

    [Fact]
    public async Task PutPreferences_WithInvalidCuisine_Returns400()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PutAsync("/api/v1/users/me/preferences", JsonBody(new
        {
            favoriteCuisines = new[] { "FakeCuisine" },
            favoriteDishTypes = Array.Empty<string>(),
            diets = Array.Empty<string>(),
            intolerances = Array.Empty<string>(),
            dislikedIngredientIds = Array.Empty<string>(),
        }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("FakeCuisine", body);
    }

    [Fact]
    public async Task PutPreferences_WithInvalidDiet_Returns400()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PutAsync("/api/v1/users/me/preferences", JsonBody(new
        {
            favoriteCuisines = Array.Empty<string>(),
            favoriteDishTypes = Array.Empty<string>(),
            diets = new[] { "carnivore" },
            intolerances = Array.Empty<string>(),
            dislikedIngredientIds = Array.Empty<string>(),
        }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutPreferences_WithInvalidIntolerance_Returns400()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PutAsync("/api/v1/users/me/preferences", JsonBody(new
        {
            favoriteCuisines = Array.Empty<string>(),
            favoriteDishTypes = Array.Empty<string>(),
            diets = Array.Empty<string>(),
            intolerances = new[] { "latex" },
            dislikedIngredientIds = Array.Empty<string>(),
        }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── PATCH /api/v1/users/me/preferences ───────────────────────────────────

    [Fact]
    public async Task PatchPreferences_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var response = await client.PatchAsync("/api/v1/users/me/preferences",
            JsonBody(new { diets = new[] { "vegan" } }));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PatchPreferences_WithValidPartialData_Returns200WithUpdatedPreferences()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PatchAsync("/api/v1/users/me/preferences",
            JsonBody(new { diets = new[] { "vegan" } }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("vegan", body);
    }

    [Fact]
    public async Task PatchPreferences_WithInvalidIntolerance_Returns400()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PatchAsync("/api/v1/users/me/preferences",
            JsonBody(new { intolerances = new[] { "latex" } }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PatchPreferences_EmptyPatch_Returns200WithUnchangedPreferences()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PatchAsync("/api/v1/users/me/preferences", JsonBody(new { }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Preference persistence ────────────────────────────────────────────────

    [Fact]
    public async Task PutPreferences_ThenGet_ReturnsSavedPreferences()
    {
        var (client, _) = await RegisterAndAuthenticateAsync();

        // PUT preferences
        await client.PutAsync("/api/v1/users/me/preferences", JsonBody(new
        {
            favoriteCuisines = new[] { "Mexican" },
            favoriteDishTypes = new[] { "breakfast" },
            diets = new[] { "paleo" },
            intolerances = new[] { "dairy" },
            dislikedIngredientIds = new[] { "ing-99" },
        }));

        // GET preferences
        var getResponse = await client.GetAsync("/api/v1/users/me/preferences");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var body = await getResponse.Content.ReadAsStringAsync();

        Assert.Contains("Mexican", body);
        Assert.Contains("breakfast", body);
        Assert.Contains("paleo", body);
        Assert.Contains("dairy", body);
        Assert.Contains("ing-99", body);
    }
}
