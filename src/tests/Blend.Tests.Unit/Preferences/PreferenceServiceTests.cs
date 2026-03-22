using Blend.Api.Preferences;
using Blend.Api.Preferences.Services;
using Blend.Api.Services.Spoonacular.Models;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Blend.Tests.Unit.Preferences;

/// <summary>
/// Unit tests for <see cref="PreferenceService"/>.
/// </summary>
public class PreferenceServiceTests
{
    private static PreferenceService CreateService(IRepository<User>? repo = null) =>
        new(NullLogger<PreferenceService>.Instance, repo);

    private static Mock<IRepository<User>> CreateUserRepoMock(User? user = null)
    {
        var mock = new Mock<IRepository<User>>();
        mock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        return mock;
    }

    // ── GetUserPreferencesAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetUserPreferencesAsync_WhenRepositoryNull_ReturnsDefaultPreferences()
    {
        var svc = CreateService();

        var prefs = await svc.GetUserPreferencesAsync("user-1");

        Assert.Empty(prefs.FavoriteCuisines);
        Assert.Empty(prefs.Diets);
        Assert.Empty(prefs.Intolerances);
    }

    [Fact]
    public async Task GetUserPreferencesAsync_WhenUserNotFound_ReturnsDefaultPreferences()
    {
        var mock = CreateUserRepoMock(null);
        var svc = CreateService(mock.Object);

        var prefs = await svc.GetUserPreferencesAsync("missing-user");

        Assert.Empty(prefs.FavoriteCuisines);
    }

    [Fact]
    public async Task GetUserPreferencesAsync_WhenUserFound_ReturnsUserPreferences()
    {
        var user = new User
        {
            Id = "user-1",
            Email = "test@example.com",
            DisplayName = "Test",
            Preferences = new UserPreferences
            {
                FavoriteCuisines = ["Italian", "Japanese"],
                Diets = ["vegetarian"],
                Intolerances = ["gluten"],
            },
        };
        var mock = CreateUserRepoMock(user);
        var svc = CreateService(mock.Object);

        var prefs = await svc.GetUserPreferencesAsync("user-1");

        Assert.Equal(["Italian", "Japanese"], prefs.FavoriteCuisines);
        Assert.Equal(["vegetarian"], prefs.Diets);
        Assert.Equal(["gluten"], prefs.Intolerances);
    }

    // ── GetExcludedIngredientIdsAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetExcludedIngredientIdsAsync_WhenNoDislikedIngredients_ReturnsEmpty()
    {
        var user = new User
        {
            Id = "user-1",
            Email = "test@example.com",
            DisplayName = "Test",
            Preferences = new UserPreferences { DislikedIngredientIds = [] },
        };
        var mock = CreateUserRepoMock(user);
        var svc = CreateService(mock.Object);

        var ids = await svc.GetExcludedIngredientIdsAsync("user-1");

        Assert.Empty(ids);
    }

    [Fact]
    public async Task GetExcludedIngredientIdsAsync_ReturnsDislikedIngredientIds()
    {
        var user = new User
        {
            Id = "user-1",
            Email = "test@example.com",
            DisplayName = "Test",
            Preferences = new UserPreferences
            {
                DislikedIngredientIds = ["ing-1", "ing-2", "ing-3"],
            },
        };
        var mock = CreateUserRepoMock(user);
        var svc = CreateService(mock.Object);

        var ids = await svc.GetExcludedIngredientIdsAsync("user-1");

        Assert.Equal(3, ids.Count);
        Assert.Contains("ing-1", ids);
        Assert.Contains("ing-2", ids);
        Assert.Contains("ing-3", ids);
    }

    // ── ApplyPreferencesToSearch ──────────────────────────────────────────────

    [Fact]
    public void ApplyPreferencesToSearch_WithNoPreferencesAndNoFilters_ReturnsDefaultFilters()
    {
        var svc = CreateService();
        var prefs = new UserPreferences();

        var result = svc.ApplyPreferencesToSearch(null, prefs);

        Assert.Null(result.Intolerances);
        Assert.Null(result.Diet);
        Assert.Null(result.Cuisine);
        Assert.Equal(10, result.Number);
    }

