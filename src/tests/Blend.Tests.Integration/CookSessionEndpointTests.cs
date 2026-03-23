using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Blend.Api.Auth.Models;
using Blend.Api.CookSessions.Models;
using Blend.Api.Ingredients.Services;
using Blend.Domain.Entities;
using Blend.Domain.Identity;
using Blend.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blend.Tests.Integration;

// ── In-memory cooking session repository ─────────────────────────────────────

public sealed class InMemoryCookSessionRepository : IRepository<CookingSession>
{
    private readonly ConcurrentDictionary<string, CookingSession> _sessions = new();

    public void Seed(CookingSession session) => _sessions[session.Id] = session;

    public Task<CookingSession?> GetByIdAsync(string id, string partitionKey, CancellationToken ct = default)
        => Task.FromResult(_sessions.GetValueOrDefault(id));

    public Task<IReadOnlyList<CookingSession>> GetByQueryAsync(
        string query, string? partitionKey = null, CancellationToken ct = default)
    {
        // Simulate the active/paused session query
        IEnumerable<CookingSession> results = _sessions.Values;
        if (partitionKey is not null)
        {
            results = results.Where(s => s.UserId == partitionKey);
        }

        if (query.Contains("status = 'Active' OR c.status = 'Paused'", StringComparison.Ordinal))
        {
            results = results.Where(s =>
                s.Status == CookingSessionStatus.Active ||
                s.Status == CookingSessionStatus.Paused);
        }

        var list = results.OrderByDescending(s => s.UpdatedAt).Take(1).ToList();
        return Task.FromResult<IReadOnlyList<CookingSession>>(list);
    }

    public Task<CookingSession> CreateAsync(CookingSession entity, CancellationToken ct = default)
    {
        _sessions[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<CookingSession> UpdateAsync(CookingSession entity, string id, string partitionKey, CancellationToken ct = default)
    {
        _sessions[id] = entity;
        return Task.FromResult(entity);
    }

    public Task<CookingSession> PatchAsync(string id, string partitionKey, IReadOnlyDictionary<string, object?> patches, CancellationToken ct = default)
        => Task.FromResult(_sessions[id]);

    public Task DeleteAsync(string id, string partitionKey, CancellationToken ct = default)
    {
        _sessions.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<PagedResult<CookingSession>> GetPagedAsync(string query, FeedPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<CookingSession> { Items = [.. _sessions.Values] });

    public Task<PagedResult<CookingSession>> GetOffsetPagedAsync(string baseQuery, OffsetPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<CookingSession> { Items = [.. _sessions.Values] });

    public Task ExecuteTransactionalBatchAsync(string partitionKey, IEnumerable<(TransactionalBatchOperation Operation, CookingSession Entity)> operations, CancellationToken ct = default)
        => Task.CompletedTask;

    public void Clear() => _sessions.Clear();
}

// ── Web application factory ────────────────────────────────────────────────────

public sealed class CookSessionTestFactory : WebApplicationFactory<Program>
{
    public readonly InMemoryCookSessionRepository SessionRepository = new();
    public readonly InMemoryUserRepository CookSessionUserRepository = new();
    private readonly InMemoryUserStore _cookSessionUserStore = new();
    public readonly InMemoryKnowledgeBaseService KnowledgeBaseService = new();
    public readonly InMemoryRecipeRepository RecipeRepository = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:SecretKey", "test-secret-key-that-is-long-enough-for-hs256-algorithm");
        builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IUserStore<BlendUser>>();
            services.AddSingleton<IUserStore<BlendUser>>(_cookSessionUserStore);

            services.RemoveAll<IRepository<User>>();
            services.AddSingleton<IRepository<User>>(CookSessionUserRepository);

            services.RemoveAll<IRepository<CookingSession>>();
            services.AddSingleton<IRepository<CookingSession>>(SessionRepository);

            services.RemoveAll<IRepository<Recipe>>();
            services.AddSingleton<IRepository<Recipe>>(RecipeRepository);

            services.RemoveAll<IKnowledgeBaseService>();
            services.AddSingleton<IKnowledgeBaseService>(KnowledgeBaseService);
        });
    }
}

