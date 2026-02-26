using Blend.Api.Models;

namespace Blend.Api.Services;

public interface IMediaService
{
    /// <summary>Generates a SAS upload URL and returns the blob path and expiry.</summary>
    Task<MediaUploadResponse> GetUploadUrlAsync(MediaUploadRequest request);

    /// <summary>Confirms upload is complete, triggers processing, and returns variant URLs.</summary>
    Task<UploadCompleteResponse> CompleteUploadAsync(UploadCompleteRequest request);
}
