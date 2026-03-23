using Microsoft.Extensions.DependencyInjection;

namespace Blend.Api.CookSessions.Services;

/// <summary>
/// Extension methods for registering Cook Mode session services.
/// </summary>
public static class CookSessionServiceExtensions
{
    /// <summary>
    /// Registers <see cref="ICookSessionService"/> and its implementation in the DI container.
    /// </summary>
    public static IServiceCollection AddCookSessionServices(this IServiceCollection services)
    {
        services.AddScoped<ICookSessionService, CookSessionService>();
        return services;
    }
}
