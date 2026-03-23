using Blend.Api.Home.Models;
using Blend.Api.Home.Services;
using Blend.Api.Preferences.Services;
using Blend.Api.Services.Cache;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Blend.Tests.Unit.Home;

/// <summary>
/// Unit tests for <see cref="HomeService"/> covering section assembly, caching, and preference filtering.
/// </summary>
public class HomeServiceTests
{
    // ── Helpers ────────────────────────────────────────────────────────────────

    private static HomeService CreateService(
        IPreferenceService? preferenceService = null,
        IConfiguration? configuration = null,
        IRepository<Content>? contentRepo = null,
        IRepository<Recipe>? recipeRepo = null,
        IRepository<Activity>? activityRepo = null,
        ICacheService? cacheService = null)
    {
        var prefs = preferenceService ?? new Mock<IPreferenceService>().Object;
        var config = configuration ?? new ConfigurationBuilder().Build();
        return new HomeService(
            NullLogger<HomeService>.Instance,
            prefs,
            config,
            contentRepo,
            recipeRepo,
            activityRepo,
            cacheService);
    }

    private static Content MakeContent(
        string id,
        ContentType type,
        string title,
        bool isPublished = true,
        string? body = null,
        string? thumbnailUrl = null,
        string? mediaUrl = null,
        string? authorName = null) => new()
    {
        Id = id,
        ContentType = type,
        Title = title,
        IsPublished = isPublished,
        Body = body,
        ThumbnailUrl = thumbnailUrl,
        MediaUrl = mediaUrl,
        AuthorName = authorName,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
    };

    private static Recipe MakeRecipe(
        string id,
        string title,
        string authorId = "author-1",
        string? cuisineType = null,
        int likeCount = 0,
        bool isPublic = true,
        IReadOnlyList<string>? tags = null) => new()
    {
        Id = id,
        AuthorId = authorId,
        Title = title,
        IsPublic = isPublic,
        CuisineType = cuisineType,
        LikeCount = likeCount,
        Tags = tags ?? [],
        Ingredients = [],
        Directions = [],
        Photos = [],
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
    };

    private static Activity MakeActivity(
        string userId,
        string recipeId,
        string referenceType = "Recipe",
        ActivityType type = ActivityType.Viewed,
        DateTimeOffset? timestamp = null) => new()
    {
        Id = $"{userId}:Viewed:{recipeId}",
        UserId = userId,
        Type = type,
        ReferenceId = recipeId,
        ReferenceType = referenceType,
        Timestamp = timestamp ?? DateTimeOffset.UtcNow,
    };

    // ── GetHomeAsync — response shape ─────────────────────────────────────────

    [Fact]
    public async Task GetHomeAsync_ReturnsAllSections()
    {
        var service = CreateService();
        var result = await service.GetHomeAsync(null);

        Assert.NotNull(result.Search);
        Assert.NotNull(result.Featured);
        Assert.NotNull(result.Community);
        Assert.NotNull(result.RecentlyViewed);
    }

    [Fact]
    public async Task GetHomeAsync_GuestUser_ReturnsEmptyRecentlyViewed()
    {
        var service = CreateService();
        var result = await service.GetHomeAsync(userId: null);

        Assert.Empty(result.RecentlyViewed.Recipes);
    }

    [Fact]
    public async Task GetHomeAsync_NoRepositories_ReturnsEmptySections()
    {
        var service = CreateService();
        var result = await service.GetHomeAsync(userId: null);

        Assert.Empty(result.Featured.Recipes);
        Assert.Empty(result.Featured.Stories);
        Assert.Empty(result.Featured.Videos);
        Assert.Empty(result.Community.Recipes);
    }

    // ── Search placeholder ────────────────────────────────────────────────────

    [Fact]
    public async Task GetHomeAsync_ReturnsNonEmptyPlaceholder()
    {
        var service = CreateService();
        var result = await service.GetHomeAsync(null);

        Assert.False(string.IsNullOrWhiteSpace(result.Search.Placeholder));
    }

