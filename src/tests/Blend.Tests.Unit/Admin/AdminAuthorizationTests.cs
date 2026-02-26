using System.Security.Claims;
using Blend.Api.Controllers.Admin;
using Blend.Api.Models.Admin;
using Blend.Api.Services;
using Blend.Domain.Entities;
using Blend.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Blend.Tests.Unit.Admin;

/// <summary>
/// Unit tests for admin role authorisation on AdminContentController.
/// </summary>
public class AdminAuthorizationTests
{
    private readonly Mock<IRepository<Content>> _contentRepoMock = new();
    private readonly AdminContentController _controller;

    public AdminAuthorizationTests()
    {
        _controller = new AdminContentController(
            _contentRepoMock.Object,
            NullLogger<AdminContentController>.Instance);
    }

    [Fact]
    public void AdminContentController_HasAuthorizePolicy_RequireAdmin()
    {
        var type = typeof(AdminContentController);
        var attr = type.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), inherit: true)
            .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .FirstOrDefault();

        Assert.NotNull(attr);
        Assert.Equal("RequireAdmin", attr.Policy);
    }

    [Fact]
    public void AdminIngredientsController_HasAuthorizePolicy_RequireAdmin()
    {
        var type = typeof(AdminIngredientsController);
        var attr = type.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), inherit: true)
            .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .FirstOrDefault();

        Assert.NotNull(attr);
        Assert.Equal("RequireAdmin", attr.Policy);
    }

    [Fact]
    public async Task GetFeaturedRecipes_ReturnsOk_WithPagedResult()
    {
        var paged = new PagedResult<Content>
        {
            Items = new List<Content>
            {
                new() { Id = "1", ContentType = "featured-recipe", Title = "Test Recipe" }
            }
        };
        _contentRepoMock
            .Setup(r => r.QueryAsync(
                It.IsAny<string>(),
                It.IsAny<PaginationOptions?>(),
                It.IsAny<IDictionary<string, object>?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        SetAdminUser();

        var result = await _controller.GetFeaturedRecipes();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CreateFeaturedRecipe_ReturnsCreatedAtAction()
    {
        var content = new Content
        {
            Id = "new-id",
            ContentType = "featured-recipe",
            Title = "New Recipe",
            RecipeId = "recipe-1",
            RecipeSource = "spoonacular"
        };

        _contentRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

        SetAdminUser();

        var request = new CreateFeaturedRecipeRequest
        {
            RecipeId = "recipe-1",
            Source = "spoonacular",
            Title = "New Recipe"
        };

        var result = await _controller.CreateFeaturedRecipe(request);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task UpdateFeaturedRecipe_ReturnsNotFound_WhenMissing()
    {
        _contentRepoMock
            .Setup(r => r.GetByIdAsync("missing", "featured-recipe", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content?)null);

        SetAdminUser();

        var result = await _controller.UpdateFeaturedRecipe("missing", new UpdateFeaturedRecipeRequest());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteFeaturedRecipe_ReturnsNoContent_WhenDeleted()
    {
        _contentRepoMock
            .Setup(r => r.DeleteAsync("id-1", "featured-recipe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        SetAdminUser();

        var result = await _controller.DeleteFeaturedRecipe("id-1");

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteFeaturedRecipe_ReturnsNotFound_WhenMissing()
    {
        _contentRepoMock
            .Setup(r => r.DeleteAsync("missing", "featured-recipe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        SetAdminUser();

        var result = await _controller.DeleteFeaturedRecipe("missing");

        Assert.IsType<NotFoundResult>(result);
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
