namespace Blend.Api.Preferences.Services;

/// <summary>
/// Extension methods for registering preference services with the DI container.
/// </summary>
public static class PreferenceServiceExtensions
{
    /// <summary>
    /// Registers the <see cref="IPreferenceService"/> implementation with the DI container.
    /// </summary>
    public static IServiceCollection AddPreferenceServices(this IServiceCollection services)
    {
        services.AddScoped<IPreferenceService, PreferenceService>();
        return services;
    }
}
