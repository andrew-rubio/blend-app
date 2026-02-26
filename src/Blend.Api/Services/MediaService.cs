using Blend.Api.Configuration;
using Blend.Api.Models;
using Microsoft.Extensions.Options;

namespace Blend.Api.Services;

public class MediaService : IMediaService
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "video/mp4"
    };

    private static readonly HashSet<string> ValidEntityTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "recipe", "profile", "content"
    };

    private readonly IBlobStorageService _blobStorage;
    private readonly IImageProcessingService _imageProcessing;
    private readonly BlobStorageOptions _storageOptions;

    public MediaService(
        IBlobStorageService blobStorage,
        IImageProcessingService imageProcessing,
        IOptions<BlobStorageOptions> storageOptions)
    {
        _blobStorage = blobStorage;
        _imageProcessing = imageProcessing;
        _storageOptions = storageOptions.Value;
    }

    public async Task<MediaUploadResponse> GetUploadUrlAsync(MediaUploadRequest request)
    {
        ValidateContentType(request.ContentType);
        ValidateEntityType(request.EntityType);
        ValidateFileSize(request.FileSizeBytes, request.ContentType);

        var blobPath = BuildBlobPath(request.EntityType, request.EntityId, request.FileName);
        var expiry = TimeSpan.FromMinutes(_storageOptions.SasExpiryMinutes);
        var uploadUrl = await _blobStorage.GenerateSasUploadUrlAsync(blobPath, request.ContentType, expiry);

        return new MediaUploadResponse
        {
            UploadUrl = uploadUrl,
            BlobPath = blobPath,
            ExpiresAt = DateTimeOffset.UtcNow.Add(expiry)
        };
    }

    public async Task<UploadCompleteResponse> CompleteUploadAsync(UploadCompleteRequest request)
    {
        ValidateEntityType(request.EntityType);

        var originalUrl = await _blobStorage.GetBlobUrlAsync(request.BlobPath);

        // Only process images; videos are returned as-is
        var contentType = InferContentType(request.BlobPath);
        string? heroUrl = null, cardUrl = null, thumbUrl = null, avatarUrl = null;

        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            var mediaType = MapEntityToMediaType(request.EntityType);
            var processed = await _imageProcessing.ProcessImageAsync(request.BlobPath, mediaType);

            heroUrl   = await GetUrlIfPathExistsAsync(processed.HeroPath);
            cardUrl   = await GetUrlIfPathExistsAsync(processed.CardPath);
            thumbUrl  = await GetUrlIfPathExistsAsync(processed.ThumbnailPath);
            avatarUrl = await GetUrlIfPathExistsAsync(processed.AvatarPath);
        }

        return new UploadCompleteResponse
        {
            MediaId = Guid.NewGuid().ToString(),
            Urls = new MediaVariantUrls
            {
                Original  = originalUrl,
                Hero      = heroUrl,
                Card      = cardUrl,
                Thumbnail = thumbUrl,
                Avatar    = avatarUrl
            }
        };
    }

    internal static string BuildBlobPath(string entityType, Guid entityId, string fileName)
    {
        var folder = entityType.ToLowerInvariant() switch
        {
            "recipe"  => $"recipes/{entityId}",
            "profile" => $"profiles/{entityId}",
            "content" => $"content/{entityId}",
            _         => throw new ArgumentException($"Unknown entity type: {entityType}")
        };
        // Sanitise fileName: keep only the final file name part
        var safeFileName = Path.GetFileName(fileName);
        return $"{folder}/{safeFileName}";
    }

    private static void ValidateContentType(string contentType)
    {
        if (!AllowedContentTypes.Contains(contentType))
            throw new ArgumentException($"Content type '{contentType}' is not allowed. Allowed types: {string.Join(", ", AllowedContentTypes)}");
    }

    private static void ValidateEntityType(string entityType)
    {
        if (!ValidEntityTypes.Contains(entityType))
            throw new ArgumentException($"Entity type '{entityType}' is not valid. Valid types: {string.Join(", ", ValidEntityTypes)}");
    }

    private void ValidateFileSize(long? fileSizeBytes, string contentType)
    {
        if (fileSizeBytes is null)
            return;

        var maxBytes = contentType.Equals("video/mp4", StringComparison.OrdinalIgnoreCase)
            ? _storageOptions.MaxVideoSizeBytes
            : _storageOptions.MaxImageSizeBytes;

        if (fileSizeBytes > maxBytes)
            throw new ArgumentException($"File size {fileSizeBytes} bytes exceeds the maximum allowed size of {maxBytes} bytes.");
    }

    private static MediaType MapEntityToMediaType(string entityType) =>
        entityType.ToLowerInvariant() switch
        {
            "profile" => MediaType.Profile,
            "content" => MediaType.Content,
            _         => MediaType.Recipe
        };

    private static string InferContentType(string blobPath)
    {
        var ext = Path.GetExtension(blobPath).TrimStart('.').ToLowerInvariant();
        return ext switch
        {
            "jpg" or "jpeg" => "image/jpeg",
            "png"           => "image/png",
            "webp"          => "image/webp",
            "mp4"           => "video/mp4",
            _               => "application/octet-stream"
        };
    }

    private async Task<string?> GetUrlIfPathExistsAsync(string? blobPath) =>
        blobPath != null ? await _blobStorage.GetBlobUrlAsync(blobPath) : null;
}
