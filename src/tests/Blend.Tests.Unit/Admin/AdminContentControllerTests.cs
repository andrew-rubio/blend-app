using Blend.Api.Controllers.Admin;
using Blend.Api.Models.Admin;
using Blend.Domain.Entities;
using Blend.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Security.Claims;

namespace Blend.Tests.Unit.Admin;

/// <summary>
/// Unit tests for AdminContentController: stories and videos CRUD.
/// </summary>
public class AdminContentControllerTests
{
    private readonly Mock<IRepository<Content>> _contentRepoMock = new();
    private readonly AdminContentController _controller;

    public AdminContentControllerTests()
    {
        _controller = new AdminContentController(
            _contentRepoMock.Object,
            NullLogger<AdminContentController>.Instance);

        SetAdminUser();
    }

    // ─── Stories ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStories_ReturnsOk_WithItems()
    {
        var paged = new PagedResult<Content>
        {
            Items = new List<Content>
            {
                new() { Id = "story-1", ContentType = "story", Title = "My Story" }
            }
        };
        _contentRepoMock
            .Setup(r => r.QueryAsync(
                It.IsAny<string>(),
                It.IsAny<PaginationOptions?>(),
                It.IsAny<IDictionary<string, object>?>(),
                "story",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _controller.GetStories();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CreateStory_ReturnsCreatedAtAction()
    {
        var content = new Content
        {
            Id = "story-new",
            ContentType = "story",
            Title = "New Story",
            Body = "# Hello"
        };
        _contentRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

        var request = new CreateStoryRequest
        {
            Title = "New Story",
            Content = "# Hello"
        };

        var result = await _controller.CreateStory(request);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task UpdateStory_ReturnsOk_WhenFound()
    {
        var existing = new Content { Id = "s1", ContentType = "story", Title = "Old" };
        _contentRepoMock
            .Setup(r => r.GetByIdAsync("s1", "story", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _contentRepoMock
            .Setup(r => r.UpdateAsync(existing, "story", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _controller.UpdateStory("s1", new UpdateStoryRequest { Title = "New Title" });

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DeleteStory_ReturnsNoContent_WhenFound()
    {
        _contentRepoMock
            .Setup(r => r.DeleteAsync("s1", "story", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.DeleteStory("s1");

        Assert.IsType<NoContentResult>(result);
    }

    // ─── Videos ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetVideos_ReturnsOk()
    {
        _contentRepoMock
            .Setup(r => r.QueryAsync(
                It.IsAny<string>(),
                It.IsAny<PaginationOptions?>(),
                It.IsAny<IDictionary<string, object>?>(),
                "video",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Content>());

        var result = await _controller.GetVideos();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CreateVideo_ReturnsCreatedAtAction()
    {
        var content = new Content
        {
            Id = "v-new",
            ContentType = "video",
            Title = "New Video",
            VideoUrl = "https://youtube.com/embed/abc"
        };
        _contentRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

        var result = await _controller.CreateVideo(new CreateVideoRequest
        {
            Title = "New Video",
            VideoUrl = "https://youtube.com/embed/abc"
        });

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task UpdateVideo_ReturnsNotFound_WhenMissing()
    {
        _contentRepoMock
            .Setup(r => r.GetByIdAsync("missing", "video", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content?)null);

        var result = await _controller.UpdateVideo("missing", new UpdateVideoRequest());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteVideo_ReturnsNotFound_WhenMissing()
    {
        _contentRepoMock
            .Setup(r => r.DeleteAsync("missing", "video", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.DeleteVideo("missing");

        Assert.IsType<NotFoundResult>(result);
    }

    // ─── Display order ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateFeaturedRecipe_UpdatesDisplayOrder()
    {
        var existing = new Content
        {
            Id = "fr-1",
            ContentType = "featured-recipe",
            Title = "Recipe",
            DisplayOrder = 1
        };
        _contentRepoMock
            .Setup(r => r.GetByIdAsync("fr-1", "featured-recipe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _contentRepoMock
            .Setup(r => r.UpdateAsync(existing, "featured-recipe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _controller.UpdateFeaturedRecipe(
            "fr-1",
            new UpdateFeaturedRecipeRequest { DisplayOrder = 5 });

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(5, existing.DisplayOrder);
    }

    private void SetAdminUser()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, "admin-1"), new Claim(ClaimTypes.Role, "admin") },
            "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }
}
