using Azure.Storage.Sas;

namespace Blend.Infrastructure.BlobStorage;

/// <summary>
/// Abstraction over Azure Blob Storage for upload, download, and SAS-token generation.
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Generates a write-only SAS URI scoped to the exact <paramref name="blobPath"/>.
    /// The caller cannot use this URI to write to any other path.
    /// </summary>
    /// <param name="blobPath">Relative path within the container (e.g. <c>profiles/user-1/photo.jpg</c>).</param>
    /// <param name="expiresOn">Absolute expiry time for the SAS token.</param>
    Uri GenerateSasUri(string blobPath, DateTimeOffset expiresOn);

    /// <summary>Opens a read stream for the blob at <paramref name="blobPath"/>.</summary>
    Task<Stream> DownloadAsync(string blobPath, CancellationToken ct = default);

    /// <summary>
    /// Uploads <paramref name="content"/> to <paramref name="blobPath"/>.
    /// Sets <c>Cache-Control: max-age=31536000</c> on versioned blob paths.
    /// </summary>
    Task UploadAsync(
        string blobPath,
        Stream content,
        string contentType,
        IDictionary<string, string>? metadata = null,
        CancellationToken ct = default);

    /// <summary>Returns the public (CDN or direct Blob Storage) URL for <paramref name="blobPath"/>.</summary>
    string GetPublicUrl(string blobPath);

    /// <summary>Returns <c>true</c> when the blob at <paramref name="blobPath"/> exists.</summary>
    Task<bool> ExistsAsync(string blobPath, CancellationToken ct = default);
}
