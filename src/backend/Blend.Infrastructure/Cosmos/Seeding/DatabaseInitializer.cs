using Blend.Domain.Entities;
using Blend.Domain.Interfaces;
using Blend.Infrastructure.Cosmos.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blend.Infrastructure.Cosmos.Seeding;

/// <summary>
/// Ensures the Cosmos DB database and all containers are created on startup.
/// </summary>
public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly CosmosClient _client;
    private readonly CosmosOptions _options;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        CosmosClient client,
        IOptions<CosmosOptions> options,
        ILogger<DatabaseInitializer> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing Cosmos DB database '{Database}'", _options.DatabaseName);

        var databaseResponse = await _client.CreateDatabaseIfNotExistsAsync(
            _options.DatabaseName,
            cancellationToken: cancellationToken);

        var database = databaseResponse.Database;
        _logger.LogInformation("Database '{Database}' ready (status: {Status})",
            _options.DatabaseName, databaseResponse.StatusCode);

        foreach (var containerDef in ContainerDefinitions.All(_options))
        {
            await EnsureContainerAsync(database, containerDef, cancellationToken);
        }

        _logger.LogInformation("Cosmos DB initialization complete");
    }

    private async Task EnsureContainerAsync(
        Database database,
        ContainerDefinition containerDef,
        CancellationToken cancellationToken)
    {
        var properties = new ContainerProperties(containerDef.Name, containerDef.PartitionKeyPath);

        if (containerDef.DefaultTtlSeconds.HasValue)
        {
            properties.DefaultTimeToLive = containerDef.DefaultTtlSeconds.Value;
        }

        var response = await database.CreateContainerIfNotExistsAsync(
            properties,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Container '{Container}' ready (partition: {PartitionKey}, TTL: {Ttl}s, status: {Status})",
            containerDef.Name,
            containerDef.PartitionKeyPath,
            containerDef.DefaultTtlSeconds?.ToString() ?? "none",
            response.StatusCode);
    }
}
