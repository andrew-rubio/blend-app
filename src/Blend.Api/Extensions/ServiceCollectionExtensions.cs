using Blend.Api.Services.Cache;
using Blend.Api.Services.Spoonacular;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Blend.Api.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register the Spoonacular service, two-tier cache, and related health checks.
    /// </summary>
    public static IServiceCollection AddSpoonacularServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind options
        services.AddOptions<SpoonacularOptions>()
            .Bind(configuration.GetSection(SpoonacularOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CacheOptions>()
            .Bind(configuration.GetSection(CacheOptions.SectionName));

        // L1: in-process memory cache
        services.AddMemoryCache();

        // L2: Cosmos DB client (optional — only registered when a connection string is provided)
        var cacheSection = configuration.GetSection(CacheOptions.SectionName);
        var cosmosConnectionString = cacheSection["CosmosConnectionString"];
        if (!string.IsNullOrWhiteSpace(cosmosConnectionString))
        {
            services.AddSingleton(new CosmosClient(
                cosmosConnectionString,
                new CosmosClientOptions
                {
                    SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase }
                }));
        }

        services.AddSingleton<ICacheService, CacheService>();

        // Spoonacular HTTP client with resilience policies
        services.AddHttpClient<ISpoonacularService, SpoonacularService>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<SpoonacularOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddStandardResilienceHandler();

        // Health checks
        services.AddHealthChecks()
            .AddCheck<SpoonacularQuotaHealthCheck>("spoonacular-quota", tags: ["spoonacular", "quota"]);

        return services;
    }
}
