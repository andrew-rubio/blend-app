using Blend.Domain.Entities;
using Blend.Domain.Interfaces;
using Blend.Infrastructure.Cosmos.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BlendUser = Blend.Domain.Entities.User;

namespace Blend.Infrastructure.Cosmos.Seeding;

/// <summary>
/// Loads development seed data into Cosmos DB containers.
/// Only runs in development/local environments.
/// </summary>
public class SeedDataProvider
{
    private readonly CosmosClient _client;
    private readonly CosmosOptions _options;
    private readonly ILogger<SeedDataProvider> _logger;

    public SeedDataProvider(
        CosmosClient client,
        IOptions<CosmosOptions> options,
        ILogger<SeedDataProvider> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Seeds all containers with development data. Idempotent — skips existing records.</summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding development data...");

        var database = _client.GetDatabase(_options.DatabaseName);

        await SeedUsersAsync(database, cancellationToken);
        await SeedRecipesAsync(database, cancellationToken);
        await SeedContentAsync(database, cancellationToken);

        _logger.LogInformation("Seed data complete");
    }

    private async Task SeedUsersAsync(Database database, CancellationToken cancellationToken)
    {
        var container = database.GetContainer(_options.Containers.Users);

        var seedUsers = new[]
        {
            new BlendUser
            {
                Id = "seed-user-alice",
                Email = "alice@example.com",
                DisplayName = "Alice Chef",
                Bio = "Home cook and food enthusiast.",
                AuthProvider = AuthProvider.Local,
                IsActive = true,
                Preferences = new UserPreferences
                {
                    CookingSkillLevel = SkillLevel.Intermediate,
                    CuisinePreferences = ["Italian", "Mediterranean"],
                    DietaryRestrictions = ["Vegetarian"],
                    MaxCookTimeMinutes = 45,
                    ServingSize = 2
                }
            },
            new BlendUser
            {
                Id = "seed-user-bob",
                Email = "bob@example.com",
                DisplayName = "Bob Baker",
                AuthProvider = AuthProvider.Google,
                IsActive = true,
                Preferences = new UserPreferences
                {
                    CookingSkillLevel = SkillLevel.Beginner,
                    CuisinePreferences = ["American", "Mexican"],
                    MaxCookTimeMinutes = 30,
                    ServingSize = 4
                }
            }
        };

        foreach (var user in seedUsers)
        {
            await UpsertIfNotExistsAsync(container, user, user.Id, cancellationToken);
        }
    }

    private async Task SeedRecipesAsync(Database database, CancellationToken cancellationToken)
    {
        var container = database.GetContainer(_options.Containers.Recipes);

        var seedRecipes = new[]
        {
            new Recipe
            {
                Id = "seed-recipe-pasta",
                AuthorId = "seed-user-alice",
                Title = "Classic Tomato Pasta",
                Description = "A simple and delicious pasta with fresh tomato sauce.",
                IsPublished = true,
                Tags = ["pasta", "italian", "vegetarian", "quick"],
                Ingredients =
                [
                    new RecipeIngredient { Name = "Spaghetti", Quantity = 400, Unit = "g" },
                    new RecipeIngredient { Name = "Tomatoes", Quantity = 4, Unit = "whole" },
                    new RecipeIngredient { Name = "Garlic", Quantity = 3, Unit = "cloves" },
                    new RecipeIngredient { Name = "Olive Oil", Quantity = 3, Unit = "tbsp" },
                    new RecipeIngredient { Name = "Fresh Basil", Quantity = 10, Unit = "leaves" }
                ],
                Steps =
                [
                    new RecipeStep { Order = 1, Instruction = "Boil salted water and cook spaghetti al dente.", TimerMinutes = 10 },
                    new RecipeStep { Order = 2, Instruction = "Sauté garlic in olive oil for 1 minute.", TimerMinutes = 1 },
                    new RecipeStep { Order = 3, Instruction = "Add diced tomatoes and simmer for 10 minutes.", TimerMinutes = 10 },
                    new RecipeStep { Order = 4, Instruction = "Combine pasta with sauce and top with basil." }
                ],
                Metadata = new RecipeMetadata
                {
                    PrepTimeMinutes = 10,
                    CookTimeMinutes = 20,
                    Servings = 4,
                    Difficulty = DifficultyLevel.Easy,
                    Cuisine = "Italian",
                    DietaryLabels = ["Vegetarian"]
                }
            }
        };

        foreach (var recipe in seedRecipes)
        {
            await UpsertIfNotExistsAsync(container, recipe, recipe.AuthorId, cancellationToken);
        }
    }

    private async Task SeedContentAsync(Database database, CancellationToken cancellationToken)
    {
        var container = database.GetContainer(_options.Containers.Content);

        var seedContent = new[]
        {
            new Content
            {
                Id = "seed-content-welcome-banner",
                ContentType = "banner",
                Title = "Welcome to Blend!",
                Slug = "welcome-banner",
                Body = "Discover, cook, and share amazing recipes with friends.",
                Status = ContentStatus.Published,
                PublishedAt = DateTimeOffset.UtcNow,
                SortOrder = 1,
                AuthorId = "system"
            }
        };

        foreach (var content in seedContent)
        {
            await UpsertIfNotExistsAsync(container, content, content.ContentType, cancellationToken);
        }
    }

    private async Task UpsertIfNotExistsAsync<T>(
        Container container,
        T item,
        string partitionKey,
        CancellationToken cancellationToken)
        where T : CosmosEntity
    {
        try
        {
            await container.ReadItemAsync<T>(
                item.Id,
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);

            _logger.LogDebug("Seed item {Id} already exists, skipping", item.Id);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await container.CreateItemAsync(item, new PartitionKey(partitionKey), cancellationToken: cancellationToken);
            _logger.LogInformation("Seeded {Type} with id {Id}", typeof(T).Name, item.Id);
        }
    }
}
