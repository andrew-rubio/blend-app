namespace Blend.Infrastructure.Media;

/// <summary>
/// A single resized/converted image variant produced by <see cref="IImageProcessingService"/>.
/// </summary>
public sealed record ProcessedVariant
{
    /// <summary>Human-readable name of the variant (e.g. <c>hero</c>, <c>avatar</c>).</summary>
    public string VariantName { get; init; } = string.Empty;

    /// <summary>Relative blob path within the container.</summary>
    public string BlobPath { get; init; } = string.Empty;

    /// <summary>Public URL (CDN or direct blob).</summary>
    public string Url { get; init; } = string.Empty;
}

/// <summary>Result returned by <see cref="IImageProcessingService.ProcessAsync"/>.</summary>
public sealed record ProcessingResult
{
    /// <summary><c>true</c> when all variants were successfully generated and uploaded.</summary>
    public bool Success { get; init; }

    /// <summary>Variants that were generated. Empty on failure.</summary>
    public IReadOnlyList<ProcessedVariant> Variants { get; init; } = [];

    /// <summary>Populated on partial or full failure.</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Processes uploaded images: validates, resizes to configured breakpoints,
/// converts to WebP, and writes optimised variants to blob storage.
/// <para>
/// In development this is called synchronously from the API after the browser
/// notifies upload completion. In production the same logic is invoked by an
/// Azure Function BlobTrigger.
/// </para>
/// </summary>
public interface IImageProcessingService
{
    /// <summary>
    /// Validates that <paramref name="imageStream"/> contains a genuine image
    /// (not merely a renamed file with a wrong extension).
    /// The stream position is reset to 0 before returning.
    /// </summary>
    Task<bool> IsValidImageAsync(Stream imageStream, CancellationToken ct = default);

    /// <summary>
    /// Downloads the blob at <paramref name="blobPath"/>, generates all variants
    /// appropriate for <paramref name="uploadUse"/>, uploads them, and returns
    /// the result including public URLs for each variant.
    /// </summary>
    Task<ProcessingResult> ProcessAsync(
        string blobPath,
        MediaUploadUse uploadUse,
        CancellationToken ct = default);
}