    [Fact]
    public async Task GetHomeAsync_UsesConfiguredPlaceholders_WhenProvided()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Home:SearchPlaceholders:0"] = "custom placeholder",
            })
            .Build();

        var service = CreateService(configuration: config);
        var result = await service.GetHomeAsync(null);

        Assert.Equal("custom placeholder", result.Search.Placeholder);
    }

    // ── Featured section ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetHomeAsync_FeaturedRecipes_ReturnsMappedData()
    {
        var contentRepo = new InMemoryContentRepository();
        contentRepo.Seed(MakeContent(
            "c1", ContentType.FeaturedRecipe, "Pasta Delight",
            body: "A wonderful pasta recipe.", thumbnailUrl: "https://img/pasta.jpg",
            authorName: "Chef Marco"));

        var service = CreateService(contentRepo: contentRepo);
        var result = await service.GetHomeAsync(null);

        var recipe = Assert.Single(result.Featured.Recipes);
        Assert.Equal("c1", recipe.Id);
        Assert.Equal("Pasta Delight", recipe.Title);
        Assert.Equal("https://img/pasta.jpg", recipe.ImageUrl);
        Assert.Equal("Chef Marco", recipe.Attribution);
        Assert.NotNull(recipe.ShortDescription);
    }

    [Fact]
    public async Task GetHomeAsync_FeaturedStories_ReturnsMappedData()
    {
        var contentRepo = new InMemoryContentRepository();
        contentRepo.Seed(MakeContent(
            "s1", ContentType.Story, "The Art of Sourdough",
            body: "Sourdough bread is fascinating.", thumbnailUrl: "https://img/sourdough.jpg",
            authorName: "Alice"));

        var service = CreateService(contentRepo: contentRepo);
        var result = await service.GetHomeAsync(null);

        var story = Assert.Single(result.Featured.Stories);
        Assert.Equal("s1", story.Id);
        Assert.Equal("The Art of Sourdough", story.Title);
        Assert.Equal("https://img/sourdough.jpg", story.CoverImageUrl);
        Assert.Equal("Alice", story.Author);
    }

    [Fact]
    public async Task GetHomeAsync_FeaturedVideos_ReturnsMappedData()
    {
        var contentRepo = new InMemoryContentRepository();
        contentRepo.Seed(MakeContent(
            "v1", ContentType.Video, "Quick Stir Fry",
            thumbnailUrl: "https://img/thumb.jpg", mediaUrl: "https://video.url/embed",
            authorName: "Bob"));

        var service = CreateService(contentRepo: contentRepo);
        var result = await service.GetHomeAsync(null);

        var video = Assert.Single(result.Featured.Videos);
        Assert.Equal("v1", video.Id);
        Assert.Equal("Quick Stir Fry", video.Title);
        Assert.Equal("https://img/thumb.jpg", video.ThumbnailUrl);
        Assert.Equal("https://video.url/embed", video.VideoUrl);
        Assert.Equal("Bob", video.Creator);
    }

    [Fact]
    public async Task GetHomeAsync_UnpublishedContent_NotIncluded()
    {
        var contentRepo = new InMemoryContentRepository();
        contentRepo.Seed(MakeContent("c1", ContentType.FeaturedRecipe, "Draft Recipe", isPublished: false));

        var service = CreateService(contentRepo: contentRepo);
        var result = await service.GetHomeAsync(null);

        Assert.Empty(result.Featured.Recipes);
    }

    [Fact]
    public async Task GetHomeAsync_FeaturedRecipeBody_TruncatedAt200Chars()
    {
        var longBody = new string('x', 300);
        var contentRepo = new InMemoryContentRepository();
        contentRepo.Seed(MakeContent("c1", ContentType.FeaturedRecipe, "Title", body: longBody));

        var service = CreateService(contentRepo: contentRepo);
        var result = await service.GetHomeAsync(null);

        var recipe = Assert.Single(result.Featured.Recipes);
        Assert.EndsWith("...", recipe.ShortDescription);
        Assert.True(recipe.ShortDescription!.Length <= 203); // 200 + "..."
    }

    [Fact]
    public async Task GetHomeAsync_FeaturedStoryBody_TruncatedAt300Chars()
    {
        var longBody = new string('x', 500);
        var contentRepo = new InMemoryContentRepository();
        contentRepo.Seed(MakeContent("s1", ContentType.Story, "Title", body: longBody));

        var service = CreateService(contentRepo: contentRepo);
        var result = await service.GetHomeAsync(null);

        var story = Assert.Single(result.Featured.Stories);
        Assert.EndsWith("...", story.Excerpt);
        Assert.True(story.Excerpt!.Length <= 303); // 300 + "..."
    }

    [Fact]
    public async Task GetHomeAsync_FeaturedStory_EstimatesReadingTime()
    {
        // 200 words ≈ 1 minute at 200 wpm.
        var bodyWith200Words = string.Join(" ", Enumerable.Repeat("word", 200));
        var contentRepo = new InMemoryContentRepository();
        contentRepo.Seed(MakeContent("s1", ContentType.Story, "Title", body: bodyWith200Words));

        var service = CreateService(contentRepo: contentRepo);
        var result = await service.GetHomeAsync(null);

        var story = Assert.Single(result.Featured.Stories);
        Assert.Equal(1, story.ReadingTimeMinutes);
    }

    // ── Community recipes section ─────────────────────────────────────────────

    [Fact]
    public async Task GetHomeAsync_CommunityRecipes_ReturnsMappedData()
    {
        var recipeRepo = new InMemoryHomeRecipeRepository();
        recipeRepo.Seed(MakeRecipe("r1", "Veggie Bowl", cuisineType: "Mediterranean", likeCount: 42));

        var service = CreateService(recipeRepo: recipeRepo);
        var result = await service.GetHomeAsync(null);

        var recipe = Assert.Single(result.Community.Recipes);
        Assert.Equal("r1", recipe.Id);
        Assert.Equal("Veggie Bowl", recipe.Title);
        Assert.Equal("Mediterranean", recipe.CuisineType);
        Assert.Equal(42, recipe.LikeCount);
    }

    [Fact]
    public async Task GetHomeAsync_PrivateCommunityRecipes_NotIncluded()
    {
        var recipeRepo = new InMemoryHomeRecipeRepository();
        recipeRepo.Seed(MakeRecipe("r1", "Secret Recipe", isPublic: false));

        var service = CreateService(recipeRepo: recipeRepo);
        var result = await service.GetHomeAsync(null);

        Assert.Empty(result.Community.Recipes);
    }

    // ── Preference filtering (intolerance exclusion) ───────────────────────────

    [Fact]
    public void ApplyIntoleranceFilter_NoIntolerances_ReturnsAll()
    {
        var items = new List<CommunityRecipeCacheItem>
        {
            new() { Id = "r1", Title = "Pasta", AuthorId = "a1", Tags = ["italian"] },
            new() { Id = "r2", Title = "Gluten Bread", AuthorId = "a1", Tags = ["gluten"] },
        };

        var result = HomeService.ApplyIntoleranceFilter(items, new UserPreferences());

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ApplyIntoleranceFilter_WithIntolerance_ExcludesMatchingRecipes()
    {
        var items = new List<CommunityRecipeCacheItem>
        {
            new() { Id = "r1", Title = "Pasta", AuthorId = "a1", Tags = ["italian"] },
            new() { Id = "r2", Title = "Gluten Bread", AuthorId = "a1", Tags = ["gluten", "baked"] },
        };

        var prefs = new UserPreferences { Intolerances = ["gluten"] };
        var result = HomeService.ApplyIntoleranceFilter(items, prefs);

        Assert.Single(result);
        Assert.Equal("r1", result[0].Id);
    }

    [Fact]
    public void ApplyIntoleranceFilter_CaseInsensitive_ExcludesMatchingRecipes()
    {
        var items = new List<CommunityRecipeCacheItem>
        {
            new() { Id = "r1", Title = "Dairy Dessert", AuthorId = "a1", Tags = ["DAIRY"] },
        };

        var prefs = new UserPreferences { Intolerances = ["dairy"] };
        var result = HomeService.ApplyIntoleranceFilter(items, prefs);

        Assert.Empty(result);
    }

    [Fact]
    public void ApplyIntoleranceFilter_MultipleIntolerances_ExcludesAllMatches()
    {
        var items = new List<CommunityRecipeCacheItem>
        {
            new() { Id = "r1", Title = "Gluten Loaf", AuthorId = "a1", Tags = ["gluten"] },
            new() { Id = "r2", Title = "Milk Shake", AuthorId = "a1", Tags = ["dairy"] },
            new() { Id = "r3", Title = "Salad", AuthorId = "a1", Tags = ["vegan"] },
        };

        var prefs = new UserPreferences { Intolerances = ["gluten", "dairy"] };
        var result = HomeService.ApplyIntoleranceFilter(items, prefs);

        Assert.Single(result);
        Assert.Equal("r3", result[0].Id);
    }

    [Fact]
    public async Task GetHomeAsync_WithIntolerances_FiltersCommunityRecipes()
    {
        var recipeRepo = new InMemoryHomeRecipeRepository();
        recipeRepo.Seed(MakeRecipe("r1", "Gluten Bread", tags: ["gluten", "baked"]));
        recipeRepo.Seed(MakeRecipe("r2", "Salad", tags: ["vegan"]));

        var prefsMock = new Mock<IPreferenceService>();
        prefsMock.Setup(p => p.GetUserPreferencesAsync("user1", default))
                 .ReturnsAsync(new UserPreferences { Intolerances = ["gluten"] });

        var service = CreateService(preferenceService: prefsMock.Object, recipeRepo: recipeRepo);
        var result = await service.GetHomeAsync("user1");

        Assert.Single(result.Community.Recipes);
        Assert.Equal("r2", result.Community.Recipes[0].Id);
    }

    // ── Recently viewed section ────────────────────────────────────────────────

    [Fact]
    public async Task GetHomeAsync_AuthenticatedUser_ReturnsRecentlyViewed()
    {
        var activityRepo = new InMemoryHomeActivityRepository();
        activityRepo.Seed(MakeActivity("user1", "recipe-abc"));

        var prefsMock = new Mock<IPreferenceService>();
        prefsMock.Setup(p => p.GetUserPreferencesAsync("user1", default))
                 .ReturnsAsync(new UserPreferences());

        var service = CreateService(preferenceService: prefsMock.Object, activityRepo: activityRepo);
        var result = await service.GetHomeAsync("user1");

        var viewed = Assert.Single(result.RecentlyViewed.Recipes);
        Assert.Equal("recipe-abc", viewed.RecipeId);
        Assert.Equal("Recipe", viewed.ReferenceType);
    }

    [Fact]
    public async Task GetHomeAsync_NoActivityRepo_ReturnsEmptyRecentlyViewed()
    {
        var prefsMock = new Mock<IPreferenceService>();
        prefsMock.Setup(p => p.GetUserPreferencesAsync("user1", default))
                 .ReturnsAsync(new UserPreferences());

        var service = CreateService(preferenceService: prefsMock.Object);
        var result = await service.GetHomeAsync("user1");

        Assert.Empty(result.RecentlyViewed.Recipes);
    }

    [Fact]
    public async Task GetHomeAsync_RecentlyViewed_OnlyViewedActivitiesReturned()
    {
        var activityRepo = new InMemoryHomeActivityRepository();
        activityRepo.Seed(MakeActivity("user1", "recipe-xyz", type: ActivityType.Liked));
        activityRepo.Seed(MakeActivity("user1", "recipe-abc", type: ActivityType.Viewed));

        var prefsMock = new Mock<IPreferenceService>();
        prefsMock.Setup(p => p.GetUserPreferencesAsync("user1", default))
                 .ReturnsAsync(new UserPreferences());

        var service = CreateService(preferenceService: prefsMock.Object, activityRepo: activityRepo);
        var result = await service.GetHomeAsync("user1");

        Assert.Single(result.RecentlyViewed.Recipes);
        Assert.Equal("recipe-abc", result.RecentlyViewed.Recipes[0].RecipeId);
    }

    // ── Caching ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetHomeAsync_FeaturedSection_ServedFromCacheOnSecondCall()
    {
        var contentRepo = new InMemoryContentRepository();
        contentRepo.Seed(MakeContent("c1", ContentType.FeaturedRecipe, "Cached Recipe"));

        var cacheMock = new Mock<ICacheService>();
        // First call: cache miss for featured
        cacheMock.SetupSequence(c => c.GetAsync<FeaturedSection>(It.IsAny<string>(), default))
                 .ReturnsAsync((FeaturedSection?)null)
                 .ReturnsAsync(new FeaturedSection { Recipes = [new FeaturedRecipeResult { Id = "cached", Title = "From Cache" }] });

        cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<FeaturedSection>(),
                    It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), default))
                 .Returns(Task.CompletedTask);

        cacheMock.Setup(c => c.GetAsync<IReadOnlyList<CommunityRecipeCacheItem>>(It.IsAny<string>(), default))
                 .ReturnsAsync((IReadOnlyList<CommunityRecipeCacheItem>?)null);

        cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<CommunityRecipeCacheItem>>(),
                    It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), default))
                 .Returns(Task.CompletedTask);

        var service = CreateService(contentRepo: contentRepo, cacheService: cacheMock.Object);

        // First call — should write to cache
        await service.GetHomeAsync(null);

        // Second call — should read from cache
        var result = await service.GetHomeAsync(null);

        Assert.Equal("cached", result.Featured.Recipes[0].Id);
        cacheMock.Verify(c => c.SetAsync("home:featured", It.IsAny<FeaturedSection>(),
            It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), default), Times.Once);
    }

    [Fact]
    public async Task GetHomeAsync_CommunitySection_ServedFromCacheOnSecondCall()
    {
        var cachedItems = new List<CommunityRecipeCacheItem>
        {
            new() { Id = "cached-r1", Title = "Cached Recipe", AuthorId = "a1", Tags = [] },
        };

        var cacheMock = new Mock<ICacheService>();
        cacheMock.Setup(c => c.GetAsync<FeaturedSection>(It.IsAny<string>(), default))
                 .ReturnsAsync((FeaturedSection?)null);
        cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<FeaturedSection>(),
                    It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), default))
                 .Returns(Task.CompletedTask);

        cacheMock.Setup(c => c.GetAsync<IReadOnlyList<CommunityRecipeCacheItem>>(It.IsAny<string>(), default))
                 .ReturnsAsync(cachedItems);

        var service = CreateService(cacheService: cacheMock.Object);
        var result = await service.GetHomeAsync(null);

        Assert.Single(result.Community.Recipes);
        Assert.Equal("cached-r1", result.Community.Recipes[0].Id);
    }
}

