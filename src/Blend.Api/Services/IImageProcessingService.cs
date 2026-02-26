namespace Blend.Api.Services;

public enum MediaType
{
    Recipe,
    Profile,
    Content
}

public class ProcessedMedia
{
    public string OriginalPath { get; set; } = "";
    public string? HeroPath { get; set; }
    public string? CardPath { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? AvatarPath { get; set; }
}

public interface IImageProcessingService
{
    /// <summary>Processes an uploaded image into all required variants (WebP).</summary>
    Task<ProcessedMedia> ProcessImageAsync(string blobPath, MediaType mediaType);

    /// <summary>Validates that a stream is a genuine image matching the declared content type.</summary>
    Task<bool> ValidateImageAsync(Stream stream, string contentType);
}
