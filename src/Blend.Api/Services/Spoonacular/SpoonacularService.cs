using System.Net;
using System.Net.Http.Json;
using System.Web;
using Blend.Api.Domain.Models;
using Blend.Api.Services.Cache;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blend.Api.Services.Spoonacular;

/// <summary>
/// Cache-aware implementation of <see cref="ISpoonacularService"/>.
/// Flow: L1 cache → L2 cache → Spoonacular API.
/// Degrades gracefully when the API is unavailable or the quota is exhausted.
/// </summary>
public sealed class SpoonacularService : ISpoonacularService
{
    private readonly HttpClient _http;
    private readonly ICacheService _cache;
    private readonly SpoonacularOptions _opts;
    private readonly QuotaTracker _quota;
    private readonly ILogger<SpoonacularService> _logger;

    public SpoonacularService(
        HttpClient http,
        ICacheService cache,
        IOptions<SpoonacularOptions> options,
        ILogger<SpoonacularService> logger)
    {
        _http = http;
        _cache = cache;
        _opts = options.Value;
        _logger = logger;
        _quota = new QuotaTracker(options, logger);
    }

    /// <inheritdoc/>
    public double CurrentQuotaUsageFraction => _quota.UsageFraction;

    /// <inheritdoc/>
    public async Task<SpoonacularResult<IReadOnlyList<RecipeSummary>>> SearchByIngredientsAsync(
        IEnumerable<string> ingredients,
        SearchByIngredientsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var opts = options ?? new SearchByIngredientsOptions();
        var cacheKey = SpoonacularCacheKeys.ForIngredientSearch(ingredients, opts);

        var cached = await _cache.GetAsync<IReadOnlyList<RecipeSummary>>(cacheKey, cancellationToken);
        if (cached.Value is not null)
            return Ok(cached.Value, cached.Tier!);

        if (_quota.IsCacheOnly)
            return LimitedEmpty<IReadOnlyList<RecipeSummary>>(Array.Empty<RecipeSummary>());

        try
        {
            var qs = HttpUtility.ParseQueryString(string.Empty);
            qs["apiKey"] = _opts.ApiKey;
            qs["ingredients"] = string.Join(",", ingredients.Select(i => i.Trim()));
            if (opts.Number.HasValue) qs["number"] = opts.Number.Value.ToString();
            if (opts.Ranking.HasValue) qs["ranking"] = opts.Ranking.Value.ToString();
            if (opts.IgnorePantry.HasValue) qs["ignorePantry"] = opts.IgnorePantry.Value.ToString().ToLowerInvariant();

            var url = $"/recipes/findByIngredients?{qs}";
            var response = await _http.GetAsync(url, cancellationToken);
            _quota.Update(response);

            if (!response.IsSuccessStatusCode)
                return HandleErrorResponse<IReadOnlyList<RecipeSummary>>(response.StatusCode, Array.Empty<RecipeSummary>());

            var dtos = await response.Content.ReadFromJsonAsync<List<SpoonacularRecipeSummaryDto>>(cancellationToken: cancellationToken)
                       ?? [];
            var result = dtos.Select(SpoonacularMapper.ToRecipeSummary).ToList().AsReadOnly() as IReadOnlyList<RecipeSummary>;

            await _cache.SetAsync(cacheKey, result, _opts.SearchL1Ttl, _opts.SearchL2Ttl, cancellationToken);
            return Ok(result, "spoonacular");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Spoonacular SearchByIngredients failed");
            return Degraded<IReadOnlyList<RecipeSummary>>(Array.Empty<RecipeSummary>());
        }
    }

