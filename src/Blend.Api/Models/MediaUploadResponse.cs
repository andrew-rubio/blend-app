namespace Blend.Api.Models;

public class MediaUploadResponse
{
    public string UploadUrl { get; set; } = "";
    public string BlobPath { get; set; } = "";
    public DateTimeOffset ExpiresAt { get; set; }
}
