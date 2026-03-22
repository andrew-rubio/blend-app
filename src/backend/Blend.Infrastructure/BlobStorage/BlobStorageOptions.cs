namespace Blend.Infrastructure.BlobStorage;

/// <summary>
/// Configuration options for Azure Blob Storage (per ADR 0007).
/// </summary>
public sealed class BlobStorageOptions
{
    public const string SectionName = "AzureBlobStorage";

    /// <summary>Azure Storage connection string (used for SAS token signing).</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>The container name that stores all Blend media assets.</summary>
    public string ContainerName { get; set; } = "blend-media";

    /// <summary>Optional CDN base URL; when set, all public URLs are CDN-based.</summary>
    public string? CdnBaseUrl { get; set; }

    /// <summary>
    /// SAS token lifetime in minutes (must be between 5 and 15 per task spec).
    /// </summary>
    public int SasTokenExpiryMinutes { get; set; } = 15;

    /// <summary>Maximum allowed upload size in bytes (default 10 MB).</summary>
    public long MaxFileSizeBytes { get; set; } = 10_485_760;

    /// <summary>WebP encoding quality for image variants (0–100; default 85).</summary>
    public int WebPQuality { get; set; } = 85;
}