// ── Integration tests ─────────────────────────────────────────────────────────

public class CookSessionEndpointTests : IClassFixture<CookSessionTestFactory>
{
    private readonly CookSessionTestFactory _factory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public CookSessionEndpointTests(CookSessionTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = false,
    });

    private static StringContent JsonBody(object payload) =>
        new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    private static string UniqueEmail() => $"cook-{Guid.NewGuid():N}@example.com";

    private async Task<(HttpClient Client, string UserId)> RegisterAndAuthenticateAsync()
    {
        var client = CreateClient();
        var email = UniqueEmail();
        var response = await client.PostAsync("/api/v1/auth/register",
            JsonBody(new { displayName = "CookUser", email, password = "ValidPass1!" }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var auth = JsonSerializer.Deserialize<AuthResponse>(body, JsonOptions);
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var userId = ExtractUserIdFromJwt(auth.AccessToken);

        await _factory.CookSessionUserRepository.CreateAsync(new User
        {
            Id = userId,
            Email = email,
            DisplayName = "CookUser",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

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

    // ── POST /api/v1/cook-sessions — Authentication ───────────────────────────

    [Fact]
    public async Task CreateSession_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var response = await client.PostAsync("/api/v1/cook-sessions",
            JsonBody(new { }));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── POST /api/v1/cook-sessions — Create ──────────────────────────────────

    [Fact]
    public async Task CreateSession_WithAuth_Returns201WithSession()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PostAsync("/api/v1/cook-sessions",
            JsonBody(new { }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var session = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);
        Assert.Equal("Active", session.GetProperty("status").GetString());
    }

    [Fact]
    public async Task CreateSession_WithInitialDishName_CreatesDishWithThatName()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PostAsync("/api/v1/cook-sessions",
            JsonBody(new { initialDishName = "Pasta Night" }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Pasta Night", body);
    }

    [Fact]
    public async Task CreateSession_WhenActiveSessionExists_Returns409()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        // Create first session
        var firstResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Attempt to create a second active session
        var secondResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    // ── GET /api/v1/cook-sessions/active ─────────────────────────────────────

    [Fact]
    public async Task GetActiveSession_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/cook-sessions/active");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetActiveSession_WhenNoActiveSession_Returns404()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.GetAsync("/api/v1/cook-sessions/active");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetActiveSession_WhenActiveSessionExists_Returns200()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        // Create a session first
        await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));

        var response = await client.GetAsync("/api/v1/cook-sessions/active");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Active", body);
    }

    // ── GET /api/v1/cook-sessions/{id} ────────────────────────────────────────

    [Fact]
    public async Task GetSession_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/cook-sessions/session-123");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSession_WhenNotFound_Returns404()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.GetAsync("/api/v1/cook-sessions/nonexistent-session");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSession_WhenFound_Returns200()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        // Create session
        var createResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var createdSession = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions);
        var sessionId = createdSession.GetProperty("id").GetString();

        var response = await client.GetAsync($"/api/v1/cook-sessions/{sessionId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── POST /api/v1/cook-sessions/{id}/ingredients ───────────────────────────

    [Fact]
    public async Task AddIngredient_WithValidData_Returns200WithUpdatedSession()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var createResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var sessionId = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions).GetProperty("id").GetString();

        var response = await client.PostAsync(
            $"/api/v1/cook-sessions/{sessionId}/ingredients",
            JsonBody(new { ingredientId = "ing-tomato", name = "Tomato" }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("ing-tomato", body);
    }

    [Fact]
    public async Task AddIngredient_WithoutIngredientId_Returns400()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var createResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var sessionId = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions).GetProperty("id").GetString();

        var response = await client.PostAsync(
            $"/api/v1/cook-sessions/{sessionId}/ingredients",
            JsonBody(new { name = "Tomato" })); // missing ingredientId

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── DELETE /api/v1/cook-sessions/{id}/ingredients/{ingredientId} ─────────

    [Fact]
    public async Task RemoveIngredient_Returns200WithUpdatedSession()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var createResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var sessionId = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions).GetProperty("id").GetString();

        // Add an ingredient
        await client.PostAsync(
            $"/api/v1/cook-sessions/{sessionId}/ingredients",
            JsonBody(new { ingredientId = "ing-tomato", name = "Tomato" }));

        // Remove it
        var response = await client.DeleteAsync(
            $"/api/v1/cook-sessions/{sessionId}/ingredients/ing-tomato");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        // Ingredient should be gone
        Assert.DoesNotContain("\"ingredientId\":\"ing-tomato\"", body);
    }

    // ── POST /api/v1/cook-sessions/{id}/dishes ────────────────────────────────

    [Fact]
    public async Task AddDish_Returns200WithUpdatedSession()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var createResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var sessionId = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions).GetProperty("id").GetString();

        var response = await client.PostAsync(
            $"/api/v1/cook-sessions/{sessionId}/dishes",
            JsonBody(new { name = "Side Salad", cuisineType = "Mediterranean" }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Side Salad", body);
    }

    [Fact]
    public async Task AddDish_WithoutName_Returns400()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var createResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var sessionId = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions).GetProperty("id").GetString();

        var response = await client.PostAsync(
            $"/api/v1/cook-sessions/{sessionId}/dishes",
            JsonBody(new { cuisineType = "Italian" })); // missing name

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── DELETE /api/v1/cook-sessions/{id}/dishes/{dishId} ────────────────────

    [Fact]
    public async Task RemoveDish_Returns200WithUpdatedSession()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var createResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var sessionElement = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions);
        var sessionId = sessionElement.GetProperty("id").GetString();

        // Add a dish
        var addDishResponse = await client.PostAsync(
            $"/api/v1/cook-sessions/{sessionId}/dishes",
            JsonBody(new { name = "Extra Dish" }));
        var addDishBody = await addDishResponse.Content.ReadAsStringAsync();
        var updatedSession = JsonSerializer.Deserialize<JsonElement>(addDishBody, JsonOptions);

        // Find the newly added dish (the one named "Extra Dish")
        var dishes = updatedSession.GetProperty("dishes").EnumerateArray().ToList();
        var extraDish = dishes.FirstOrDefault(d => d.GetProperty("name").GetString() == "Extra Dish");
        var dishId = extraDish.GetProperty("dishId").GetString();

        var response = await client.DeleteAsync($"/api/v1/cook-sessions/{sessionId}/dishes/{dishId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Extra Dish", body);
    }

    // ── POST /api/v1/cook-sessions/{id}/pause ────────────────────────────────

    [Fact]
    public async Task PauseSession_Returns200WithPausedSession()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var createResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var sessionId = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions).GetProperty("id").GetString();

        var response = await client.PostAsync($"/api/v1/cook-sessions/{sessionId}/pause", JsonBody(new { }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Paused", body);
    }

    // ── POST /api/v1/cook-sessions/{id}/complete ─────────────────────────────

    [Fact]
    public async Task CompleteSession_Returns200WithCompletedSession()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var createResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var sessionId = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions).GetProperty("id").GetString();

        var response = await client.PostAsync($"/api/v1/cook-sessions/{sessionId}/complete", JsonBody(new { }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Completed", body);
    }

    // ── Session recovery ──────────────────────────────────────────────────────

    [Fact]
    public async Task SessionRecovery_AfterPause_CanBeResumed()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        // Create and pause session
        var createResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var sessionId = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions).GetProperty("id").GetString();

        await client.PostAsync($"/api/v1/cook-sessions/{sessionId}/pause", JsonBody(new { }));

        // GetActive should return the paused session
        var activeResponse = await client.GetAsync("/api/v1/cook-sessions/active");
        Assert.Equal(HttpStatusCode.OK, activeResponse.StatusCode);
        var activeBody = await activeResponse.Content.ReadAsStringAsync();
        Assert.Contains("Paused", activeBody);
    }

    // ── GET /api/v1/cook-sessions/{id}/suggestions ───────────────────────────

    [Fact]
    public async Task GetSuggestions_WhenKbUnavailable_ReturnsKbUnavailableFlag()
    {
        _factory.SessionRepository.Clear();
        _factory.KnowledgeBaseService.SetAvailable(false);

        try
        {
            var (client, _) = await RegisterAndAuthenticateAsync();

            var createResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
            var createBody = await createResponse.Content.ReadAsStringAsync();
            var sessionId = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions).GetProperty("id").GetString();

            var response = await client.GetAsync($"/api/v1/cook-sessions/{sessionId}/suggestions");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("kbUnavailable", body);
        }
        finally
        {
            _factory.KnowledgeBaseService.SetAvailable(true);
        }
    }

    [Fact]
    public async Task GetSuggestions_WithIngredients_ReturnsSuggestions()
    {
        _factory.SessionRepository.Clear();
        _factory.KnowledgeBaseService.SetAvailable(true);
        _factory.KnowledgeBaseService.SeedIngredient(new Blend.Api.Ingredients.Models.IngredientDocument
        {
            IngredientId = "ing-basil",
            Name = "Basil",
        });
        _factory.KnowledgeBaseService.SeedPairing(new IngredientPairing
        {
            Id = "ing-tomato:ing-basil",
            IngredientId = "ing-tomato",
            PairedIngredientId = "ing-basil",
            Score = 0.95,
            SourceType = PairingSourceType.Reference,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        var (client, _) = await RegisterAndAuthenticateAsync();

        var createResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var sessionId = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions).GetProperty("id").GetString();

        // Add tomato to get suggestions based on it
        await client.PostAsync(
            $"/api/v1/cook-sessions/{sessionId}/ingredients",
            JsonBody(new { ingredientId = "ing-tomato", name = "Tomato" }));

        var response = await client.GetAsync($"/api/v1/cook-sessions/{sessionId}/suggestions");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("suggestions", body);
    }

    // ── Multi-dish support ────────────────────────────────────────────────────

    [Fact]
    public async Task MultiDish_AddIngredientToDish_ScopedCorrectly()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var createResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var sessionElement = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions);
        var sessionId = sessionElement.GetProperty("id").GetString();

        // Get the initial dish ID
        var initialDishId = sessionElement.GetProperty("dishes")[0].GetProperty("dishId").GetString();

        // Add ingredient scoped to the initial dish
        var addResponse = await client.PostAsync(
            $"/api/v1/cook-sessions/{sessionId}/ingredients",
            JsonBody(new { ingredientId = "ing-garlic", name = "Garlic", dishId = initialDishId }));

        Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);
        var body = await addResponse.Content.ReadAsStringAsync();

        // Garlic should be in the dish's ingredients, not in addedIngredients
        var updatedSession = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);
        var addedIngredients = updatedSession.GetProperty("addedIngredients").EnumerateArray().ToList();
        Assert.Empty(addedIngredients);

        var dish = updatedSession.GetProperty("dishes").EnumerateArray()
            .First(d => d.GetProperty("dishId").GetString() == initialDishId);
        var dishIngredients = dish.GetProperty("ingredients").EnumerateArray().ToList();
        Assert.Single(dishIngredients);
        Assert.Equal("ing-garlic", dishIngredients[0].GetProperty("ingredientId").GetString());
    }

    // ── POST /api/v1/cook-sessions/{id}/feedback ─────────────────────────────

    [Fact]
    public async Task SubmitFeedback_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var response = await client.PostAsync("/api/v1/cook-sessions/any-id/feedback",
            JsonBody(new { feedback = Array.Empty<object>() }));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SubmitFeedback_SessionNotFound_Returns404()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PostAsync(
            "/api/v1/cook-sessions/nonexistent/feedback",
            JsonBody(new
            {
                feedback = new[]
                {
                    new { ingredientId1 = "ing-a", ingredientId2 = "ing-b", rating = 4 },
                },
            }));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SubmitFeedback_InvalidRating_Returns400()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var createResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var sessionId = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions).GetProperty("id").GetString();

        var response = await client.PostAsync(
            $"/api/v1/cook-sessions/{sessionId}/feedback",
            JsonBody(new
            {
                feedback = new[]
                {
                    new { ingredientId1 = "ing-a", ingredientId2 = "ing-b", rating = 6 },
                },
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SubmitFeedback_ValidPayload_Returns204()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var createResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var sessionId = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions).GetProperty("id").GetString();

        var response = await client.PostAsync(
            $"/api/v1/cook-sessions/{sessionId}/feedback",
            JsonBody(new
            {
                feedback = new[]
                {
                    new { ingredientId1 = "ing-tomato", ingredientId2 = "ing-basil", rating = 5, comment = (string?)"Great combo!" },
                    new { ingredientId1 = "ing-garlic", ingredientId2 = "ing-olive-oil", rating = 4, comment = (string?)null },
                },
            }));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // ── POST /api/v1/cook-sessions/{id}/publish ───────────────────────────────

    [Fact]
    public async Task PublishSession_WithoutAuth_Returns401()
    {
        var client = CreateClient();
        var response = await client.PostAsync("/api/v1/cook-sessions/any-id/publish",
            JsonBody(new { title = "My Recipe", directions = Array.Empty<object>() }));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PublishSession_SessionNotFound_Returns404()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var response = await client.PostAsync(
            "/api/v1/cook-sessions/nonexistent/publish",
            JsonBody(new
            {
                title = "My Recipe",
                directions = new[] { new { stepNumber = 1, text = "Cook it." } },
            }));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PublishSession_MissingTitle_Returns400()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var createResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var sessionId = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions).GetProperty("id").GetString();

        var response = await client.PostAsync(
            $"/api/v1/cook-sessions/{sessionId}/publish",
            JsonBody(new
            {
                title = "",
                directions = new[] { new { stepNumber = 1, text = "Cook it." } },
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PublishSession_NoDirections_Returns400()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var createResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var sessionId = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions).GetProperty("id").GetString();

        var response = await client.PostAsync(
            $"/api/v1/cook-sessions/{sessionId}/publish",
            JsonBody(new
            {
                title = "My Recipe",
                directions = Array.Empty<object>(),
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PublishSession_ValidPayload_Returns201WithRecipeId()
    {
        _factory.SessionRepository.Clear();
        var (client, _) = await RegisterAndAuthenticateAsync();

        var createResponse = await client.PostAsync("/api/v1/cook-sessions", JsonBody(new { }));
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var sessionId = JsonSerializer.Deserialize<JsonElement>(createBody, JsonOptions).GetProperty("id").GetString();

        // Add an ingredient so the recipe has one
        await client.PostAsync(
            $"/api/v1/cook-sessions/{sessionId}/ingredients",
            JsonBody(new { ingredientId = "ing-tomato", name = "Tomato" }));

        var publishResponse = await client.PostAsync(
            $"/api/v1/cook-sessions/{sessionId}/publish",
            JsonBody(new
            {
                title = "Tomato Delight",
                description = "A simple tomato dish",
                directions = new[] { new { stepNumber = 1, text = "Chop the tomato." } },
                photos = new[] { "https://example.com/photo.jpg" },
                cuisineType = "Italian",
                tags = new[] { "tomato", "simple" },
                servings = 2,
                prepTime = 10,
                cookTime = 20,
            }));

        Assert.Equal(HttpStatusCode.Created, publishResponse.StatusCode);
        var body = await publishResponse.Content.ReadAsStringAsync();
        Assert.Contains("recipeId", body);
        var result = JsonSerializer.Deserialize<JsonElement>(body, JsonOptions);
        Assert.False(string.IsNullOrEmpty(result.GetProperty("recipeId").GetString()));
    }
}

