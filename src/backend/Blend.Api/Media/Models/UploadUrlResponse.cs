namespace Blend.Api.Media.Models;

/// <summary>Response body for <c>POST /api/v1/media/upload-url</c>.</summary>
public sealed record UploadUrlResponse
{
    /// <summary>Time-limited SAS URL the browser uses to PUT the file directly to Blob Storage.</summary>
    public string SasUrl { get; init; } = string.Empty;

    /// <summary>Blob path within the container; sent back in the upload-complete request.</summary>
    public string BlobPath { get; init; } = string.Empty;

    /// <summary>UTC time at which the SAS token expires.</summary>
    public DateTimeOffset ExpiresAt { get; init; }
}
