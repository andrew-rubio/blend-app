using Blend.Api.Preferences.Services;
using Blend.Api.Search.Models;
using Blend.Api.Search.Services;
using Blend.Api.Services.Spoonacular;
using Blend.Api.Services.Spoonacular.Models;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Blend.Tests.Unit.Search;

/// <summary>
/// Unit tests for <see cref="SearchService"/> covering query building, ranking,
/// partial matching, preference application, and quota-exhaustion fallback.
/// </summary>
public class SearchServiceTests
{
    // ── Helpers ────────────────────────────────────────────────────────────────

    private static SearchService CreateService(
        ISpoonacularService? spoonacular = null,
        IRepository<Recipe>? recipeRepo = null,
        IRepository<Activity>? activityRepo = null,
        IPreferenceService? preferences = null)
    {
        var prefService = preferences ?? new Mock<IPreferenceService>().Object;
        return new SearchService(
            NullLogger<SearchService>.Instance,
            prefService,
            spoonacular,
            recipeRepo,
            activityRepo);
    }

    private static Recipe MakeRecipe(
        string id,
        string title,
        string? cuisine = null,
        string? dishType = null,
        int likeCount = 0,
        bool isPublic = true,
        int prepTime = 10,
        int cookTime = 20,
        string? description = null) => new()
    {
        Id = id,
        AuthorId = "author-1",
        Title = title,
        Description = description,
        Ingredients =
        [
            new RecipeIngredient { Quantity = 1, Unit = "cup", IngredientName = "Flour" },
        ],
        Directions = [],
        PrepTime = prepTime,
        CookTime = cookTime,
        Servings = 4,
        CuisineType = cuisine,
        DishType = dishType,
        Tags = [],
        IsPublic = isPublic,
        LikeCount = likeCount,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
    };

    // ── Tokenisation ──────────────────────────────────────────────────────────

    [Fact]
    public void Tokenise_SingleWord_ReturnsSingleToken()
    {
        var tokens = SearchService.Tokenise("chicken");
        Assert.Single(tokens);
        Assert.Equal("chicken", tokens[0]);
    }

    [Fact]
    public void Tokenise_MultiWord_ReturnsMultipleTokens()
    {
        var tokens = SearchService.Tokenise("chicken pasta");
        Assert.Equal(2, tokens.Count);
        Assert.Contains("chicken", tokens);
        Assert.Contains("pasta", tokens);
    }

    [Fact]
    public void Tokenise_UpperCase_NormalisesToLowerCase()
    {
        var tokens = SearchService.Tokenise("CHICKEN");
        Assert.Single(tokens);
        Assert.Equal("chicken", tokens[0]);
    }

    [Fact]
    public void Tokenise_ShortTokensFiltered_ExcludesSingleChar()
    {
        // Single-character tokens are excluded (min length 2).
        var tokens = SearchService.Tokenise("a chicken");
        Assert.Single(tokens);
        Assert.Equal("chicken", tokens[0]);
    }

    [Fact]
    public void Tokenise_Deduplication_RemovesDuplicates()
    {
        var tokens = SearchService.Tokenise("chicken chicken");
        Assert.Single(tokens);
    }

    [Fact]
    public void Tokenise_EmptyString_ReturnsEmpty()
    {
        var tokens = SearchService.Tokenise("   ");
        Assert.Empty(tokens);
    }

    // ── ParseCommaSeparated ────────────────────────────────────────────────────

    [Fact]
    public void ParseCommaSeparated_MultipleValues_ReturnsTrimmedList()
    {
        var result = SearchService.ParseCommaSeparated(" Italian , Mexican ");
        Assert.Equal(2, result.Count);
        Assert.Contains("Italian", result);
        Assert.Contains("Mexican", result);
    }

    [Fact]
    public void ParseCommaSeparated_EmptyEntries_Excluded()
    {
        var result = SearchService.ParseCommaSeparated("Italian,,Mexican");
        Assert.Equal(2, result.Count);
    }

