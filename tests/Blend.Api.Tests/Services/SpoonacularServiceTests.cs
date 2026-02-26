using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Blend.Api.Domain.Models;
using Blend.Api.Services.Cache;
using Blend.Api.Services.Spoonacular;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Blend.Api.Tests.Services;

/// <summary>
/// Integration tests for SpoonacularService using a mocked HttpMessageHandler and a real in-memory CacheService.
/// </summary>
public class SpoonacularServiceTests : IDisposable
{
    private readonly IMemoryCache _memoryCache;
    private readonly ICacheService _cacheService;
    private readonly SpoonacularOptions _opts;

    public SpoonacularServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cacheOpts = Options.Create(new CacheOptions());
        var sp = new ServiceCollection().BuildServiceProvider();
        _cacheService = new CacheService(_memoryCache, cacheOpts, NullLogger<CacheService>.Instance, sp);
        _opts = new SpoonacularOptions { ApiKey = "test-key" };
    }

    private SpoonacularService CreateService(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.spoonacular.com") };
        var opts = Options.Create(_opts);
        return new SpoonacularService(httpClient, _cacheService, opts, NullLogger<SpoonacularService>.Instance);
    }

    // --- SearchByIngredients ---

    [Fact]
    public async Task SearchByIngredients_ApiSuccess_ReturnsMappedResultsFromApi()
    {
        var dtos = new[]
        {
            new { id = 1, title = "Chicken Soup", image = (string?)null, usedIngredientCount = 2, missedIngredientCount = 1, likes = 10.0 }
        };
        var handler = BuildHandler(HttpStatusCode.OK, JsonSerializer.Serialize(dtos));
        var sut = CreateService(handler);

        var result = await sut.SearchByIngredientsAsync(["chicken", "onion"]);

        Assert.NotNull(result);
        Assert.Equal("spoonacular", result.DataSource);
        Assert.False(result.IsLimitedResults);
        Assert.Single(result.Data);
        Assert.Equal(1, result.Data[0].Id);
    }

    [Fact]
    public async Task SearchByIngredients_SecondCall_ReturnsCachedResult()
    {
        var dtos = new[] { new { id = 5, title = "Soup", image = (string?)null, usedIngredientCount = (int?)null, missedIngredientCount = (int?)null, likes = (double?)null } };
        var handler = BuildHandler(HttpStatusCode.OK, JsonSerializer.Serialize(dtos));
        var sut = CreateService(handler);

        var ingredients = new[] { "carrot", "potato" };
        await sut.SearchByIngredientsAsync(ingredients);
        var result2 = await sut.SearchByIngredientsAsync(ingredients);

        Assert.True(result2.IsFromCache);
        Assert.Equal("l1-cache", result2.DataSource);
    }

    [Fact]
    public async Task SearchByIngredients_ApiUnavailable_ReturnsDegradedEmpty()
    {
        var handler = BuildFailingHandler();
        var sut = CreateService(handler);

        var result = await sut.SearchByIngredientsAsync(["garlic"]);

        Assert.Empty(result.Data);
        Assert.True(result.IsLimitedResults);
        Assert.Equal("none", result.DataSource);
    }

    [Fact]
    public async Task SearchByIngredients_Http402_ActivatesCacheOnlyMode()
    {
        var handler = BuildHandler((HttpStatusCode)402, "");
        var sut = CreateService(handler);

        var result = await sut.SearchByIngredientsAsync(["tomato"]);

        Assert.True(result.IsLimitedResults);
    }

    // --- ComplexSearch ---

    [Fact]
    public async Task ComplexSearch_ApiSuccess_ReturnsMappedResults()
    {
        var responseBody = JsonSerializer.Serialize(new
        {
            results = new[] { new { id = 99, title = "Pizza", image = (string?)null, usedIngredientCount = (int?)null, missedIngredientCount = (int?)null, likes = (double?)null } },
            totalResults = 1
        });
        var handler = BuildHandler(HttpStatusCode.OK, responseBody);
        var sut = CreateService(handler);

        var result = await sut.ComplexSearchAsync(new ComplexSearchOptions { Query = "pizza", Cuisine = "Italian" });

        Assert.Equal("spoonacular", result.DataSource);
        Assert.Single(result.Data);
        Assert.Equal(99, result.Data[0].Id);
    }

    [Fact]
    public async Task ComplexSearch_SecondIdenticalQuery_HitsCache()
    {
        var responseBody = JsonSerializer.Serialize(new { results = Array.Empty<object>(), totalResults = 0 });
        var handler = BuildHandler(HttpStatusCode.OK, responseBody);
        var sut = CreateService(handler);

        var opts = new ComplexSearchOptions { Query = "pasta" };
        await sut.ComplexSearchAsync(opts);
        var result2 = await sut.ComplexSearchAsync(opts);

        Assert.True(result2.IsFromCache);
    }

    // --- GetRecipeInformation ---

    [Fact]
    public async Task GetRecipeInformation_ApiSuccess_ReturnsMappedDetail()
    {
        var detailDto = new
        {
            id = 42,
            title = "Spaghetti",
            image = "img.jpg",
            readyInMinutes = 30,
            servings = 2,
            summary = "Classic pasta.",
            cuisines = new[] { "Italian" },
            dishTypes = new[] { "main course" },
            extendedIngredients = new[] { new { id = 1, name = "pasta", amount = 200.0, unit = "g", image = (string?)null } },
            analyzedInstructions = Array.Empty<object>(),
            vegetarian = true,
            vegan = false,
            glutenFree = false,
            dairyFree = true
        };
        var handler = BuildHandler(HttpStatusCode.OK, JsonSerializer.Serialize(detailDto));
        var sut = CreateService(handler);

        var result = await sut.GetRecipeInformationAsync(42);

        Assert.Equal("spoonacular", result.DataSource);
        Assert.NotNull(result.Data);
        Assert.Equal(42, result.Data!.Id);
        Assert.Equal("Spaghetti", result.Data.Title);
    }

    [Fact]
    public async Task GetRecipeInformation_NotFound_ReturnsNullData()
    {
        var handler = BuildHandler(HttpStatusCode.NotFound, "");
        var sut = CreateService(handler);

        var result = await sut.GetRecipeInformationAsync(9999);

        Assert.Equal("spoonacular", result.DataSource);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetRecipeInformation_SecondCall_HitsL1Cache()
    {
        var detailDto = new
        {
            id = 55, title = "Soup", image = (string?)null, readyInMinutes = (int?)null, servings = (int?)null,
            summary = (string?)null, cuisines = Array.Empty<string>(), dishTypes = Array.Empty<string>(),
            extendedIngredients = Array.Empty<object>(), analyzedInstructions = Array.Empty<object>(),
            vegetarian = (bool?)null, vegan = (bool?)null, glutenFree = (bool?)null, dairyFree = (bool?)null
        };
        var handler = BuildHandler(HttpStatusCode.OK, JsonSerializer.Serialize(detailDto));
        var sut = CreateService(handler);

        await sut.GetRecipeInformationAsync(55);
        var result2 = await sut.GetRecipeInformationAsync(55);

        Assert.Equal("l1-cache", result2.DataSource);
        Assert.True(result2.IsFromCache);
    }

    // --- GetIngredientSubstitutes ---

    [Fact]
    public async Task GetIngredientSubstitutes_ApiSuccess_ReturnsMappedSubstitutes()
    {
        var dto = new { ingredient = "butter", substitutes = new[] { "margarine", "coconut oil" }, message = "Use in equal parts." };
        var handler = BuildHandler(HttpStatusCode.OK, JsonSerializer.Serialize(dto));
        var sut = CreateService(handler);

        var result = await sut.GetIngredientSubstitutesAsync("butter");

        Assert.Equal("spoonacular", result.DataSource);
        Assert.NotNull(result.Data);
        Assert.Equal("butter", result.Data!.IngredientName);
        Assert.Contains("margarine", result.Data.Substitutes);
    }

    [Fact]
    public async Task GetIngredientSubstitutes_SecondCall_HitsCache()
    {
        var dto = new { ingredient = "egg", substitutes = new[] { "flax egg" }, message = (string?)null };
        var handler = BuildHandler(HttpStatusCode.OK, JsonSerializer.Serialize(dto));
        var sut = CreateService(handler);

        await sut.GetIngredientSubstitutesAsync("Egg");
        var result2 = await sut.GetIngredientSubstitutesAsync("egg"); // normalised → same key

        Assert.True(result2.IsFromCache);
    }

    [Fact]
    public async Task GetIngredientSubstitutes_ApiFailure_ReturnsNullDegraded()
    {
        var handler = BuildFailingHandler();
        var sut = CreateService(handler);

        var result = await sut.GetIngredientSubstitutesAsync("salt");

        Assert.Null(result.Data);
        Assert.True(result.IsLimitedResults);
    }

    // --- GetRecipeBulkInformation ---

    [Fact]
    public async Task GetRecipeBulkInformation_EmptyIds_ReturnsEmptyImmediately()
    {
        var handler = BuildHandler(HttpStatusCode.OK, "[]");
        var sut = CreateService(handler);

        var result = await sut.GetRecipeBulkInformationAsync([]);

        Assert.Empty(result.Data);
        Assert.Equal("none", result.DataSource);
    }

    [Fact]
    public async Task GetRecipeBulkInformation_AllCached_DoesNotCallApi()
    {
        // Pre-populate the cache directly so the bulk call finds everything cached
        var detail10 = SpoonacularMapper.ToRecipeDetail(new SpoonacularRecipeDetailDto { Id = 10, Title = "Recipe 10" });
        var detail20 = SpoonacularMapper.ToRecipeDetail(new SpoonacularRecipeDetailDto { Id = 20, Title = "Recipe 20" });

        await _cacheService.SetAsync(
            SpoonacularCacheKeys.ForRecipe(10), detail10,
            TimeSpan.FromMinutes(5), TimeSpan.FromHours(1));
        await _cacheService.SetAsync(
            SpoonacularCacheKeys.ForRecipe(20), detail20,
            TimeSpan.FromMinutes(5), TimeSpan.FromHours(1));

        // Use a failing handler to prove the API is never called
        var handler = BuildFailingHandler();
        var sut = CreateService(handler);

        var result = await sut.GetRecipeBulkInformationAsync([10, 20]);

        Assert.Equal(2, result.Data.Count);
        Assert.True(result.IsFromCache);
    }

    // --- Graceful degradation ---

    [Fact]
    public async Task ComplexSearch_ApiThrows_NeverLeaksException()
    {
        var handler = BuildFailingHandler();
        var sut = CreateService(handler);

        var result = await sut.ComplexSearchAsync(new ComplexSearchOptions { Query = "soup" });

        // Should not throw; should return empty degraded result
        Assert.NotNull(result);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task SpoonacularResult_IsFromCache_FalseForLiveData()
    {
        var dtos = new[] { new { id = 1, title = "Test", image = (string?)null, usedIngredientCount = (int?)null, missedIngredientCount = (int?)null, likes = (double?)null } };
        var handler = BuildHandler(HttpStatusCode.OK, JsonSerializer.Serialize(dtos));
        var sut = CreateService(handler);

        var result = await sut.SearchByIngredientsAsync(["onion"]);

        Assert.False(result.IsFromCache);
        Assert.Equal("spoonacular", result.DataSource);
    }

    // --- helpers ---

    private static HttpMessageHandler BuildHandler(HttpStatusCode status, string body)
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
            });
        return mock.Object;
    }

    private static HttpMessageHandler BuildFailingHandler()
    {
        var mock = new Mock<HttpMessageHandler>();
        mock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Simulated network failure"));
        return mock.Object;
    }

    public void Dispose() => _memoryCache.Dispose();
}
