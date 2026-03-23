using Microsoft.Extensions.DependencyInjection;

namespace Blend.Api.Home.Services;

/// <summary>Extension methods for registering home page services.</summary>
public static class HomeServiceExtensions
{
    /// <summary>Registers <see cref="HomeService"/> as the <see cref="IHomeService"/> implementation.</summary>
    public static IServiceCollection AddHomeServices(this IServiceCollection services)
    {
        services.AddScoped<IHomeService, HomeService>();
        return services;
    }
}