    /// <inheritdoc/>
    public async Task<SpoonacularResult<IReadOnlyList<RecipeSummary>>> ComplexSearchAsync(
        ComplexSearchOptions options,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = SpoonacularCacheKeys.ForComplexSearch(options);

        var cached = await _cache.GetAsync<IReadOnlyList<RecipeSummary>>(cacheKey, cancellationToken);
        if (cached.Value is not null)
            return Ok(cached.Value, cached.Tier!);

        if (_quota.IsCacheOnly)
            return LimitedEmpty<IReadOnlyList<RecipeSummary>>(Array.Empty<RecipeSummary>());

        try
        {
            var qs = HttpUtility.ParseQueryString(string.Empty);
            qs["apiKey"] = _opts.ApiKey;
            if (!string.IsNullOrWhiteSpace(options.Query)) qs["query"] = options.Query;
            if (!string.IsNullOrWhiteSpace(options.Cuisine)) qs["cuisine"] = options.Cuisine;
            if (!string.IsNullOrWhiteSpace(options.Diet)) qs["diet"] = options.Diet;
            if (!string.IsNullOrWhiteSpace(options.Intolerances)) qs["intolerances"] = options.Intolerances;
            if (options.MaxReadyTime.HasValue) qs["maxReadyTime"] = options.MaxReadyTime.Value.ToString();
            if (options.Number.HasValue) qs["number"] = options.Number.Value.ToString();
            if (options.Offset.HasValue) qs["offset"] = options.Offset.Value.ToString();

            var url = $"/recipes/complexSearch?{qs}";
            var response = await _http.GetAsync(url, cancellationToken);
            _quota.Update(response);

            if (!response.IsSuccessStatusCode)
                return HandleErrorResponse<IReadOnlyList<RecipeSummary>>(response.StatusCode, Array.Empty<RecipeSummary>());

            var dto = await response.Content.ReadFromJsonAsync<SpoonacularComplexSearchResponse>(cancellationToken: cancellationToken)
                      ?? new SpoonacularComplexSearchResponse();
            var result = dto.Results.Select(SpoonacularMapper.ToRecipeSummary).ToList().AsReadOnly() as IReadOnlyList<RecipeSummary>;

            await _cache.SetAsync(cacheKey, result, _opts.SearchL1Ttl, _opts.SearchL2Ttl, cancellationToken);
            return Ok(result, "spoonacular");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Spoonacular ComplexSearch failed");
            return Degraded<IReadOnlyList<RecipeSummary>>(Array.Empty<RecipeSummary>());
        }
    }

    /// <inheritdoc/>
    public async Task<SpoonacularResult<RecipeDetail?>> GetRecipeInformationAsync(
        int recipeId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = SpoonacularCacheKeys.ForRecipe(recipeId);

        var cached = await _cache.GetAsync<RecipeDetail>(cacheKey, cancellationToken);
        if (cached.Value is not null)
            return Ok<RecipeDetail?>(cached.Value, cached.Tier!);

        if (_quota.IsCacheOnly)
            return LimitedEmpty<RecipeDetail?>(null);

        try
        {
            var url = $"/recipes/{recipeId}/information?apiKey={_opts.ApiKey}&includeNutrition=false";
            var response = await _http.GetAsync(url, cancellationToken);
            _quota.Update(response);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return Ok<RecipeDetail?>(null, "spoonacular");

            if (!response.IsSuccessStatusCode)
                return HandleErrorResponse<RecipeDetail?>(response.StatusCode, null);

            var dto = await response.Content.ReadFromJsonAsync<SpoonacularRecipeDetailDto>(cancellationToken: cancellationToken);
            if (dto is null)
                return Ok<RecipeDetail?>(null, "spoonacular");

            var detail = SpoonacularMapper.ToRecipeDetail(dto);
            await _cache.SetAsync(cacheKey, detail, _opts.RecipeL1Ttl, _opts.RecipeL2Ttl, cancellationToken);
            return Ok<RecipeDetail?>(detail, "spoonacular");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Spoonacular GetRecipeInformation failed for recipe {RecipeId}", recipeId);
            return Degraded<RecipeDetail?>(null);
        }
    }

    /// <inheritdoc/>
    public async Task<SpoonacularResult<IReadOnlyList<RecipeDetail>>> GetRecipeBulkInformationAsync(
        IEnumerable<int> recipeIds,
        CancellationToken cancellationToken = default)
    {
        var ids = recipeIds.Distinct().ToList();
        if (ids.Count == 0)
            return Ok<IReadOnlyList<RecipeDetail>>(Array.Empty<RecipeDetail>(), "none");

        // Check each id against the cache first
        var results = new List<RecipeDetail>();
        var missingIds = new List<int>();
        var highestTier = "none";

        foreach (var id in ids)
        {
            var cacheKey = SpoonacularCacheKeys.ForRecipe(id);
            var cached = await _cache.GetAsync<RecipeDetail>(cacheKey, cancellationToken);
            if (cached.Value is not null)
            {
                results.Add(cached.Value);
                highestTier = BetterTier(highestTier, cached.Tier);
            }
            else
            {
                missingIds.Add(id);
            }
        }

        if (missingIds.Count == 0)
            return Ok<IReadOnlyList<RecipeDetail>>(results.AsReadOnly(), highestTier);

        if (_quota.IsCacheOnly)
        {
            // Return what we have from cache with a limited flag
            return new SpoonacularResult<IReadOnlyList<RecipeDetail>>
            {
                Data = results.AsReadOnly(),
                DataSource = results.Count > 0 ? highestTier : "none",
                IsLimitedResults = true
            };
        }

        try
        {
            var idsParam = string.Join(",", missingIds);
            var url = $"/recipes/informationBulk?apiKey={_opts.ApiKey}&ids={idsParam}&includeNutrition=false";
            var response = await _http.GetAsync(url, cancellationToken);
            _quota.Update(response);

            if (!response.IsSuccessStatusCode)
            {
                if (results.Count > 0)
                    return new SpoonacularResult<IReadOnlyList<RecipeDetail>>
                    {
                        Data = results.AsReadOnly(),
                        DataSource = highestTier,
                        IsLimitedResults = true
                    };
                return HandleErrorResponse<IReadOnlyList<RecipeDetail>>(response.StatusCode, Array.Empty<RecipeDetail>());
            }

            var dtos = await response.Content.ReadFromJsonAsync<List<SpoonacularRecipeDetailDto>>(cancellationToken: cancellationToken) ?? [];

            foreach (var dto in dtos)
            {
                var detail = SpoonacularMapper.ToRecipeDetail(dto);
                results.Add(detail);
                var key = SpoonacularCacheKeys.ForRecipe(dto.Id);
                await _cache.SetAsync(key, detail, _opts.RecipeL1Ttl, _opts.RecipeL2Ttl, cancellationToken);
            }

            return Ok<IReadOnlyList<RecipeDetail>>(results.AsReadOnly(), "spoonacular");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Spoonacular GetRecipeBulkInformation failed for ids {Ids}", string.Join(",", missingIds));
            return new SpoonacularResult<IReadOnlyList<RecipeDetail>>
            {
                Data = results.AsReadOnly(),
                DataSource = results.Count > 0 ? highestTier : "none",
                IsLimitedResults = true
            };
        }
    }

