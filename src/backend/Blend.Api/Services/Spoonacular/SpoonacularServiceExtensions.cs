using Blend.Api.Services.Cache;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Blend.Api.Services.Spoonacular;

/// <summary>
/// Registers Spoonacular-related services in the DI container.
/// </summary>
public static class SpoonacularServiceExtensions
{
    /// <summary>
    /// Registers <see cref="ICacheService"/>, <see cref="ISpoonacularService"/>,
    /// <see cref="SpoonacularQuotaMonitor"/>, and the Spoonacular health check.
    /// </summary>
    public static IServiceCollection AddSpoonacularServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Options
        services.Configure<SpoonacularOptions>(
            configuration.GetSection(SpoonacularOptions.SectionName));

        // L1 memory cache (may already be registered — safe to call multiple times)
        services.AddMemoryCache();

        // Cache service (L1 + optional L2 via IRepository<CacheEntry>)
        services.AddSingleton<ICacheService, CacheService>();

        // Quota monitor (singleton so quota state is shared across all requests)
        services.AddSingleton<SpoonacularQuotaMonitor>();

        // Named HTTP client for Spoonacular with base address from options
        services.AddHttpClient("Spoonacular", (sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<SpoonacularOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl);
        });

        // Cache-aware Spoonacular service
        services.AddScoped<ISpoonacularService, SpoonacularService>();

        // Health check
        services.AddHealthChecks()
            .AddCheck<SpoonacularHealthCheck>(
                "spoonacular-quota",
                tags: ["ready", "spoonacular"]);

        return services;
    }
}
