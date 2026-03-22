using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Blend.Api.Services.Cache;
using Blend.Api.Services.Spoonacular.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blend.Api.Services.Spoonacular;

/// <summary>
/// Cache-aware implementation of <see cref="ISpoonacularService"/> (per ADR 0009).
/// <list type="bullet">
///   <item><description>Always checks the two-tier cache before calling the external API.</description></item>
///   <item><description>Tracks the <c>X-API-Quota-Used</c> header to enforce rate-limit thresholds.</description></item>
///   <item><description>Degrades gracefully when Spoonacular is unavailable or quota is exhausted.</description></item>
/// </list>
/// </summary>
public sealed class SpoonacularService : ISpoonacularService
{
    private const string QuotaUsedHeader = "X-API-Quota-Used";
    private const string HttpClientName = "Spoonacular";

    // TTLs per ADR 0009
    private static readonly TimeSpan SearchL1Ttl = TimeSpan.FromHours(1);
    private static readonly TimeSpan SearchL2Ttl = TimeSpan.FromHours(24);
    private static readonly TimeSpan RecipeL1Ttl = TimeSpan.FromHours(2);
    private static readonly TimeSpan RecipeL2Ttl = TimeSpan.FromDays(7);
    private static readonly TimeSpan SubstituteL1Ttl = TimeSpan.FromHours(4);
    private static readonly TimeSpan SubstituteL2Ttl = TimeSpan.FromDays(30);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICacheService _cache;
    private readonly SpoonacularQuotaMonitor _quotaMonitor;
    private readonly SpoonacularOptions _options;
    private readonly ILogger<SpoonacularService> _logger;

    public SpoonacularService(
        IHttpClientFactory httpClientFactory,
        ICacheService cache,
        SpoonacularQuotaMonitor quotaMonitor,
        IOptions<SpoonacularOptions> options,
        ILogger<SpoonacularService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _quotaMonitor = quotaMonitor;
        _options = options.Value;
        _logger = logger;
    }

