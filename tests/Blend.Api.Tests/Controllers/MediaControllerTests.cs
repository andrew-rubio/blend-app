using Blend.Api.Controllers;
using Blend.Api.Models;
using Blend.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Blend.Api.Tests.Controllers;

public class MediaControllerTests
{
    private static MediaController CreateController(Mock<IMediaService>? mediaMock = null)
    {
        mediaMock ??= new Mock<IMediaService>();
        var controller = new MediaController(mediaMock.Object, NullLogger<MediaController>.Instance);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    // ── GetUploadUrl tests ───────────────────────────────────────────────────

    [Fact]
    public async Task GetUploadUrl_ValidRequest_Returns200WithUploadUrl()
    {
        var mediaMock = new Mock<IMediaService>();
        mediaMock.Setup(m => m.GetUploadUrlAsync(It.IsAny<MediaUploadRequest>()))
                 .ReturnsAsync(new MediaUploadResponse
                 {
                     UploadUrl = "https://storage.blob.core.windows.net/blend-media/recipes/abc/photo.jpg?sv=...",
                     BlobPath  = "recipes/abc/photo.jpg",
                     ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
                 });

        var controller = CreateController(mediaMock);
        var request = new MediaUploadRequest
        {
            FileName    = "photo.jpg",
            ContentType = "image/jpeg",
            EntityType  = "recipe",
            EntityId    = Guid.NewGuid()
        };

        var result = await controller.GetUploadUrl(request);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<MediaUploadResponse>(ok.Value);
        Assert.NotEmpty(response.UploadUrl);
        Assert.NotEmpty(response.BlobPath);
    }

    [Fact]
    public async Task GetUploadUrl_InvalidContentType_Returns400()
    {
        var mediaMock = new Mock<IMediaService>();
        mediaMock.Setup(m => m.GetUploadUrlAsync(It.IsAny<MediaUploadRequest>()))
                 .ThrowsAsync(new ArgumentException("Content type 'text/plain' is not allowed."));

        var controller = CreateController(mediaMock);
        var request = new MediaUploadRequest
        {
            FileName    = "file.txt",
            ContentType = "text/plain",
            EntityType  = "recipe",
            EntityId    = Guid.NewGuid()
        };

        var result = await controller.GetUploadUrl(request);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }

    [Fact]
    public async Task GetUploadUrl_ModelStateInvalid_Returns400()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("FileName", "Required");

        var request = new MediaUploadRequest();
        var result = await controller.GetUploadUrl(request);

        Assert.IsType<ValidationProblemDetails>(
            ((ObjectResult)result).Value);
    }

    [Fact]
    public async Task GetUploadUrl_SetsCorrelationIdHeader()
    {
        var mediaMock = new Mock<IMediaService>();
        mediaMock.Setup(m => m.GetUploadUrlAsync(It.IsAny<MediaUploadRequest>()))
                 .ReturnsAsync(new MediaUploadResponse
                 {
                     UploadUrl = "https://example.com/sas",
                     BlobPath  = "recipes/abc/photo.jpg",
                     ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
                 });

        var controller = CreateController(mediaMock);
        var request = new MediaUploadRequest
        {
            FileName    = "photo.jpg",
            ContentType = "image/jpeg",
            EntityType  = "recipe",
            EntityId    = Guid.NewGuid()
        };

        await controller.GetUploadUrl(request);

        Assert.True(controller.Response.Headers.ContainsKey("X-Correlation-Id"));
    }

    [Fact]
    public async Task GetUploadUrl_WithIncomingCorrelationId_EchoesIt()
    {
        var mediaMock = new Mock<IMediaService>();
        mediaMock.Setup(m => m.GetUploadUrlAsync(It.IsAny<MediaUploadRequest>()))
                 .ReturnsAsync(new MediaUploadResponse
                 {
                     UploadUrl = "https://example.com/sas",
                     BlobPath  = "recipes/abc/photo.jpg",
                     ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
                 });

        var controller = CreateController(mediaMock);
        var correlationId = "test-correlation-123";
        controller.Request.Headers["X-Correlation-Id"] = correlationId;

        var request = new MediaUploadRequest
        {
            FileName    = "photo.jpg",
            ContentType = "image/jpeg",
            EntityType  = "recipe",
            EntityId    = Guid.NewGuid()
        };

        await controller.GetUploadUrl(request);

        Assert.Equal(correlationId, controller.Response.Headers["X-Correlation-Id"].ToString());
    }

    // ── UploadComplete tests ─────────────────────────────────────────────────

    [Fact]
    public async Task UploadComplete_ValidRequest_Returns200WithVariantUrls()
    {
        var mediaMock = new Mock<IMediaService>();
        mediaMock.Setup(m => m.CompleteUploadAsync(It.IsAny<UploadCompleteRequest>()))
                 .ReturnsAsync(new UploadCompleteResponse
                 {
                     MediaId = Guid.NewGuid().ToString(),
                     Urls = new MediaVariantUrls
                     {
                         Original  = "https://cdn.example.com/recipes/abc/photo.jpg",
                         Hero      = "https://cdn.example.com/recipes/abc/photo-hero.webp",
                         Card      = "https://cdn.example.com/recipes/abc/photo-card.webp",
                         Thumbnail = "https://cdn.example.com/recipes/abc/photo-thumbnail.webp"
                     }
                 });

        var controller = CreateController(mediaMock);
        var request = new UploadCompleteRequest
        {
            BlobPath   = "recipes/abc/photo.jpg",
            EntityType = "recipe",
            EntityId   = Guid.NewGuid()
        };

        var result = await controller.UploadComplete(request);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UploadCompleteResponse>(ok.Value);
        Assert.NotEmpty(response.MediaId);
        Assert.NotEmpty(response.Urls.Original);
    }

    [Fact]
    public async Task UploadComplete_InvalidEntityType_Returns400()
    {
        var mediaMock = new Mock<IMediaService>();
        mediaMock.Setup(m => m.CompleteUploadAsync(It.IsAny<UploadCompleteRequest>()))
                 .ThrowsAsync(new ArgumentException("Entity type 'bad' is not valid."));

        var controller = CreateController(mediaMock);
        var request = new UploadCompleteRequest
        {
            BlobPath   = "recipes/abc/photo.jpg",
            EntityType = "bad",
            EntityId   = Guid.NewGuid()
        };

        var result = await controller.UploadComplete(request);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }
}
