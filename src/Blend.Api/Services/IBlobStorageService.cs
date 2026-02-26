namespace Blend.Api.Services;

public interface IBlobStorageService
{
    /// <summary>Generates a write-only SAS URL for direct client upload.</summary>
    Task<string> GenerateSasUploadUrlAsync(string blobPath, string contentType, TimeSpan expiry);

    /// <summary>Returns the blob URL (CDN in prod, direct in dev).</summary>
    Task<string> GetBlobUrlAsync(string blobPath);

    /// <summary>Server-side upload of a stream to blob storage.</summary>
    Task UploadAsync(string blobPath, Stream data, string contentType);

    /// <summary>Deletes a blob from storage.</summary>
    Task DeleteAsync(string blobPath);
}
