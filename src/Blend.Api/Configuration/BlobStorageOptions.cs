namespace Blend.Api.Configuration;

public class BlobStorageOptions
{
    public string ConnectionString { get; set; } = "";
    public string ContainerName { get; set; } = "blend-media";
    public string? CdnBaseUrl { get; set; }
    public int SasExpiryMinutes { get; set; } = 15;
    public long MaxImageSizeBytes { get; set; } = 52_428_800; // 50MB
    public long MaxVideoSizeBytes { get; set; } = 524_288_000; // 500MB
}
