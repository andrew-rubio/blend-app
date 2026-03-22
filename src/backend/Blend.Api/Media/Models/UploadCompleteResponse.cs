using Blend.Infrastructure.Media;

namespace Blend.Api.Media.Models;

/// <summary>Response body for <c>POST /api/v1/media/upload-complete</c>.</summary>
public sealed record UploadCompleteResponse
{
    /// <summary>Public URL of the original uploaded file.</summary>
    public string MediaUrl { get; init; } = string.Empty;

    /// <summary>Variants generated synchronously (development) or an empty list (production, async).</summary>
    public IReadOnlyList<ProcessedVariant> Variants { get; init; } = [];

    /// <summary>
    /// <c>true</c> when image processing will happen asynchronously (production);
    /// <c>false</c> when variants have already been generated (development).
    /// </summary>
    public bool ProcessingPending { get; init; }
}