    /// <inheritdoc/>
    public async Task<SpoonacularResult<IngredientSubstitute?>> GetIngredientSubstitutesAsync(
        string ingredientName,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = SpoonacularCacheKeys.ForSubstitute(ingredientName);

        var cached = await _cache.GetAsync<IngredientSubstitute>(cacheKey, cancellationToken);
        if (cached.Value is not null)
            return Ok<IngredientSubstitute?>(cached.Value, cached.Tier!);

        if (_quota.IsCacheOnly)
            return LimitedEmpty<IngredientSubstitute?>(null);

        try
        {
            var url = $"/food/ingredients/{Uri.EscapeDataString(ingredientName.Trim())}/substitutes?apiKey={_opts.ApiKey}";
            var response = await _http.GetAsync(url, cancellationToken);
            _quota.Update(response);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return Ok<IngredientSubstitute?>(null, "spoonacular");

            if (!response.IsSuccessStatusCode)
                return HandleErrorResponse<IngredientSubstitute?>(response.StatusCode, null);

            var dto = await response.Content.ReadFromJsonAsync<SpoonacularSubstituteResponse>(cancellationToken: cancellationToken);
            if (dto is null)
                return Ok<IngredientSubstitute?>(null, "spoonacular");

            var substitute = SpoonacularMapper.ToIngredientSubstitute(dto);
            await _cache.SetAsync(cacheKey, substitute, _opts.SubstituteL1Ttl, _opts.SubstituteL2Ttl, cancellationToken);
            return Ok<IngredientSubstitute?>(substitute, "spoonacular");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Spoonacular GetIngredientSubstitutes failed for {Ingredient}", ingredientName);
            return Degraded<IngredientSubstitute?>(null);
        }
    }

    // --- Result helpers ---

    private static SpoonacularResult<T> Ok<T>(T data, string tier) =>
        new() { Data = data, DataSource = tier };

    private SpoonacularResult<T> LimitedEmpty<T>(T data) =>
        new() { Data = data, DataSource = "none", IsLimitedResults = true };

    private SpoonacularResult<T> Degraded<T>(T fallback) =>
        new() { Data = fallback, DataSource = "none", IsLimitedResults = true };

    private SpoonacularResult<T> HandleErrorResponse<T>(HttpStatusCode status, T fallback)
    {
        if (status == HttpStatusCode.PaymentRequired || status == (HttpStatusCode)429)
        {
            _logger.LogWarning("Spoonacular returned {StatusCode} — quota exceeded; switching to cache-only", (int)status);
            // Force cache-only mode by bumping tracked quota to max
            _quota.Update(BuildQuotaExhaustedResponse());
            return LimitedEmpty(fallback);
        }

        _logger.LogWarning("Spoonacular returned unexpected status {StatusCode}", (int)status);
        return Degraded(fallback);
    }

    private static HttpResponseMessage BuildQuotaExhaustedResponse()
    {
        // Simulate a fully-used quota so the tracker flips to cache-only mode
        var msg = new HttpResponseMessage(HttpStatusCode.PaymentRequired);
        msg.Headers.Add("X-API-Quota-Used", "150");
        return msg;
    }

    private static string BetterTier(string current, string? candidate)
    {
        // Preference order: l1-cache > l2-cache > spoonacular > none
        var order = new[] { "none", "spoonacular", "l2-cache", "l1-cache" };
        var ci = Array.IndexOf(order, current);
        var ni = Array.IndexOf(order, candidate ?? "none");
        return ni > ci ? (candidate ?? current) : current;
    }
}
