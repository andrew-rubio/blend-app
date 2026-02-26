using Blend.Api.Configuration;
using Blend.Api.Models;
using Blend.Api.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace Blend.Api.Tests.Services;

public class MediaServiceTests
{
    private static IOptions<BlobStorageOptions> StorageOptions(Action<BlobStorageOptions>? cfg = null)
    {
        var o = new BlobStorageOptions
        {
            ConnectionString = "UseDevelopmentStorage=true",
            ContainerName = "blend-media",
            SasExpiryMinutes = 15,
            MaxImageSizeBytes = 52_428_800,
            MaxVideoSizeBytes = 524_288_000
        };
        cfg?.Invoke(o);
        return Options.Create(o);
    }

    private static MediaService CreateService(
        Mock<IBlobStorageService>? blobMock = null,
        Mock<IImageProcessingService>? imageMock = null)
    {
        blobMock ??= new Mock<IBlobStorageService>();
        imageMock ??= new Mock<IImageProcessingService>();
        return new MediaService(blobMock.Object, imageMock.Object, StorageOptions());
    }

    // ── BuildBlobPath tests ──────────────────────────────────────────────────

    [Fact]
    public void BuildBlobPath_RecipeEntity_UsesRecipesFolder()
    {
        var id = Guid.NewGuid();
        var path = MediaService.BuildBlobPath("recipe", id, "photo.jpg");
        Assert.StartsWith($"recipes/{id}/", path);
    }

    [Fact]
    public void BuildBlobPath_ProfileEntity_UsesProfilesFolder()
    {
        var id = Guid.NewGuid();
        var path = MediaService.BuildBlobPath("profile", id, "avatar.png");
        Assert.StartsWith($"profiles/{id}/", path);
    }

    [Fact]
    public void BuildBlobPath_ContentEntity_UsesContentFolder()
    {
        var id = Guid.NewGuid();
        var path = MediaService.BuildBlobPath("content", id, "hero.webp");
        Assert.StartsWith($"content/{id}/", path);
    }

