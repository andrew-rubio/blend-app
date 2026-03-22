using Blend.Infrastructure.Cosmos;
using Blend.Infrastructure.Cosmos.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Blend.Tests.Integration;

/// <summary>
/// xUnit collection fixture that provides a shared <see cref="CosmosClient"/> connected to the
/// Cosmos DB emulator. Tests are skipped automatically when the emulator is not available.
/// </summary>
/// <remarks>
/// Set the <c>COSMOS_EMULATOR_CONNECTION_STRING</c> environment variable to the emulator's
/// connection string before running the integration tests, e.g.:
/// <code>
/// AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==
/// </code>
/// </remarks>
public sealed class CosmosEmulatorFixture : IDisposable
{
    private const string ConnectionStringEnvVar = "COSMOS_EMULATOR_CONNECTION_STRING";
    private const string TestDatabaseName = "blend-test";

    public static readonly string? EmulatorConnectionString =
        Environment.GetEnvironmentVariable(ConnectionStringEnvVar);

    public static readonly bool IsAvailable = !string.IsNullOrWhiteSpace(EmulatorConnectionString);

    public CosmosClient? Client { get; private set; }
    public CosmosOptions? Options { get; private set; }
    public DatabaseInitializer? Initializer { get; private set; }

    public CosmosEmulatorFixture()
    {
        if (!IsAvailable)
        {
            return;
        }

        Options = new CosmosOptions
        {
            ConnectionString = EmulatorConnectionString,
            DatabaseName = TestDatabaseName,
            EnsureCreated = true,
        };

        Client = new CosmosClient(
            EmulatorConnectionString,
            new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                },
                // Emulator uses self-signed certificate; skip SSL validation for tests
                HttpClientFactory = () => new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
                }),
                ConnectionMode = ConnectionMode.Gateway,
            });

        Initializer = new DatabaseInitializer(
            Client,
            Microsoft.Extensions.Options.Options.Create(Options),
            NullLogger<DatabaseInitializer>.Instance);
    }

    /// <summary>Creates a repository for type <typeparamref name="T"/> on the given container.</summary>
    public CosmosRepository<T> GetRepository<T>(string containerName) where T : class
    {
        if (Client is null || Options is null)
        {
            throw new InvalidOperationException("Cosmos emulator fixture is not initialised.");
        }

        var container = Client.GetContainer(Options.DatabaseName, containerName);
        return new CosmosRepository<T>(container, NullLogger<CosmosRepository<T>>.Instance);
    }

    public void Dispose()
    {
        Client?.Dispose();
    }
}

/// <summary>xUnit collection definition for tests that share the emulator fixture.</summary>
[CollectionDefinition(Name)]
public sealed class CosmosEmulatorCollection : ICollectionFixture<CosmosEmulatorFixture>
{
    public const string Name = "CosmosEmulator";
}
