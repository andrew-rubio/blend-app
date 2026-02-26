using Blend.Domain.Entities;
using Blend.Domain.Interfaces;
using Blend.Infrastructure.Cosmos.Configuration;
using Blend.Infrastructure.Cosmos.Repositories;
using Blend.Infrastructure.Cosmos.Seeding;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Content = Blend.Domain.Entities.Content;
using IngredientSubmission = Blend.Domain.Entities.IngredientSubmission;
using Notification = Blend.Domain.Entities.Notification;

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

        // Register typed repositories
        RegisterRepository<Content>(services, opts => opts.Containers.Content);
        RegisterRepository<IngredientSubmission>(services, opts => opts.Containers.Content);
        RegisterRepository<Notification>(services, opts => opts.Containers.Notifications);

        // Register database initializer
        services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();

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