    // ── ISpoonacularService ────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<SpoonacularResult<IReadOnlyList<RecipeSummary>>> SearchByIngredientsAsync(
        IReadOnlyList<string> ingredients,
        SearchByIngredientsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var key = SpoonacularCacheKeys.ForSearchByIngredients(ingredients, options);
        return await ExecuteWithCacheAsync<IReadOnlyList<RecipeSummary>>(
            key,
            SearchL1Ttl,
            SearchL2Ttl,
            () => FetchSearchByIngredientsAsync(ingredients, options, cancellationToken),
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<SpoonacularResult<IReadOnlyList<RecipeSummary>>> ComplexSearchAsync(
        string query,
        ComplexSearchFilters? filters = null,
        CancellationToken cancellationToken = default)
    {
        var key = SpoonacularCacheKeys.ForComplexSearch(query, filters);
        return await ExecuteWithCacheAsync<IReadOnlyList<RecipeSummary>>(
            key,
            SearchL1Ttl,
            SearchL2Ttl,
            () => FetchComplexSearchAsync(query, filters, cancellationToken),
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<SpoonacularResult<RecipeDetail>> GetRecipeInformationAsync(
        int recipeId,
        CancellationToken cancellationToken = default)
    {
        var key = SpoonacularCacheKeys.ForRecipe(recipeId);
        return await ExecuteWithCacheAsync<RecipeDetail>(
            key,
            RecipeL1Ttl,
            RecipeL2Ttl,
            () => FetchRecipeInformationAsync(recipeId, cancellationToken),
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<SpoonacularResult<IReadOnlyList<RecipeDetail>>> GetRecipeBulkInformationAsync(
        IReadOnlyList<int> recipeIds,
        CancellationToken cancellationToken = default)
    {
        var key = SpoonacularCacheKeys.ForRecipeBulk(recipeIds);
        return await ExecuteWithCacheAsync<IReadOnlyList<RecipeDetail>>(
            key,
            RecipeL1Ttl,
            RecipeL2Ttl,
            () => FetchRecipeBulkInformationAsync(recipeIds, cancellationToken),
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<SpoonacularResult<IngredientSubstitute>> GetIngredientSubstitutesAsync(
        string ingredientName,
        CancellationToken cancellationToken = default)
    {
        var key = SpoonacularCacheKeys.ForSubstitute(ingredientName);
        return await ExecuteWithCacheAsync<IngredientSubstitute>(
            key,
            SubstituteL1Ttl,
            SubstituteL2Ttl,
            () => FetchIngredientSubstitutesAsync(ingredientName, cancellationToken),
            cancellationToken);
    }

    // ── Core cache-first execution pattern ────────────────────────────────────

    private async Task<SpoonacularResult<T>> ExecuteWithCacheAsync<T>(
        string cacheKey,
        TimeSpan l1Ttl,
        TimeSpan l2Ttl,
        Func<Task<T?>> fetchFromApi,
        CancellationToken cancellationToken)
    {
        // 1. Cache check
        var cached = await _cache.GetAsync<T>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            if (_quotaMonitor.IsAtCacheOnlyThreshold(_options.DailyQuotaLimit, _options.CacheOnlyAtQuotaPercent))
            {
                return SpoonacularResult<T>.FromCacheLimited(cached);
            }

            return SpoonacularResult<T>.FromCache(cached);
        }

        // 2. Cache-only mode when quota is exhausted and no cached copy is available
        if (_quotaMonitor.IsAtCacheOnlyThreshold(_options.DailyQuotaLimit, _options.CacheOnlyAtQuotaPercent))
        {
            _logger.LogWarning(
                "Spoonacular quota exhausted ({Used}/{Limit}); returning empty result for key {Key}",
                _quotaMonitor.QuotaUsed, _options.DailyQuotaLimit, cacheKey);
            return SpoonacularResult<T>.RateLimited();
        }

        // 3. Call Spoonacular
        try
        {
            var data = await fetchFromApi();
            if (data is null)
            {
                return SpoonacularResult<T>.Degraded();
            }

            await _cache.SetAsync(cacheKey, data, l1Ttl, l2Ttl, cancellationToken);
            return SpoonacularResult<T>.FromApi(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Spoonacular API call failed for key {Key}; degrading gracefully", cacheKey);
            return SpoonacularResult<T>.Degraded();
        }
    }

    // ── Spoonacular HTTP fetch helpers ─────────────────────────────────────────

    private async Task<IReadOnlyList<RecipeSummary>?> FetchSearchByIngredientsAsync(
        IReadOnlyList<string> ingredients,
        SearchByIngredientsOptions? options,
        CancellationToken cancellationToken)
    {
        var number = options?.Number ?? 10;
        var ranking = (options?.RankByMaxUsed ?? true) ? 2 : 1;
        var ignorePantry = (options?.IgnorePantry ?? true) ? "true" : "false";
        var ingredientsCsv = string.Join(",", ingredients.Select(i => i.Trim()));

        var url = $"/recipes/findByIngredients?ingredients={Uri.EscapeDataString(ingredientsCsv)}" +
                  $"&number={number}&ranking={ranking}&ignorePantry={ignorePantry}";

        var json = await GetJsonAsync(url, cancellationToken);
        if (json is null)
        {
            return null;
        }

        var results = new List<RecipeSummary>();
        foreach (var item in json.AsArray())
        {
            if (item is null)
            {
                continue;
            }
            results.Add(new RecipeSummary
            {
                SpoonacularId = item["id"]?.GetValue<int>() ?? 0,
                Title = item["title"]?.GetValue<string>() ?? string.Empty,
                ImageUrl = item["image"]?.GetValue<string>(),
                UsedIngredientCount = item["usedIngredientCount"]?.GetValue<int>(),
                MissedIngredientCount = item["missedIngredientCount"]?.GetValue<int>(),
                Likes = item["likes"]?.GetValue<int>(),
            });
        }

        return results;
    }

    private async Task<IReadOnlyList<RecipeSummary>?> FetchComplexSearchAsync(
        string query,
        ComplexSearchFilters? filters,
        CancellationToken cancellationToken)
    {
        var number = filters?.Number ?? 10;
        var url = $"/recipes/complexSearch?query={Uri.EscapeDataString(query)}&number={number}" +
                  $"&addRecipeInformation=false";

        if (!string.IsNullOrWhiteSpace(filters?.Cuisine))
        {
            url += $"&cuisine={Uri.EscapeDataString(filters.Cuisine)}";
        }

        if (!string.IsNullOrWhiteSpace(filters?.Diet))
        {
            url += $"&diet={Uri.EscapeDataString(filters.Diet)}";
        }

        if (!string.IsNullOrWhiteSpace(filters?.Intolerances))
        {
            url += $"&intolerances={Uri.EscapeDataString(filters.Intolerances)}";
        }

        if (filters?.MaxReadyTime is not null)
        {
            url += $"&maxReadyTime={filters.MaxReadyTime.Value}";
        }

        var json = await GetJsonAsync(url, cancellationToken);
        if (json is null)
        {
            return null;
        }

        var resultsNode = json["results"]?.AsArray();
        if (resultsNode is null)
        {
            return [];
        }

        var results = new List<RecipeSummary>();
        foreach (var item in resultsNode)
        {
            if (item is null)
            {
                continue;
            }
            results.Add(new RecipeSummary
            {
                SpoonacularId = item["id"]?.GetValue<int>() ?? 0,
                Title = item["title"]?.GetValue<string>() ?? string.Empty,
                ImageUrl = item["image"]?.GetValue<string>(),
                ReadyInMinutes = item["readyInMinutes"]?.GetValue<int?>(),
                Servings = item["servings"]?.GetValue<int?>(),
                Cuisines = ReadStringArray(item["cuisines"]),
                DishTypes = ReadStringArray(item["dishTypes"]),
            });
        }

        return results;
    }

    private async Task<RecipeDetail?> FetchRecipeInformationAsync(
        int recipeId,
        CancellationToken cancellationToken)
    {
        var json = await GetJsonAsync($"/recipes/{recipeId}/information", cancellationToken);
        return json is null ? null : MapRecipeDetail(json);
    }

    private async Task<IReadOnlyList<RecipeDetail>?> FetchRecipeBulkInformationAsync(
        IReadOnlyList<int> recipeIds,
        CancellationToken cancellationToken)
    {
        var idsCsv = string.Join(",", recipeIds);
        var json = await GetJsonAsync($"/recipes/informationBulk?ids={Uri.EscapeDataString(idsCsv)}", cancellationToken);
        if (json is null)
        {
            return null;
        }

        var results = new List<RecipeDetail>();
        foreach (var item in json.AsArray())
        {
            if (item is null)
            {
                continue;
            }
            results.Add(MapRecipeDetail(item));
        }

        return results;
    }

    private async Task<IngredientSubstitute?> FetchIngredientSubstitutesAsync(
        string ingredientName,
        CancellationToken cancellationToken)
    {
        var json = await GetJsonAsync(
            $"/food/ingredients/substitutes?ingredientName={Uri.EscapeDataString(ingredientName)}",
            cancellationToken);

        if (json is null)
        {
            return null;
        }

        return new IngredientSubstitute
        {
            Ingredient = json["ingredient"]?.GetValue<string>() ?? ingredientName,
            Substitutes = ReadStringArray(json["substitutes"]),
            Message = json["message"]?.GetValue<string>(),
        };
    }

    // ── HTTP helper ────────────────────────────────────────────────────────────

    private async Task<JsonNode?> GetJsonAsync(string relativeUrl, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);
        var separator = relativeUrl.Contains('?') ? '&' : '?';
        var url = $"{relativeUrl}{separator}apiKey={_options.ApiKey}";

        using var response = await client.GetAsync(url, cancellationToken);

        // Update quota monitor
        if (response.Headers.TryGetValues(QuotaUsedHeader, out var values)
            && int.TryParse(values.FirstOrDefault(), out var quotaUsed))
        {
            _quotaMonitor.Update(quotaUsed);

            if (_quotaMonitor.IsAtWarningThreshold(_options.DailyQuotaLimit, _options.WarnAtQuotaPercent))
            {
                _logger.LogWarning(
                    "Spoonacular quota warning: {Used}/{Limit} requests used today",
                    quotaUsed, _options.DailyQuotaLimit);
            }
        }

        // Handle rate-limit / payment-required responses
        if (response.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.PaymentRequired)
        {
            _quotaMonitor.Update(_options.DailyQuotaLimit); // treat as fully exhausted
            _logger.LogWarning(
                "Spoonacular returned {StatusCode}; switching to cache-only mode",
                response.StatusCode);
            throw new HttpRequestException(
                $"Spoonacular rate limit exceeded: {response.StatusCode}",
                inner: null,
                statusCode: response.StatusCode);
        }

        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonNode.ParseAsync(stream, cancellationToken: cancellationToken);
    }

    // ── Mapping helpers ────────────────────────────────────────────────────────

    private static RecipeDetail MapRecipeDetail(JsonNode node) =>
        new()
        {
            SpoonacularId = node["id"]?.GetValue<int>() ?? 0,
            Title = node["title"]?.GetValue<string>() ?? string.Empty,
            ImageUrl = node["image"]?.GetValue<string>(),
            ReadyInMinutes = node["readyInMinutes"]?.GetValue<int>() ?? 0,
            Servings = node["servings"]?.GetValue<int>() ?? 0,
            Summary = node["summary"]?.GetValue<string>(),
            Instructions = node["instructions"]?.GetValue<string>(),
            Ingredients = MapIngredients(node["extendedIngredients"]),
            Cuisines = ReadStringArray(node["cuisines"]),
            DishTypes = ReadStringArray(node["dishTypes"]),
            Diets = ReadStringArray(node["diets"]),
            SourceUrl = node["sourceUrl"]?.GetValue<string>(),
            Vegetarian = node["vegetarian"]?.GetValue<bool>() ?? false,
            Vegan = node["vegan"]?.GetValue<bool>() ?? false,
            GlutenFree = node["glutenFree"]?.GetValue<bool>() ?? false,
            DairyFree = node["dairyFree"]?.GetValue<bool>() ?? false,
        };

    private static IReadOnlyList<RecipeIngredientInfo> MapIngredients(JsonNode? node)
    {
        if (node is not JsonArray array)
        {
            return [];
        }
        var list = new List<RecipeIngredientInfo>();
        foreach (var item in array)
        {
            if (item is null)
            {
                continue;
            }
            list.Add(new RecipeIngredientInfo
            {
                Id = item["id"]?.GetValue<int>() ?? 0,
                Name = item["name"]?.GetValue<string>() ?? string.Empty,
                OriginalString = item["original"]?.GetValue<string>()
                              ?? item["originalString"]?.GetValue<string>()
                              ?? string.Empty,
                Amount = item["amount"]?.GetValue<double>() ?? 0,
                Unit = item["unit"]?.GetValue<string>() ?? string.Empty,
                ImageUrl = item["image"]?.GetValue<string>(),
            });
        }

        return list;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonNode? node)
    {
        if (node is not JsonArray array)
        {
            return [];
        }
        return array
            .OfType<JsonNode>()
            .Select(n => n.GetValue<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    }
}