    // ── ComputeScore ──────────────────────────────────────────────────────────

    [Fact]
    public void ComputeScore_TitleMatchHigherThanDescriptionMatch()
    {
        var titleMatch = new UnifiedRecipeResult
        {
            Title = "Chicken Pasta",
            Description = "A simple dish",
            Cuisines = [],
            DishTypes = [],
            DataSource = RecipeDataSource.Community,
        };

        var descMatch = new UnifiedRecipeResult
        {
            Title = "Pasta Bake",
            Description = "Made with chicken",
            Cuisines = [],
            DishTypes = [],
            DataSource = RecipeDataSource.Community,
        };

        var tokens = SearchService.Tokenise("chicken");
        var preferences = new UserPreferences();

        var scoreTitle = SearchService.ComputeScore(titleMatch, tokens, preferences);
        var scoreDesc = SearchService.ComputeScore(descMatch, tokens, preferences);

        Assert.True(scoreTitle > scoreDesc);
    }

    [Fact]
    public void ComputeScore_PreferredCuisineBoost_IncreasesScore()
    {
        var italianRecipe = new UnifiedRecipeResult
        {
            Title = "Pizza",
            Cuisines = ["Italian"],
            DishTypes = [],
            DataSource = RecipeDataSource.Community,
        };

        var nonPreferred = new UnifiedRecipeResult
        {
            Title = "Pizza",
            Cuisines = ["Mexican"],
            DishTypes = [],
            DataSource = RecipeDataSource.Community,
        };

        var preferences = new UserPreferences
        {
            FavoriteCuisines = ["Italian"],
        };

        var scorePreferred = SearchService.ComputeScore(italianRecipe, [], preferences);
        var scoreNonPreferred = SearchService.ComputeScore(nonPreferred, [], preferences);

        Assert.True(scorePreferred > scoreNonPreferred);
    }

    [Fact]
    public void ComputeScore_PopularityContributes_HigherLikesHigherScore()
    {
        var popular = new UnifiedRecipeResult
        {
            Title = "Cake",
            Cuisines = [],
            DishTypes = [],
            Popularity = 100,
            DataSource = RecipeDataSource.Community,
        };

        var unpopular = new UnifiedRecipeResult
        {
            Title = "Cake",
            Cuisines = [],
            DishTypes = [],
            Popularity = 0,
            DataSource = RecipeDataSource.Community,
        };

        var preferences = new UserPreferences();
        var scorePopular = SearchService.ComputeScore(popular, [], preferences);
        var scoreUnpopular = SearchService.ComputeScore(unpopular, [], preferences);

        Assert.True(scorePopular > scoreUnpopular);
    }

    // ── RankResults ────────────────────────────────────────────────────────────

    [Fact]
    public void RankResults_RelevanceSort_MostRelevantFirst()
    {
        var results = new List<UnifiedRecipeResult>
        {
            new() { Title = "Pasta Bake", Description = null, Cuisines = [], DishTypes = [], DataSource = RecipeDataSource.Community },
            new() { Title = "Chicken Pasta", Description = null, Cuisines = [], DishTypes = [], DataSource = RecipeDataSource.Community },
        };

        var ranked = SearchService.RankResults(results, "chicken", "relevance", new UserPreferences());

        Assert.Equal("Chicken Pasta", ranked[0].Title);
    }

    [Fact]
    public void RankResults_PopularitySort_MostPopularFirst()
    {
        var results = new List<UnifiedRecipeResult>
        {
            new() { Title = "A", Popularity = 5, Cuisines = [], DishTypes = [], DataSource = RecipeDataSource.Community },
            new() { Title = "B", Popularity = 50, Cuisines = [], DishTypes = [], DataSource = RecipeDataSource.Community },
            new() { Title = "C", Popularity = 1, Cuisines = [], DishTypes = [], DataSource = RecipeDataSource.Community },
        };

        var ranked = SearchService.RankResults(results, null, "popularity", new UserPreferences());

        Assert.Equal("B", ranked[0].Title);
        Assert.Equal("A", ranked[1].Title);
        Assert.Equal("C", ranked[2].Title);
    }

