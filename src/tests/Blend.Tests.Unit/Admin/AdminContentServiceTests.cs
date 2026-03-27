using Blend.Api.Admin.Models;
using Blend.Api.Admin.Services;
using Blend.Api.Ingredients.Services;
using Blend.Api.Notifications.Services;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Blend.Tests.Unit.Admin;

/// <summary>Unit tests for <see cref="AdminContentService"/>.</summary>
public class AdminContentServiceTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static AdminContentService CreateService(
        IRepository<Content>? contentRepo = null,
        IKnowledgeBaseService? kbService = null,
        INotificationService? notificationService = null) =>
        new(NullLogger<AdminContentService>.Instance, contentRepo, kbService, notificationService);

    private static Mock<IRepository<Content>> CreateContentRepoMock() => new();

    // ── GetFeaturedRecipesAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetFeaturedRecipes_WhenRepoNull_ReturnsEmptyList()
    {
        var svc = CreateService();
        var result = await svc.GetFeaturedRecipesAsync();
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFeaturedRecipes_ReturnsOnlyFeaturedRecipeType()
    {
        var mock = CreateContentRepoMock();
        var items = new List<Content>
        {
            new() { Id = "1", ContentType = ContentType.FeaturedRecipe, Title = "Recipe A", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new() { Id = "2", ContentType = ContentType.FeaturedRecipe, Title = "Recipe B", CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
        };

        mock.Setup(r => r.GetByQueryAsync(
                It.Is<string>(q => q.Contains("FeaturedRecipe")),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Content>)items);
        mock.Setup(r => r.GetByQueryAsync(It.Is<string>(q => q.Contains("FeaturedRecipe")), It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Content>)items);

        var svc = CreateService(mock.Object);
        var result = await svc.GetFeaturedRecipesAsync();

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(ContentType.FeaturedRecipe, r.ContentType));
    }

    // ── CreateFeaturedRecipeAsync ─────────────────────────────────────────────

    [Fact]
    public async Task CreateFeaturedRecipe_WhenRepoNull_ReturnsUnsavedContent()
    {
        var svc = CreateService();

        var request = new CreateFeaturedRecipeRequest
        {
            RecipeId = "r1",
            Source = "spoonacular",
            Title = "Test Recipe",
            DisplayOrder = 1,
        };

        var result = await svc.CreateFeaturedRecipeAsync(request);

        Assert.Equal("Test Recipe", result.Title);
        Assert.Equal(ContentType.FeaturedRecipe, result.ContentType);
        Assert.Equal("r1", result.RecipeId);
        Assert.Equal("spoonacular", result.Source);
        Assert.Equal(1, result.DisplayOrder);
    }

    [Fact]
    public async Task CreateFeaturedRecipe_CallsRepoCreateAsync()
    {
        var mock = CreateContentRepoMock();
        Content? captured = null;
        mock.Setup(r => r.CreateAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()))
            .Callback<Content, CancellationToken>((c, _) => captured = c)
            .ReturnsAsync((Content c, CancellationToken _) => c);

        var svc = CreateService(mock.Object);
        var request = new CreateFeaturedRecipeRequest
        {
            RecipeId = "r2",
            Source = "community",
            Title = "Community Recipe",
            Description = "Desc",
            DisplayOrder = 2,
        };

        await svc.CreateFeaturedRecipeAsync(request);

        Assert.NotNull(captured);
        Assert.Equal(ContentType.FeaturedRecipe, captured.ContentType);
        Assert.Equal("r2", captured.RecipeId);
        Assert.Equal("community", captured.Source);
        Assert.Equal("Desc", captured.Body);
        Assert.Equal(2, captured.DisplayOrder);
    }

    // ── UpdateFeaturedRecipeAsync ─────────────────────────────────────────────

    [Fact]
    public async Task UpdateFeaturedRecipe_WhenRepoNull_ReturnsNull()
    {
        var svc = CreateService();
        var result = await svc.UpdateFeaturedRecipeAsync("id1", new UpdateFeaturedRecipeRequest());
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateFeaturedRecipe_WhenNotFound_ReturnsNull()
    {
        var mock = CreateContentRepoMock();
        mock.Setup(r => r.GetByIdAsync("notexist", "notexist", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content?)null);

        var svc = CreateService(mock.Object);
        var result = await svc.UpdateFeaturedRecipeAsync("notexist", new UpdateFeaturedRecipeRequest { Title = "New" });
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateFeaturedRecipe_WhenFound_UpdatesAndReturnsContent()
    {
        var existing = new Content
        {
            Id = "fr1",
            ContentType = ContentType.FeaturedRecipe,
            Title = "Old Title",
            RecipeId = "r1",
            Source = "spoonacular",
            DisplayOrder = 1,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1),
        };

        var mock = CreateContentRepoMock();
        mock.Setup(r => r.GetByIdAsync("fr1", "fr1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        mock.Setup(r => r.UpdateAsync(It.IsAny<Content>(), "fr1", "fr1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content c, string _, string _, CancellationToken _) => c);

        var svc = CreateService(mock.Object);
        var result = await svc.UpdateFeaturedRecipeAsync("fr1", new UpdateFeaturedRecipeRequest
        {
            Title = "New Title",
            DisplayOrder = 5,
        });

        Assert.NotNull(result);
        Assert.Equal("New Title", result.Title);
        Assert.Equal(5, result.DisplayOrder);
        Assert.Equal("r1", result.RecipeId); // Unchanged
    }

    // ── DeleteContentAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteContent_WhenRepoNull_ReturnsFalse()
    {
        var svc = CreateService();
        var result = await svc.DeleteContentAsync("id1", ContentType.FeaturedRecipe);
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteContent_WhenNotFound_ReturnsFalse()
    {
        var mock = CreateContentRepoMock();
        mock.Setup(r => r.GetByIdAsync("notexist", "notexist", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content?)null);

        var svc = CreateService(mock.Object);
        var result = await svc.DeleteContentAsync("notexist", ContentType.FeaturedRecipe);
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteContent_WhenWrongContentType_ReturnsFalse()
    {
        var existing = new Content
        {
            Id = "s1",
            ContentType = ContentType.Story,
            Title = "A Story",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        var mock = CreateContentRepoMock();
        mock.Setup(r => r.GetByIdAsync("s1", "s1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var svc = CreateService(mock.Object);
        // Try to delete as FeaturedRecipe but it's a Story
        var result = await svc.DeleteContentAsync("s1", ContentType.FeaturedRecipe);
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteContent_WhenFound_CallsDeleteAndReturnsTrue()
    {
        var existing = new Content
        {
            Id = "fr2",
            ContentType = ContentType.FeaturedRecipe,
            Title = "To Delete",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        var mock = CreateContentRepoMock();
        mock.Setup(r => r.GetByIdAsync("fr2", "fr2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        mock.Setup(r => r.DeleteAsync("fr2", "fr2", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var svc = CreateService(mock.Object);
        var result = await svc.DeleteContentAsync("fr2", ContentType.FeaturedRecipe);

        Assert.True(result);
        mock.Verify(r => r.DeleteAsync("fr2", "fr2", It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Stories ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStories_WhenRepoNull_ReturnsEmptyList()
    {
        var svc = CreateService();
        var result = await svc.GetStoriesAsync();
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateStory_WhenRepoNull_ReturnsUnsavedStory()
    {
        var svc = CreateService();
        var request = new CreateStoryRequest
        {
            Title = "My Story",
            Author = "Jane Doe",
            Content = "# Hello",
            ReadingTimeMinutes = 3,
        };

        var result = await svc.CreateStoryAsync(request);

        Assert.Equal("My Story", result.Title);
        Assert.Equal(ContentType.Story, result.ContentType);
        Assert.Equal("Jane Doe", result.AuthorName);
        Assert.Equal("# Hello", result.Body);
        Assert.Equal(3, result.ReadingTimeMinutes);
    }

    [Fact]
    public async Task CreateStory_SetsRelatedRecipeIds()
    {
        var mock = CreateContentRepoMock();
        mock.Setup(r => r.CreateAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content c, CancellationToken _) => c);

        var svc = CreateService(mock.Object);
        var request = new CreateStoryRequest
        {
            Title = "Story With Recipes",
            RelatedRecipeIds = ["recipe-1", "recipe-2"],
        };

        var result = await svc.CreateStoryAsync(request);

        Assert.NotNull(result.RelatedRecipeIds);
        Assert.Equal(2, result.RelatedRecipeIds.Count);
    }

    [Fact]
    public async Task UpdateStory_WhenNotFound_ReturnsNull()
    {
        var mock = CreateContentRepoMock();
        mock.Setup(r => r.GetByIdAsync("missing", "missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content?)null);

        var svc = CreateService(mock.Object);
        var result = await svc.UpdateStoryAsync("missing", new UpdateStoryRequest { Title = "New" });
        Assert.Null(result);
    }

    // ── Videos ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetVideos_WhenRepoNull_ReturnsEmptyList()
    {
        var svc = CreateService();
        var result = await svc.GetVideosAsync();
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateVideo_WhenRepoNull_ReturnsUnsavedVideo()
    {
        var svc = CreateService();
        var request = new CreateVideoRequest
        {
            Title = "Cooking Video",
            VideoUrl = "https://youtube.com/embed/abc",
            DurationSeconds = 300,
            Creator = "Chef Bob",
        };

        var result = await svc.CreateVideoAsync(request);

        Assert.Equal("Cooking Video", result.Title);
        Assert.Equal(ContentType.Video, result.ContentType);
        Assert.Equal("https://youtube.com/embed/abc", result.MediaUrl);
        Assert.Equal(300, result.DurationSeconds);
        Assert.Equal("Chef Bob", result.AuthorName);
    }

    [Fact]
    public async Task UpdateVideo_WhenNotFound_ReturnsNull()
    {
        var mock = CreateContentRepoMock();
        mock.Setup(r => r.GetByIdAsync("missing", "missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content?)null);

        var svc = CreateService(mock.Object);
        var result = await svc.UpdateVideoAsync("missing", new UpdateVideoRequest { Title = "New" });
        Assert.Null(result);
    }

    // ── Ingredient Submissions ────────────────────────────────────────────────

    [Fact]
    public async Task GetIngredientSubmissions_WhenRepoNull_ReturnsEmptyPage()
    {
        var svc = CreateService();
        var result = await svc.GetIngredientSubmissionsAsync(null, 20, null);

        Assert.Empty(result.Items);
        Assert.Null(result.NextCursor);
        Assert.False(result.HasMore);
    }

    [Fact]
    public async Task GetIngredientSubmissions_WithStatusFilter_BuildsCorrectQuery()
    {
        var mock = CreateContentRepoMock();
        string? capturedQuery = null;
        mock.Setup(r => r.GetPagedAsync(
                It.IsAny<string>(),
                It.IsAny<FeedPaginationOptions>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, FeedPaginationOptions, string?, CancellationToken>((q, _, _, _) => capturedQuery = q)
            .ReturnsAsync(new PagedResult<Content> { Items = [] });

        var svc = CreateService(mock.Object);
        await svc.GetIngredientSubmissionsAsync("pending", 10, null);

        Assert.NotNull(capturedQuery);
        Assert.Contains("Pending", capturedQuery);
    }

    [Fact]
    public async Task ApproveSubmission_WhenRepoNull_ReturnsNull()
    {
        var svc = CreateService();
        var result = await svc.ApproveIngredientSubmissionAsync("sub1");
        Assert.Null(result);
    }

    [Fact]
    public async Task ApproveSubmission_WhenNotFound_ReturnsNull()
    {
        var mock = CreateContentRepoMock();
        mock.Setup(r => r.GetByIdAsync("notexist", "notexist", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content?)null);

        var svc = CreateService(mock.Object);
        var result = await svc.ApproveIngredientSubmissionAsync("notexist");
        Assert.Null(result);
    }

    [Fact]
    public async Task ApproveSubmission_SetsStatusApprovedAndIndexesInKB()
    {
        var existing = new Content
        {
            Id = "sub1",
            ContentType = ContentType.IngredientSubmission,
            Title = "Turmeric",
            Category = "spice",
            SubmittedByUserId = "user-1",
            SubmissionStatus = SubmissionStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1),
        };

        var contentMock = CreateContentRepoMock();
        contentMock.Setup(r => r.GetByIdAsync("sub1", "sub1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        Content? saved = null;
        contentMock.Setup(r => r.UpdateAsync(It.IsAny<Content>(), "sub1", "sub1", It.IsAny<CancellationToken>()))
            .Callback<Content, string, string, CancellationToken>((c, _, _, _) => saved = c)
            .ReturnsAsync((Content c, string _, string _, CancellationToken _) => c);

        var kbMock = new Mock<IKnowledgeBaseService>();
        kbMock.Setup(k => k.IndexIngredientAsync("sub1", "Turmeric", "spice", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var notifMock = new Mock<INotificationService>();
        notifMock.Setup(n => n.CreateNotificationAsync(
                "user-1",
                NotificationType.IngredientApproved,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Blend.Domain.Entities.Notification());

        var svc = CreateService(contentMock.Object, kbMock.Object, notifMock.Object);
        var result = await svc.ApproveIngredientSubmissionAsync("sub1");

        Assert.NotNull(result);
        Assert.NotNull(saved);
        Assert.Equal(SubmissionStatus.Approved, saved.SubmissionStatus);

        kbMock.Verify(k => k.IndexIngredientAsync("sub1", "Turmeric", "spice", It.IsAny<CancellationToken>()), Times.Once);
        notifMock.Verify(n => n.CreateNotificationAsync(
            "user-1",
            NotificationType.IngredientApproved,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejectSubmission_WhenRepoNull_ReturnsNull()
    {
        var svc = CreateService();
        var result = await svc.RejectIngredientSubmissionAsync("sub1", "Not valid");
        Assert.Null(result);
    }

    [Fact]
    public async Task RejectSubmission_WhenNotFound_ReturnsNull()
    {
        var mock = CreateContentRepoMock();
        mock.Setup(r => r.GetByIdAsync("notexist", "notexist", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content?)null);

        var svc = CreateService(mock.Object);
        var result = await svc.RejectIngredientSubmissionAsync("notexist", "reason");
        Assert.Null(result);
    }

    [Fact]
    public async Task RejectSubmission_SetsStatusRejectedWithReasonAndNotifiesUser()
    {
        var existing = new Content
        {
            Id = "sub2",
            ContentType = ContentType.IngredientSubmission,
            Title = "Mystery Herb",
            SubmittedByUserId = "user-2",
            SubmissionStatus = SubmissionStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1),
        };

        var contentMock = CreateContentRepoMock();
        contentMock.Setup(r => r.GetByIdAsync("sub2", "sub2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        Content? saved = null;
        contentMock.Setup(r => r.UpdateAsync(It.IsAny<Content>(), "sub2", "sub2", It.IsAny<CancellationToken>()))
            .Callback<Content, string, string, CancellationToken>((c, _, _, _) => saved = c)
            .ReturnsAsync((Content c, string _, string _, CancellationToken _) => c);

        var notifMock = new Mock<INotificationService>();
        notifMock.Setup(n => n.CreateNotificationAsync(
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Blend.Domain.Entities.Notification());

        var svc = CreateService(contentMock.Object, notificationService: notifMock.Object);
        var result = await svc.RejectIngredientSubmissionAsync("sub2", "Insufficient information");

        Assert.NotNull(result);
        Assert.NotNull(saved);
        Assert.Equal(SubmissionStatus.Rejected, saved.SubmissionStatus);
        Assert.Equal("Insufficient information", saved.RejectionReason);

        notifMock.Verify(n => n.CreateNotificationAsync(
            "user-2",
            NotificationType.IngredientRejected,
            It.IsAny<string>(),
            It.Is<string>(m => m.Contains("Insufficient information")),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejectSubmission_WithoutReason_NotifiesUserWithoutReason()
    {
        var existing = new Content
        {
            Id = "sub3",
            ContentType = ContentType.IngredientSubmission,
            Title = "Odd Spice",
            SubmittedByUserId = "user-3",
            SubmissionStatus = SubmissionStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var contentMock = CreateContentRepoMock();
        contentMock.Setup(r => r.GetByIdAsync("sub3", "sub3", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        contentMock.Setup(r => r.UpdateAsync(It.IsAny<Content>(), "sub3", "sub3", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content c, string _, string _, CancellationToken _) => c);

        var notifMock = new Mock<INotificationService>();
        notifMock.Setup(n => n.CreateNotificationAsync(
                It.IsAny<string>(),
                It.IsAny<NotificationType>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Blend.Domain.Entities.Notification());

        var svc = CreateService(contentMock.Object, notificationService: notifMock.Object);
        var result = await svc.RejectIngredientSubmissionAsync("sub3", null);

        Assert.NotNull(result);
        // Message should not contain "Reason:" when no reason given
        notifMock.Verify(n => n.CreateNotificationAsync(
            "user-3",
            NotificationType.IngredientRejected,
            It.IsAny<string>(),
            It.Is<string>(m => !m.Contains("Reason:")),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
