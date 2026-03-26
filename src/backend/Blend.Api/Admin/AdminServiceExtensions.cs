using Microsoft.Extensions.DependencyInjection;

namespace Blend.Api.Admin.Services;

/// <summary>Registers admin content management services.</summary>
public static class AdminServiceExtensions
{
    public static IServiceCollection AddAdminServices(this IServiceCollection services)
    {
        services.AddScoped<IAdminContentService, AdminContentService>();
        return services;
    }
}
