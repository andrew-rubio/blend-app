using Blend.Domain.Entities;
using Blend.Domain.Interfaces;
using Blend.Infrastructure.Cosmos.Configuration;
using Blend.Infrastructure.Cosmos.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using BlendUser = Blend.Domain.Entities.User;

namespace Blend.Tests.Integration;

/// <summary>
/// Integration tests for CosmosRepository against the Cosmos DB emulator.
/// Skipped when the emulator is not available.
/// Set COSMOS_EMULATOR_CONNECTION_STRING environment variable to run these tests.
/// Default emulator endpoint: AccountEndpoint=https://localhost:8081/;AccountKey=C2y6y...
/// </summary>
[Trait("Category", "Integration")]
public class CosmosRepositoryIntegrationTests : IAsyncLifetime, IDisposable
{
    private const string EmulatorKey =
        "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMssZaR8r/8ZbT1A==";

    private static readonly string EmulatorEndpoint =
        Environment.GetEnvironmentVariable("COSMOS_EMULATOR_CONNECTION_STRING")
        ?? $"AccountEndpoint=https://localhost:8081/;AccountKey={EmulatorKey}";

    private CosmosClient? _client;
    private IRepository<BlendUser>? _userRepository;
    private readonly string _databaseName = $"blend-test-{Guid.NewGuid():N}";
    private readonly List<string> _createdIds = [];
    private bool _emulatorAvailable;

    public async Task InitializeAsync()
    {
        _emulatorAvailable = await IsEmulatorAvailableAsync();
        if (!_emulatorAvailable) return;

        var opts = new CosmosOptions
        {
            ConnectionString = EmulatorEndpoint,
            DatabaseName = _databaseName,
            MaxRetryAttemptsOnRateLimitedRequests = 0,
            RequestTimeoutSeconds = 10
        };

        _client = new CosmosClient(EmulatorEndpoint, new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                IgnoreNullValues = true
            }
        });

        // Create test database and container
        var db = await _client.CreateDatabaseIfNotExistsAsync(_databaseName);
        await db.Database.CreateContainerIfNotExistsAsync(
            new ContainerProperties("users", "/id"));

        var container = _client.GetContainer(_databaseName, "users");
        _userRepository = new CosmosRepository<BlendUser>(
            container,
            NullLogger<CosmosRepository<BlendUser>>.Instance);
    }

    public async Task DisposeAsync()
    {
        if (_client is not null && _emulatorAvailable)
        {
            try
            {
                var db = _client.GetDatabase(_databaseName);
                await db.DeleteAsync();
            }
            catch { /* best-effort cleanup */ }
        }
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    [SkippableFact]
    public async Task CreateAsync_StoresAndReturnsEntity()
    {
        Skip.IfNot(_emulatorAvailable, "Cosmos DB emulator not available");

        var user = new BlendUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            DisplayName = "Test User",
            AuthProvider = AuthProvider.Local
        };

        var created = await _userRepository!.CreateAsync(user);

        Assert.Equal(user.Id, created.Id);
        Assert.Equal("test@example.com", created.Email);
        _createdIds.Add(created.Id);
    }

    [SkippableFact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        Skip.IfNot(_emulatorAvailable, "Cosmos DB emulator not available");

        var result = await _userRepository!.GetByIdAsync("nonexistent", "nonexistent");
        Assert.Null(result);
    }

    [SkippableFact]
    public async Task CreateAndGetById_RoundTrip()
    {
        Skip.IfNot(_emulatorAvailable, "Cosmos DB emulator not available");

        var id = Guid.NewGuid().ToString();
        var user = new BlendUser
        {
            Id = id,
            Email = "roundtrip@example.com",
            DisplayName = "Round Trip User",
            AuthProvider = AuthProvider.Google
        };

        await _userRepository!.CreateAsync(user);
        var fetched = await _userRepository.GetByIdAsync(id, id);

        Assert.NotNull(fetched);
        Assert.Equal("roundtrip@example.com", fetched.Email);
        _createdIds.Add(id);
    }

    [SkippableFact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        Skip.IfNot(_emulatorAvailable, "Cosmos DB emulator not available");

        var result = await _userRepository!.DeleteAsync("does-not-exist", "does-not-exist");
        Assert.False(result);
    }

    [SkippableFact]
    public async Task ExistsAsync_ReturnsTrue_AfterCreate()
    {
        Skip.IfNot(_emulatorAvailable, "Cosmos DB emulator not available");

        var id = Guid.NewGuid().ToString();
        var user = new BlendUser { Id = id, Email = "exists@example.com", DisplayName = "Exists User" };

        await _userRepository!.CreateAsync(user);
        var exists = await _userRepository.ExistsAsync(id, id);

        Assert.True(exists);
        _createdIds.Add(id);
    }

    [SkippableFact]
    public async Task QueryAsync_ReturnsPaginatedResults()
    {
        Skip.IfNot(_emulatorAvailable, "Cosmos DB emulator not available");

        // Create a few users
        for (int i = 0; i < 3; i++)
        {
            var id = Guid.NewGuid().ToString();
            var user = new BlendUser { Id = id, Email = $"page{i}@example.com", DisplayName = $"Page User {i}" };
            await _userRepository!.CreateAsync(user);
            _createdIds.Add(id);
        }

        var page = await _userRepository!.QueryAsync(
            "SELECT * FROM c",
            new PaginationOptions { PageSize = 2 });

        Assert.True(page.Count <= 2);
    }

    private static async Task<bool> IsEmulatorAvailableAsync()
    {
        try
        {
            using var client = new CosmosClient(EmulatorEndpoint, new CosmosClientOptions
            {
                MaxRetryAttemptsOnRateLimitedRequests = 0
            });
            await client.ReadAccountAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
