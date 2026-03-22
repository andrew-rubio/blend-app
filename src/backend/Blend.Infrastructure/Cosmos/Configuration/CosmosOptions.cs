namespace Blend.Infrastructure.Cosmos.Configuration;

/// <summary>
/// Configuration options for the Azure Cosmos DB client, bound from the <c>CosmosDb</c>
/// section of application configuration.
/// </summary>
public sealed class CosmosOptions
{
    public const string SectionName = "CosmosDb";

    /// <summary>
    /// The Cosmos DB account endpoint URI (e.g. <c>https://account.documents.azure.com:443/</c>).
    /// Used when <see cref="ConnectionString"/> is not provided.
    /// </summary>
    public string? EndpointUri { get; set; }

    /// <summary>
    /// Primary or secondary account key. Used together with <see cref="EndpointUri"/> when
    /// <see cref="ConnectionString"/> is not provided.
    /// </summary>
    public string? AccountKey { get; set; }

    /// <summary>
    /// Full Cosmos DB connection string. When provided this takes precedence over
    /// <see cref="EndpointUri"/> / <see cref="AccountKey"/>.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>The name of the Cosmos DB database to use. Defaults to <c>"blend"</c>.</summary>
    public string DatabaseName { get; set; } = "blend";

    /// <summary>
    /// When <c>true</c> the application will attempt to create the database and all containers
    /// on startup if they do not already exist. Defaults to <c>true</c>.
    /// </summary>
    public bool EnsureCreated { get; set; } = true;

    /// <summary>
    /// Throughput for provisioned-throughput mode (RU/s). When <c>null</c> the database is
    /// created in serverless mode (development default). Defaults to <c>null</c>.
    /// </summary>
    public int? ProvisionedThroughput { get; set; }

    /// <summary>
    /// Maximum number of retries on rate-limited (429) requests. Defaults to 9.
    /// </summary>
    public int MaxRetryAttemptsOnRateLimitedRequests { get; set; } = 9;

    /// <summary>
    /// Maximum cumulative wait time across all retry attempts, in seconds. Defaults to 30.
    /// </summary>
    public int MaxRetryWaitTimeInSeconds { get; set; } = 30;

    /// <summary>
    /// Request timeout in seconds. Defaults to 60.
    /// </summary>
    public int RequestTimeoutInSeconds { get; set; } = 60;
}
