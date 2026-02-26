using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Blend.Api.Configuration;
using Microsoft.Extensions.Options;

namespace Blend.Api.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _serviceClient;
    private readonly BlobContainerClient _containerClient;
    private readonly BlobStorageOptions _options;

    public BlobStorageService(IOptions<BlobStorageOptions> options)
    {
        _options = options.Value;
        _serviceClient = new BlobServiceClient(_options.ConnectionString);
        _containerClient = _serviceClient.GetBlobContainerClient(_options.ContainerName);
    }

    public async Task<string> GenerateSasUploadUrlAsync(string blobPath, string contentType, TimeSpan expiry)
    {
        var blobClient = _containerClient.GetBlobClient(blobPath);

        // Ensure container exists
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _options.ContainerName,
            BlobName = blobPath,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry),
            ContentType = contentType
        };
        // Write-only: create (upload new blob)
        sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return sasUri.ToString();
    }

    public Task<string> GetBlobUrlAsync(string blobPath)
    {
        if (!string.IsNullOrEmpty(_options.CdnBaseUrl))
        {
            var cdnUrl = $"{_options.CdnBaseUrl.TrimEnd('/')}/{blobPath}";
            return Task.FromResult(cdnUrl);
        }

        var blobClient = _containerClient.GetBlobClient(blobPath);
        return Task.FromResult(blobClient.Uri.ToString());
    }

    public async Task UploadAsync(string blobPath, Stream data, string contentType)
    {
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        var blobClient = _containerClient.GetBlobClient(blobPath);
        await blobClient.UploadAsync(data, new Azure.Storage.Blobs.Models.BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        });
    }

    public async Task DeleteAsync(string blobPath)
    {
        var blobClient = _containerClient.GetBlobClient(blobPath);
        await blobClient.DeleteIfExistsAsync();
    }
}
