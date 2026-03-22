using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using Blend.Infrastructure.BlobStorage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blend.Infrastructure.Media;

/// <summary>
/// ImageSharp-based implementation of <see cref="IImageProcessingService"/>.
/// </summary>
public sealed class ImageProcessingService : IImageProcessingService
{
    // ── Variant definitions ─────────────────────────────────────────────────
    // Each record describes a single output variant: name, max width, optional fixed height.
    private sealed record VariantSpec(string Name, int Width, int? Height = null);

    private static readonly IReadOnlyList<VariantSpec> ProfileVariants =
    [
        new("avatar", 200, 200),
    ];

    private static readonly IReadOnlyList<VariantSpec> RecipeVariants =
    [
        new("hero",      1200),
        new("card",       600),
        new("thumbnail",  300),
    ];

    private static readonly IReadOnlyList<VariantSpec> ContentVariants =
    [
        new("hero",      1200),
        new("thumbnail",  300),
    ];

    private readonly IBlobStorageService _blobStorage;
    private readonly ILogger<ImageProcessingService> _logger;
    private readonly int _webPQuality;

    public ImageProcessingService(
        IBlobStorageService blobStorage,
        IOptions<BlobStorageOptions> options,
        ILogger<ImageProcessingService> logger)
    {
        _blobStorage = blobStorage;
        _logger = logger;
        _webPQuality = options.Value.WebPQuality;
    }

    /// <inheritdoc/>
    public async Task<bool> IsValidImageAsync(Stream imageStream, CancellationToken ct = default)
    {
        try
        {
            var imageInfo = await Image.IdentifyAsync(imageStream, ct);
            imageStream.Position = 0;
            return imageInfo is not null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Stream could not be identified as a valid image");
            imageStream.Position = 0;
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<ProcessingResult> ProcessAsync(
        string blobPath,
        MediaUploadUse uploadUse,
        CancellationToken ct = default)
    {
        var variants = GetVariants(uploadUse);
        var folder = BlobPathBuilder.GetFolder(blobPath);
        var results = new List<ProcessedVariant>(variants.Count);

        try
        {
            await using var originalStream = await _blobStorage.DownloadAsync(blobPath, ct);

            using var image = await Image.LoadAsync(originalStream, ct);

            foreach (var spec in variants)
            {
                var variantBlobPath = $"{folder}{spec.Name}.webp";

                using var variantImage = image.Clone(ctx => ApplyResize(ctx, spec));

                using var outputStream = new MemoryStream();
                await variantImage.SaveAsync(outputStream, new WebpEncoder { Quality = _webPQuality }, ct);
                outputStream.Position = 0;

                await _blobStorage.UploadAsync(variantBlobPath, outputStream, "image/webp", ct: ct);

                results.Add(new ProcessedVariant
                {
                    VariantName = spec.Name,
                    BlobPath = variantBlobPath,
                    Url = _blobStorage.GetPublicUrl(variantBlobPath),
                });

                _logger.LogDebug(
                    "Generated variant '{Variant}' at {BlobPath} ({Width}x{Height})",
                    spec.Name, variantBlobPath, spec.Width, spec.Height ?? 0);
            }

            return new ProcessingResult { Success = true, Variants = results };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Image processing failed for blob {BlobPath}", blobPath);
            return new ProcessingResult
            {
                Success = false,
                ErrorMessage = "Image processing failed; original is still available.",
            };
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static IReadOnlyList<VariantSpec> GetVariants(MediaUploadUse use) => use switch
    {
        MediaUploadUse.Profile => ProfileVariants,
        MediaUploadUse.Recipe  => RecipeVariants,
        MediaUploadUse.Content => ContentVariants,
        _ => throw new ArgumentOutOfRangeException(nameof(use), use, "Unknown upload use"),
    };

    private static void ApplyResize(IImageProcessingContext ctx, VariantSpec spec)
    {
        if (spec.Height.HasValue)
        {
            // Fixed width × height — crop to fill (avatar variant)
            ctx.Resize(new ResizeOptions
            {
                Size = new Size(spec.Width, spec.Height.Value),
                Mode = ResizeMode.Crop,
            });
        }
        else
        {
            // Width-constrained resize that preserves aspect ratio
            ctx.Resize(new ResizeOptions
            {
                Size = new Size(spec.Width, 0),
                Mode = ResizeMode.Max,
            });
        }
    }
}
