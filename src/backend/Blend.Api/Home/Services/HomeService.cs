using Blend.Api.Home.Models;
using Blend.Api.Preferences.Services;
using Blend.Api.Services.Cache;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Blend.Api.Home.Services;

/// <summary>
/// Aggregates featured content, community recipes, and recently-viewed history for the home page
/// (HOME-01 through HOME-24). Parallel data fetching per ADR 0006; L1/L2 caching per ADR 0009.
/// </summary>
public sealed class HomeService : IHomeService
{
    private readonly IRepository<Content>? _contentRepository;
    private readonly IRepository<Recipe>? _recipeRepository;
    private readonly IRepository<Activity>? _activityRepository;
    private readonly IRepository<Connection>? _connectionRepository;
    private readonly IPreferenceService _preferenceService;
    private readonly ICacheService? _cacheService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HomeService> _logger;

    // Placeholder prompts used when none are configured.
    private static readonly string[] DefaultPlaceholders =
    [
        "Search for pasta...",
        "Try chicken and garlic...",
        "Find recipes with avocado...",
        "Explore salmon dishes...",
    ];

    // Featured content: changes infrequently — aggressive cache.
    private static readonly TimeSpan FeaturedL1Ttl = TimeSpan.FromHours(1);
    private static readonly TimeSpan FeaturedL2Ttl = TimeSpan.FromHours(24);

    // Community recipes: short cache TTL (HOME-16).
    private static readonly TimeSpan CommunityL1Ttl = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan CommunityL2Ttl = TimeSpan.FromMinutes(5);

    private const string FeaturedCacheKey = "home:featured";
    private const string CommunityCacheKey = "home:community";
    private const int CommunityRecipeLimit = 10;
    private const int RecentlyViewedLimit = 10;
    private const int WordsPerMinute = 200;

    public HomeService(
        ILogger<HomeService> logger,
        IPreferenceService preferenceService,
        IConfiguration configuration,
        IRepository<Content>? contentRepository = null,
        IRepository<Recipe>? recipeRepository = null,
        IRepository<Activity>? activityRepository = null,
        IRepository<Connection>? connectionRepository = null,
        ICacheService? cacheService = null)
    {
        _logger = logger;
        _preferenceService = preferenceService;
        _configuration = configuration;
        _contentRepository = contentRepository;
        _recipeRepository = recipeRepository;
        _activityRepository = activityRepository;
        _connectionRepository = connectionRepository;
        _cacheService = cacheService;
    }

    /// <inheritdoc />
    public async Task<HomeResponse> GetHomeAsync(string? userId, CancellationToken ct = default)
    {
        var placeholder = GetSearchPlaceholder();

        // Preferences are needed to filter community recipes; fetch upfront for authenticated users.
        var preferences = userId is not null
            ? await _preferenceService.GetUserPreferencesAsync(userId, ct)
            : new UserPreferences();

        // Launch all sections in parallel (ADR 0006 aggregation pattern).
        var featuredTask = GetFeaturedSectionAsync(ct);
        var communityTask = GetCommunitySectionAsync(userId, ct);
        var recentlyViewedTask = userId is not null
            ? GetRecentlyViewedSectionAsync(userId, ct)
            : Task.FromResult(new RecentlyViewedSection { Recipes = [] });

        await Task.WhenAll(featuredTask, communityTask, recentlyViewedTask);

        var featured = await featuredTask;
        var communityItems = await communityTask;

        // Apply intolerance filter in-memory against the cached/fetched items.
        var filteredRecipes = ApplyIntoleranceFilter(communityItems, preferences);
        var community = new CommunitySection { Recipes = filteredRecipes };

        var recentlyViewed = await recentlyViewedTask;

        return new HomeResponse
        {
            Search = new SearchSection { Placeholder = placeholder },
            Featured = featured,
            Community = community,
            RecentlyViewed = recentlyViewed,
        };
    }

