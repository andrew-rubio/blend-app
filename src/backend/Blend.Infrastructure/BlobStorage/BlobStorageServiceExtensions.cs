using Azure.Storage.Blobs;
using Blend.Infrastructure.Media;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Blend.Infrastructure.BlobStorage;

/// <summary>
/// Extension methods for registering Azure Blob Storage services in the DI container.
/// </summary>
public static class BlobStorageServiceExtensions
{
    /// <summary>
    /// Registers <see cref="IBlobStorageService"/> and <see cref="IImageProcessingService"/>
    /// using the connection string resolved from configuration.
    /// <para>
    /// Resolution order:
    /// <list type="number">
    ///   <item><description><c>ConnectionStrings:blobs</c> (Aspire-injected)</description></item>
    ///   <item><description><c>AzureBlobStorage:ConnectionString</c> (manual config)</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public static IServiceCollection AddBlobStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BlobStorageOptions>(
            configuration.GetSection(BlobStorageOptions.SectionName));

        // Register BlobServiceClient as singleton — the client is thread-safe and expensive to construct.
        services.AddSingleton(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<BlobStorageOptions>>().Value;

            // Prefer Aspire-injected connection string, fall back to manual config.
            var connectionString =
                configuration.GetConnectionString("blobs")
                ?? opts.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Azure Blob Storage is not configured. Provide either " +
                    "'ConnectionStrings:blobs' (Aspire) or " +
                    "'AzureBlobStorage:ConnectionString' in configuration.");
            }

            return new BlobServiceClient(connectionString);
        });

        services.AddSingleton<IBlobStorageService, BlobStorageService>();
        services.AddSingleton<IImageProcessingService, ImageProcessingService>();

        return services;
    }
}
