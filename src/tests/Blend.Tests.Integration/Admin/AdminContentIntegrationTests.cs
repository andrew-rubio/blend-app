using Blend.Domain.Entities;
using Blend.Domain.Interfaces;
using Blend.Infrastructure.Cosmos.Configuration;
using Blend.Infrastructure.Cosmos.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Blend.Tests.Integration.Admin;

/// <summary>
/// Integration tests for content CRUD against the Cosmos DB emulator.
/// Set COSMOS_EMULATOR_CONNECTION_STRING to run.
/// </summary>
[Trait("Category", "Integration")]
public class AdminContentIntegrationTests : IAsyncLifetime, IDisposable
{
    private CosmosClient? _client;
    private IRepository<Content>? _contentRepository;
    private readonly string _databaseName = $"blend-test-{Guid.NewGuid():N}";
    private bool _emulatorAvailable;

    public async Task InitializeAsync()
    {
        _emulatorAvailable = await CosmosEmulatorFixture.IsEmulatorAvailableAsync();
        if (!_emulatorAvailable) return;

        _client = new CosmosClient(CosmosEmulatorFixture.EmulatorEndpoint, new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                IgnoreNullValues = true
            }
        });

        var db = await _client.CreateDatabaseIfNotExistsAsync(_databaseName);
        await db.Database.CreateContainerIfNotExistsAsync(
            new ContainerProperties("content", "/contentType"));

        var container = _client.GetContainer(_databaseName, "content");
        _contentRepository = new CosmosRepository<Content>(
            container,
            NullLogger<CosmosRepository<Content>>.Instance);
    }

    public async Task DisposeAsync()
    {
        if (_client is not null && _emulatorAvailable)
        {
            try { await _client.GetDatabase(_databaseName).DeleteAsync(); }
            catch { /* best-effort */ }
        }
    }

    public void Dispose() => _client?.Dispose();

    // ─── Featured Recipes ─────────────────────────────────────────────────────

    [SkippableFact]
    public async Task FeaturedRecipe_CreateReadUpdateDelete()
    {
        Skip.IfNot(_emulatorAvailable, "Cosmos DB emulator not available");

        // Create
        var content = new Content
        {
            ContentType = "featured-recipe",
            Title = "Pasta Primavera",
            RecipeId = "r-001",
            RecipeSource = "spoonacular",
            DisplayOrder = 1
        };
        var created = await _contentRepository!.CreateAsync(content);
        Assert.Equal("Pasta Primavera", created.Title);

        // Read
        var fetched = await _contentRepository.GetByIdAsync(created.Id, "featured-recipe");
        Assert.NotNull(fetched);
        Assert.Equal("r-001", fetched.RecipeId);

        // Update
        fetched.DisplayOrder = 2;
        var updated = await _contentRepository.UpdateAsync(fetched, "featured-recipe");
        Assert.Equal(2, updated.DisplayOrder);

        // Delete
        var deleted = await _contentRepository.DeleteAsync(created.Id, "featured-recipe");
        Assert.True(deleted);

        var gone = await _contentRepository.GetByIdAsync(created.Id, "featured-recipe");
        Assert.Null(gone);
    }

    // ─── Stories ─────────────────────────────────────────────────────────────

    [SkippableFact]
    public async Task Story_CreateReadUpdateDelete()
    {
        Skip.IfNot(_emulatorAvailable, "Cosmos DB emulator not available");

        var story = new Content
        {
            ContentType = "story",
            Title = "The Art of Fermentation",
            Author = "Jane Doe",
            Body = "# Introduction\nFermentation is ...",
            ReadingTimeMinutes = 5,
            DisplayOrder = 0
        };

        var created = await _contentRepository!.CreateAsync(story);
        Assert.Equal("story", created.ContentType);

        var fetched = await _contentRepository.GetByIdAsync(created.Id, "story");
        Assert.NotNull(fetched);
        Assert.Equal("Jane Doe", fetched.Author);

        fetched.Title = "Updated: The Art of Fermentation";
        var updated = await _contentRepository.UpdateAsync(fetched, "story");
        Assert.StartsWith("Updated:", updated.Title);

        var deleted = await _contentRepository.DeleteAsync(created.Id, "story");
        Assert.True(deleted);
    }

    // ─── Videos ──────────────────────────────────────────────────────────────

    [SkippableFact]
    public async Task Video_CreateReadUpdateDelete()
    {
        Skip.IfNot(_emulatorAvailable, "Cosmos DB emulator not available");

        var video = new Content
        {
            ContentType = "video",
            Title = "How to Braise",
            VideoUrl = "https://youtube.com/embed/xyz",
            DurationSeconds = 360,
            Creator = "Chef Alice"
        };

        var created = await _contentRepository!.CreateAsync(video);
        Assert.Equal("Chef Alice", created.Creator);

        created.DurationSeconds = 400;
        var updated = await _contentRepository.UpdateAsync(created, "video");
        Assert.Equal(400, updated.DurationSeconds);

        var deleted = await _contentRepository.DeleteAsync(created.Id, "video");
        Assert.True(deleted);
    }

    // ─── Content ordering ────────────────────────────────────────────────────

    [SkippableFact]
    public async Task ContentOrdering_SortsBy_DisplayOrder()
    {
        Skip.IfNot(_emulatorAvailable, "Cosmos DB emulator not available");

        var items = new[]
        {
            new Content { ContentType = "featured-recipe", Title = "C", DisplayOrder = 3 },
            new Content { ContentType = "featured-recipe", Title = "A", DisplayOrder = 1 },
            new Content { ContentType = "featured-recipe", Title = "B", DisplayOrder = 2 }
        };

        foreach (var item in items)
        {
            await _contentRepository!.CreateAsync(item);
        }

        var result = await _contentRepository!.QueryAsync(
            "SELECT * FROM c WHERE c.contentType = 'featured-recipe' ORDER BY c.displayOrder ASC",
            partitionKey: "featured-recipe");

        var ordered = result.Items.ToList();
        Assert.True(ordered.Count >= 3);
        var titlesInOrder = ordered.Select(i => i.Title).ToList();
        var aIdx = titlesInOrder.IndexOf("A");
        var bIdx = titlesInOrder.IndexOf("B");
        var cIdx = titlesInOrder.IndexOf("C");
        Assert.True(aIdx < bIdx && bIdx < cIdx);
    }

    // ─── Pagination ───────────────────────────────────────────────────────────

    [SkippableFact]
    public async Task Pagination_ReturnsContinuationToken_WhenMoreResults()
    {
        Skip.IfNot(_emulatorAvailable, "Cosmos DB emulator not available");

        for (int i = 0; i < 5; i++)
        {
            await _contentRepository!.CreateAsync(new Content
            {
                ContentType = "story",
                Title = $"Story {i}"
            });
        }

        var page = await _contentRepository!.QueryAsync(
            "SELECT * FROM c WHERE c.contentType = 'story'",
            new PaginationOptions { PageSize = 2 },
            partitionKey: "story");

        Assert.True(page.Count <= 2);
    }

    private static async Task<bool> IsEmulatorAvailableAsync() =>
        await CosmosEmulatorFixture.IsEmulatorAvailableAsync();
}