    [Fact]
    public void RankResults_TimeSort_FastestFirst()
    {
        var results = new List<UnifiedRecipeResult>
        {
            new() { Title = "Slow", ReadyInMinutes = 90, Cuisines = [], DishTypes = [], DataSource = RecipeDataSource.Community },
            new() { Title = "Fast", ReadyInMinutes = 15, Cuisines = [], DishTypes = [], DataSource = RecipeDataSource.Community },
        };

        var ranked = SearchService.RankResults(results, null, "time", new UserPreferences());

        Assert.Equal("Fast", ranked[0].Title);
    }

    [Fact]
    public void RankResults_NewestSort_MostRecentFirst()
    {
        var now = DateTimeOffset.UtcNow;
        var results = new List<UnifiedRecipeResult>
        {
            new() { Title = "Old", CreatedAt = now.AddDays(-10), Cuisines = [], DishTypes = [], DataSource = RecipeDataSource.Community },
            new() { Title = "New", CreatedAt = now, Cuisines = [], DishTypes = [], DataSource = RecipeDataSource.Community },
        };

        var ranked = SearchService.RankResults(results, null, "newest", new UserPreferences());

        Assert.Equal("New", ranked[0].Title);
    }

    [Fact]
    public void RankResults_EmptyList_ReturnsEmpty()
    {
        var ranked = SearchService.RankResults([], null, "relevance", new UserPreferences());
        Assert.Empty(ranked);
    }

    // ── SearchRecipesAsync — internal results ─────────────────────────────────

    [Fact]
    public async Task SearchRecipesAsync_WithInternalRecipes_ReturnsCommunityResults()
    {
        var recipe = MakeRecipe("r1", "Chicken Soup");
        var mockRepo = new Mock<IRepository<Recipe>>();
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync([recipe]);
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync([recipe]);

        var mockPref = new Mock<IPreferenceService>();
        mockPref.Setup(p => p.GetUserPreferencesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserPreferences());
        mockPref.Setup(p => p.ApplyPreferencesToSearch(It.IsAny<ComplexSearchFilters>(), It.IsAny<UserPreferences>()))
                .Returns(new ComplexSearchFilters());

        var svc = CreateService(recipeRepo: mockRepo.Object, preferences: mockPref.Object);
        var request = new SearchRecipesRequest { Q = "chicken" };

        var response = await svc.SearchRecipesAsync(request, null);

        Assert.NotEmpty(response.Results);
        Assert.Contains(response.Results, r => r.DataSource == RecipeDataSource.Community);
    }

    [Fact]
    public async Task SearchRecipesAsync_WithNoServices_ReturnsEmptyResults()
    {
        var mockPref = new Mock<IPreferenceService>();
        mockPref.Setup(p => p.GetUserPreferencesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserPreferences());
        mockPref.Setup(p => p.ApplyPreferencesToSearch(It.IsAny<ComplexSearchFilters>(), It.IsAny<UserPreferences>()))
                .Returns(new ComplexSearchFilters());

        var svc = CreateService(preferences: mockPref.Object);
        var request = new SearchRecipesRequest { Q = "chicken" };

        var response = await svc.SearchRecipesAsync(request, null);

        Assert.Empty(response.Results);
        Assert.False(response.Metadata.QuotaExhausted);
    }

