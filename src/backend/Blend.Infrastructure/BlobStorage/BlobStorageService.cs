using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blend.Infrastructure.BlobStorage;

/// <summary>
/// Azure Blob Storage implementation of <see cref="IBlobStorageService"/>.
/// Uses the connection string (which contains the account key) to sign SAS tokens so that
/// each token is scoped to the exact blob path — users cannot write to arbitrary paths.
/// </summary>
public sealed class BlobStorageService : IBlobStorageService
{
    // Blob paths that contain a version segment (e.g. recipes/{id}/v1/…) get a
    // long-lived Cache-Control header, matching the CDN strategy in ADR 0007.
    private static readonly string[] VersionedPathPrefixes = ["recipes/"];

    private readonly BlobContainerClient _containerClient;
    private readonly BlobStorageOptions _options;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(
        BlobServiceClient blobServiceClient,
        IOptions<BlobStorageOptions> options,
        ILogger<BlobStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _containerClient = blobServiceClient.GetBlobContainerClient(_options.ContainerName);
    }

    /// <inheritdoc/>
    public Uri GenerateSasUri(string blobPath, DateTimeOffset expiresOn)
    {
        var blobClient = _containerClient.GetBlobClient(blobPath);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = blobPath,
            Resource = "b",   // scope to the specific blob, not the container
            ExpiresOn = expiresOn,
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create);

        // GenerateSasUri requires the client to have been created with a
        // StorageSharedKeyCredential (i.e. from a connection string that includes the key).
        return blobClient.GenerateSasUri(sasBuilder);
    }

    /// <inheritdoc/>
    public async Task<Stream> DownloadAsync(string blobPath, CancellationToken ct = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobPath);
        var response = await blobClient.DownloadStreamingAsync(cancellationToken: ct);
        return response.Value.Content;
    }

    /// <inheritdoc/>
    public async Task UploadAsync(
        string blobPath,
        Stream content,
        string contentType,
        IDictionary<string, string>? metadata = null,
        CancellationToken ct = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobPath);

        var headers = new BlobHttpHeaders { ContentType = contentType };

        // Versioned paths (e.g. recipes/…) get a 1-year cache header per ADR 0007.
        if (VersionedPathPrefixes.Any(p => blobPath.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            headers.CacheControl = "max-age=31536000, immutable";
        }

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = headers,
            Metadata = metadata != null ? new Dictionary<string, string>(metadata) : null,
        };

        _logger.LogDebug("Uploading blob {BlobPath} ({ContentType})", blobPath, contentType);
        await blobClient.UploadAsync(content, uploadOptions, cancellationToken: ct);
    }

    /// <inheritdoc/>
    public string GetPublicUrl(string blobPath)
    {
        if (!string.IsNullOrWhiteSpace(_options.CdnBaseUrl))
        {
            return $"{_options.CdnBaseUrl.TrimEnd('/')}/{_options.ContainerName}/{blobPath}";
        }

        return _containerClient.GetBlobClient(blobPath).Uri.ToString();
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string blobPath, CancellationToken ct = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobPath);
        var response = await blobClient.ExistsAsync(ct);
        return response.Value;
    }
}
