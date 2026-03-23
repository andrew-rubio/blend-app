using Blend.Api.CookSessions.Models;
using Blend.Api.CookSessions.Services;
using Blend.Api.Ingredients.Models;
using Blend.Api.Ingredients.Services;
using Blend.Api.Preferences.Services;
using Blend.Api.Recipes.Models;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Blend.Tests.Unit.CookSessions;

/// <summary>
/// Unit tests for <see cref="CookSessionService"/>.
/// Covers suggestion scoring, intolerance exclusion, and session state transitions.
/// </summary>
public class CookSessionServiceTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static CookSessionService CreateService(
        IRepository<CookingSession>? sessionRepo = null,
        IRepository<Recipe>? recipeRepo = null,
        IKnowledgeBaseService? kb = null,
        IPreferenceService? preferenceService = null)
    {
        return new CookSessionService(
            NullLogger<CookSessionService>.Instance,
            sessionRepo,
            recipeRepo,
            kb,
            preferenceService);
    }

    private static CookingSession BuildSession(
        string sessionId = "session-1",
        string userId = "user-1",
        CookingSessionStatus status = CookingSessionStatus.Active,
        IReadOnlyList<CookingSessionDish>? dishes = null,
        IReadOnlyList<SessionIngredient>? addedIngredients = null)
    {
        return new CookingSession
        {
            Id = sessionId,
            UserId = userId,
            Status = status,
            Dishes = dishes ?? [new CookingSessionDish
            {
                DishId = "dish-1",
                Name = "Test Dish",
                Ingredients = [],
            }],
            AddedIngredients = addedIngredients ?? [],
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    private static Mock<IRepository<CookingSession>> BuildSessionRepoMock(CookingSession? session = null)
    {
        var mock = new Mock<IRepository<CookingSession>>();
        mock.Setup(r => r.GetByIdAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        mock.Setup(r => r.CreateAsync(
                It.IsAny<CookingSession>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CookingSession s, CancellationToken _) => s);
        mock.Setup(r => r.UpdateAsync(
                It.IsAny<CookingSession>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CookingSession s, string _, string _, CancellationToken _) => s);
        mock.Setup(r => r.GetByQueryAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session is null ? [] : (IReadOnlyList<CookingSession>)[session]);
        return mock;
    }

    // ── CreateSession ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateSession_WhenRepositoryNull_ReturnsNull()
    {
        var svc = CreateService(sessionRepo: null);
        var result = await svc.CreateSessionAsync("user-1", new CreateCookSessionRequest());
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateSession_CreatesActiveSession()
    {
        var repoMock = BuildSessionRepoMock();
        var svc = CreateService(sessionRepo: repoMock.Object);

        var result = await svc.CreateSessionAsync("user-1", new CreateCookSessionRequest());

        Assert.NotNull(result);
        Assert.Equal(CookingSessionStatus.Active, result.Status);
        Assert.Equal("user-1", result.UserId);
    }

    [Fact]
    public async Task CreateSession_CreatesDefaultDishWhenNoRecipe()
    {
        var repoMock = BuildSessionRepoMock();
        var svc = CreateService(sessionRepo: repoMock.Object);

        var result = await svc.CreateSessionAsync("user-1", new CreateCookSessionRequest());

        Assert.NotNull(result);
        Assert.Single(result.Dishes);
        Assert.Equal("My Dish", result.Dishes[0].Name);
    }

    [Fact]
    public async Task CreateSession_WithInitialDishName_UsesProvidedName()
    {
        var repoMock = BuildSessionRepoMock();
        var svc = CreateService(sessionRepo: repoMock.Object);

        var result = await svc.CreateSessionAsync("user-1", new CreateCookSessionRequest
        {
            InitialDishName = "Pasta Night",
        });

        Assert.NotNull(result);
        Assert.Single(result.Dishes);
        Assert.Equal("Pasta Night", result.Dishes[0].Name);
    }

    // ── GetSession ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSession_WhenRepositoryNull_ReturnsNull()
    {
        var svc = CreateService(sessionRepo: null);
        var result = await svc.GetSessionAsync("session-1", "user-1");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSession_WhenOwnerMismatch_ReturnsNull()
    {
        var session = BuildSession(userId: "user-1");
        var repoMock = BuildSessionRepoMock(session);
        var svc = CreateService(sessionRepo: repoMock.Object);

        var result = await svc.GetSessionAsync("session-1", "user-2"); // different user
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSession_WhenOwnerMatches_ReturnsSession()
    {
        var session = BuildSession(userId: "user-1");
        var repoMock = BuildSessionRepoMock(session);
        var svc = CreateService(sessionRepo: repoMock.Object);

        var result = await svc.GetSessionAsync("session-1", "user-1");
        Assert.NotNull(result);
        Assert.Equal("session-1", result.Id);
    }

    // ── Session state transitions ─────────────────────────────────────────────

    [Fact]
    public async Task PauseSession_SetsStatusToPaused()
    {
        var session = BuildSession(status: CookingSessionStatus.Active);
        var repoMock = BuildSessionRepoMock(session);
        var svc = CreateService(sessionRepo: repoMock.Object);

        var result = await svc.PauseSessionAsync("session-1", "user-1");

        Assert.NotNull(result);
        Assert.Equal(CookingSessionStatus.Paused, result.Status);
        Assert.NotNull(result.PausedAt);
    }

    [Fact]
    public async Task PauseSession_SetsTtlTo86400Seconds()
    {
        var session = BuildSession(status: CookingSessionStatus.Active);
        var repoMock = BuildSessionRepoMock(session);
        var svc = CreateService(sessionRepo: repoMock.Object);

        var result = await svc.PauseSessionAsync("session-1", "user-1");

        Assert.NotNull(result);
        Assert.Equal(86400, result.Ttl);
    }

    [Fact]
    public async Task CompleteSession_SetsStatusToCompleted()
    {
        var session = BuildSession(status: CookingSessionStatus.Active);
        var repoMock = BuildSessionRepoMock(session);
        var svc = CreateService(sessionRepo: repoMock.Object);

        var result = await svc.CompleteSessionAsync("session-1", "user-1");

        Assert.NotNull(result);
        Assert.Equal(CookingSessionStatus.Completed, result.Status);
    }

    [Fact]
    public async Task CompleteSession_ClearsTtl()
    {
        var session = BuildSession(status: CookingSessionStatus.Paused);
        var repoMock = BuildSessionRepoMock(session);
        var svc = CreateService(sessionRepo: repoMock.Object);

        var result = await svc.CompleteSessionAsync("session-1", "user-1");

        Assert.NotNull(result);
        Assert.Null(result.Ttl);
    }

    [Fact]
    public async Task PauseSession_WhenSessionNotFound_ReturnsNull()
    {
        var repoMock = BuildSessionRepoMock(session: null);
        var svc = CreateService(sessionRepo: repoMock.Object);

        var result = await svc.PauseSessionAsync("nonexistent", "user-1");
        Assert.Null(result);
    }

    [Fact]
    public async Task CompleteSession_WhenSessionNotFound_ReturnsNull()
    {
        var repoMock = BuildSessionRepoMock(session: null);
        var svc = CreateService(sessionRepo: repoMock.Object);

        var result = await svc.CompleteSessionAsync("nonexistent", "user-1");
        Assert.Null(result);
    }

    // ── AddIngredient ─────────────────────────────────────────────────────────

    [Fact]
    public async Task AddIngredient_ToSessionLevel_AddsToAddedIngredients()
    {
        var session = BuildSession();
        var repoMock = BuildSessionRepoMock(session);
        var svc = CreateService(sessionRepo: repoMock.Object);

        var request = new AddIngredientRequest
        {
            IngredientId = "ing-tomato",
            Name = "Tomato",
        };

        var result = await svc.AddIngredientAsync("session-1", "user-1", request);

        Assert.NotNull(result);
        Assert.Single(result.AddedIngredients);
        Assert.Equal("ing-tomato", result.AddedIngredients[0].IngredientId);
        Assert.Equal("Tomato", result.AddedIngredients[0].Name);
    }

    [Fact]
    public async Task AddIngredient_ToDish_AddsToCorrectDish()
    {
        var dish = new CookingSessionDish
        {
            DishId = "dish-1",
            Name = "Test Dish",
            Ingredients = [],
        };
        var session = BuildSession(dishes: [dish]);
        var repoMock = BuildSessionRepoMock(session);
        var svc = CreateService(sessionRepo: repoMock.Object);

        var request = new AddIngredientRequest
        {
            IngredientId = "ing-basil",
            Name = "Basil",
            DishId = "dish-1",
        };

        var result = await svc.AddIngredientAsync("session-1", "user-1", request);

        Assert.NotNull(result);
        var updatedDish = result.Dishes.Single(d => d.DishId == "dish-1");
        Assert.Single(updatedDish.Ingredients);
        Assert.Equal("ing-basil", updatedDish.Ingredients[0].IngredientId);
    }

    // ── RemoveIngredient ──────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveIngredient_FromSessionLevel_RemovesCorrectIngredient()
    {
        var sessionIngredients = new List<SessionIngredient>
        {
            new() { IngredientId = "ing-tomato", Name = "Tomato", AddedAt = DateTimeOffset.UtcNow },
            new() { IngredientId = "ing-basil", Name = "Basil", AddedAt = DateTimeOffset.UtcNow },
        };
        var session = BuildSession(addedIngredients: sessionIngredients);
        var repoMock = BuildSessionRepoMock(session);
        var svc = CreateService(sessionRepo: repoMock.Object);

        var result = await svc.RemoveIngredientAsync("session-1", "user-1", "ing-tomato", dishId: null);

        Assert.NotNull(result);
        Assert.Single(result.AddedIngredients);
        Assert.Equal("ing-basil", result.AddedIngredients[0].IngredientId);
    }

    // ── AddDish ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddDish_AddsNewDishToSession()
    {
        var session = BuildSession();
        var repoMock = BuildSessionRepoMock(session);
        var svc = CreateService(sessionRepo: repoMock.Object);

        var request = new AddDishRequest { Name = "Side Salad", CuisineType = "Mediterranean" };
        var result = await svc.AddDishAsync("session-1", "user-1", request);

        Assert.NotNull(result);
        Assert.Equal(2, result.Dishes.Count);
        var newDish = result.Dishes.Single(d => d.Name == "Side Salad");
        Assert.NotNull(newDish.DishId);
        Assert.Equal("Mediterranean", newDish.CuisineType);
    }

    // ── RemoveDish ────────────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveDish_RemovesCorrectDish()
    {
        var dishes = new List<CookingSessionDish>
        {
            new() { DishId = "dish-1", Name = "Main", Ingredients = [] },
            new() { DishId = "dish-2", Name = "Side", Ingredients = [] },
        };
        var session = BuildSession(dishes: dishes);
        var repoMock = BuildSessionRepoMock(session);
        var svc = CreateService(sessionRepo: repoMock.Object);

        var result = await svc.RemoveDishAsync("session-1", "user-1", "dish-1");

        Assert.NotNull(result);
        Assert.Single(result.Dishes);
        Assert.Equal("dish-2", result.Dishes[0].DishId);
    }

    // ── HasActiveSession ──────────────────────────────────────────────────────

    [Fact]
    public async Task HasActiveSession_WhenActiveSessionExists_ReturnsTrue()
    {
        var session = BuildSession(status: CookingSessionStatus.Active);
        var repoMock = BuildSessionRepoMock(session);
        var svc = CreateService(sessionRepo: repoMock.Object);

        var result = await svc.HasActiveSessionAsync("user-1");
        Assert.True(result);
    }

    [Fact]
    public async Task HasActiveSession_WhenPausedSessionExists_ReturnsFalse()
    {
        var session = BuildSession(status: CookingSessionStatus.Paused);
        var repoMock = BuildSessionRepoMock(session);
        var svc = CreateService(sessionRepo: repoMock.Object);

        var result = await svc.HasActiveSessionAsync("user-1");
        Assert.False(result);
    }

    [Fact]
    public async Task HasActiveSession_WhenNoSession_ReturnsFalse()
    {
        var repoMock = BuildSessionRepoMock(session: null);
        repoMock.Setup(r => r.GetByQueryAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var svc = CreateService(sessionRepo: repoMock.Object);

        var result = await svc.HasActiveSessionAsync("user-1");
        Assert.False(result);
    }

    // ── GetSuggestions — KB unavailable ───────────────────────────────────────

    [Fact]
    public async Task GetSuggestions_WhenKbNull_ReturnsKbUnavailableFlag()
    {
        var svc = CreateService(kb: null);
        var result = await svc.GetSuggestionsAsync("session-1", "user-1", null, 10);

        Assert.True(result.KbUnavailable);
        Assert.Empty(result.Suggestions);
    }

    [Fact]
    public async Task GetSuggestions_WhenKbUnavailable_ReturnsKbUnavailableFlag()
    {
        var kbMock = new Mock<IKnowledgeBaseService>();
        kbMock.Setup(kb => kb.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var svc = CreateService(kb: kbMock.Object);
        var result = await svc.GetSuggestionsAsync("session-1", "user-1", null, 10);

        Assert.True(result.KbUnavailable);
        Assert.Empty(result.Suggestions);
    }

    // ── GetSuggestions — scoring algorithm ───────────────────────────────────

    [Fact]
    public async Task GetSuggestions_WithSingleIngredient_ReturnsPairings()
    {
        var sessionIngredient = new SessionIngredient
        {
            IngredientId = "ing-tomato",
            Name = "Tomato",
            AddedAt = DateTimeOffset.UtcNow,
        };
        var session = BuildSession(addedIngredients: [sessionIngredient]);

        var repoMock = BuildSessionRepoMock(session);

        var kbMock = new Mock<IKnowledgeBaseService>();
        kbMock.Setup(kb => kb.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        kbMock.Setup(kb => kb.GetIngredientAsync("ing-tomato", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IngredientDocument?)null);
        kbMock.Setup(kb => kb.GetPairingsAsync(
                "ing-tomato",
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PairingSuggestion>
            {
                new("ing-basil", "Basil", 0.95, "herb", "reference"),
                new("ing-garlic", "Garlic", 0.87, "vegetable", "reference"),
            });

        var svc = CreateService(sessionRepo: repoMock.Object, kb: kbMock.Object);
        var result = await svc.GetSuggestionsAsync("session-1", "user-1", null, 10);

        Assert.False(result.KbUnavailable);
        Assert.Equal(2, result.Suggestions.Count);
    }

    [Fact]
    public async Task GetSuggestions_SortsByAggregateScoreDescending()
    {
        var ingredients = new List<SessionIngredient>
        {
            new() { IngredientId = "ing-tomato", Name = "Tomato", AddedAt = DateTimeOffset.UtcNow },
            new() { IngredientId = "ing-onion", Name = "Onion", AddedAt = DateTimeOffset.UtcNow },
        };
        var session = BuildSession(addedIngredients: ingredients);
        var repoMock = BuildSessionRepoMock(session);

        var kbMock = new Mock<IKnowledgeBaseService>();
        kbMock.Setup(kb => kb.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        kbMock.Setup(kb => kb.GetIngredientAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IngredientDocument?)null);

        // Tomato pairs with garlic (0.9) and basil (0.6)
        kbMock.Setup(kb => kb.GetPairingsAsync(
                "ing-tomato",
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PairingSuggestion>
            {
                new("ing-garlic", "Garlic", 0.9, null, "reference"),
                new("ing-basil", "Basil", 0.6, null, "reference"),
            });

        // Onion pairs with garlic (0.85) — garlic now has aggregated 0.9 + 0.85 = 1.75
        kbMock.Setup(kb => kb.GetPairingsAsync(
                "ing-onion",
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PairingSuggestion>
            {
                new("ing-garlic", "Garlic", 0.85, null, "reference"),
            });

        var svc = CreateService(sessionRepo: repoMock.Object, kb: kbMock.Object);
        var result = await svc.GetSuggestionsAsync("session-1", "user-1", null, 10);

        Assert.False(result.KbUnavailable);
        Assert.Equal(2, result.Suggestions.Count);
        // Garlic should rank first due to aggregated score
        Assert.Equal("ing-garlic", result.Suggestions[0].IngredientId);
        Assert.True(result.Suggestions[0].AggregateScore > result.Suggestions[1].AggregateScore);
    }

    [Fact]
    public async Task GetSuggestions_ExcludesCurrentSessionIngredients()
    {
        var sessionIngredient = new SessionIngredient
        {
            IngredientId = "ing-tomato",
            Name = "Tomato",
            AddedAt = DateTimeOffset.UtcNow,
        };
        var session = BuildSession(addedIngredients: [sessionIngredient]);
        var repoMock = BuildSessionRepoMock(session);

        var kbMock = new Mock<IKnowledgeBaseService>();
        kbMock.Setup(kb => kb.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        kbMock.Setup(kb => kb.GetIngredientAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IngredientDocument?)null);

        // KB suggests tomato pairs with itself (edge case) and basil
        kbMock.Setup(kb => kb.GetPairingsAsync(
                "ing-tomato",
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PairingSuggestion>
            {
                new("ing-tomato", "Tomato", 1.0, null, "reference"),  // self — should be excluded
                new("ing-basil", "Basil", 0.95, null, "reference"),
            });

        var svc = CreateService(sessionRepo: repoMock.Object, kb: kbMock.Object);
        var result = await svc.GetSuggestionsAsync("session-1", "user-1", null, 10);

        Assert.DoesNotContain(result.Suggestions, s => s.IngredientId == "ing-tomato");
        Assert.Contains(result.Suggestions, s => s.IngredientId == "ing-basil");
    }

    [Fact]
    public async Task GetSuggestions_ExcludesDislikedIngredients()
    {
        var sessionIngredient = new SessionIngredient
        {
            IngredientId = "ing-tomato",
            Name = "Tomato",
            AddedAt = DateTimeOffset.UtcNow,
        };
        var session = BuildSession(addedIngredients: [sessionIngredient]);
        var repoMock = BuildSessionRepoMock(session);

        var kbMock = new Mock<IKnowledgeBaseService>();
        kbMock.Setup(kb => kb.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        kbMock.Setup(kb => kb.GetIngredientAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IngredientDocument?)null);
        kbMock.Setup(kb => kb.GetPairingsAsync(
                "ing-tomato",
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PairingSuggestion>
            {
                new("ing-garlic", "Garlic", 0.9, null, "reference"),
                new("ing-onion", "Onion", 0.8, null, "reference"),
            });

        var prefMock = new Mock<IPreferenceService>();
        // User dislikes garlic
        prefMock.Setup(p => p.GetExcludedIngredientIdsAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(["ing-garlic"]);

        var svc = CreateService(sessionRepo: repoMock.Object, kb: kbMock.Object, preferenceService: prefMock.Object);
        var result = await svc.GetSuggestionsAsync("session-1", "user-1", null, 10);

        Assert.DoesNotContain(result.Suggestions, s => s.IngredientId == "ing-garlic");
        Assert.Contains(result.Suggestions, s => s.IngredientId == "ing-onion");
    }

    [Fact]
    public async Task GetSuggestions_LimitIsRespected()
    {
        var sessionIngredient = new SessionIngredient
        {
            IngredientId = "ing-tomato",
            Name = "Tomato",
            AddedAt = DateTimeOffset.UtcNow,
        };
        var session = BuildSession(addedIngredients: [sessionIngredient]);
        var repoMock = BuildSessionRepoMock(session);

        var pairings = Enumerable.Range(1, 20)
            .Select(i => new PairingSuggestion($"ing-{i}", $"Ingredient {i}", 0.5 + i * 0.01, null, "reference"))
            .ToList();

        var kbMock = new Mock<IKnowledgeBaseService>();
        kbMock.Setup(kb => kb.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        kbMock.Setup(kb => kb.GetIngredientAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IngredientDocument?)null);
        kbMock.Setup(kb => kb.GetPairingsAsync(
                "ing-tomato",
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pairings);

        var svc = CreateService(sessionRepo: repoMock.Object, kb: kbMock.Object);
        var result = await svc.GetSuggestionsAsync("session-1", "user-1", null, limit: 5);

        Assert.True(result.Suggestions.Count <= 5);
    }

    [Fact]
    public async Task GetSuggestions_WithEmptySession_ReturnsEmptySuggestions()
    {
        // Session with no ingredients
        var session = BuildSession(
            addedIngredients: [],
            dishes: [new CookingSessionDish { DishId = "d1", Name = "Empty", Ingredients = [] }]);
        var repoMock = BuildSessionRepoMock(session);

        var kbMock = new Mock<IKnowledgeBaseService>();
        kbMock.Setup(kb => kb.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var svc = CreateService(sessionRepo: repoMock.Object, kb: kbMock.Object);
        var result = await svc.GetSuggestionsAsync("session-1", "user-1", null, 10);

        Assert.Empty(result.Suggestions);
        Assert.False(result.KbUnavailable);
    }

    // ── GetIngredientDetail ───────────────────────────────────────────────────

    [Fact]
    public async Task GetIngredientDetail_WhenKbNull_ReturnsNull()
    {
        var svc = CreateService(kb: null);
        var result = await svc.GetIngredientDetailAsync("session-1", "user-1", "ing-tomato");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetIngredientDetail_WhenIngredientNotInKb_ReturnsNull()
    {
        var session = BuildSession();
        var repoMock = BuildSessionRepoMock(session);

        var kbMock = new Mock<IKnowledgeBaseService>();
        kbMock.Setup(kb => kb.GetIngredientAsync("ing-unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IngredientDocument?)null);

        var svc = CreateService(sessionRepo: repoMock.Object, kb: kbMock.Object);
        var result = await svc.GetIngredientDetailAsync("session-1", "user-1", "ing-unknown");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetIngredientDetail_ReturnsFlavourProfileAndSubstitutes()
    {
        var sessionIngredient = new SessionIngredient
        {
            IngredientId = "ing-basil",
            Name = "Basil",
            AddedAt = DateTimeOffset.UtcNow,
        };
        var session = BuildSession(addedIngredients: [sessionIngredient]);
        var repoMock = BuildSessionRepoMock(session);

        var kbMock = new Mock<IKnowledgeBaseService>();
        kbMock.Setup(kb => kb.GetIngredientAsync("ing-tomato", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IngredientDocument
            {
                IngredientId = "ing-tomato",
                Name = "Tomato",
                Category = "vegetable",
                FlavourProfile = "sweet",
                NutritionSummary = "Low calorie",
            });
        kbMock.Setup(kb => kb.GetSubstitutesAsync("ing-tomato", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Blend.Api.Ingredients.Models.SubstituteSuggestion>
            {
                new("ing-sun-tomato", "Sun-dried Tomato", null),
            });

        var svc = CreateService(sessionRepo: repoMock.Object, kb: kbMock.Object);
        var result = await svc.GetIngredientDetailAsync("session-1", "user-1", "ing-tomato");

        Assert.NotNull(result);
        Assert.Equal("ing-tomato", result.IngredientId);
        Assert.Equal("sweet", result.FlavourProfile);
        Assert.Single(result.Substitutes);
        Assert.Equal("Sun-dried Tomato", result.Substitutes[0]);
    }

    [Fact]
    public async Task GetIngredientDetail_WithCoIngredients_BuildsWhyItPairs()
    {
        var sessionIngredient = new SessionIngredient
        {
            IngredientId = "ing-basil",
            Name = "Basil",
            AddedAt = DateTimeOffset.UtcNow,
        };
        var session = BuildSession(addedIngredients: [sessionIngredient]);
        var repoMock = BuildSessionRepoMock(session);

        var kbMock = new Mock<IKnowledgeBaseService>();
        kbMock.Setup(kb => kb.GetIngredientAsync("ing-tomato", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IngredientDocument { IngredientId = "ing-tomato", Name = "Tomato" });
        kbMock.Setup(kb => kb.GetSubstitutesAsync("ing-tomato", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var svc = CreateService(sessionRepo: repoMock.Object, kb: kbMock.Object);
        var result = await svc.GetIngredientDetailAsync("session-1", "user-1", "ing-tomato");

        Assert.NotNull(result);
        Assert.NotNull(result.WhyItPairs);
        Assert.Contains("Basil", result.WhyItPairs);
    }

    // ── SubmitFeedback ────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitFeedback_WhenRepositoryNull_ReturnsFalse()
    {
        var svc = CreateService(sessionRepo: null);
        var request = new SubmitFeedbackRequest
        {
            Feedback = [new PairingFeedbackItem { IngredientId1 = "ing-a", IngredientId2 = "ing-b", Rating = 4 }],
        };

        var result = await svc.SubmitFeedbackAsync("session-1", "user-1", request);

        Assert.False(result);
    }

    [Fact]
    public async Task SubmitFeedback_WhenSessionNotFound_ReturnsFalse()
    {
        var repoMock = BuildSessionRepoMock(session: null);
        var svc = CreateService(sessionRepo: repoMock.Object);
        var request = new SubmitFeedbackRequest
        {
            Feedback = [new PairingFeedbackItem { IngredientId1 = "ing-a", IngredientId2 = "ing-b", Rating = 4 }],
        };

        var result = await svc.SubmitFeedbackAsync("session-1", "user-1", request);

        Assert.False(result);
    }

    [Fact]
    public async Task SubmitFeedback_WithValidPayload_ReturnsTrue()
    {
        var session = BuildSession();
        var repoMock = BuildSessionRepoMock(session);

        var kbMock = new Mock<IKnowledgeBaseService>();
        kbMock.Setup(kb => kb.UpdatePairingScoreAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var svc = CreateService(sessionRepo: repoMock.Object, kb: kbMock.Object);
        var request = new SubmitFeedbackRequest
        {
            Feedback =
            [
                new PairingFeedbackItem { IngredientId1 = "ing-tomato", IngredientId2 = "ing-basil", Rating = 5 },
                new PairingFeedbackItem { IngredientId1 = "ing-garlic", IngredientId2 = "ing-olive-oil", Rating = 4 },
            ],
        };

        var result = await svc.SubmitFeedbackAsync("session-1", "user-1", request);

        Assert.True(result);
        kbMock.Verify(kb => kb.UpdatePairingScoreAsync(
            "ing-tomato", "ing-basil", 1.0, It.IsAny<CancellationToken>()), Times.Once);
        kbMock.Verify(kb => kb.UpdatePairingScoreAsync(
            "ing-garlic", "ing-olive-oil", 0.8, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitFeedback_WithKbNull_ReturnsTrueWithoutUpdating()
    {
        var session = BuildSession();
        var repoMock = BuildSessionRepoMock(session);
        var svc = CreateService(sessionRepo: repoMock.Object, kb: null);

        var request = new SubmitFeedbackRequest
        {
            Feedback = [new PairingFeedbackItem { IngredientId1 = "ing-a", IngredientId2 = "ing-b", Rating = 3 }],
        };

        // Should succeed even without KB (graceful degradation)
        var result = await svc.SubmitFeedbackAsync("session-1", "user-1", request);
        Assert.True(result);
    }

    [Fact]
    public async Task SubmitFeedback_NormalisesRatingCorrectly()
    {
        var session = BuildSession();
        var repoMock = BuildSessionRepoMock(session);

        double capturedRating = 0;
        var kbMock = new Mock<IKnowledgeBaseService>();
        kbMock.Setup(kb => kb.UpdatePairingScoreAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, double, CancellationToken>((_, _, rating, _) => capturedRating = rating)
            .Returns(Task.CompletedTask);

        var svc = CreateService(sessionRepo: repoMock.Object, kb: kbMock.Object);
        var request = new SubmitFeedbackRequest
        {
            Feedback = [new PairingFeedbackItem { IngredientId1 = "ing-a", IngredientId2 = "ing-b", Rating = 3 }],
        };

        await svc.SubmitFeedbackAsync("session-1", "user-1", request);

        // 3 / 5 = 0.6
        Assert.Equal(0.6, capturedRating, precision: 10);
    }

    // ── PublishSession ────────────────────────────────────────────────────────

    [Fact]
    public async Task PublishSession_WhenRepositoryNull_ReturnsNull()
    {
        var svc = CreateService(sessionRepo: null);
        var request = new PublishSessionRequest
        {
            Title = "My Recipe",
            Directions = [new Blend.Api.Recipes.Models.RecipeDirectionRequest { StepNumber = 1, Text = "Cook." }],
        };

        var result = await svc.PublishSessionAsync("session-1", "user-1", request);

        Assert.Null(result);
    }

    [Fact]
    public async Task PublishSession_WhenSessionNotFound_ReturnsNull()
    {
        var sessionRepoMock = BuildSessionRepoMock(session: null);
        var recipeRepoMock = new Mock<IRepository<Recipe>>();
        var svc = CreateService(sessionRepo: sessionRepoMock.Object, recipeRepo: recipeRepoMock.Object);

        var request = new PublishSessionRequest
        {
            Title = "My Recipe",
            Directions = [new Blend.Api.Recipes.Models.RecipeDirectionRequest { StepNumber = 1, Text = "Cook." }],
        };

        var result = await svc.PublishSessionAsync("session-1", "user-1", request);

        Assert.Null(result);
    }

    [Fact]
    public async Task PublishSession_CreatesRecipeWithSessionIngredients()
    {
        var dish = new CookingSessionDish
        {
            DishId = "dish-1",
            Name = "Pasta",
            Ingredients =
            [
                new SessionIngredient { IngredientId = "ing-tomato", Name = "Tomato", AddedAt = DateTimeOffset.UtcNow },
                new SessionIngredient { IngredientId = "ing-basil", Name = "Basil", AddedAt = DateTimeOffset.UtcNow },
            ],
        };
        var session = BuildSession(dishes: [dish]);
        var sessionRepoMock = BuildSessionRepoMock(session);

        Recipe? capturedRecipe = null;
        var recipeRepoMock = new Mock<IRepository<Recipe>>();
        recipeRepoMock.Setup(r => r.CreateAsync(It.IsAny<Recipe>(), It.IsAny<CancellationToken>()))
            .Callback<Recipe, CancellationToken>((r, _) => capturedRecipe = r)
            .ReturnsAsync((Recipe r, CancellationToken _) => r);

        var svc = CreateService(sessionRepo: sessionRepoMock.Object, recipeRepo: recipeRepoMock.Object);

        var request = new PublishSessionRequest
        {
            Title = "Tomato Pasta",
            Directions = [new Blend.Api.Recipes.Models.RecipeDirectionRequest { StepNumber = 1, Text = "Boil pasta." }],
        };

        var result = await svc.PublishSessionAsync("session-1", "user-1", request);

        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.RecipeId));
        Assert.NotNull(capturedRecipe);
        Assert.Equal("Tomato Pasta", capturedRecipe.Title);
        Assert.Equal(2, capturedRecipe.Ingredients.Count);
        Assert.True(capturedRecipe.IsPublic);
    }

    [Fact]
    public async Task PublishSession_DeduplicatesIngredientsAcrossDishesAndSession()
    {
        var sharedIngredient = new SessionIngredient
        {
            IngredientId = "ing-garlic",
            Name = "Garlic",
            AddedAt = DateTimeOffset.UtcNow,
        };

        var dish = new CookingSessionDish
        {
            DishId = "dish-1",
            Name = "Main",
            Ingredients = [sharedIngredient],
        };

        // Same garlic also at session level
        var session = BuildSession(dishes: [dish], addedIngredients: [sharedIngredient]);
        var sessionRepoMock = BuildSessionRepoMock(session);

        Recipe? capturedRecipe = null;
        var recipeRepoMock = new Mock<IRepository<Recipe>>();
        recipeRepoMock.Setup(r => r.CreateAsync(It.IsAny<Recipe>(), It.IsAny<CancellationToken>()))
            .Callback<Recipe, CancellationToken>((r, _) => capturedRecipe = r)
            .ReturnsAsync((Recipe r, CancellationToken _) => r);

        var svc = CreateService(sessionRepo: sessionRepoMock.Object, recipeRepo: recipeRepoMock.Object);

        var request = new PublishSessionRequest
        {
            Title = "Garlic Dish",
            Directions = [new Blend.Api.Recipes.Models.RecipeDirectionRequest { StepNumber = 1, Text = "Cook garlic." }],
        };

        await svc.PublishSessionAsync("session-1", "user-1", request);

        Assert.NotNull(capturedRecipe);
        // Should have only one garlic, not two
        Assert.Single(capturedRecipe.Ingredients);
        Assert.Equal("ing-garlic", capturedRecipe.Ingredients[0].IngredientId);
    }
}

