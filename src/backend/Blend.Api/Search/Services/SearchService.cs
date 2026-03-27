using Blend.Api.Preferences.Services;
using Blend.Api.Search.Models;
using Blend.Api.Services.Spoonacular;
using Blend.Api.Services.Spoonacular.Models;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Blend.Api.Search.Services;

/// <summary>
/// Implements unified recipe search by merging results from Spoonacular and the internal recipe store.
/// Applies user preferences, partial matching, ranking, and quota-exhaustion fallback per the
/// Explore &amp; Search FRD (EXPL-08 through EXPL-19, EXPL-34, EXPL-35) and ADR 0006/0009.
/// </summary>
public sealed class SearchService : ISearchService
{
    private readonly ISpoonacularService? _spoonacularService;
    private readonly IPreferenceService _preferenceService;
    private readonly IRepository<Recipe>? _recipeRepository;
    private readonly IRepository<Activity>? _activityRepository;
    private readonly ILogger<SearchService> _logger;

    // Maximum Spoonacular results fetched per search (kept moderate to stay within quota).
    private const int SpoonacularResultCount = 20;

    // Maximum internal results fetched before pagination is applied.
    private const int InternalResultLimit = 50;

    public SearchService(
        ILogger<SearchService> logger,
        IPreferenceService preferenceService,
        ISpoonacularService? spoonacularService = null,
        IRepository<Recipe>? recipeRepository = null,
        IRepository<Activity>? activityRepository = null)
    {
        _logger = logger;
        _preferenceService = preferenceService;
        _spoonacularService = spoonacularService;
        _recipeRepository = recipeRepository;
        _activityRepository = activityRepository;
    }

    /// <inheritdoc />
    public async Task<UnifiedSearchResponse> SearchRecipesAsync(
        SearchRecipesRequest request,
        string? userId,
        CancellationToken ct = default)
    {
        var clampedPageSize = Math.Clamp(request.PageSize, 1, 50);

        // ── Decode cursor ──────────────────────────────────────────────────────
        var cursorOffset = DecodeCursor(request.Cursor);

        // ── Fetch user preferences (if authenticated) ──────────────────────────
        var preferences = userId is not null
            ? await _preferenceService.GetUserPreferencesAsync(userId, ct)
            : new UserPreferences();

        // ── Launch parallel queries ────────────────────────────────────────────
        var spoonacularTask = FetchSpoonacularResultsAsync(request, preferences, ct);
        var internalTask = FetchInternalResultsAsync(request, ct);

        await Task.WhenAll(spoonacularTask, internalTask);

        var (spoonacularResults, quotaExhausted, spoonacularUnavailable) = await spoonacularTask;
        var internalResults = await internalTask;

        // Degraded mode = Spoonacular is unavailable for any reason (quota, service down, not configured)
        var degradedMode = spoonacularUnavailable || _spoonacularService is null;

        // ── Map to unified results ─────────────────────────────────────────────
        var unified = new List<UnifiedRecipeResult>(spoonacularResults.Count + internalResults.Count);

        unified.AddRange(spoonacularResults);
        unified.AddRange(internalResults);

        // ── Rank results ───────────────────────────────────────────────────────
        var ranked = RankResults(unified, request.Q, request.Sort, preferences);

        // ── Paginate ───────────────────────────────────────────────────────────
        var totalResults = ranked.Count;
        var paged = ranked.Skip(cursorOffset).Take(clampedPageSize).ToList();

        var nextOffset = cursorOffset + paged.Count;
        var nextCursor = nextOffset < totalResults ? EncodeCursor(nextOffset) : null;

        if (degradedMode)
        {
            _logger.LogWarning(
                "Search is running in degraded mode (Spoonacular unavailable). Returning internal results only.");
        }

        return new UnifiedSearchResponse
        {
            Results = paged,
            Metadata = new SearchResponseMetadata
            {
                TotalResults = totalResults,
                QuotaExhausted = quotaExhausted,
                DegradedMode = degradedMode,
                NextCursor = nextCursor,
            },
        };
    }

