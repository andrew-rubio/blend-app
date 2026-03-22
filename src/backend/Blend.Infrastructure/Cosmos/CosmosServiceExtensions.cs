using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Blend.Infrastructure.Cosmos.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DomainUser = Blend.Domain.Entities.User;

namespace Blend.Infrastructure.Cosmos;

/// <summary>
/// Extension methods for registering all Cosmos DB infrastructure services.
/// </summary>
public static class CosmosServiceExtensions
{
    /// <summary>
    /// Registers <see cref="CosmosClient"/>, all entity repositories, and the database
    /// initializer in the DI container. Call this from your application's <c>Program.cs</c>.
    /// </summary>
    public static IServiceCollection AddCosmosDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CosmosOptions>(configuration.GetSection(CosmosOptions.SectionName));

        services.AddSingleton(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<CosmosOptions>>().Value;

            var clientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                },
                MaxRetryAttemptsOnRateLimitedRequests = opts.MaxRetryAttemptsOnRateLimitedRequests,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(opts.MaxRetryWaitTimeInSeconds),
                RequestTimeout = TimeSpan.FromSeconds(opts.RequestTimeoutInSeconds),
            };

            if (!string.IsNullOrWhiteSpace(opts.ConnectionString))
            {
                return new CosmosClient(opts.ConnectionString, clientOptions);
            }

            if (!string.IsNullOrWhiteSpace(opts.EndpointUri) && !string.IsNullOrWhiteSpace(opts.AccountKey))
            {
                return new CosmosClient(opts.EndpointUri, opts.AccountKey, clientOptions);
            }

            throw new InvalidOperationException(
                "Cosmos DB is not configured. Provide either 'CosmosDb:ConnectionString' " +
                "or both 'CosmosDb:EndpointUri' and 'CosmosDb:AccountKey' in configuration.");
        });

        // Register DatabaseInitializer
        services.AddSingleton<DatabaseInitializer>();

        // Register per-container repository instances
        RegisterRepository<DomainUser>(services, "users");
        RegisterRepository<Recipe>(services, "recipes");
        RegisterRepository<Connection>(services, "connections");
        // Activity and CookingSession share the 'activity' container (per ADR 0003 § container definitions)
        RegisterRepository<Activity>(services, "activity");
        RegisterRepository<CookingSession>(services, "activity");
        RegisterRepository<Notification>(services, "notifications");
        RegisterRepository<Content>(services, "content");
        RegisterRepository<CacheEntry>(services, "cache");
        RegisterRepository<IngredientPairing>(services, "ingredientPairings");

        return services;
    }

    /// <summary>
    /// Runs the <see cref="DatabaseInitializer"/> to ensure all containers exist.
    /// Call this from your application startup (e.g. after <c>app.Build()</c>).
    /// </summary>
    public static async Task EnsureCosmosDbAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        using var scope = host.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await initializer.EnsureDatabaseAsync(cancellationToken);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static void RegisterRepository<T>(IServiceCollection services, string containerName) where T : class
    {
        services.AddSingleton<IRepository<T>>(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            var opts = sp.GetRequiredService<IOptions<CosmosOptions>>().Value;
            var logger = sp.GetRequiredService<ILogger<CosmosRepository<T>>>();
            var container = client.GetContainer(opts.DatabaseName, containerName);
            return new CosmosRepository<T>(container, logger);
        });
    }
}