    [Fact]
    public void ApplyPreferencesToSearch_IntolerancesAppliedAsStrictExclusion()
    {
        var svc = CreateService();
        var prefs = new UserPreferences
        {
            Intolerances = ["gluten", "dairy"],
        };

        var result = svc.ApplyPreferencesToSearch(null, prefs);

        Assert.NotNull(result.Intolerances);
        var parts = result.Intolerances.Split(',');
        Assert.Contains("gluten", parts);
        Assert.Contains("dairy", parts);
    }

    [Fact]
    public void ApplyPreferencesToSearch_DietAppliedAsDeprioritisation()
    {
        var svc = CreateService();
        var prefs = new UserPreferences
        {
            Diets = ["vegetarian", "vegan"],
        };

        var result = svc.ApplyPreferencesToSearch(null, prefs);

        // First diet in the list is used (Spoonacular supports a single diet value)
        Assert.Equal("vegetarian", result.Diet);
    }

    [Fact]
    public void ApplyPreferencesToSearch_CuisinesAppliedToFilter()
    {
        var svc = CreateService();
        var prefs = new UserPreferences
        {
            FavoriteCuisines = ["Italian", "Japanese"],
        };

        var result = svc.ApplyPreferencesToSearch(null, prefs);

        Assert.NotNull(result.Cuisine);
        var parts = result.Cuisine.Split(',');
        Assert.Contains("Italian", parts);
        Assert.Contains("Japanese", parts);
    }

    [Fact]
    public void ApplyPreferencesToSearch_ExistingFiltersPreserved()
    {
        var svc = CreateService();
        var existing = new ComplexSearchFilters
        {
            Cuisine = "French",
            Diet = "paleo",
            Intolerances = "peanut",
            MaxReadyTime = 30,
            Number = 5,
        };
        var prefs = new UserPreferences
        {
            FavoriteCuisines = ["Italian"],
            Diets = ["vegetarian"],
            Intolerances = ["gluten"],
        };

        var result = svc.ApplyPreferencesToSearch(existing, prefs);

        // Existing filter diet takes precedence
        Assert.Equal("paleo", result.Diet);
        // Cuisines merged
        Assert.Contains("French", result.Cuisine!);
        Assert.Contains("Italian", result.Cuisine!);
        // Intolerances merged
        Assert.Contains("peanut", result.Intolerances!);
        Assert.Contains("gluten", result.Intolerances!);
        // Preserved settings
        Assert.Equal(30, result.MaxReadyTime);
        Assert.Equal(5, result.Number);
    }

    [Fact]
    public void ApplyPreferencesToSearch_IntolerancesMergedWithExisting()
    {
        var svc = CreateService();
        var existing = new ComplexSearchFilters { Intolerances = "peanut,dairy" };
        var prefs = new UserPreferences { Intolerances = ["gluten", "dairy"] }; // dairy is duplicate

        var result = svc.ApplyPreferencesToSearch(existing, prefs);

        var parts = result.Intolerances!.Split(',');
        Assert.Contains("peanut", parts);
        Assert.Contains("gluten", parts);
        Assert.Contains("dairy", parts);
        // No duplicates
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public void ApplyPreferencesToSearch_PreservesNumberAndMaxReadyTime()
    {
        var svc = CreateService();
        var filters = new ComplexSearchFilters { Number = 20, MaxReadyTime = 45 };

        var result = svc.ApplyPreferencesToSearch(filters, new UserPreferences());

        Assert.Equal(20, result.Number);
        Assert.Equal(45, result.MaxReadyTime);
    }
}

/// <summary>
/// Unit tests for <see cref="PreferenceLists"/> validation helpers.
/// </summary>
public class PreferenceValidationTests
{
    // ── Valid values ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Italian")]
    [InlineData("Japanese")]
    [InlineData("Mexican")]
    [InlineData("Mediterranean")]
    public void GetInvalidCuisines_WithValidValue_ReturnsEmpty(string cuisine)
    {
        var invalid = PreferenceLists.GetInvalidCuisines([cuisine]);
        Assert.Empty(invalid);
    }

