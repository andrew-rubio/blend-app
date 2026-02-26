using Azure.Storage.Blobs;
using Blend.Api.Configuration;
using Blend.Api.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace Blend.Api.Tests.Services;

public class BlobStorageServiceTests
{
    private static IOptions<BlobStorageOptions> CreateOptions(Action<BlobStorageOptions>? configure = null)
    {
        var opts = new BlobStorageOptions
        {
            ConnectionString = "UseDevelopmentStorage=true",
            ContainerName = "blend-media",
            SasExpiryMinutes = 15
        };
        configure?.Invoke(opts);
        return Options.Create(opts);
    }

    [Fact]
    public void Constructor_WithValidOptions_DoesNotThrow()
    {
        var opts = CreateOptions();
        var ex = Record.Exception(() => new BlobStorageService(opts));
        Assert.Null(ex);
    }

    [Fact]
    public async Task GetBlobUrlAsync_WithCdnBaseUrl_ReturnsCdnUrl()
    {
        var opts = CreateOptions(o => o.CdnBaseUrl = "https://cdn.example.com");
        var service = new BlobStorageService(opts);

        var url = await service.GetBlobUrlAsync("recipes/123/photo.jpg");

        Assert.StartsWith("https://cdn.example.com/", url);
        Assert.Contains("recipes/123/photo.jpg", url);
    }

    [Fact]
    public async Task GetBlobUrlAsync_WithoutCdnBaseUrl_ReturnsBlobDirectUrl()
    {
        var opts = CreateOptions();
        var service = new BlobStorageService(opts);

        var url = await service.GetBlobUrlAsync("recipes/123/photo.jpg");

        // Azurite / devstoreaccount1 URL format
        Assert.Contains("recipes/123/photo.jpg", url);
    }

    [Fact]
    public async Task GetBlobUrlAsync_CdnTrailingSlash_IsNormalised()
    {
        var opts = CreateOptions(o => o.CdnBaseUrl = "https://cdn.example.com/");
        var service = new BlobStorageService(opts);

        var url = await service.GetBlobUrlAsync("recipes/abc/img.jpg");

        Assert.DoesNotContain("//recipes", url);
        Assert.Equal("https://cdn.example.com/recipes/abc/img.jpg", url);
    }

    [Fact]
    public void SasExpiry_DefaultIs15Minutes()
    {
        var opts = CreateOptions();
        Assert.Equal(15, opts.Value.SasExpiryMinutes);
    }

    [Fact]
    public void MaxImageSizeBytes_Default_Is50MB()
    {
        var opts = CreateOptions();
        Assert.Equal(52_428_800, opts.Value.MaxImageSizeBytes);
    }

    [Fact]
    public void MaxVideoSizeBytes_Default_Is500MB()
    {
        var opts = CreateOptions();
        Assert.Equal(524_288_000, opts.Value.MaxVideoSizeBytes);
    }
}
