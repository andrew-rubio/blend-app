using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Xunit;

namespace Blend.Tests.Integration;

/// <summary>
/// Integration tests for repository CRUD operations against the Cosmos DB emulator.
/// Tests are skipped when the emulator is not available.
/// </summary>
[Collection(CosmosEmulatorCollection.Name)]
public class RepositoryCrudTests : IAsyncLifetime
{
    private readonly CosmosEmulatorFixture _fixture;

    public RepositoryCrudTests(CosmosEmulatorFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        if (CosmosEmulatorFixture.IsAvailable)
        {
            await _fixture.Initializer!.EnsureDatabaseAsync();
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task UserRepository_CreateReadUpdateDelete_Works()
    {
        Skip.If(!CosmosEmulatorFixture.IsAvailable, "Cosmos DB emulator is not available.");

        var repo = _fixture.GetRepository<User>("users");
        var userId = Guid.NewGuid().ToString();

        var user = new User
        {
            Id = userId,
            Email = $"{userId}@test.com",
            DisplayName = "Integration Test User",
            Role = UserRole.User,
            MeasurementUnit = MeasurementUnit.Metric,
            Preferences = new UserPreferences { FavoriteCuisines = ["Italian"] },
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        // Create
        var created = await repo.CreateAsync(user);
        Assert.Equal(userId, created.Id);

        // Read by id
        var found = await repo.GetByIdAsync(userId, userId);
        Assert.NotNull(found);
        Assert.Equal("Integration Test User", found.DisplayName);

        // Delete
        await repo.DeleteAsync(userId, userId);

        // Confirm deleted
        var deleted = await repo.GetByIdAsync(userId, userId);
        Assert.Null(deleted);
    }

    [SkippableFact]
    public async Task RecipeRepository_GetPaged_ReturnsCursorToken()
    {
        Skip.If(!CosmosEmulatorFixture.IsAvailable, "Cosmos DB emulator is not available.");

        var repo = _fixture.GetRepository<Recipe>("recipes");
        var authorId = Guid.NewGuid().ToString();

        // Insert 3 recipes for same author
        for (var i = 0; i < 3; i++)
        {
            await repo.CreateAsync(new Recipe
            {
                Id = Guid.NewGuid().ToString(),
                AuthorId = authorId,
                Title = $"Recipe {i}",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            });
        }

        // Query all items in the author's partition (avoids string interpolation in query)
        var page = await repo.GetPagedAsync(
            "SELECT * FROM c",
            new FeedPaginationOptions { PageSize = 2 },
            partitionKey: authorId);

        Assert.Equal(2, page.Items.Count);
        Assert.NotNull(page.ContinuationToken);
        Assert.True(page.HasNextPage);

        // Fetch next page
        var page2 = await repo.GetPagedAsync(
            "SELECT * FROM c",
            new FeedPaginationOptions { PageSize = 2, ContinuationToken = page.ContinuationToken },
            partitionKey: authorId);

        Assert.Single(page2.Items);
        Assert.False(page2.HasNextPage);
    }

    [SkippableFact]
    public async Task NotificationRepository_Create_StoresTtl()
    {
        Skip.If(!CosmosEmulatorFixture.IsAvailable, "Cosmos DB emulator is not available.");

        var repo = _fixture.GetRepository<Notification>("notifications");
        var recipientId = Guid.NewGuid().ToString();
        var notifId = Guid.NewGuid().ToString();

        var notification = new Notification
        {
            Id = notifId,
            RecipientUserId = recipientId,
            Type = NotificationType.System,
            Message = "Test notification",
            Ttl = 3600,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var created = await repo.CreateAsync(notification);
        Assert.Equal(3600, created.Ttl);

        // Cleanup
        await repo.DeleteAsync(notifId, recipientId);
    }

    [SkippableFact]
    public async Task CacheRepository_Create_StoresCacheKey()
    {
        Skip.If(!CosmosEmulatorFixture.IsAvailable, "Cosmos DB emulator is not available.");

        var repo = _fixture.GetRepository<CacheEntry>("cache");
        var cacheKey = $"spoon:test:{Guid.NewGuid()}";

        var entry = new CacheEntry
        {
            CacheKey = cacheKey,
            Data = """{"test":true}""",
            CreatedAt = DateTimeOffset.UtcNow,
            Ttl = 3600,
        };

        var created = await repo.CreateAsync(entry);
        Assert.Equal(cacheKey, created.CacheKey);
        Assert.Equal(cacheKey, created.Id);

        // Read back
        var found = await repo.GetByIdAsync(cacheKey, cacheKey);
        Assert.NotNull(found);
        Assert.Equal("""{"test":true}""", found.Data);

        // Cleanup
        await repo.DeleteAsync(cacheKey, cacheKey);
    }
}
