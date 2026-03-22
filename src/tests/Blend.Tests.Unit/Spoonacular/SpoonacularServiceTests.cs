using System.Net;
using System.Net.Http.Json;
using Blend.Api.Services.Cache;
using Blend.Api.Services.Spoonacular;
using Blend.Api.Services.Spoonacular.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Blend.Tests.Unit.Spoonacular;

/// <summary>
/// Unit tests for <see cref="SpoonacularService"/> graceful degradation, rate-limit handling,
/// and response mapping.
/// </summary>
public class SpoonacularServiceTests
{
    private static readonly SpoonacularOptions DefaultOptions = new()
    {
        ApiKey = "test-api-key",
        BaseUrl = "https://api.spoonacular.com",
        DailyQuotaLimit = 150,
        WarnAtQuotaPercent = 0.80,
        CacheOnlyAtQuotaPercent = 0.95,
    };

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static (SpoonacularService Service, Mock<ICacheService> CacheMock, SpoonacularQuotaMonitor Monitor)
        CreateService(HttpMessageHandler handler, SpoonacularOptions? options = null)
    {
        var opt = options ?? DefaultOptions;
        var cacheMock = new Mock<ICacheService>();
        var monitor = new SpoonacularQuotaMonitor();

        var factory = new TestHttpClientFactory("Spoonacular", new HttpClient(handler)
        {
            BaseAddress = new Uri(opt.BaseUrl),
        });

        var svc = new SpoonacularService(
            factory,
            cacheMock.Object,
            monitor,
            Options.Create(opt),
            NullLogger<SpoonacularService>.Instance);

        return (svc, cacheMock, monitor);
    }

