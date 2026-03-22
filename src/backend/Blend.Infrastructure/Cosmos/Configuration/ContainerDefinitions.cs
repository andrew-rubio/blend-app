namespace Blend.Infrastructure.Cosmos.Configuration;

/// <summary>
/// Defines the Cosmos DB container names, partition keys, and TTL settings for all
/// Blend containers (per ADR 0003 and ADR 0009).
/// </summary>
public static class ContainerDefinitions
{
    /// <summary>Container definitions indexed by container name.</summary>
    public static IReadOnlyList<ContainerDefinition> All { get; } =
    [
        new("users",              "/id",                 null),
        new("recipes",            "/authorId",           null),
        new("connections",        "/userId",             null),
        new("activity",           "/userId",             null),
        new("content",            "/contentType",        null),
        new("notifications",      "/recipientUserId",    7776000),  // 90 days
        new("cache",              "/cacheKey",           86400),    // 24 h default; per-document override supported
        new("ingredientPairings", "/ingredientId",       null),
    ];
}

/// <summary>Describes a single Cosmos DB container's configuration.</summary>
/// <param name="Name">Container name.</param>
/// <param name="PartitionKeyPath">Hierarchical partition key path (e.g. <c>/userId</c>).</param>
/// <param name="TtlSeconds">
/// Default container-level TTL in seconds, or <c>null</c> to disable TTL on the container.
/// Individual documents can still override this with a per-document <c>ttl</c> property when
/// the container TTL is set to <c>-1</c> (inherit from document).
/// </param>
public sealed record ContainerDefinition(
    string Name,
    string PartitionKeyPath,
    int? TtlSeconds);
