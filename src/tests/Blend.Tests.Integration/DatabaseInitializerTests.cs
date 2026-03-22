using Blend.Domain.Entities;
using Blend.Infrastructure.Cosmos.Configuration;
using Xunit;

namespace Blend.Tests.Integration;

/// <summary>
/// Integration tests for container creation on application startup.
/// Tests are skipped when the Cosmos DB emulator is not available.
/// </summary>
[Collection(CosmosEmulatorCollection.Name)]
public class DatabaseInitializerTests
{
    private readonly CosmosEmulatorFixture _fixture;

    public DatabaseInitializerTests(CosmosEmulatorFixture fixture)
    {
        _fixture = fixture;
    }

    [SkippableFact]
    public async Task EnsureDatabaseAsync_CreatesAllContainers()
    {
        Skip.If(!CosmosEmulatorFixture.IsAvailable, "Cosmos DB emulator is not available.");

        var initializer = _fixture.Initializer!;
        await initializer.EnsureDatabaseAsync();

        // Verify all expected containers are created
        var client = _fixture.Client!;
        var database = client.GetDatabase(_fixture.Options!.DatabaseName);

        foreach (var def in ContainerDefinitions.All)
        {
            var container = database.GetContainer(def.Name);
            var properties = await container.ReadContainerAsync();
            Assert.NotNull(properties);
            Assert.Equal(def.Name, properties.Resource.Id);
            Assert.Equal(def.PartitionKeyPath, properties.Resource.PartitionKeyPath);
        }
    }

    [SkippableFact]
    public async Task EnsureDatabaseAsync_IsIdempotent()
    {
        Skip.If(!CosmosEmulatorFixture.IsAvailable, "Cosmos DB emulator is not available.");

        var initializer = _fixture.Initializer!;

        // Run twice; second call should not throw
        await initializer.EnsureDatabaseAsync();
        await initializer.EnsureDatabaseAsync();
    }
}
