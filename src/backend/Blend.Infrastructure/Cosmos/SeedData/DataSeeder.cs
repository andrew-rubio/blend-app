using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Blend.Infrastructure.Cosmos.SeedData;

/// <summary>
/// Seeds development data (sample users, recipes, content) into Cosmos DB.
/// Only runs when the application environment is Development.
/// </summary>
public sealed class DataSeeder
{
    private readonly IRepository<User> _users;
    private readonly IRepository<Recipe> _recipes;
    private readonly IRepository<Content> _content;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(
        IRepository<User> users,
        IRepository<Recipe> recipes,
        IRepository<Content> content,
        ILogger<DataSeeder> logger)
    {
        _users = users;
        _recipes = recipes;
        _content = content;
        _logger = logger;
    }

    /// <summary>Seeds all development data if the collections are empty.</summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking whether development seed data is needed...");
        await SeedUsersAsync(cancellationToken);
        await SeedRecipesAsync(cancellationToken);
        await SeedContentAsync(cancellationToken);
        _logger.LogInformation("Seed data check complete.");
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        var existing = await _users.GetByQueryAsync(
            "SELECT VALUE COUNT(1) FROM c",
            cancellationToken: cancellationToken);

        if (existing.Count > 0)
        {
            _logger.LogDebug("Users container already has data; skipping user seed.");
            return;
        }

        var seedUsers = new[]
        {
            new User
            {
                Id = "seed-user-1",
                Email = "alice@example.com",
                DisplayName = "Alice Smith",
                Role = UserRole.User,
                MeasurementUnit = MeasurementUnit.Metric,
                Preferences = new UserPreferences
                {
                    FavoriteCuisines = ["Italian", "Japanese"],
                    Diets = ["vegetarian"],
                },
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            },
            new User
            {
                Id = "seed-user-2",
                Email = "bob@example.com",
                DisplayName = "Bob Jones",
                Role = UserRole.User,
                MeasurementUnit = MeasurementUnit.Imperial,
                Preferences = new UserPreferences
                {
                    FavoriteCuisines = ["American", "Mexican"],
                },
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            },
            new User
            {
                Id = "seed-admin-1",
                Email = "admin@example.com",
                DisplayName = "Admin",
                Role = UserRole.Admin,
                MeasurementUnit = MeasurementUnit.Metric,
                Preferences = new UserPreferences(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            },
        };

        foreach (var user in seedUsers)
        {
            await _users.CreateAsync(user, cancellationToken);
            _logger.LogDebug("Seeded user '{DisplayName}'.", user.DisplayName);
        }
    }

    private async Task SeedRecipesAsync(CancellationToken cancellationToken)
    {
        var existing = await _recipes.GetByQueryAsync(
            "SELECT VALUE COUNT(1) FROM c",
            cancellationToken: cancellationToken);

        if (existing.Count > 0)
        {
            _logger.LogDebug("Recipes container already has data; skipping recipe seed.");
            return;
        }

        var seedRecipes = new[]
        {
            new Recipe
            {
                Id = "seed-recipe-1",
                AuthorId = "seed-user-1",
                Title = "Simple Tomato Pasta",
                Description = "A quick and easy weeknight pasta.",
                Ingredients =
                [
                    new RecipeIngredient { Quantity = 200, Unit = "g", IngredientName = "spaghetti" },
                    new RecipeIngredient { Quantity = 400, Unit = "g", IngredientName = "canned tomatoes" },
                    new RecipeIngredient { Quantity = 2,   Unit = "cloves", IngredientName = "garlic" },
                ],
                Directions =
                [
                    new RecipeDirection { StepNumber = 1, Text = "Boil spaghetti according to package instructions." },
                    new RecipeDirection { StepNumber = 2, Text = "Fry garlic in olive oil, add tomatoes, simmer 10 minutes." },
                    new RecipeDirection { StepNumber = 3, Text = "Toss pasta with sauce and serve." },
                ],
                PrepTime = 5,
                CookTime = 20,
                Servings = 2,
                CuisineType = "Italian",
                DishType = "main course",
                Tags = ["quick", "vegetarian"],
                IsPublic = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            },
        };

        foreach (var recipe in seedRecipes)
        {
            await _recipes.CreateAsync(recipe, cancellationToken);
            _logger.LogDebug("Seeded recipe '{Title}'.", recipe.Title);
        }
    }

    private async Task SeedContentAsync(CancellationToken cancellationToken)
    {
        var existing = await _content.GetByQueryAsync(
            "SELECT VALUE COUNT(1) FROM c",
            cancellationToken: cancellationToken);

        if (existing.Count > 0)
        {
            _logger.LogDebug("Content container already has data; skipping content seed.");
            return;
        }

        var seedContent = new[]
        {
            new Content
            {
                Id = "seed-content-1",
                ContentType = ContentType.Story,
                Title = "Welcome to Blend",
                Body = "Discover new recipes, track your cooking, and connect with friends.",
                AuthorName = "The Blend Team",
                IsPublished = true,
                PublishedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            },
        };

        foreach (var item in seedContent)
        {
            await _content.CreateAsync(item, cancellationToken);
            _logger.LogDebug("Seeded content '{Title}'.", item.Title);
        }
    }
}
