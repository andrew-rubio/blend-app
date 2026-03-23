using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blend.Api.Ingredients.Services;

/// <summary>
/// Registers Ingredient Knowledge Base services in the DI container.
/// </summary>
public static class KnowledgeBaseServiceExtensions
{
    /// <summary>
    /// Registers <see cref="IKnowledgeBaseService"/> and its configuration options.
    /// </summary>
    public static IServiceCollection AddKnowledgeBaseServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<IngredientSearchOptions>(
            configuration.GetSection(IngredientSearchOptions.SectionName));

        // Singleton: KnowledgeBaseService holds circuit-breaker state that must persist across requests.
        services.AddSingleton<IKnowledgeBaseService, KnowledgeBaseService>();

        return services;
    }
}