    [Fact]
    public void BuildBlobPath_UnknownEntity_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            MediaService.BuildBlobPath("unknown", Guid.NewGuid(), "file.jpg"));
    }

    [Fact]
    public void BuildBlobPath_PathTraversal_IsSanitised()
    {
        var id = Guid.NewGuid();
        // Attempt to inject path traversal in file name
        var path = MediaService.BuildBlobPath("recipe", id, "../../etc/passwd");
        Assert.EndsWith("passwd", path);
        Assert.DoesNotContain("..", path);
    }

    // ── GetUploadUrlAsync tests ──────────────────────────────────────────────

    [Fact]
    public async Task GetUploadUrl_ValidRequest_ReturnsSasUrl()
    {
        var blobMock = new Mock<IBlobStorageService>();
        blobMock.Setup(b => b.GenerateSasUploadUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync("https://storage.blob.core.windows.net/blend-media/recipes/abc/photo.jpg?sv=...");

        var service = CreateService(blobMock);
        var request = new MediaUploadRequest
        {
            FileName = "photo.jpg",
            ContentType = "image/jpeg",
            EntityType = "recipe",
            EntityId = Guid.NewGuid()
        };

        var response = await service.GetUploadUrlAsync(request);

        Assert.NotEmpty(response.UploadUrl);
        Assert.NotEmpty(response.BlobPath);
        Assert.True(response.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData("text/plain")]
    [InlineData("application/json")]
    [InlineData("image/gif")]
    public async Task GetUploadUrl_InvalidContentType_Throws(string contentType)
    {
        var service = CreateService();
        var request = new MediaUploadRequest
        {
            FileName = "file.txt",
            ContentType = contentType,
            EntityType = "recipe",
            EntityId = Guid.NewGuid()
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetUploadUrlAsync(request));
    }

    [Fact]
    public async Task GetUploadUrl_OversizedImage_Throws()
    {
        var service = CreateService();
        var request = new MediaUploadRequest
        {
            FileName = "huge.jpg",
            ContentType = "image/jpeg",
            EntityType = "recipe",
            EntityId = Guid.NewGuid(),
            FileSizeBytes = 100_000_000 // 100MB > 50MB limit
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetUploadUrlAsync(request));
    }

    [Fact]
    public async Task GetUploadUrl_OversizedVideo_Throws()
    {
        var service = CreateService();
        var request = new MediaUploadRequest
        {
            FileName = "huge.mp4",
            ContentType = "video/mp4",
            EntityType = "recipe",
            EntityId = Guid.NewGuid(),
            FileSizeBytes = 600_000_000 // 600MB > 500MB limit
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetUploadUrlAsync(request));
    }

    [Fact]
    public async Task GetUploadUrl_ValidVideo_WithinSizeLimit_Succeeds()
    {
        var blobMock = new Mock<IBlobStorageService>();
        blobMock.Setup(b => b.GenerateSasUploadUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync("https://example.com/sas");

        var service = CreateService(blobMock);
        var request = new MediaUploadRequest
        {
            FileName = "clip.mp4",
            ContentType = "video/mp4",
            EntityType = "content",
            EntityId = Guid.NewGuid(),
            FileSizeBytes = 100_000_000 // 100MB < 500MB limit
        };

        var response = await service.GetUploadUrlAsync(request);
        Assert.NotEmpty(response.UploadUrl);
    }

    // ── CompleteUploadAsync tests ────────────────────────────────────────────

    [Fact]
    public async Task CompleteUpload_ImageBlob_TriggersProcessing()
    {
        var blobMock = new Mock<IBlobStorageService>();
        blobMock.Setup(b => b.GetBlobUrlAsync(It.IsAny<string>()))
                .ReturnsAsync((string p) => $"https://cdn.example.com/{p}");

        var imageMock = new Mock<IImageProcessingService>();
        imageMock.Setup(i => i.ProcessImageAsync(It.IsAny<string>(), It.IsAny<MediaType>()))
                 .ReturnsAsync(new ProcessedMedia
                 {
                     OriginalPath  = "recipes/x/photo.jpg",
                     HeroPath      = "recipes/x/photo-hero.webp",
                     CardPath      = "recipes/x/photo-card.webp",
                     ThumbnailPath = "recipes/x/photo-thumbnail.webp"
                 });

        var service = CreateService(blobMock, imageMock);
        var request = new UploadCompleteRequest
        {
            BlobPath   = "recipes/x/photo.jpg",
            EntityType = "recipe",
            EntityId   = Guid.NewGuid()
        };

        var response = await service.CompleteUploadAsync(request);

        Assert.NotEmpty(response.MediaId);
        Assert.NotEmpty(response.Urls.Original);
        Assert.NotNull(response.Urls.Hero);
        Assert.NotNull(response.Urls.Card);
        Assert.NotNull(response.Urls.Thumbnail);
        imageMock.Verify(i => i.ProcessImageAsync(It.IsAny<string>(), MediaType.Recipe), Times.Once);
    }

    [Fact]
    public async Task CompleteUpload_VideoBlob_SkipsImageProcessing()
    {
        var blobMock = new Mock<IBlobStorageService>();
        blobMock.Setup(b => b.GetBlobUrlAsync(It.IsAny<string>()))
                .ReturnsAsync("https://cdn.example.com/content/x/clip.mp4");

        var imageMock = new Mock<IImageProcessingService>();

        var service = CreateService(blobMock, imageMock);
        var request = new UploadCompleteRequest
        {
            BlobPath   = "content/x/clip.mp4",
            EntityType = "content",
            EntityId   = Guid.NewGuid()
        };

        var response = await service.CompleteUploadAsync(request);

        Assert.NotEmpty(response.Urls.Original);
        imageMock.Verify(i => i.ProcessImageAsync(It.IsAny<string>(), It.IsAny<MediaType>()), Times.Never);
    }

    [Fact]
    public async Task CompleteUpload_ProfileEntity_UsesProfileMediaType()
    {
        var blobMock = new Mock<IBlobStorageService>();
        blobMock.Setup(b => b.GetBlobUrlAsync(It.IsAny<string>()))
                .ReturnsAsync((string p) => $"https://cdn.example.com/{p}");

        var imageMock = new Mock<IImageProcessingService>();
        imageMock.Setup(i => i.ProcessImageAsync(It.IsAny<string>(), MediaType.Profile))
                 .ReturnsAsync(new ProcessedMedia
                 {
                     OriginalPath = "profiles/u/avatar.jpg",
                     AvatarPath   = "profiles/u/avatar-avatar.webp"
                 });

        var service = CreateService(blobMock, imageMock);
        var request = new UploadCompleteRequest
        {
            BlobPath   = "profiles/u/avatar.jpg",
            EntityType = "profile",
            EntityId   = Guid.NewGuid()
        };

        await service.CompleteUploadAsync(request);

        imageMock.Verify(i => i.ProcessImageAsync(It.IsAny<string>(), MediaType.Profile), Times.Once);
    }
}
