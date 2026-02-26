namespace Blend.Infrastructure.Cosmos.Configuration;

/// <summary>
/// Defines Cosmos DB container metadata: name, partition key path, and optional TTL.
/// </summary>
public sealed record ContainerDefinition(
    string Name,
    string PartitionKeyPath,
    int? DefaultTtlSeconds = null);

/// <summary>
/// All container definitions for the Blend application per the database schema (ADR 0003).
/// </summary>
public static class ContainerDefinitions
{
    public static ContainerDefinition Users(CosmosOptions opts) =>
        new(opts.Containers.Users, "/id");

    public static ContainerDefinition Recipes(CosmosOptions opts) =>
        new(opts.Containers.Recipes, "/authorId");

    public static ContainerDefinition Connections(CosmosOptions opts) =>
        new(opts.Containers.Connections, "/userId");

    public static ContainerDefinition Activity(CosmosOptions opts) =>
        new(opts.Containers.Activity, "/userId");

    public static ContainerDefinition Content(CosmosOptions opts) =>
        new(opts.Containers.Content, "/contentType");

    public static ContainerDefinition Notifications(CosmosOptions opts) =>
        new(opts.Containers.Notifications, "/recipientUserId", opts.Containers.NotificationsTtlSeconds);

    public static ContainerDefinition Cache(CosmosOptions opts) =>
        new(opts.Containers.Cache, "/cacheKey", opts.Containers.CacheTtlSeconds);

    public static ContainerDefinition IngredientPairings(CosmosOptions opts) =>
        new(opts.Containers.IngredientPairings, "/ingredientId");

    /// <summary>Returns all container definitions in order.</summary>
    public static IEnumerable<ContainerDefinition> All(CosmosOptions opts)
    {
        yield return Users(opts);
        yield return Recipes(opts);
        yield return Connections(opts);
        yield return Activity(opts);
        yield return Content(opts);
        yield return Notifications(opts);
        yield return Cache(opts);
        yield return IngredientPairings(opts);
    }
}
