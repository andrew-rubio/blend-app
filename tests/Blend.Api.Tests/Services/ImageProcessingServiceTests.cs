using Blend.Api.Configuration;
using Blend.Api.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace Blend.Api.Tests.Services;

public class ImageProcessingServiceTests
{
    private static IOptions<MediaOptions> DefaultMediaOptions() => Options.Create(new MediaOptions
    {
        HeroWidth = 1200,
        CardWidth = 600,
        ThumbnailWidth = 300,
        AvatarSize = 200,
        WebPQuality = 85
    });

    private static ImageProcessingService CreateService(Mock<IBlobStorageService>? blobMock = null)
    {
        blobMock ??= new Mock<IBlobStorageService>();
        var httpFactoryMock = new Mock<IHttpClientFactory>();
        return new ImageProcessingService(blobMock.Object, httpFactoryMock.Object, DefaultMediaOptions());
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    public async Task ValidateImageAsync_EmptyStream_ReturnsFalse(string contentType)
    {
        var service = CreateService();

        // Pass empty stream – we expect false because the stream has no bytes
        using var stream = new MemoryStream();
        var result = await service.ValidateImageAsync(stream, contentType);

        Assert.False(result); // empty stream: read < 4 bytes
    }

    [Fact]
    public async Task ValidateImageAsync_InvalidContentType_ReturnsFalse()
    {
        var service = CreateService();

        using var stream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }); // JPEG bytes
        var result = await service.ValidateImageAsync(stream, "video/mp4");

        Assert.False(result);
    }

    [Fact]
    public async Task ValidateImageAsync_JpegMagicBytes_ReturnsTrue()
    {
        var service = CreateService();

        // Minimal JPEG magic bytes
        using var stream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x00, 0x00, 0x00 });
        var result = await service.ValidateImageAsync(stream, "image/jpeg");

        Assert.True(result);
    }

    [Fact]
    public async Task ValidateImageAsync_PngMagicBytes_ReturnsTrue()
    {
        var service = CreateService();

        // PNG magic: 89 50 4E 47 0D 0A 1A 0A
        using var stream = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        var result = await service.ValidateImageAsync(stream, "image/png");

        Assert.True(result);
    }

    [Fact]
    public async Task ValidateImageAsync_WebpMagicBytes_ReturnsTrue()
    {
        var service = CreateService();

        // WebP starts with RIFF: 52 49 46 46
        using var stream = new MemoryStream(new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00 });
        var result = await service.ValidateImageAsync(stream, "image/webp");

        Assert.True(result);
    }

    [Fact]
    public async Task ValidateImageAsync_WrongMagicBytes_ReturnsFalse()
    {
        var service = CreateService();

        // PNG magic bytes but declared as JPEG
        using var stream = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        var result = await service.ValidateImageAsync(stream, "image/jpeg");

        Assert.False(result);
    }
}
