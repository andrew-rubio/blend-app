using Blend.Infrastructure.BlobStorage;
using Blend.Infrastructure.Media;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Blend.Tests.Unit.Media;

/// <summary>
/// Unit tests for <see cref="ImageProcessingService"/> covering variant selection,
/// resize dimensions, and error handling.
/// </summary>
public class ImageProcessingServiceTests
{
    private static readonly BlobStorageOptions DefaultOptions = new()
    {
        ContainerName = "blend-media",
        SasTokenExpiryMinutes = 15,
        MaxFileSizeBytes = 10_485_760,
    };

    // ── IsValidImageAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task IsValidImageAsync_ValidPng_ReturnsTrue()
    {
        var (sut, _) = CreateSut();
        using var stream = CreateMinimalPng();

        var result = await sut.IsValidImageAsync(stream);

        Assert.True(result);
    }

    [Fact]
    public async Task IsValidImageAsync_InvalidData_ReturnsFalse()
    {
        var (sut, _) = CreateSut();
        using var stream = new MemoryStream("this is not an image"u8.ToArray());

        var result = await sut.IsValidImageAsync(stream);

        Assert.False(result);
    }

    [Fact]
    public async Task IsValidImageAsync_ResetsStreamPosition()
    {
        var (sut, _) = CreateSut();
        using var stream = CreateMinimalPng();

        await sut.IsValidImageAsync(stream);

        Assert.Equal(0, stream.Position);
    }

    // ── ProcessAsync — profile (avatar) ─────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_ProfileUpload_GeneratesAvatarVariant()
    {
        var (sut, blobMock) = CreateSut();
        SetupDownloadWithImage(blobMock, "profiles/u1/original.jpg");

        var result = await sut.ProcessAsync("profiles/u1/original.jpg", MediaUploadUse.Profile);

        Assert.True(result.Success);
        Assert.Single(result.Variants);
        Assert.Equal("avatar", result.Variants[0].VariantName);
    }

    [Fact]
    public async Task ProcessAsync_ProfileUpload_AvatarBlobPathIsCorrect()
    {
        var (sut, blobMock) = CreateSut();
        SetupDownloadWithImage(blobMock, "profiles/u1/original.jpg");

        var result = await sut.ProcessAsync("profiles/u1/original.jpg", MediaUploadUse.Profile);

        Assert.Equal("profiles/u1/avatar.webp", result.Variants[0].BlobPath);
    }

    // ── ProcessAsync — recipe ────────────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_RecipeUpload_GeneratesHeroCardThumbnailVariants()
    {
        var (sut, blobMock) = CreateSut();
        SetupDownloadWithImage(blobMock, "recipes/r1/v1/original.jpg");

        var result = await sut.ProcessAsync("recipes/r1/v1/original.jpg", MediaUploadUse.Recipe);

        Assert.True(result.Success);
        Assert.Equal(3, result.Variants.Count);
        var names = result.Variants.Select(v => v.VariantName).ToHashSet();
        Assert.Contains("hero", names);
        Assert.Contains("card", names);
        Assert.Contains("thumbnail", names);
    }

    // ── ProcessAsync — content ───────────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_ContentUpload_GeneratesHeroAndThumbnailVariants()
    {
        var (sut, blobMock) = CreateSut();
        SetupDownloadWithImage(blobMock, "content/c1/original.jpg");

        var result = await sut.ProcessAsync("content/c1/original.jpg", MediaUploadUse.Content);

        Assert.True(result.Success);
        Assert.Equal(2, result.Variants.Count);
        var names = result.Variants.Select(v => v.VariantName).ToHashSet();
        Assert.Contains("hero", names);
        Assert.Contains("thumbnail", names);
    }

    // ── ProcessAsync — WebP upload format ───────────────────────────────────

    [Fact]
    public async Task ProcessAsync_Variants_AreUploadedAsWebP()
    {
        var (sut, blobMock) = CreateSut();
        SetupDownloadWithImage(blobMock, "profiles/u1/original.jpg");

        await sut.ProcessAsync("profiles/u1/original.jpg", MediaUploadUse.Profile);

        blobMock.Verify(
            b => b.UploadAsync(
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                "image/webp",
                It.IsAny<IDictionary<string, string>?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── ProcessAsync — error handling ────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_DownloadFails_ReturnsFailureResult()
    {
        var (sut, blobMock) = CreateSut();
        blobMock.Setup(b => b.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Simulated download error"));

        var result = await sut.ProcessAsync("profiles/u1/original.jpg", MediaUploadUse.Profile);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Empty(result.Variants);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static (ImageProcessingService sut, Mock<IBlobStorageService> blobMock) CreateSut()
    {
        var blobMock = new Mock<IBlobStorageService>();
        blobMock.Setup(b => b.GetPublicUrl(It.IsAny<string>()))
                .Returns<string>(path => $"https://blob.example.com/blend-media/{path}");

        var sut = new ImageProcessingService(
            blobMock.Object,
            NullLogger<ImageProcessingService>.Instance);

        return (sut, blobMock);
    }

    private static void SetupDownloadWithImage(Mock<IBlobStorageService> blobMock, string blobPath)
    {
        blobMock.Setup(b => b.DownloadAsync(blobPath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => (Stream)CreateMinimalPng());
    }

    /// <summary>
    /// Creates a valid minimal 1×1 PNG using ImageSharp (available transitively through Blend.Infrastructure).
    /// </summary>
    private static MemoryStream CreateMinimalPng()
    {
        // Create a 1x1 blue pixel image and encode it as PNG
        using var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgb24>(1, 1);
        image[0, 0] = new SixLabors.ImageSharp.PixelFormats.Rgb24(0, 0, 255);

        var ms = new MemoryStream();
        image.Save(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
        ms.Position = 0;
        return ms;
    }
}