    /// <inheritdoc />
    public async Task RecordViewAsync(
        string recipeId,
        RecipeDataSource dataSource,
        string userId,
        CancellationToken ct = default)
    {
        if (_activityRepository is null)
        {
            _logger.LogWarning("Activity repository unavailable; cannot record view for recipe {RecipeId}.", recipeId);
            return;
        }

        var activity = new Activity
        {
            Id = $"{userId}:Viewed:{recipeId}",
            UserId = userId,
            Type = ActivityType.Viewed,
            ReferenceId = recipeId,
            ReferenceType = dataSource == RecipeDataSource.Community ? "Recipe" : "SpoonacularRecipe",
            Timestamp = DateTimeOffset.UtcNow,
        };

        try
        {
            await _activityRepository.CreateAsync(activity, ct);
        }
        catch (Exception ex)
        {
            // Non-fatal — log but do not propagate.
            _logger.LogWarning(ex, "Failed to record view activity for recipe {RecipeId} by user {UserId}.", recipeId, userId);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Activity>> GetRecentlyViewedAsync(
        string userId,
        int pageSize,
        CancellationToken ct = default)
    {
        if (_activityRepository is null)
        {
            _logger.LogWarning("Activity repository unavailable; cannot retrieve recently viewed for user {UserId}.", userId);
            return [];
        }

        var clampedSize = Math.Clamp(pageSize, 1, 50);

        var query =
            $"SELECT TOP {clampedSize} * FROM c " +
            "WHERE c.userId = @userId " +
            "AND c.type = 'Viewed' " +
            "ORDER BY c.timestamp DESC";

        var parameters = new Dictionary<string, object> { ["@userId"] = userId };

        return await _activityRepository.GetByQueryAsync(query, parameters, partitionKey: userId, ct);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Fetches results from Spoonacular, applying user preferences.
    /// Returns:
    /// <list type="bullet">
    ///   <item><description><c>Results</c> — recipe results (empty when unavailable).</description></item>
    ///   <item><description><c>QuotaExhausted</c> — true when the API quota is rate-limited.</description></item>
    ///   <item><description><c>Unavailable</c> — true when Spoonacular is completely unavailable (service down or quota exhausted with no cache).</description></item>
    /// </list>
    /// </summary>
    private async Task<(IReadOnlyList<UnifiedRecipeResult> Results, bool QuotaExhausted, bool Unavailable)> FetchSpoonacularResultsAsync(
        SearchRecipesRequest request,
        UserPreferences preferences,
        CancellationToken ct)
    {
        if (_spoonacularService is null)
        {
            return ([], false, true);
        }

        var baseFilters = new ComplexSearchFilters
        {
            Cuisine = request.Cuisines,
            Diet = request.Diets,
            Intolerances = null,
            MaxReadyTime = request.MaxReadyTime,
            Number = SpoonacularResultCount,
        };

        var filters = _preferenceService.ApplyPreferencesToSearch(baseFilters, preferences);

        var spoonResult = await _spoonacularService.ComplexSearchAsync(
            request.Q ?? string.Empty,
            filters,
            ct);

        if (!spoonResult.IsAvailable)
        {
            return ([], spoonResult.IsLimited, true);
        }

        var results = (spoonResult.Data ?? [])
            .Select(s => MapSpoonacularSummary(s))
            .ToList();

        return (results, spoonResult.IsLimited, false);
    }

    /// <summary>
    /// Builds and executes a Cosmos DB query against the internal recipes container,
    /// supporting partial ingredient matching and multi-word tokenisation (EXPL-15).
    /// </summary>
    private async Task<IReadOnlyList<UnifiedRecipeResult>> FetchInternalResultsAsync(
        SearchRecipesRequest request,
        CancellationToken ct)
    {
        if (_recipeRepository is null)
        {
            return [];
        }

        var conditions = new List<string> { "c.isPublic = true" };
        var parameters = new Dictionary<string, object>();
        var paramIndex = 0;

        // ── Text search with partial matching (EXPL-15) ────────────────────────
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var tokens = Tokenise(request.Q);
            var tokenClauses = new List<string>();
            foreach (var token in tokens)
            {
                var paramName = $"@token{paramIndex++}";
                parameters[paramName] = token.ToLowerInvariant();
                tokenClauses.Add(
                    $"(CONTAINS(LOWER(c.title), {paramName}, true) " +
                    $"OR CONTAINS(LOWER(c.description), {paramName}, true) " +
                    $"OR EXISTS(SELECT VALUE i FROM i IN c.ingredients " +
                    $"WHERE CONTAINS(LOWER(i.ingredientName), {paramName}, true)))");
            }
            conditions.Add($"({string.Join(" OR ", tokenClauses)})");
        }

        // ── Cuisine filter ─────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(request.Cuisines))
        {
            var cuisineList = ParseCommaSeparated(request.Cuisines);
            if (cuisineList.Count > 0)
            {
                var cuisineClauses = new List<string>();
                foreach (var cuisine in cuisineList)
                {
                    var paramName = $"@cuisine{paramIndex++}";
                    parameters[paramName] = cuisine.ToLowerInvariant();
                    cuisineClauses.Add($"LOWER(c.cuisineType) = {paramName}");
                }
                conditions.Add($"({string.Join(" OR ", cuisineClauses)})");
            }
        }

        // ── Diet filter (match against tags) ──────────────────────────────────
        if (!string.IsNullOrWhiteSpace(request.Diets))
        {
            var dietList = ParseCommaSeparated(request.Diets);
            if (dietList.Count > 0)
            {
                var dietClauses = new List<string>();
                foreach (var diet in dietList)
                {
                    var paramName = $"@diet{paramIndex++}";
                    parameters[paramName] = diet.ToLowerInvariant();
                    dietClauses.Add($"ARRAY_CONTAINS(c.tags, {paramName}, true)");
                }
                conditions.Add($"({string.Join(" OR ", dietClauses)})");
            }
        }

        // ── Dish type filter ───────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(request.DishTypes))
        {
            var dishList = ParseCommaSeparated(request.DishTypes);
            if (dishList.Count > 0)
            {
                var dishClauses = new List<string>();
                foreach (var dish in dishList)
                {
                    var paramName = $"@dish{paramIndex++}";
                    parameters[paramName] = dish.ToLowerInvariant();
                    dishClauses.Add($"LOWER(c.dishType) = {paramName}");
                }
                conditions.Add($"({string.Join(" OR ", dishClauses)})");
            }
        }

        // ── Max ready time filter ──────────────────────────────────────────────
        if (request.MaxReadyTime.HasValue)
        {
            parameters["@maxReadyTime"] = request.MaxReadyTime.Value;
            conditions.Add("(c.prepTime + c.cookTime) <= @maxReadyTime");
        }

        var whereClause = string.Join(" AND ", conditions);
        var query = $"SELECT TOP {InternalResultLimit} * FROM c WHERE {whereClause}";

        IReadOnlyList<Recipe> recipes;
        try
        {
            recipes = await _recipeRepository.GetByQueryAsync(query, parameters, partitionKey: null, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Internal recipe query failed; returning empty internal results.");
            return [];
        }

        return recipes.Select(MapInternalRecipe).ToList();
    }

    /// <summary>
    /// Ranks a merged list of results by relevance, preference alignment, and popularity.
    /// The ranking is deterministic given the same inputs (EXPL-34, EXPL-35).
    /// </summary>
    public static IReadOnlyList<UnifiedRecipeResult> RankResults(
        IReadOnlyList<UnifiedRecipeResult> results,
        string? query,
        string sort,
        UserPreferences preferences)
    {
        var tokens = string.IsNullOrWhiteSpace(query) ? [] : Tokenise(query);

        var scored = results.Select(r =>
        {
            var score = ComputeScore(r, tokens, preferences);
            return new UnifiedRecipeResult
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                ImageUrl = r.ImageUrl,
                ReadyInMinutes = r.ReadyInMinutes,
                Servings = r.Servings,
                Cuisines = r.Cuisines,
                DishTypes = r.DishTypes,
                Popularity = r.Popularity,
                DataSource = r.DataSource,
                CreatedAt = r.CreatedAt,
                Score = score,
            };
        }).ToList();

        return sort switch
        {
            "popularity" => scored.OrderByDescending(r => r.Popularity).ThenByDescending(r => r.Score).ToList(),
            "time" => scored
                .OrderBy(r => r.ReadyInMinutes ?? int.MaxValue)
                .ThenByDescending(r => r.Score)
                .ToList(),
            "newest" => scored
                .OrderByDescending(r => r.CreatedAt ?? DateTimeOffset.MinValue)
                .ThenByDescending(r => r.Score)
                .ToList(),
            _ => scored.OrderByDescending(r => r.Score).ThenByDescending(r => r.Popularity).ToList(),
        };
    }

    /// <summary>
    /// Computes a deterministic ranking score for a single result.
    /// Higher is better.
    /// </summary>
    public static double ComputeScore(
        UnifiedRecipeResult result,
        IReadOnlyList<string> queryTokens,
        UserPreferences preferences)
    {
        double score = 0;

        // ── 1. Relevance to query tokens ──────────────────────────────────────
        if (queryTokens.Count > 0)
        {
            var titleLower = result.Title.ToLowerInvariant();
            var descriptionLower = result.Description?.ToLowerInvariant() ?? string.Empty;

            foreach (var token in queryTokens)
            {
                if (titleLower.Contains(token, StringComparison.Ordinal))
                {
                    score += 3;
                }
                else if (descriptionLower.Contains(token, StringComparison.Ordinal))
                {
                    score += 1;
                }
            }
        }

        // ── 2. Preference alignment ───────────────────────────────────────────
        // Boost results matching the user's favourite cuisines.
        foreach (var cuisine in preferences.FavoriteCuisines)
        {
            if (result.Cuisines.Any(c => string.Equals(c, cuisine, StringComparison.OrdinalIgnoreCase)))
            {
                score += 2;
                break;
            }
        }

        // ── 3. Popularity ─────────────────────────────────────────────────────
        // Normalise popularity to a 0–1 range using logarithm to avoid huge disparities.
        if (result.Popularity > 0)
        {
            score += Math.Log(result.Popularity + 1) * 0.5;
        }

        return score;
    }

    /// <summary>
    /// Tokenises a free-text query into lower-case terms for partial matching (EXPL-15).
    /// </summary>
    public static IReadOnlyList<string> Tokenise(string query) =>
        query
            .ToLowerInvariant()
            .Split([' ', ',', ';', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => t.Length >= 2)
            .Distinct()
            .ToList();

    /// <summary>
    /// Parses a comma-separated string into a trimmed list of non-empty values.
    /// </summary>
    public static IReadOnlyList<string> ParseCommaSeparated(string value) =>
        value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();

    private static UnifiedRecipeResult MapSpoonacularSummary(RecipeSummary summary) => new()
    {
        Id = summary.SpoonacularId.ToString(),
        Title = summary.Title,
        Description = null,
        ImageUrl = summary.ImageUrl,
        ReadyInMinutes = summary.ReadyInMinutes,
        Servings = summary.Servings,
        Cuisines = summary.Cuisines,
        DishTypes = summary.DishTypes,
        Popularity = summary.Likes ?? 0,
        DataSource = RecipeDataSource.Spoonacular,
        CreatedAt = null,
        Score = 0,
    };

    private static UnifiedRecipeResult MapInternalRecipe(Recipe recipe) => new()
    {
        Id = recipe.Id,
        Title = recipe.Title,
        Description = recipe.Description,
        ImageUrl = recipe.FeaturedPhotoUrl,
        ReadyInMinutes = recipe.PrepTime + recipe.CookTime,
        Servings = recipe.Servings,
        Cuisines = recipe.CuisineType is not null ? [recipe.CuisineType] : [],
        DishTypes = recipe.DishType is not null ? [recipe.DishType] : [],
        Popularity = recipe.LikeCount,
        DataSource = RecipeDataSource.Community,
        CreatedAt = recipe.CreatedAt,
        Score = 0,
    };

    /// <summary>Encodes a page-offset integer into a URL-safe Base64 cursor string.</summary>
    private static string EncodeCursor(int offset) =>
        Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(offset.ToString()));

    /// <summary>Decodes a cursor string back to a page offset. Returns 0 for null/invalid cursors.</summary>
    private static int DecodeCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return 0;
        }

        try
        {
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            return int.TryParse(decoded, out var offset) ? Math.Max(0, offset) : 0;
        }
        catch
        {
            return 0;
        }
    }
}
