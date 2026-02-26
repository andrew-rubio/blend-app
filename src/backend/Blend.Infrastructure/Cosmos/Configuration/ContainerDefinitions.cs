namespace Blend.Infrastructure.Cosmos.Configuration;

/// <summary>
/// Defines Cosmos DB container metadata: name, partition key path, and optional TTL.
/// </summary>
public sealed record ContainerDefinition(
    string Name,
    string PartitionKeyPath,
    int? DefaultTtlSeconds = null);

/// <summary>
/// All container definitions for the Blend application.
/// </summary>
public static class ContainerDefinitions
{
    public static ContainerDefinition Content(CosmosOptions opts) =>
        new(opts.Containers.Content, "/contentType");

    public static ContainerDefinition Notifications(CosmosOptions opts) =>
        new(opts.Containers.Notifications, "/recipientUserId", opts.Containers.NotificationsTtlSeconds);

    /// <summary>Returns all container definitions in order.</summary>
    public static IEnumerable<ContainerDefinition> All(CosmosOptions opts)
    {
        yield return Content(opts);
        yield return Notifications(opts);
    }
}