// ── In-memory repositories used by unit tests ──────────────────────────────────

internal sealed class InMemoryContentRepository : IRepository<Content>
{
    private readonly List<Content> _items = [];

    public void Seed(Content content) => _items.Add(content);

    public Task<Content?> GetByIdAsync(string id, string partitionKey, CancellationToken ct = default)
        => Task.FromResult(_items.FirstOrDefault(c => c.Id == id));

    public Task<IReadOnlyList<Content>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken ct = default)
    {
        IEnumerable<Content> results = _items;

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
        _items.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<Content> UpdateAsync(Content entity, string id, string partitionKey, CancellationToken ct = default)
    {
        var index = _items.FindIndex(c => c.Id == id);
        if (index >= 0)
        {
            _items[index] = entity;
        }

        return Task.FromResult(entity);
    }

    public Task<Content> PatchAsync(string id, string partitionKey, IReadOnlyDictionary<string, object?> patches, CancellationToken ct = default)
        => Task.FromResult(_items.First(c => c.Id == id));

    public Task DeleteAsync(string id, string partitionKey, CancellationToken ct = default)
    {
        _items.RemoveAll(c => c.Id == id);
        return Task.CompletedTask;
    }

    public Task<PagedResult<Content>> GetPagedAsync(string query, FeedPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<Content> { Items = [.. _items] });

    public Task<PagedResult<Content>> GetOffsetPagedAsync(string baseQuery, OffsetPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<Content> { Items = [.. _items] });

    public Task ExecuteTransactionalBatchAsync(string partitionKey, IEnumerable<(TransactionalBatchOperation Operation, Content Entity)> operations, CancellationToken ct = default)
        => Task.CompletedTask;
}

