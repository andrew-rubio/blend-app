namespace Blend.Domain.Entities;

/// <summary>
/// User dietary and cooking preferences, embedded within the User document.
/// </summary>
public class UserPreferences
{
    public List<string> DietaryRestrictions { get; set; } = [];

    public List<string> AllergenExclusions { get; set; } = [];

    public List<string> CuisinePreferences { get; set; } = [];

    public SkillLevel CookingSkillLevel { get; set; } = SkillLevel.Beginner;

    public int MaxCookTimeMinutes { get; set; } = 60;

    public int ServingSize { get; set; } = 2;

    public bool MetricUnits { get; set; } = false;

    public NotificationSettings Notifications { get; set; } = new();
}

public enum SkillLevel
{
    Beginner,
    Intermediate,
    Advanced,
    Expert
}

public class NotificationSettings
{
    public bool RecipeRecommendations { get; set; } = true;

    public bool FriendActivity { get; set; } = true;

    public bool CookingReminders { get; set; } = true;

    public bool SystemUpdates { get; set; } = true;
}
