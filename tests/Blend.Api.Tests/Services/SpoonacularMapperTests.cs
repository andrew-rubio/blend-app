using Blend.Api.Services.Spoonacular;
using Xunit;

namespace Blend.Api.Tests.Services;

public class SpoonacularMapperTests
{
    [Fact]
    public void ToRecipeSummary_MapsAllFields()
    {
        var dto = new SpoonacularRecipeSummaryDto
        {
            Id = 1,
            Title = "Chicken Soup",
            Image = "https://example.com/img.jpg",
            UsedIngredientCount = 3,
            MissedIngredientCount = 1,
            Likes = 42
        };

        var result = SpoonacularMapper.ToRecipeSummary(dto);

        Assert.Equal(1, result.Id);
        Assert.Equal("Chicken Soup", result.Title);
        Assert.Equal("https://example.com/img.jpg", result.Image);
        Assert.Equal(3, result.UsedIngredientCount);
        Assert.Equal(1, result.MissedIngredientCount);
        Assert.Equal(42, result.Likes);
    }

    [Fact]
    public void ToRecipeSummary_NullOptionalFields_DoesNotThrow()
    {
        var dto = new SpoonacularRecipeSummaryDto { Id = 2, Title = "Test" };

        var result = SpoonacularMapper.ToRecipeSummary(dto);

        Assert.Equal(2, result.Id);
        Assert.Null(result.Image);
        Assert.Null(result.UsedIngredientCount);
        Assert.Null(result.Likes);
    }

    [Fact]
    public void ToRecipeDetail_MapsAllFields()
    {
        var dto = new SpoonacularRecipeDetailDto
        {
            Id = 10,
            Title = "Pasta Bolognese",
            Image = "https://example.com/pasta.jpg",
            ReadyInMinutes = 45,
            Servings = 4,
            Summary = "A classic Italian pasta dish.",
            Cuisines = ["Italian"],
            DishTypes = ["main course"],
            ExtendedIngredients =
            [
                new SpoonacularIngredientDto { Id = 100, Name = "pasta", Amount = 200, Unit = "g", Image = null }
            ],
            AnalyzedInstructions =
            [
                new SpoonacularInstructionDto
                {
                    Name = "",
                    Steps = [new SpoonacularStepDto { Number = 1, Step = "Cook pasta." }]
                }
            ],
            Vegetarian = false,
            Vegan = false,
            GlutenFree = false,
            DairyFree = true
        };

        var result = SpoonacularMapper.ToRecipeDetail(dto);

        Assert.Equal(10, result.Id);
        Assert.Equal("Pasta Bolognese", result.Title);
        Assert.Equal(45, result.ReadyInMinutes);
        Assert.Equal(4, result.Servings);
        Assert.Equal("A classic Italian pasta dish.", result.Summary);
        Assert.Single(result.Cuisines);
        Assert.Equal("Italian", result.Cuisines[0]);
        Assert.Single(result.Ingredients);
        Assert.Equal("pasta", result.Ingredients[0].Name);
        Assert.Single(result.Instructions);
        Assert.Single(result.Instructions[0].Steps);
        Assert.Equal("Cook pasta.", result.Instructions[0].Steps[0].Step);
        Assert.False(result.Vegetarian);
        Assert.True(result.DairyFree);
    }

    [Fact]
    public void ToRecipeDetail_EmptyCollections_DoNotThrow()
    {
        var dto = new SpoonacularRecipeDetailDto { Id = 5, Title = "Minimal" };

        var result = SpoonacularMapper.ToRecipeDetail(dto);

        Assert.Empty(result.Cuisines);
        Assert.Empty(result.Ingredients);
        Assert.Empty(result.Instructions);
    }

    [Fact]
    public void ToIngredientSubstitute_MapsAllFields()
    {
        var dto = new SpoonacularSubstituteResponse
        {
            Ingredient = "butter",
            Substitutes = ["margarine", "coconut oil"],
            Message = "Use in equal amounts."
        };

        var result = SpoonacularMapper.ToIngredientSubstitute(dto);

        Assert.Equal("butter", result.IngredientName);
        Assert.Equal(2, result.Substitutes.Count);
        Assert.Contains("margarine", result.Substitutes);
        Assert.Equal("Use in equal amounts.", result.Message);
    }

    [Fact]
    public void ToIngredientSubstitute_NullMessage_IsAllowed()
    {
        var dto = new SpoonacularSubstituteResponse { Ingredient = "egg", Substitutes = [] };

        var result = SpoonacularMapper.ToIngredientSubstitute(dto);

        Assert.Equal("egg", result.IngredientName);
        Assert.Null(result.Message);
    }
}
