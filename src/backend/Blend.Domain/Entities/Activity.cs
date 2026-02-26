namespace Blend.Domain.Entities;

/// <summary>
/// Records user activity such as recipe views and cooking sessions.
/// Partition key: /userId
/// </summary>
public class Activity : CosmosEntity
{
    public string UserId { get; set; } = string.Empty;

    public ActivityType ActivityType { get; set; }

    public string? RecipeId { get; set; }

    public string? RecipeTitle { get; set; }

    public string? CookingSessionId { get; set; }

    public ActivityMetadata? Metadata { get; set; }
}

public enum ActivityType
{
    RecipeView,
    RecipeSave,
    RecipeUnsave,
    RecipeShare,
    CookingSessionStarted,
    CookingSessionCompleted,
    RecipeRated,
    RecipeCreated,
    RecipePublished,
    UserFollowed,
    UserUnfollowed
}

public class ActivityMetadata
{
    public int? DurationSeconds { get; set; }

    public int? Rating { get; set; }

    public string? Source { get; set; }

    public Dictionary<string, string> Extra { get; set; } = [];
}
