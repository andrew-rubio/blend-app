using Blend.Domain.Entities;
using Blend.Domain.Interfaces;
using Xunit;

namespace Blend.Tests.Unit;

/// <summary>
/// Unit tests for domain entity serialisation and partition key conventions.
/// </summary>
public class EntityTests
{
    [Fact]
    public void CosmosEntity_NewInstance_HasNonEmptyId()
    {
        var entity = new User();
        Assert.NotNull(entity.Id);
        Assert.NotEmpty(entity.Id);
    }

    [Fact]
    public void CosmosEntity_NewInstance_CreatedAtIsUtcNow()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var entity = new User();
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        Assert.True(entity.CreatedAt >= before);
        Assert.True(entity.CreatedAt <= after);
    }

    [Fact]
    public void User_PartitionKey_EqualsId()
    {
        var user = new User { Id = "user-123" };
        Assert.Equal(user.Id, user.PartitionKey);
    }

    [Fact]
    public void Recipe_HasCorrectDefaults()
    {
        var recipe = new Recipe();
        Assert.False(recipe.IsPublished);
        Assert.False(recipe.IsDeleted);
        Assert.Empty(recipe.Tags);
        Assert.Empty(recipe.Ingredients);
        Assert.Empty(recipe.Steps);
    }

    [Fact]
    public void RecipeMetadata_TotalTime_SumsPrepAndCook()
    {
        var meta = new RecipeMetadata { PrepTimeMinutes = 15, CookTimeMinutes = 30 };
        Assert.Equal(45, meta.TotalTimeMinutes);
    }

    [Fact]
    public void UserPreferences_Defaults_AreReasonable()
    {
        var prefs = new UserPreferences();
        Assert.Equal(SkillLevel.Beginner, prefs.CookingSkillLevel);
        Assert.Equal(60, prefs.MaxCookTimeMinutes);
        Assert.Equal(2, prefs.ServingSize);
        Assert.False(prefs.MetricUnits);
    }

    [Fact]
    public void Notification_Default_IsUnread()
    {
        var n = new Notification();
        Assert.False(n.IsRead);
        Assert.Null(n.ReadAt);
    }

    [Fact]
    public void CookingSession_Default_IsInProgress()
    {
        var session = new CookingSession();
        Assert.Equal(CookingSessionStatus.InProgress, session.Status);
        Assert.Equal(0, session.CurrentStepIndex);
    }

    [Fact]
    public void Connection_Default_IsPending()
    {
        var conn = new Connection();
        Assert.Equal(ConnectionStatus.Pending, conn.Status);
    }

    [Fact]
    public void Content_Default_IsDraft()
    {
        var content = new Content();
        Assert.Equal(ContentStatus.Draft, content.Status);
    }
}