    [Fact]
    public async Task SearchRecipesAsync_QuotaExhaustedSpoonacular_FlagSet()
    {
        var mockSpoon = new Mock<ISpoonacularService>();
        mockSpoon.Setup(s => s.ComplexSearchAsync(It.IsAny<string>(), It.IsAny<ComplexSearchFilters>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(SpoonacularResult<IReadOnlyList<RecipeSummary>>.RateLimited());

        var mockPref = new Mock<IPreferenceService>();
        mockPref.Setup(p => p.GetUserPreferencesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserPreferences());
        mockPref.Setup(p => p.ApplyPreferencesToSearch(It.IsAny<ComplexSearchFilters>(), It.IsAny<UserPreferences>()))
                .Returns(new ComplexSearchFilters());

        var svc = CreateService(spoonacular: mockSpoon.Object, preferences: mockPref.Object);
        var request = new SearchRecipesRequest { Q = "pasta" };

        var response = await svc.SearchRecipesAsync(request, null);

        Assert.True(response.Metadata.QuotaExhausted);
    }

    [Fact]
    public async Task SearchRecipesAsync_SpoonacularReturnsResults_DataSourceIsSpoonacular()
    {
        var spoonResults = (IReadOnlyList<RecipeSummary>)
        [
            new RecipeSummary { SpoonacularId = 42, Title = "Spaghetti Carbonara", Likes = 100 },
        ];

        var mockSpoon = new Mock<ISpoonacularService>();
        mockSpoon.Setup(s => s.ComplexSearchAsync(It.IsAny<string>(), It.IsAny<ComplexSearchFilters>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(SpoonacularResult<IReadOnlyList<RecipeSummary>>.FromApi(spoonResults));

        var mockPref = new Mock<IPreferenceService>();
        mockPref.Setup(p => p.GetUserPreferencesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserPreferences());
        mockPref.Setup(p => p.ApplyPreferencesToSearch(It.IsAny<ComplexSearchFilters>(), It.IsAny<UserPreferences>()))
                .Returns(new ComplexSearchFilters());

        var svc = CreateService(spoonacular: mockSpoon.Object, preferences: mockPref.Object);
        var request = new SearchRecipesRequest { Q = "spaghetti" };

        var response = await svc.SearchRecipesAsync(request, null);

        Assert.NotEmpty(response.Results);
        Assert.All(response.Results, r => Assert.Equal(RecipeDataSource.Spoonacular, r.DataSource));
    }

    [Fact]
    public async Task SearchRecipesAsync_BothSources_ResultsAreMerged()
    {
        var spoonResults = (IReadOnlyList<RecipeSummary>)
        [
            new RecipeSummary { SpoonacularId = 1, Title = "Spoon Recipe" },
        ];

        var mockSpoon = new Mock<ISpoonacularService>();
        mockSpoon.Setup(s => s.ComplexSearchAsync(It.IsAny<string>(), It.IsAny<ComplexSearchFilters>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(SpoonacularResult<IReadOnlyList<RecipeSummary>>.FromApi(spoonResults));

        var recipe = MakeRecipe("internal-1", "Internal Recipe");
        var mockRepo = new Mock<IRepository<Recipe>>();
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync([recipe]);
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync([recipe]);

        var mockPref = new Mock<IPreferenceService>();
        mockPref.Setup(p => p.GetUserPreferencesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserPreferences());
        mockPref.Setup(p => p.ApplyPreferencesToSearch(It.IsAny<ComplexSearchFilters>(), It.IsAny<UserPreferences>()))
                .Returns(new ComplexSearchFilters());

        var svc = CreateService(
            spoonacular: mockSpoon.Object,
            recipeRepo: mockRepo.Object,
            preferences: mockPref.Object);

        var request = new SearchRecipesRequest { Q = "recipe" };
        var response = await svc.SearchRecipesAsync(request, null);

        Assert.Equal(2, response.Results.Count);
        Assert.Contains(response.Results, r => r.DataSource == RecipeDataSource.Spoonacular);
        Assert.Contains(response.Results, r => r.DataSource == RecipeDataSource.Community);
    }

    // ── Pagination ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchRecipesAsync_Pagination_NextCursorSetWhenMoreResults()
    {
        var recipes = Enumerable.Range(1, 5)
            .Select(i => MakeRecipe($"r{i}", $"Recipe {i}"))
            .ToList();

        var mockRepo = new Mock<IRepository<Recipe>>();
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(recipes);
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(recipes);

        var mockPref = new Mock<IPreferenceService>();
        mockPref.Setup(p => p.GetUserPreferencesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserPreferences());
        mockPref.Setup(p => p.ApplyPreferencesToSearch(It.IsAny<ComplexSearchFilters>(), It.IsAny<UserPreferences>()))
                .Returns(new ComplexSearchFilters());

        var svc = CreateService(recipeRepo: mockRepo.Object, preferences: mockPref.Object);
        var request = new SearchRecipesRequest { PageSize = 2 };
        var response = await svc.SearchRecipesAsync(request, null);

        Assert.Equal(2, response.Results.Count);
        Assert.NotNull(response.Metadata.NextCursor);
    }

    [Fact]
    public async Task SearchRecipesAsync_Pagination_NoNextCursorOnLastPage()
    {
        var recipes = new List<Recipe> { MakeRecipe("r1", "Only Recipe") };

        var mockRepo = new Mock<IRepository<Recipe>>();
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(recipes);
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(recipes);

        var mockPref = new Mock<IPreferenceService>();
        mockPref.Setup(p => p.GetUserPreferencesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserPreferences());
        mockPref.Setup(p => p.ApplyPreferencesToSearch(It.IsAny<ComplexSearchFilters>(), It.IsAny<UserPreferences>()))
                .Returns(new ComplexSearchFilters());

        var svc = CreateService(recipeRepo: mockRepo.Object, preferences: mockPref.Object);
        var request = new SearchRecipesRequest { PageSize = 20 };
        var response = await svc.SearchRecipesAsync(request, null);

        Assert.Null(response.Metadata.NextCursor);
    }

    // ── RecordViewAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task RecordViewAsync_ActivityRepositoryAvailable_CreatesActivity()
    {
        var mockActivity = new Mock<IRepository<Activity>>();
        mockActivity.Setup(a => a.CreateAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Activity a, CancellationToken _) => a);

        var mockPref = new Mock<IPreferenceService>();
        var svc = CreateService(activityRepo: mockActivity.Object, preferences: mockPref.Object);

        await svc.RecordViewAsync("recipe-123", RecipeDataSource.Community, "user-1");

        mockActivity.Verify(a => a.CreateAsync(
            It.Is<Activity>(act =>
                act.UserId == "user-1" &&
                act.ReferenceId == "recipe-123" &&
                act.Type == ActivityType.Viewed),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordViewAsync_SpoonacularRecipe_SetsCorrectReferenceType()
    {
        Activity? captured = null;
        var mockActivity = new Mock<IRepository<Activity>>();
        mockActivity.Setup(a => a.CreateAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
                    .Callback<Activity, CancellationToken>((a, _) => captured = a)
                    .ReturnsAsync((Activity a, CancellationToken _) => a);

        var mockPref = new Mock<IPreferenceService>();
        var svc = CreateService(activityRepo: mockActivity.Object, preferences: mockPref.Object);

        await svc.RecordViewAsync("42", RecipeDataSource.Spoonacular, "user-1");

        Assert.NotNull(captured);
        Assert.Equal("SpoonacularRecipe", captured!.ReferenceType);
    }

    [Fact]
    public async Task RecordViewAsync_NoActivityRepository_DoesNotThrow()
    {
        var mockPref = new Mock<IPreferenceService>();
        var svc = CreateService(preferences: mockPref.Object);

        // Should not throw even without activity repository.
        await svc.RecordViewAsync("recipe-123", RecipeDataSource.Community, "user-1");
    }

    // ── GetRecentlyViewedAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetRecentlyViewedAsync_ActivityRepositoryAvailable_ReturnsActivities()
    {
        var activities = new List<Activity>
        {
            new() { Id = "a1", UserId = "user-1", Type = ActivityType.Viewed, ReferenceId = "r1", ReferenceType = "Recipe", Timestamp = DateTimeOffset.UtcNow },
        };

        var mockActivity = new Mock<IRepository<Activity>>();
        mockActivity.Setup(a => a.GetByQueryAsync(It.IsAny<string>(), "user-1", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(activities);
        mockActivity.Setup(a => a.GetByQueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), "user-1", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(activities);

        var mockPref = new Mock<IPreferenceService>();
        var svc = CreateService(activityRepo: mockActivity.Object, preferences: mockPref.Object);

        var result = await svc.GetRecentlyViewedAsync("user-1", 10);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetRecentlyViewedAsync_NoRepository_ReturnsEmpty()
    {
        var mockPref = new Mock<IPreferenceService>();
        var svc = CreateService(preferences: mockPref.Object);

        var result = await svc.GetRecentlyViewedAsync("user-1", 10);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecentlyViewedAsync_PageSizeClamped_UsesClampedValue()
    {
        var mockActivity = new Mock<IRepository<Activity>>();
        mockActivity.Setup(a => a.GetByQueryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync([]);
        mockActivity.Setup(a => a.GetByQueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync([]);

        var mockPref = new Mock<IPreferenceService>();
        var svc = CreateService(activityRepo: mockActivity.Object, preferences: mockPref.Object);

        // pageSize 200 → clamped to 50; should not throw.
        var result = await svc.GetRecentlyViewedAsync("user-1", 200);
        Assert.Empty(result);
    }

    // ── Preference application ────────────────────────────────────────────────

    [Fact]
    public async Task SearchRecipesAsync_AuthenticatedUser_LoadsPreferences()
    {
        var mockSpoon = new Mock<ISpoonacularService>();
        mockSpoon.Setup(s => s.ComplexSearchAsync(It.IsAny<string>(), It.IsAny<ComplexSearchFilters>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(SpoonacularResult<IReadOnlyList<RecipeSummary>>.Degraded());

        var mockPref = new Mock<IPreferenceService>();
        mockPref.Setup(p => p.GetUserPreferencesAsync("user-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserPreferences { FavoriteCuisines = ["Italian"] });
        mockPref.Setup(p => p.ApplyPreferencesToSearch(It.IsAny<ComplexSearchFilters>(), It.IsAny<UserPreferences>()))
                .Returns(new ComplexSearchFilters());

        var svc = CreateService(spoonacular: mockSpoon.Object, preferences: mockPref.Object);
        var request = new SearchRecipesRequest { Q = "pasta" };

        await svc.SearchRecipesAsync(request, "user-1");

        // Verify preferences were loaded for the authenticated user.
        mockPref.Verify(p => p.GetUserPreferencesAsync("user-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchRecipesAsync_AnonymousUser_DoesNotLoadPreferences()
    {
        var mockSpoon = new Mock<ISpoonacularService>();
        mockSpoon.Setup(s => s.ComplexSearchAsync(It.IsAny<string>(), It.IsAny<ComplexSearchFilters>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(SpoonacularResult<IReadOnlyList<RecipeSummary>>.Degraded());

        var mockPref = new Mock<IPreferenceService>();
        mockPref.Setup(p => p.ApplyPreferencesToSearch(It.IsAny<ComplexSearchFilters>(), It.IsAny<UserPreferences>()))
                .Returns(new ComplexSearchFilters());

        var svc = CreateService(spoonacular: mockSpoon.Object, preferences: mockPref.Object);
        var request = new SearchRecipesRequest { Q = "pasta" };

        await svc.SearchRecipesAsync(request, null);

        // Should not call GetUserPreferencesAsync for anonymous requests.
        mockPref.Verify(p => p.GetUserPreferencesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── DegradedMode tests ─────────────────────────────────────────────────────

    [Fact]
    public async Task SearchRecipesAsync_WhenNoSpoonacularConfigured_DegradedModeTrue()
    {
        var mockPref = new Mock<IPreferenceService>();
        mockPref.Setup(p => p.GetUserPreferencesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserPreferences());
        mockPref.Setup(p => p.ApplyPreferencesToSearch(It.IsAny<ComplexSearchFilters>(), It.IsAny<UserPreferences>()))
                .Returns(new ComplexSearchFilters());

        // No Spoonacular service registered
        var svc = CreateService(preferences: mockPref.Object);
        var response = await svc.SearchRecipesAsync(new SearchRecipesRequest { Q = "chicken" }, null);

        Assert.True(response.Metadata.DegradedMode);
    }

    [Fact]
    public async Task SearchRecipesAsync_WhenSpoonacularDown_DegradedModeTrue()
    {
        var mockSpoon = new Mock<ISpoonacularService>();
        mockSpoon.Setup(s => s.ComplexSearchAsync(It.IsAny<string>(), It.IsAny<ComplexSearchFilters>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(SpoonacularResult<IReadOnlyList<RecipeSummary>>.Degraded());

        var mockPref = new Mock<IPreferenceService>();
        mockPref.Setup(p => p.GetUserPreferencesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserPreferences());
        mockPref.Setup(p => p.ApplyPreferencesToSearch(It.IsAny<ComplexSearchFilters>(), It.IsAny<UserPreferences>()))
                .Returns(new ComplexSearchFilters());

        var svc = CreateService(spoonacular: mockSpoon.Object, preferences: mockPref.Object);
        var response = await svc.SearchRecipesAsync(new SearchRecipesRequest { Q = "pasta" }, null);

        Assert.True(response.Metadata.DegradedMode);
    }

    [Fact]
    public async Task SearchRecipesAsync_WhenSpoonacularRateLimited_DegradedModeTrue()
    {
        var mockSpoon = new Mock<ISpoonacularService>();
        mockSpoon.Setup(s => s.ComplexSearchAsync(It.IsAny<string>(), It.IsAny<ComplexSearchFilters>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(SpoonacularResult<IReadOnlyList<RecipeSummary>>.RateLimited());

        var mockPref = new Mock<IPreferenceService>();
        mockPref.Setup(p => p.GetUserPreferencesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserPreferences());
        mockPref.Setup(p => p.ApplyPreferencesToSearch(It.IsAny<ComplexSearchFilters>(), It.IsAny<UserPreferences>()))
                .Returns(new ComplexSearchFilters());

        var svc = CreateService(spoonacular: mockSpoon.Object, preferences: mockPref.Object);
        var response = await svc.SearchRecipesAsync(new SearchRecipesRequest { Q = "pizza" }, null);

        Assert.True(response.Metadata.DegradedMode);
        Assert.True(response.Metadata.QuotaExhausted);
    }

    [Fact]
    public async Task SearchRecipesAsync_WhenSpoonacularAvailable_DegradedModeFalse()
    {
        var spoonResults = (IReadOnlyList<RecipeSummary>)
        [
            new RecipeSummary { SpoonacularId = 1, Title = "Pasta", Likes = 10 },
        ];

        var mockSpoon = new Mock<ISpoonacularService>();
        mockSpoon.Setup(s => s.ComplexSearchAsync(It.IsAny<string>(), It.IsAny<ComplexSearchFilters>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(SpoonacularResult<IReadOnlyList<RecipeSummary>>.FromApi(spoonResults));

        var mockPref = new Mock<IPreferenceService>();
        mockPref.Setup(p => p.GetUserPreferencesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserPreferences());
        mockPref.Setup(p => p.ApplyPreferencesToSearch(It.IsAny<ComplexSearchFilters>(), It.IsAny<UserPreferences>()))
                .Returns(new ComplexSearchFilters());

        var svc = CreateService(spoonacular: mockSpoon.Object, preferences: mockPref.Object);
        var response = await svc.SearchRecipesAsync(new SearchRecipesRequest { Q = "pasta" }, null);

        Assert.False(response.Metadata.DegradedMode);
        Assert.False(response.Metadata.QuotaExhausted);
    }
}
