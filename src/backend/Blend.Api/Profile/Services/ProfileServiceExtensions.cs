using Blend.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Blend.Api.Profile.Services;

public static class ProfileServiceExtensions
{
    public static IServiceCollection AddProfileServices(this IServiceCollection services)
    {
        services.AddScoped<IProfileService, ProfileService>();
        return services;
    }
}
