using Blend.Domain.Entities;
using Blend.Domain.Interfaces;
using Blend.Infrastructure.Cosmos.Configuration;
using Blend.Infrastructure.Cosmos.Repositories;
using Blend.Infrastructure.Cosmos.Seeding;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Activity = Blend.Domain.Entities.Activity;
using CacheEntry = Blend.Domain.Entities.CacheEntry;
using Connection = Blend.Domain.Entities.Connection;
using Content = Blend.Domain.Entities.Content;
using IngredientPairing = Blend.Domain.Entities.IngredientPairing;
using Notification = Blend.Domain.Entities.Notification;
using Recipe = Blend.Domain.Entities.Recipe;
using User = Blend.Domain.Entities.User;

namespace Blend.Infrastructure.Cosmos;

/// <summary>
/// Extension methods for registering Cosmos DB services with the DI container.
/// </summary>
public static class CosmosServiceExtensions
{
    /// <summary>
    /// Registers the Cosmos DB client, repositories, and initializer.
    /// Call <see cref="IDatabaseInitializer.InitializeAsync"/> at startup to ensure containers exist.
    /// </summary>
    public static IServiceCollection AddCosmosDb(
        this IServiceCollection services,
        Action<CosmosOptions>? configureOptions = null)
    {
        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.AddOptions<CosmosOptions>()
                .BindConfiguration(CosmosOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }

        // Register CosmosClient as a singleton with resilience settings
        services.AddSingleton<CosmosClient>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<CosmosOptions>>().Value;

            var clientOptions = new CosmosClientOptions
            {
                MaxRetryAttemptsOnRateLimitedRequests = opts.MaxRetryAttemptsOnRateLimitedRequests,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(opts.MaxRetryWaitTimeInSeconds),
                RequestTimeout = TimeSpan.FromSeconds(opts.RequestTimeoutSeconds),
                AllowBulkExecution = opts.AllowBulkExecution,
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                    IgnoreNullValues = true
                }
            };

            return new CosmosClient(opts.ConnectionString, clientOptions);
        });

        // Register Container factories for each entity type
        services.AddSingleton<Func<string, Container>>(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            var opts = sp.GetRequiredService<IOptions<CosmosOptions>>().Value;
            return containerName => client.GetContainer(opts.DatabaseName, containerName);
        });

        // Register typed repositories
        RegisterRepository<User>(services, opts => opts.Containers.Users);
        RegisterRepository<Recipe>(services, opts => opts.Containers.Recipes);
        RegisterRepository<Connection>(services, opts => opts.Containers.Connections);
        RegisterRepository<Activity>(services, opts => opts.Containers.Activity);
        RegisterRepository<Content>(services, opts => opts.Containers.Content);
        RegisterRepository<Notification>(services, opts => opts.Containers.Notifications);
        RegisterRepository<CacheEntry>(services, opts => opts.Containers.Cache);
        RegisterRepository<IngredientPairing>(services, opts => opts.Containers.IngredientPairings);

        // Register database initializer and seed data provider
        services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();
        services.AddSingleton<SeedDataProvider>();

        return services;
    }

    private static void RegisterRepository<T>(
        IServiceCollection services,
        Func<CosmosOptions, string> containerNameSelector)
        where T : CosmosEntity
    {
        services.AddScoped<IRepository<T>>(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            var opts = sp.GetRequiredService<IOptions<CosmosOptions>>().Value;
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CosmosRepository<T>>>();
            var container = client.GetContainer(opts.DatabaseName, containerNameSelector(opts));
            return new CosmosRepository<T>(container, logger);
        });
    }
}