    // ── Cache hit ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchByIngredientsAsync_WhenCacheHit_ReturnsCachedResultWithoutCallingApi()
    {
        var cachedData = (IReadOnlyList<RecipeSummary>)[new RecipeSummary { SpoonacularId = 1, Title = "Cached" }];
        var handler = new StubHttpHandler(HttpStatusCode.OK, "[]");
        var (svc, cacheMock, _) = CreateService(handler);

        cacheMock.Setup(c => c.GetAsync<IReadOnlyList<RecipeSummary>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(cachedData);

        var result = await svc.SearchByIngredientsAsync(["apple"]);

        Assert.True(result.IsAvailable);
        Assert.Equal(DataSource.Cache, result.DataSource);
        Assert.False(handler.WasCalled);
    }

    // ── Rate-limit mode ───────────────────────────────────────────────────────

    [Fact]
    public async Task SearchByIngredientsAsync_WhenCacheOnlyThresholdReached_ReturnsCacheDataWithLimitedFlag()
    {
        var cachedData = (IReadOnlyList<RecipeSummary>)[new RecipeSummary { SpoonacularId = 99, Title = "Limited" }];
        var handler = new StubHttpHandler(HttpStatusCode.OK, "[]");
        var (svc, cacheMock, monitor) = CreateService(handler);

        monitor.Update(150); // 100% — above 95% threshold

        cacheMock.Setup(c => c.GetAsync<IReadOnlyList<RecipeSummary>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(cachedData);

        var result = await svc.SearchByIngredientsAsync(["apple"]);

        Assert.True(result.IsAvailable);
        Assert.True(result.IsLimited);
        Assert.Equal(DataSource.Cache, result.DataSource);
        Assert.False(handler.WasCalled);
    }

    [Fact]
    public async Task SearchByIngredientsAsync_WhenCacheOnlyAndNoCachedData_ReturnsRateLimitedResult()
    {
        var handler = new StubHttpHandler(HttpStatusCode.OK, "[]");
        var (svc, cacheMock, monitor) = CreateService(handler);

        monitor.Update(150);

        cacheMock.Setup(c => c.GetAsync<IReadOnlyList<RecipeSummary>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((IReadOnlyList<RecipeSummary>?)null);

        var result = await svc.SearchByIngredientsAsync(["apple"]);

        Assert.False(result.IsAvailable);
        Assert.True(result.IsLimited);
        Assert.Equal(DataSource.Degraded, result.DataSource);
        Assert.False(handler.WasCalled);
    }

    [Fact]
    public async Task GetRecipeInformationAsync_When429Response_UpdatesQuotaAndThrows()
    {
        var handler = new StubHttpHandler(HttpStatusCode.TooManyRequests, "rate limited");
        var (svc, cacheMock, monitor) = CreateService(handler);

        cacheMock.Setup(c => c.GetAsync<RecipeDetail>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((RecipeDetail?)null);

        var result = await svc.GetRecipeInformationAsync(42);

        Assert.False(result.IsAvailable);
        Assert.Equal(DataSource.Degraded, result.DataSource);
        // Monitor should have been set to full quota
        Assert.Equal(DefaultOptions.DailyQuotaLimit, monitor.QuotaUsed);
    }

    // ── Graceful degradation ──────────────────────────────────────────────────

    [Fact]
    public async Task SearchByIngredientsAsync_WhenApiThrows_ReturnsDegradedWithCachedData()
    {
        var cachedData = (IReadOnlyList<RecipeSummary>)[new RecipeSummary { SpoonacularId = 5, Title = "Old cached" }];
        var handler = new ThrowingHttpHandler(new HttpRequestException("Network error"));
        var (svc, cacheMock, _) = CreateService(handler);

        // First call: cache miss
        var callCount = 0;
        cacheMock.Setup(c => c.GetAsync<IReadOnlyList<RecipeSummary>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(() =>
                 {
                     callCount++;
                     return null;
                 });

        var result = await svc.SearchByIngredientsAsync(["apple"]);

        Assert.False(result.IsAvailable);
        Assert.Equal(DataSource.Degraded, result.DataSource);
    }

    [Fact]
    public async Task ComplexSearchAsync_WhenApiReturns500_ReturnsDegraded()
    {
        var handler = new StubHttpHandler(HttpStatusCode.InternalServerError, "Server error");
        var (svc, cacheMock, _) = CreateService(handler);

        cacheMock.Setup(c => c.GetAsync<IReadOnlyList<RecipeSummary>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((IReadOnlyList<RecipeSummary>?)null);

        var result = await svc.ComplexSearchAsync("pasta");

        Assert.False(result.IsAvailable);
        Assert.Equal(DataSource.Degraded, result.DataSource);
        Assert.False(result.IsLimited);
    }

    // ── Response mapping ──────────────────────────────────────────────────────

    [Fact]
    public async Task SearchByIngredientsAsync_MapsResponseCorrectly()
    {
        const string responseJson = """
            [
              {
                "id": 123,
                "title": "Apple Cake",
                "image": "https://img.example.com/cake.jpg",
                "usedIngredientCount": 2,
                "missedIngredientCount": 1,
                "likes": 50
              }
            ]
            """;

        var handler = new StubHttpHandler(HttpStatusCode.OK, responseJson);
        var (svc, cacheMock, _) = CreateService(handler);

        cacheMock.Setup(c => c.GetAsync<IReadOnlyList<RecipeSummary>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((IReadOnlyList<RecipeSummary>?)null);

        cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<RecipeSummary>>(),
                         It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var result = await svc.SearchByIngredientsAsync(["apple"]);

        Assert.True(result.IsAvailable);
        Assert.Equal(DataSource.Spoonacular, result.DataSource);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);

        var recipe = result.Data[0];
        Assert.Equal(123, recipe.SpoonacularId);
        Assert.Equal("Apple Cake", recipe.Title);
        Assert.Equal("https://img.example.com/cake.jpg", recipe.ImageUrl);
        Assert.Equal(2, recipe.UsedIngredientCount);
        Assert.Equal(1, recipe.MissedIngredientCount);
        Assert.Equal(50, recipe.Likes);
    }

    [Fact]
    public async Task ComplexSearchAsync_MapsResponseCorrectly()
    {
        const string responseJson = """
            {
              "results": [
                {
                  "id": 456,
                  "title": "Pasta Carbonara",
                  "image": "https://img.example.com/pasta.jpg",
                  "readyInMinutes": 30,
                  "servings": 2,
                  "cuisines": ["Italian"],
                  "dishTypes": ["main course"]
                }
              ],
              "offset": 0,
              "number": 10,
              "totalResults": 1
            }
            """;

        var handler = new StubHttpHandler(HttpStatusCode.OK, responseJson);
        var (svc, cacheMock, _) = CreateService(handler);

        cacheMock.Setup(c => c.GetAsync<IReadOnlyList<RecipeSummary>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((IReadOnlyList<RecipeSummary>?)null);

        cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<RecipeSummary>>(),
                         It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var result = await svc.ComplexSearchAsync("pasta");

        Assert.True(result.IsAvailable);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);

        var recipe = result.Data[0];
        Assert.Equal(456, recipe.SpoonacularId);
        Assert.Equal("Pasta Carbonara", recipe.Title);
        Assert.Equal(30, recipe.ReadyInMinutes);
        Assert.Equal(2, recipe.Servings);
        Assert.Equal(["Italian"], recipe.Cuisines);
        Assert.Equal(["main course"], recipe.DishTypes);
    }

    [Fact]
    public async Task GetRecipeInformationAsync_MapsAllFields()
    {
        const string responseJson = """
            {
              "id": 789,
              "title": "Beef Stew",
              "image": "https://img.example.com/stew.jpg",
              "readyInMinutes": 120,
              "servings": 6,
              "summary": "A hearty stew.",
              "instructions": "Cook for 2 hours.",
              "extendedIngredients": [
                {
                  "id": 1,
                  "name": "beef",
                  "original": "500g of beef",
                  "amount": 500,
                  "unit": "g",
                  "image": "beef.jpg"
                }
              ],
              "cuisines": ["American"],
              "dishTypes": ["main course"],
              "diets": [],
              "sourceUrl": "https://example.com/stew",
              "vegetarian": false,
              "vegan": false,
              "glutenFree": true,
              "dairyFree": false
            }
            """;

        var handler = new StubHttpHandler(HttpStatusCode.OK, responseJson);
        var (svc, cacheMock, _) = CreateService(handler);

        cacheMock.Setup(c => c.GetAsync<RecipeDetail>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((RecipeDetail?)null);

        cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<RecipeDetail>(),
                         It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var result = await svc.GetRecipeInformationAsync(789);

        Assert.True(result.IsAvailable);
        Assert.NotNull(result.Data);

        var detail = result.Data;
        Assert.Equal(789, detail.SpoonacularId);
        Assert.Equal("Beef Stew", detail.Title);
        Assert.Equal(120, detail.ReadyInMinutes);
        Assert.Equal(6, detail.Servings);
        Assert.Equal("A hearty stew.", detail.Summary);
        Assert.Equal("Cook for 2 hours.", detail.Instructions);
        Assert.True(detail.GlutenFree);
        Assert.False(detail.Vegetarian);
        Assert.Single(detail.Ingredients);
        Assert.Equal("beef", detail.Ingredients[0].Name);
        Assert.Equal(500, detail.Ingredients[0].Amount);
    }

    [Fact]
    public async Task GetIngredientSubstitutesAsync_MapsResponseCorrectly()
    {
        const string responseJson = """
            {
              "ingredient": "butter",
              "substitutes": [
                "1/2 cup of butter = 1/2 cup of margarine",
                "1/2 cup of butter = 1/2 cup of shortening"
              ],
              "message": "Use these alternatives when needed."
            }
            """;

        var handler = new StubHttpHandler(HttpStatusCode.OK, responseJson);
        var (svc, cacheMock, _) = CreateService(handler);

        cacheMock.Setup(c => c.GetAsync<IngredientSubstitute>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((IngredientSubstitute?)null);

        cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<IngredientSubstitute>(),
                         It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var result = await svc.GetIngredientSubstitutesAsync("butter");

        Assert.True(result.IsAvailable);
        Assert.NotNull(result.Data);
        Assert.Equal("butter", result.Data.Ingredient);
        Assert.Equal(2, result.Data.Substitutes.Count);
        Assert.Equal("Use these alternatives when needed.", result.Data.Message);
    }

    [Fact]
    public async Task GetIngredientSubstitutesAsync_NullFields_HandledGracefully()
    {
        const string responseJson = """
            {
              "ingredient": "xanthan gum",
              "substitutes": null,
              "message": null
            }
            """;

        var handler = new StubHttpHandler(HttpStatusCode.OK, responseJson);
        var (svc, cacheMock, _) = CreateService(handler);

        cacheMock.Setup(c => c.GetAsync<IngredientSubstitute>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((IngredientSubstitute?)null);

        cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<IngredientSubstitute>(),
                         It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var result = await svc.GetIngredientSubstitutesAsync("xanthan gum");

        Assert.True(result.IsAvailable);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data.Substitutes);
        Assert.Null(result.Data.Message);
    }

    // ── Quota header handling ─────────────────────────────────────────────────

    [Fact]
    public async Task SearchByIngredientsAsync_WhenApiReturns402_UpdatesQuotaAndReturnsDegraded()
    {
        var handler = new StubHttpHandler(HttpStatusCode.PaymentRequired, "quota exceeded");
        var (svc, cacheMock, monitor) = CreateService(handler);

        cacheMock.Setup(c => c.GetAsync<IReadOnlyList<RecipeSummary>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((IReadOnlyList<RecipeSummary>?)null);

        var result = await svc.SearchByIngredientsAsync(["apple"]);

        Assert.False(result.IsAvailable);
        Assert.Equal(DefaultOptions.DailyQuotaLimit, monitor.QuotaUsed);
    }

    // ── Cache write-through ───────────────────────────────────────────────────

    [Fact]
    public async Task ComplexSearchAsync_OnApiSuccess_CachesResult()
    {
        const string responseJson = """{"results":[{"id":1,"title":"T"}]}""";
        var handler = new StubHttpHandler(HttpStatusCode.OK, responseJson);
        var (svc, cacheMock, _) = CreateService(handler);

        cacheMock.Setup(c => c.GetAsync<IReadOnlyList<RecipeSummary>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((IReadOnlyList<RecipeSummary>?)null);

        cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<RecipeSummary>>(),
                         It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        await svc.ComplexSearchAsync("test");

        cacheMock.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<IReadOnlyList<RecipeSummary>>(),
            TimeSpan.FromHours(1),
            TimeSpan.FromHours(24),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

// ── Test infrastructure ────────────────────────────────────────────────────────

internal sealed class StubHttpHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _body;
    public bool WasCalled { get; private set; }

    public StubHttpHandler(HttpStatusCode statusCode, string body)
    {
        _statusCode = statusCode;
        _body = body;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        WasCalled = true;
        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_body, System.Text.Encoding.UTF8, "application/json"),
        };
        return Task.FromResult(response);
    }
}

internal sealed class ThrowingHttpHandler : HttpMessageHandler
{
    private readonly Exception _exception;

    public ThrowingHttpHandler(Exception exception)
    {
        _exception = exception;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken) =>
        throw _exception;
}

internal sealed class TestHttpClientFactory : IHttpClientFactory
{
    private readonly string _expectedName;
    private readonly HttpClient _client;

    public TestHttpClientFactory(string expectedName, HttpClient client)
    {
        _expectedName = expectedName;
        _client = client;
    }

    public HttpClient CreateClient(string name)
    {
        if (name == _expectedName)
        {
            return _client;
        }

        throw new InvalidOperationException($"Unexpected client name: {name}");
    }
}
