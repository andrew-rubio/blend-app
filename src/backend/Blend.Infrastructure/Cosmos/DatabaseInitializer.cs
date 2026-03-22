using Blend.Infrastructure.Cosmos.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blend.Infrastructure.Cosmos;

/// <summary>
/// Hosted service that ensures the Cosmos DB database and all required containers exist
/// with the correct partition keys, TTL settings, and indexing policies on application startup.
/// </summary>
public sealed class DatabaseInitializer
{
    private readonly CosmosClient _client;
    private readonly CosmosOptions _options;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        CosmosClient client,
        IOptions<CosmosOptions> options,
        ILogger<DatabaseInitializer> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Ensures the database and all containers defined in <see cref="ContainerDefinitions.All"/>
    /// exist with the correct configuration.
    /// </summary>
    public async Task EnsureDatabaseAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ensuring Cosmos DB database '{DatabaseName}' exists...", _options.DatabaseName);

        DatabaseResponse dbResponse;
        if (_options.ProvisionedThroughput.HasValue)
        {
            dbResponse = await _client.CreateDatabaseIfNotExistsAsync(
                _options.DatabaseName,
                ThroughputProperties.CreateManualThroughput(_options.ProvisionedThroughput.Value),
                cancellationToken: cancellationToken);
        }
        else
        {
            dbResponse = await _client.CreateDatabaseIfNotExistsAsync(
                _options.DatabaseName,
                cancellationToken: cancellationToken);
        }

        var database = dbResponse.Database;
        _logger.LogInformation("Database '{DatabaseName}' is ready.", _options.DatabaseName);

        foreach (var def in ContainerDefinitions.All)
        {
            await EnsureContainerAsync(database, def, cancellationToken);
        }
    }

    private async Task EnsureContainerAsync(
        Database database,
        ContainerDefinition def,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Ensuring container '{Container}' (partitionKey={PartitionKey}) exists...", def.Name, def.PartitionKeyPath);

        var properties = new ContainerProperties(def.Name, def.PartitionKeyPath);

        if (def.TtlSeconds.HasValue)
        {
            // -1 means: use per-document ttl; positive value = container default
            properties.DefaultTimeToLive = def.TtlSeconds.Value;
        }

        await database.CreateContainerIfNotExistsAsync(properties, cancellationToken: cancellationToken);

        _logger.LogDebug("Container '{Container}' is ready.", def.Name);
    }
}
