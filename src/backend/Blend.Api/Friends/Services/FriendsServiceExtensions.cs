using Blend.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Blend.Api.Friends.Services;

public static class FriendsServiceExtensions
{
    public static IServiceCollection AddFriendsServices(this IServiceCollection services)
    {
        services.AddScoped<IFriendsService, FriendsService>();
        return services;
    }
}