    [Theory]
    [InlineData("vegetarian")]
    [InlineData("vegan")]
    [InlineData("gluten free")]
    [InlineData("ketogenic")]
    public void GetInvalidDiets_WithValidValue_ReturnsEmpty(string diet)
    {
        var invalid = PreferenceLists.GetInvalidDiets([diet]);
        Assert.Empty(invalid);
    }

    [Theory]
    [InlineData("gluten")]
    [InlineData("dairy")]
    [InlineData("peanut")]
    [InlineData("tree nut")]
    public void GetInvalidIntolerances_WithValidValue_ReturnsEmpty(string intolerance)
    {
        var invalid = PreferenceLists.GetInvalidIntolerances([intolerance]);
        Assert.Empty(invalid);
    }

    [Theory]
    [InlineData("main course")]
    [InlineData("dessert")]
    [InlineData("breakfast")]
    public void GetInvalidDishTypes_WithValidValue_ReturnsEmpty(string dishType)
    {
        var invalid = PreferenceLists.GetInvalidDishTypes([dishType]);
        Assert.Empty(invalid);
    }

    // ── Invalid values ────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Klingon")]
    [InlineData("FakeCuisine")]
    [InlineData("")]
    public void GetInvalidCuisines_WithInvalidValue_ReturnsIt(string cuisine)
    {
        var invalid = PreferenceLists.GetInvalidCuisines([cuisine]);
        Assert.Contains(cuisine, invalid);
    }

    [Theory]
    [InlineData("carnivore")]
    [InlineData("raw food")]
    [InlineData("")]
    public void GetInvalidDiets_WithInvalidValue_ReturnsIt(string diet)
    {
        var invalid = PreferenceLists.GetInvalidDiets([diet]);
        Assert.Contains(diet, invalid);
    }

    [Theory]
    [InlineData("latex")]
    [InlineData("corn")]
    [InlineData("")]
    public void GetInvalidIntolerances_WithInvalidValue_ReturnsIt(string intolerance)
    {
        var invalid = PreferenceLists.GetInvalidIntolerances([intolerance]);
        Assert.Contains(intolerance, invalid);
    }

    // ── Case-insensitivity ────────────────────────────────────────────────────

    [Theory]
    [InlineData("ITALIAN")]
    [InlineData("italian")]
    [InlineData("Italian")]
    public void GetInvalidCuisines_IsCaseInsensitive(string cuisine)
    {
        var invalid = PreferenceLists.GetInvalidCuisines([cuisine]);
        Assert.Empty(invalid);
    }

    [Theory]
    [InlineData("VEGETARIAN")]
    [InlineData("Vegetarian")]
    [InlineData("vegetarian")]
    public void GetInvalidDiets_IsCaseInsensitive(string diet)
    {
        var invalid = PreferenceLists.GetInvalidDiets([diet]);
        Assert.Empty(invalid);
    }

    // ── Multiple values ───────────────────────────────────────────────────────

    [Fact]
    public void GetInvalidCuisines_WithMixedValues_ReturnsOnlyInvalid()
    {
        var invalid = PreferenceLists.GetInvalidCuisines(["Italian", "FakeCuisine", "Mexican", "Xyz"]);
        Assert.Equal(2, invalid.Count);
        Assert.Contains("FakeCuisine", invalid);
        Assert.Contains("Xyz", invalid);
    }

    [Fact]
    public void GetInvalidIntolerances_WithAllValid_ReturnsEmpty()
    {
        var invalid = PreferenceLists.GetInvalidIntolerances(["gluten", "dairy", "peanut"]);
        Assert.Empty(invalid);
    }
}
