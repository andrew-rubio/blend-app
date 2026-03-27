using Blend.Api.Recipes.Services;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Blend.Tests.Unit.Recipes;

public class RecipeServiceTests
{
    private static RecipeService CreateService(
        IRepository<Recipe>? recipeRepo = null,
        IRepository<Activity>? activityRepo = null) =>
        new(NullLogger<RecipeService>.Instance, recipeRepo, activityRepo);

    private static Recipe MakeRecipe(string id, string authorId, bool isPublic) => new()
    {
        Id = id,
        AuthorId = authorId,
        Title = "Test Recipe",
        IsPublic = isPublic,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
    };

    // ── GetRecipeAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRecipeAsync_PrivateRecipe_NonOwner_ReturnsNull()
    {
        var recipe = MakeRecipe("r1", "author-1", isPublic: false);
        var mockRepo = new Mock<IRepository<Recipe>>();
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([recipe]);
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([recipe]);

        var svc = CreateService(mockRepo.Object);
        var result = await svc.GetRecipeAsync("r1", "other-user");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetRecipeAsync_PrivateRecipe_Owner_ReturnsRecipe()
    {
        var recipe = MakeRecipe("r1", "author-1", isPublic: false);
        var mockRepo = new Mock<IRepository<Recipe>>();
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([recipe]);
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([recipe]);

        var svc = CreateService(mockRepo.Object);
        var result = await svc.GetRecipeAsync("r1", "author-1");
        Assert.NotNull(result);
        Assert.Equal("r1", result.Id);
    }

    [Fact]
    public async Task GetRecipeAsync_PublicRecipe_AnyUser_ReturnsRecipe()
    {
        var recipe = MakeRecipe("r1", "author-1", isPublic: true);
        var mockRepo = new Mock<IRepository<Recipe>>();
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([recipe]);
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([recipe]);

        var svc = CreateService(mockRepo.Object);
        var result = await svc.GetRecipeAsync("r1", "some-other-user");
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetRecipeAsync_PublicRecipe_AnonymousUser_ReturnsRecipe()
    {
        var recipe = MakeRecipe("r1", "author-1", isPublic: true);
        var mockRepo = new Mock<IRepository<Recipe>>();
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([recipe]);
        mockRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([recipe]);

        var svc = CreateService(mockRepo.Object);
        var result = await svc.GetRecipeAsync("r1", null);
        Assert.NotNull(result);
    }

    // ── LikeRecipeAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task LikeRecipeAsync_AlreadyLiked_ReturnsAlreadyLiked()
    {
        var recipe = MakeRecipe("r1", "author-1", isPublic: true);
        var activity = new Activity
        {
            Id = "user-1:like:r1",
            UserId = "user-1",
            Type = ActivityType.Liked,
            ReferenceId = "r1",
            ReferenceType = "Recipe",
            Timestamp = DateTimeOffset.UtcNow,
        };

        var mockRecipeRepo = new Mock<IRepository<Recipe>>();
        mockRecipeRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([recipe]);
        mockRecipeRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([recipe]);

        var mockActivityRepo = new Mock<IRepository<Activity>>();
        mockActivityRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([activity]);
        mockActivityRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([activity]);

        var svc = CreateService(mockRecipeRepo.Object, mockActivityRepo.Object);
        var result = await svc.LikeRecipeAsync("r1", "user-1");
        Assert.Equal(RecipeOpResult.AlreadyLiked, result);
    }

    // ── UnlikeRecipeAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task UnlikeRecipeAsync_NotLiked_ReturnsNotLiked()
    {
        var recipe = MakeRecipe("r1", "author-1", isPublic: true);

        var mockRecipeRepo = new Mock<IRepository<Recipe>>();
        mockRecipeRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([recipe]);
        mockRecipeRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([recipe]);

        var mockActivityRepo = new Mock<IRepository<Activity>>();
        mockActivityRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        mockActivityRepo.Setup(r => r.GetByQueryAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var svc = CreateService(mockRecipeRepo.Object, mockActivityRepo.Object);
        var result = await svc.UnlikeRecipeAsync("r1", "user-1");
        Assert.Equal(RecipeOpResult.NotLiked, result);
    }
}