internal sealed class InMemoryHomeRecipeRepository : IRepository<Recipe>
{
    private readonly List<Recipe> _items = [];

    public void Seed(Recipe recipe) => _items.Add(recipe);

    public Task<Recipe?> GetByIdAsync(string id, string partitionKey, CancellationToken ct = default)
        => Task.FromResult(_items.FirstOrDefault(r => r.Id == id));

    public Task<IReadOnlyList<Recipe>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken ct = default)
    {
        IEnumerable<Recipe> results = _items;

        if (query.Contains("c.isPublic = true"))
        {
            results = results.Where(r => r.IsPublic);
        }

        return Task.FromResult<IReadOnlyList<Recipe>>([.. results.OrderByDescending(r => r.CreatedAt)]);
    }

    public Task<Recipe> CreateAsync(Recipe entity, CancellationToken ct = default)
    {
        _items.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<Recipe> UpdateAsync(Recipe entity, string id, string partitionKey, CancellationToken ct = default)
    {
        var index = _items.FindIndex(r => r.Id == id);
        if (index >= 0)
        {
            _items[index] = entity;
        }

        return Task.FromResult(entity);
    }

    public Task<Recipe> PatchAsync(string id, string partitionKey, IReadOnlyDictionary<string, object?> patches, CancellationToken ct = default)
        => Task.FromResult(_items.First(r => r.Id == id));

    public Task DeleteAsync(string id, string partitionKey, CancellationToken ct = default)
    {
        _items.RemoveAll(r => r.Id == id);
        return Task.CompletedTask;
    }

    public Task<PagedResult<Recipe>> GetPagedAsync(string query, FeedPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<Recipe> { Items = [.. _items] });

    public Task<PagedResult<Recipe>> GetOffsetPagedAsync(string baseQuery, OffsetPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<Recipe> { Items = [.. _items] });

    public Task ExecuteTransactionalBatchAsync(string partitionKey, IEnumerable<(TransactionalBatchOperation Operation, Recipe Entity)> operations, CancellationToken ct = default)
        => Task.CompletedTask;
}

