using Blend.Api.Recipes.Models;
using Blend.Api.Recipes.Services;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Blend.Tests.Unit.Recipes;

public class RecipeValidationTests
{
    private static RecipeService CreateService(
        IRepository<Recipe>? recipeRepo = null,
        IRepository<Activity>? activityRepo = null) =>
        new(NullLogger<RecipeService>.Instance, recipeRepo, activityRepo);

    private static CreateRecipeRequest ValidPrivateRequest() => new()
    {
        Title = "Test Recipe",
        Ingredients = [],
        Directions = [],
        Tags = [],
        Photos = [],
        IsPublic = false,
    };

    private static CreateRecipeRequest ValidPublicRequest() => new()
    {
        Title = "Test Recipe",
        Ingredients = [new RecipeIngredientRequest { Quantity = 1.0, Unit = "cup", IngredientName = "Flour" }],
        Directions = [new RecipeDirectionRequest { StepNumber = 1, Text = "Mix everything." }],
        Tags = [],
        Photos = [],
        IsPublic = true,
    };

    // ── Title validation ──────────────────────────────────────────────────────

    [Fact]
    public void ValidateCreateRecipe_EmptyTitle_ReturnsError()
    {
        var svc = CreateService();
        var request = new CreateRecipeRequest { Title = "", Ingredients = [], Directions = [], Tags = [], Photos = [], IsPublic = false };
        var errors = svc.ValidateCreateRecipe(request);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void ValidateCreateRecipe_TitleOver200Chars_ReturnsError()
    {
        var svc = CreateService();
        var request = new CreateRecipeRequest { Title = new string('x', 201), Ingredients = [], Directions = [], Tags = [], Photos = [], IsPublic = false };
        var errors = svc.ValidateCreateRecipe(request);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void ValidateCreateRecipe_Title200Chars_NoError()
    {
        var svc = CreateService();
        var request = new CreateRecipeRequest { Title = new string('x', 200), Ingredients = [], Directions = [], Tags = [], Photos = [], IsPublic = false };
        var errors = svc.ValidateCreateRecipe(request);
        Assert.Empty(errors);
    }

    // ── Ingredient/Direction validation ──────────────────────────────────────

    [Fact]
    public void ValidateCreateRecipe_IngredientQuantityZero_ReturnsError()
    {
        var svc = CreateService();
        var request = new CreateRecipeRequest
        {
            Title = "Test Recipe",
            Ingredients = [new RecipeIngredientRequest { Quantity = 0, Unit = "cup", IngredientName = "Flour" }],
            Directions = [new RecipeDirectionRequest { StepNumber = 1, Text = "Mix everything." }],
            Tags = [],
            Photos = [],
            IsPublic = true,
        };
        var errors = svc.ValidateCreateRecipe(request);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void ValidateCreateRecipe_IngredientQuantityNegative_ReturnsError()
    {
        var svc = CreateService();
        var request = new CreateRecipeRequest
        {
            Title = "Test Recipe",
            Ingredients = [new RecipeIngredientRequest { Quantity = -1.5, Unit = "cup", IngredientName = "Flour" }],
            Directions = [new RecipeDirectionRequest { StepNumber = 1, Text = "Mix everything." }],
            Tags = [],
            Photos = [],
            IsPublic = true,
        };
        var errors = svc.ValidateCreateRecipe(request);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void ValidateCreateRecipe_DirectionStepNumberZero_ReturnsError()
    {
        var svc = CreateService();
        var request = new CreateRecipeRequest
        {
            Title = "Test Recipe",
            Ingredients = [new RecipeIngredientRequest { Quantity = 1.0, Unit = "cup", IngredientName = "Flour" }],
            Directions = [new RecipeDirectionRequest { StepNumber = 0, Text = "Do something." }],
            Tags = [],
            Photos = [],
            IsPublic = true,
        };
        var errors = svc.ValidateCreateRecipe(request);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void ValidateCreateRecipe_PublicWithNoIngredients_ReturnsError()
    {
        var svc = CreateService();
        var request = new CreateRecipeRequest
        {
            Title = "Test Recipe",
            Ingredients = [],
            Directions = [new RecipeDirectionRequest { StepNumber = 1, Text = "Mix everything." }],
            Tags = [],
            Photos = [],
            IsPublic = true,
        };
        var errors = svc.ValidateCreateRecipe(request);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void ValidateCreateRecipe_PublicWithNoDirections_ReturnsError()
    {
        var svc = CreateService();
        var request = new CreateRecipeRequest
        {
            Title = "Test Recipe",
            Ingredients = [new RecipeIngredientRequest { Quantity = 1.0, Unit = "cup", IngredientName = "Flour" }],
            Directions = [],
            Tags = [],
            Photos = [],
            IsPublic = true,
        };
        var errors = svc.ValidateCreateRecipe(request);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void ValidateCreateRecipe_PrivateWithNoIngredients_NoError()
    {
        var svc = CreateService();
        var errors = svc.ValidateCreateRecipe(ValidPrivateRequest());
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateCreateRecipe_ValidPublicRecipe_NoErrors()
    {
        var svc = CreateService();
        var errors = svc.ValidateCreateRecipe(ValidPublicRequest());
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateUpdateRecipe_EmptyTitle_ReturnsError()
    {
        var svc = CreateService();
        var request = new UpdateRecipeRequest { Title = "", Ingredients = [], Directions = [], Tags = [], Photos = [] };
        var errors = svc.ValidateUpdateRecipe(request);
        Assert.NotEmpty(errors);
    }

    // ── Authorization tests ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdateRecipeAsync_NonOwner_ReturnsForbidden()
    {
        var recipeId = Guid.NewGuid().ToString();
        var authorId = "author-1";
        var otherId = "other-user";

        var recipe = new Recipe
        {
            Id = recipeId,
            AuthorId = authorId,
            Title = "Recipe",
            IsPublic = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var mockRepo = new Mock<IRepository<Recipe>>();
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([recipe]);
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([recipe]);

        var svc = CreateService(mockRepo.Object);
        var request = new UpdateRecipeRequest
        {
            Title = "New Title",
            Ingredients = [],
            Directions = [],
            Tags = [],
            Photos = [],
        };

        var (_, result, _) = await svc.UpdateRecipeAsync(recipeId, otherId, request, default);
        Assert.Equal(RecipeOpResult.Forbidden, result);
    }

    [Fact]
    public async Task UpdateRecipeAsync_Owner_ReturnsSuccess()
    {
        var recipeId = Guid.NewGuid().ToString();
        var authorId = "author-1";

        var recipe = new Recipe
        {
            Id = recipeId,
            AuthorId = authorId,
            Title = "Recipe",
            IsPublic = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var updatedRecipe = new Recipe
        {
            Id = recipeId,
            AuthorId = authorId,
            Title = "New Title",
            IsPublic = false,
            CreatedAt = recipe.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var mockRepo = new Mock<IRepository<Recipe>>();
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([recipe]);
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([recipe]);
        mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedRecipe);

        var svc = CreateService(mockRepo.Object);
        var request = new UpdateRecipeRequest
        {
            Title = "New Title",
            Ingredients = [],
            Directions = [],
            Tags = [],
            Photos = [],
        };

        var (resultRecipe, result, _) = await svc.UpdateRecipeAsync(recipeId, authorId, request, default);
        Assert.Equal(RecipeOpResult.Success, result);
        Assert.NotNull(resultRecipe);
    }
}
