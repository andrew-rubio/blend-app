using Blend.Infrastructure.Media;

namespace Blend.Tests.Unit.Media;

public class BlobPathBuilderTests
{
    // ── ForProfile ───────────────────────────────────────────────────────────

    [Fact]
    public void ForProfile_BuildsExpectedPath()
    {
        var path = BlobPathBuilder.ForProfile("user-123", "photo.jpg");
        Assert.Equal("profiles/user-123/photo.jpg", path);
    }

    [Fact]
    public void ForProfile_IncludesUserIdInPath()
    {
        var path = BlobPathBuilder.ForProfile("user-abc", "avatar.webp");
        Assert.StartsWith("profiles/user-abc/", path);
    }

    // ── ForRecipe ────────────────────────────────────────────────────────────

    [Fact]
    public void ForRecipe_BuildsExpectedPath()
    {
        var path = BlobPathBuilder.ForRecipe("recipe-456", "v1", "hero.jpg");
        Assert.Equal("recipes/recipe-456/v1/hero.jpg", path);
    }

    [Fact]
    public void ForRecipe_IncludesVersionInPath()
    {
        var path = BlobPathBuilder.ForRecipe("recipe-abc", "v2", "card.jpg");
        Assert.Contains("/v2/", path);
    }

    // ── ForContent ───────────────────────────────────────────────────────────

    [Fact]
    public void ForContent_BuildsExpectedPath()
    {
        var path = BlobPathBuilder.ForContent("content-789", "banner.png");
        Assert.Equal("content/content-789/banner.png", path);
    }

    // ── GetFolder ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("profiles/user-1/photo.jpg",        "profiles/user-1/")]
    [InlineData("recipes/recipe-1/v1/original.jpg", "recipes/recipe-1/v1/")]
    [InlineData("content/content-1/img.png",        "content/content-1/")]
    public void GetFolder_ReturnsCorrectFolderPrefix(string blobPath, string expectedFolder)
    {
        Assert.Equal(expectedFolder, BlobPathBuilder.GetFolder(blobPath));
    }

    [Fact]
    public void GetFolder_NoSlash_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, BlobPathBuilder.GetFolder("noslash"));
    }

    // ── Path security: profile path always scoped to userId ──────────────────

    [Fact]
    public void ForProfile_TwoUsers_ProduceDifferentPaths()
    {
        var path1 = BlobPathBuilder.ForProfile("user-1", "photo.jpg");
        var path2 = BlobPathBuilder.ForProfile("user-2", "photo.jpg");
        Assert.NotEqual(path1, path2);
        Assert.DoesNotContain("user-2", path1);
        Assert.DoesNotContain("user-1", path2);
    }
}
