using Microsoft.Extensions.DependencyInjection;

namespace Blend.Api.Account.Services;

/// <summary>Registers account services in the DI container.</summary>
public static class AccountServiceExtensions
{
    public static IServiceCollection AddAccountServices(this IServiceCollection services)
    {
        services.AddScoped<IAccountDeletionService, AccountDeletionService>();
        return services;
    }
}
