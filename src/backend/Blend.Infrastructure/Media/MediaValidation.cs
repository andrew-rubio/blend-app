namespace Blend.Infrastructure.Media;

/// <summary>
/// Centralises validation rules for media uploads (PLAT-27, PLAT-28).
/// </summary>
public static class MediaValidation
{
    /// <summary>
    /// MIME types accepted for image uploads (JPEG, PNG, WebP) and video uploads (MP4).
    /// </summary>
    public static readonly IReadOnlySet<string> AllowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "video/mp4",
    };

    /// <summary>Returns <c>true</c> when <paramref name="contentType"/> is in the allowed list.</summary>
    public static bool IsAllowedContentType(string contentType) =>
        AllowedContentTypes.Contains(contentType);

    /// <summary>Returns <c>true</c> when <paramref name="fileSizeBytes"/> is within the configured limit.</summary>
    public static bool IsWithinSizeLimit(long fileSizeBytes, long maxFileSizeBytes) =>
        fileSizeBytes > 0 && fileSizeBytes <= maxFileSizeBytes;
}
