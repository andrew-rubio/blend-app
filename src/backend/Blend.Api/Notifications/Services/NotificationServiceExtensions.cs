using Microsoft.Extensions.DependencyInjection;

namespace Blend.Api.Notifications.Services;

/// <summary>Registers notification services in the DI container.</summary>
public static class NotificationServiceExtensions
{
    public static IServiceCollection AddNotificationServices(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationService>();
        return services;
    }
}
