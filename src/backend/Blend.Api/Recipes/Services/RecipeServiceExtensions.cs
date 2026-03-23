using Microsoft.Extensions.DependencyInjection;

namespace Blend.Api.Recipes.Services;

public static class RecipeServiceExtensions
{
    public static IServiceCollection AddRecipeServices(this IServiceCollection services)
    {
        services.AddScoped<IRecipeService, RecipeService>();
        return services;
    }
}