    // ── Search placeholder ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns a placeholder string that rotates on a per-minute clock tick (HOME-01 through HOME-04).
    /// Values come from <c>Home:SearchPlaceholders</c> config; falls back to built-in defaults.
    /// </summary>
    private string GetSearchPlaceholder()
    {
        var configured = _configuration.GetSection("Home:SearchPlaceholders").Get<string[]>();
        var placeholders = configured is { Length: > 0 } ? configured : DefaultPlaceholders;
        var index = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 60) % placeholders.Length;
        return placeholders[index];
    }

    // ── Featured section ───────────────────────────────────────────────────────

    private async Task<FeaturedSection> GetFeaturedSectionAsync(CancellationToken ct)
    {
        if (_cacheService is not null)
        {
            var cached = await _cacheService.GetAsync<FeaturedSection>(FeaturedCacheKey, ct);
            if (cached is not null)
            {
                return cached;
            }
        }

        if (_contentRepository is null)
        {
            return new FeaturedSection { Recipes = [], Stories = [], Videos = [] };
        }

        // Fetch all three content types in parallel.
        var recipesTask = FetchPublishedContentAsync(ContentType.FeaturedRecipe, ct);
        var storiesTask = FetchPublishedContentAsync(ContentType.Story, ct);
        var videosTask = FetchPublishedContentAsync(ContentType.Video, ct);

        await Task.WhenAll(recipesTask, storiesTask, videosTask);

        var section = new FeaturedSection
        {
            Recipes = (await recipesTask).Select(MapToFeaturedRecipe).ToList(),
            Stories = (await storiesTask).Select(MapToFeaturedStory).ToList(),
            Videos = (await videosTask).Select(MapToFeaturedVideo).ToList(),
        };

        if (_cacheService is not null)
        {
            await _cacheService.SetAsync(FeaturedCacheKey, section, FeaturedL1Ttl, FeaturedL2Ttl, ct);
        }

        return section;
    }

    private async Task<IReadOnlyList<Content>> FetchPublishedContentAsync(ContentType type, CancellationToken ct)
    {
        var typeValue = type.ToString();
        var query = "SELECT * FROM c WHERE c.isPublished = true";

        try
        {
            return await _contentRepository!.GetByQueryAsync(query, partitionKey: typeValue, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch content of type {ContentType}; returning empty list.", type);
            return [];
        }
    }

    // ── Community recipes section ──────────────────────────────────────────────

    private async Task<IReadOnlyList<CommunityRecipeCacheItem>> GetCommunitySectionAsync(string? userId, CancellationToken ct)
    {
        // Try friends-specific cache first, then fall back to global cache.
        var cacheKey = userId is not null ? $"home:community:{userId}" : CommunityCacheKey;

        if (_cacheService is not null)
        {
            var cached = await _cacheService.GetAsync<IReadOnlyList<CommunityRecipeCacheItem>>(cacheKey, ct);
            if (cached is not null)
            {
                return cached;
            }
        }

        if (_recipeRepository is null)
        {
            return [];
        }

        // When the user has friends, prioritize their recipes.
        var friendIds = userId is not null ? await GetFriendIdsAsync(userId, ct) : [];

        IReadOnlyList<Recipe> recipes;
        try
        {
            if (friendIds.Count > 0)
            {
                // Fetch friends' public recipes first, then backfill with other public recipes.
                var friendRecipes = await FetchFriendsRecipesAsync(friendIds, ct);
                if (friendRecipes.Count >= CommunityRecipeLimit)
                {
                    recipes = friendRecipes.Take(CommunityRecipeLimit).ToList();
                }
                else
                {
                    // Backfill with general public recipes (excluding friends to avoid duplicates).
                    var friendIdSet = new HashSet<string>(friendIds);
                    var generalQuery =
                        $"SELECT TOP {CommunityRecipeLimit} * FROM c " +
                        "WHERE c.isPublic = true " +
                        "ORDER BY c.createdAt DESC";
                    var general = await _recipeRepository.GetByQueryAsync(generalQuery, partitionKey: null, ct);
                    var backfill = general.Where(r => !friendIdSet.Contains(r.AuthorId));
                    recipes = friendRecipes.Concat(backfill).Take(CommunityRecipeLimit).ToList();
                }
            }
            else
            {
                var query =
                    $"SELECT TOP {CommunityRecipeLimit} * FROM c " +
                    "WHERE c.isPublic = true " +
                    "ORDER BY c.createdAt DESC";
                recipes = await _recipeRepository.GetByQueryAsync(query, partitionKey: null, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch community recipes; returning empty list.");
            return [];
        }

        var items = recipes.Select(MapToCommunityRecipeCacheItem).ToList();

        if (_cacheService is not null)
        {
            await _cacheService.SetAsync<IReadOnlyList<CommunityRecipeCacheItem>>(
                cacheKey, items, CommunityL1Ttl, CommunityL2Ttl, ct);
        }

        return items;
    }

    /// <summary>Returns accepted friend user IDs for the given user.</summary>
    private async Task<IReadOnlyList<string>> GetFriendIdsAsync(string userId, CancellationToken ct)
    {
        if (_connectionRepository is null)
        {
            return [];
        }

        try
        {
            var query = "SELECT c.friendUserId FROM c WHERE c.status = 'Accepted'";
            var connections = await _connectionRepository.GetByQueryAsync(query, partitionKey: userId, ct);
            return connections.Select(c => c.FriendUserId).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch friend IDs for user {UserId}; falling back to general community.", userId);
            return [];
        }
    }

    /// <summary>Fetches recent public recipes from the given friend author IDs.</summary>
    private async Task<IReadOnlyList<Recipe>> FetchFriendsRecipesAsync(IReadOnlyList<string> friendIds, CancellationToken ct)
    {
        // Cap fan-out to avoid unbounded parallel Cosmos queries.
        var tasks = friendIds.Take(20).Select(async friendId =>
        {
            var query = $"SELECT TOP {CommunityRecipeLimit} * FROM c WHERE c.isPublic = true ORDER BY c.createdAt DESC";
            return await _recipeRepository!.GetByQueryAsync(query, partitionKey: friendId, ct);
        });

        var results = await Task.WhenAll(tasks);
        return results
            .SelectMany(r => r)
            .OrderByDescending(r => r.CreatedAt)
            .Take(CommunityRecipeLimit)
            .ToList();
    }

    // ── Recently viewed section ────────────────────────────────────────────────

    private async Task<RecentlyViewedSection> GetRecentlyViewedSectionAsync(string userId, CancellationToken ct)
    {
        if (_activityRepository is null)
        {
            _logger.LogWarning("Activity repository unavailable; cannot retrieve recently viewed for user {UserId}.", userId);
            return new RecentlyViewedSection { Recipes = [] };
        }

        var query =
            $"SELECT TOP {RecentlyViewedLimit} * FROM c " +
            "WHERE c.userId = @userId " +
            "AND c.type = 'Viewed' " +
            "ORDER BY c.timestamp DESC";

        var parameters = new Dictionary<string, object> { ["@userId"] = userId };

        IReadOnlyList<Activity> activities;
        try
        {
            activities = await _activityRepository.GetByQueryAsync(query, parameters, partitionKey: userId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch recently viewed for user {UserId}; returning empty list.", userId);
            return new RecentlyViewedSection { Recipes = [] };
        }

        var results = activities
            .Select(a => new RecentlyViewedRecipeResult
            {
                RecipeId = a.ReferenceId,
                ReferenceType = a.ReferenceType,
                ViewedAt = a.Timestamp,
            })
            .ToList();

        return new RecentlyViewedSection { Recipes = results };
    }

    // ── Preference filtering ───────────────────────────────────────────────────

    /// <summary>
    /// Removes community recipe items whose tags contain any of the user's intolerances
    /// (case-insensitive string match against recipe tags).
    /// </summary>
    public static IReadOnlyList<CommunityRecipeResult> ApplyIntoleranceFilter(
        IReadOnlyList<CommunityRecipeCacheItem> items,
        UserPreferences preferences)
    {
        if (preferences.Intolerances.Count == 0)
        {
            return items.Select(i => i.ToResult()).ToList();
        }

        return items
            .Where(item => !preferences.Intolerances.Any(intolerance =>
                item.Tags.Any(tag =>
                    string.Equals(tag, intolerance, StringComparison.OrdinalIgnoreCase))))
            .Select(i => i.ToResult())
            .ToList();
    }

    // ── Mapping helpers ────────────────────────────────────────────────────────

    private static FeaturedRecipeResult MapToFeaturedRecipe(Content content) => new()
    {
        Id = content.Id,
        Title = content.Title,
        ImageUrl = content.ThumbnailUrl,
        Attribution = content.AuthorName,
        ShortDescription = content.Body is { Length: > 200 }
            ? content.Body[..200] + "..."
            : content.Body,
    };

    private static FeaturedStoryResult MapToFeaturedStory(Content content) => new()
    {
        Id = content.Id,
        Title = content.Title,
        CoverImageUrl = content.ThumbnailUrl,
        Author = content.AuthorName,
        Excerpt = content.Body is { Length: > 300 }
            ? content.Body[..300] + "..."
            : content.Body,
        ReadingTimeMinutes = EstimateReadingTime(content.Body),
    };

    private static FeaturedVideoResult MapToFeaturedVideo(Content content) => new()
    {
        Id = content.Id,
        Title = content.Title,
        ThumbnailUrl = content.ThumbnailUrl,
        VideoUrl = content.MediaUrl,
        Creator = content.AuthorName,
    };

    private static CommunityRecipeCacheItem MapToCommunityRecipeCacheItem(Recipe recipe) => new()
    {
        Id = recipe.Id,
        Title = recipe.Title,
        ImageUrl = recipe.FeaturedPhotoUrl,
        AuthorId = recipe.AuthorId,
        CuisineType = recipe.CuisineType,
        LikeCount = recipe.LikeCount,
        Tags = recipe.Tags,
    };

    private static int? EstimateReadingTime(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        var wordCount = body.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, (int)Math.Ceiling((double)wordCount / WordsPerMinute));
    }
}

/// <summary>
/// Internal cache item that retains recipe tags so that intolerance filtering can be applied
/// in-memory after a cache hit without a round-trip to the database.
/// </summary>
public sealed class CommunityRecipeCacheItem
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public string? ImageUrl { get; init; }
    public required string AuthorId { get; init; }
    public string? CuisineType { get; init; }
    public int LikeCount { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>Projects this cache item to the public DTO.</summary>
    public CommunityRecipeResult ToResult() => new()
    {
        Id = Id,
        Title = Title,
        ImageUrl = ImageUrl,
        AuthorId = AuthorId,
        CuisineType = CuisineType,
        LikeCount = LikeCount,
    };
}
