using Microsoft.Extensions.DependencyInjection;

namespace Blend.Api.Search.Services;

public static class SearchServiceExtensions
{
    public static IServiceCollection AddSearchServices(this IServiceCollection services)
    {
        services.AddScoped<ISearchService, SearchService>();
        return services;
    }
}