internal sealed class InMemoryHomeActivityRepository : IRepository<Activity>
{
    private readonly List<Activity> _items = [];

    public void Seed(Activity activity) => _items.Add(activity);

    public Task<Activity?> GetByIdAsync(string id, string partitionKey, CancellationToken ct = default)
        => Task.FromResult(_items.FirstOrDefault(a => a.Id == id));

    public Task<IReadOnlyList<Activity>> GetByQueryAsync(string query, string? partitionKey = null, CancellationToken ct = default)
    {
        IEnumerable<Activity> results = _items;

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
        _items.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<Activity> UpdateAsync(Activity entity, string id, string partitionKey, CancellationToken ct = default)
    {
        var index = _items.FindIndex(a => a.Id == id);
        if (index >= 0)
        {
            _items[index] = entity;
        }

        return Task.FromResult(entity);
    }

    public Task<Activity> PatchAsync(string id, string partitionKey, IReadOnlyDictionary<string, object?> patches, CancellationToken ct = default)
        => Task.FromResult(_items.First(a => a.Id == id));

    public Task DeleteAsync(string id, string partitionKey, CancellationToken ct = default)
    {
        _items.RemoveAll(a => a.Id == id);
        return Task.CompletedTask;
    }

    public Task<PagedResult<Activity>> GetPagedAsync(string query, FeedPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<Activity> { Items = [.. _items] });

    public Task<PagedResult<Activity>> GetOffsetPagedAsync(string baseQuery, OffsetPaginationOptions options, string? partitionKey = null, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<Activity> { Items = [.. _items] });

    public Task ExecuteTransactionalBatchAsync(string partitionKey, IEnumerable<(TransactionalBatchOperation Operation, Activity Entity)> operations, CancellationToken ct = default)
        => Task.CompletedTask;
}
