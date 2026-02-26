using Blend.Api.Configuration;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Blend.Api.Services;

public class ImageProcessingService : IImageProcessingService
{
    private static readonly HashSet<string> SupportedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    // Magic bytes for supported formats
    private static readonly byte[] JpegMagic = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngMagic = [0x89, 0x50, 0x4E, 0x47];
    private static readonly byte[] WebpMagic = [0x52, 0x49, 0x46, 0x46]; // RIFF header

    private readonly IBlobStorageService _blobStorage;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MediaOptions _options;

    public ImageProcessingService(
        IBlobStorageService blobStorage,
        IHttpClientFactory httpClientFactory,
        IOptions<MediaOptions> options)
    {
        _blobStorage = blobStorage;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<ProcessedMedia> ProcessImageAsync(string blobPath, MediaType mediaType)
    {
        var originalUrl = await _blobStorage.GetBlobUrlAsync(blobPath);

        using var httpClient = _httpClientFactory.CreateClient("BlobDownload");
        using var originalStream = await httpClient.GetStreamAsync(originalUrl);
        using var image = await Image.LoadAsync(originalStream);

        var basePath = Path.GetDirectoryName(blobPath) ?? "";
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(blobPath);

        var result = new ProcessedMedia { OriginalPath = blobPath };

        var encoder = new WebpEncoder { Quality = _options.WebPQuality };

        if (mediaType == MediaType.Profile)
        {
            result.AvatarPath = await CreateVariantAsync(image, encoder, basePath, fileNameWithoutExt, "avatar",
                _options.AvatarSize, _options.AvatarSize, true);
        }
        else
        {
            result.HeroPath = await CreateVariantAsync(image, encoder, basePath, fileNameWithoutExt, "hero",
                _options.HeroWidth, null, false);
            result.CardPath = await CreateVariantAsync(image, encoder, basePath, fileNameWithoutExt, "card",
                _options.CardWidth, null, false);
            result.ThumbnailPath = await CreateVariantAsync(image, encoder, basePath, fileNameWithoutExt, "thumbnail",
                _options.ThumbnailWidth, null, false);
        }

        return result;
    }

    public async Task<bool> ValidateImageAsync(Stream stream, string contentType)
    {
        if (!SupportedImageTypes.Contains(contentType))
            return false;

        var magic = new byte[8];
        var read = await stream.ReadAsync(magic.AsMemory(0, magic.Length));
        if (read < 4)
            return false;

        if (stream.CanSeek)
            stream.Seek(0, SeekOrigin.Begin);

        return contentType switch
        {
            "image/jpeg" => magic[0] == JpegMagic[0] && magic[1] == JpegMagic[1] && magic[2] == JpegMagic[2],
            "image/png"  => magic[0] == PngMagic[0] && magic[1] == PngMagic[1] && magic[2] == PngMagic[2] && magic[3] == PngMagic[3],
            "image/webp" => magic[0] == WebpMagic[0] && magic[1] == WebpMagic[1] && magic[2] == WebpMagic[2] && magic[3] == WebpMagic[3],
            _ => false
        };
    }

    private async Task<string> CreateVariantAsync(Image image, WebpEncoder encoder,
        string basePath, string fileNameWithoutExt, string variantName, int width, int? height, bool crop)
    {
        using var variantImage = image.Clone(ctx =>
        {
            if (crop && height.HasValue)
            {
                ctx.Resize(new ResizeOptions
                {
                    Size = new Size(width, height.Value),
                    Mode = ResizeMode.Crop
                });
            }
            else
            {
                ctx.Resize(new ResizeOptions
                {
                    Size = new Size(width, 0),
                    Mode = ResizeMode.Max
                });
            }
        });

        using var ms = new MemoryStream();
        await variantImage.SaveAsync(ms, encoder);
        ms.Seek(0, SeekOrigin.Begin);

        var variantPath = string.IsNullOrEmpty(basePath)
            ? $"{fileNameWithoutExt}-{variantName}.webp"
            : $"{basePath}/{fileNameWithoutExt}-{variantName}.webp";

        await _blobStorage.UploadAsync(variantPath, ms, "image/webp");
        return variantPath;
    }
}
