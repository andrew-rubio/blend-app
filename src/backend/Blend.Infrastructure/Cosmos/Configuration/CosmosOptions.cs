namespace Blend.Infrastructure.Cosmos.Configuration;

/// <summary>
/// Configuration options for the Cosmos DB client, bound from appsettings.
/// </summary>
public class CosmosOptions
{
    public const string SectionName = "CosmosDb";

    public string ConnectionString { get; set; } = string.Empty;

    public string DatabaseName { get; set; } = "blend";

    public int MaxRetryAttemptsOnRateLimitedRequests { get; set; } = 9;

    public int MaxRetryWaitTimeInSeconds { get; set; } = 30;

    public int RequestTimeoutSeconds { get; set; } = 60;

    public bool AllowBulkExecution { get; set; } = false;

    public ContainerOptions Containers { get; set; } = new();
}

/// <summary>
/// Per-container configuration including partition key and TTL.
/// </summary>
public class ContainerOptions
{
    public string Users { get; set; } = "users";

    public string Recipes { get; set; } = "recipes";

    public string Connections { get; set; } = "connections";

    public string Activity { get; set; } = "activity";

    public string Content { get; set; } = "content";

    public string Notifications { get; set; } = "notifications";

    public string Cache { get; set; } = "cache";

    public string IngredientPairings { get; set; } = "ingredientPairings";

    /// <summary>Default TTL for the notifications container in seconds (30 days).</summary>
    public int NotificationsTtlSeconds { get; set; } = 2_592_000;

    /// <summary>Default TTL for the cache container in seconds (24 hours).</summary>
    public int CacheTtlSeconds { get; set; } = 86_400;
}
