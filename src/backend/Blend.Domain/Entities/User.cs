using System.Text.Json.Serialization;

namespace Blend.Domain.Entities;

/// <summary>User roles within the application.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    User,
    Admin,
}

/// <summary>The unit of measurement preferred by the user.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MeasurementUnit
{
    Metric,
    Imperial,
}

/// <summary>Theme preference for the application UI.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ThemePreference
{
    System,
    Light,
    Dark,
}

/// <summary>Notification preferences embedded on the user document.</summary>
public sealed class NotificationPreferences
{
    [JsonPropertyName("friendRequests")]
    public bool FriendRequests { get; init; } = true;

    [JsonPropertyName("recipeLikes")]
    public bool RecipeLikes { get; init; } = true;

    [JsonPropertyName("recipePublished")]
    public bool RecipePublished { get; init; } = true;

    [JsonPropertyName("systemAnnouncements")]
    public bool SystemAnnouncements { get; init; } = true;
}

/// <summary>App settings embedded on the user document.</summary>
public sealed class AppSettings
{
    [JsonPropertyName("unitSystem")]
    public MeasurementUnit UnitSystem { get; init; } = MeasurementUnit.Metric;

    [JsonPropertyName("theme")]
    public ThemePreference Theme { get; init; } = ThemePreference.System;

    [JsonPropertyName("notifications")]
    public NotificationPreferences Notifications { get; init; } = new();
}

/// <summary>Embedded object representing a user's food preferences.</summary>
public sealed class UserPreferences
{
    [JsonPropertyName("favoriteCuisines")]
    public IReadOnlyList<string> FavoriteCuisines { get; init; } = [];

    [JsonPropertyName("favoriteDishTypes")]
    public IReadOnlyList<string> FavoriteDishTypes { get; init; } = [];

    [JsonPropertyName("diets")]
    public IReadOnlyList<string> Diets { get; init; } = [];

    [JsonPropertyName("intolerances")]
    public IReadOnlyList<string> Intolerances { get; init; } = [];

    [JsonPropertyName("dislikedIngredientIds")]
    public IReadOnlyList<string> DislikedIngredientIds { get; init; } = [];
}

/// <summary>A Blend user account.</summary>
public sealed class User
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; init; } = string.Empty;

    [JsonPropertyName("profilePhotoUrl")]
    public string? ProfilePhotoUrl { get; init; }

    /// <summary>Reference to a securely stored password hash (e.g., ASP.NET Core Identity).</summary>
    [JsonPropertyName("passwordHashRef")]
    public string? PasswordHashRef { get; init; }

    [JsonPropertyName("preferences")]
    public UserPreferences Preferences { get; init; } = new();

    [JsonPropertyName("measurementUnit")]
    public MeasurementUnit MeasurementUnit { get; init; } = MeasurementUnit.Metric;

    [JsonPropertyName("settings")]
    public AppSettings Settings { get; init; } = new();

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonPropertyName("unreadNotificationCount")]
    public int UnreadNotificationCount { get; init; }

    [JsonPropertyName("role")]
    public UserRole Role { get; init; } = UserRole.User;

    /// <summary>When set, the account is pending deletion. After 30 days the account is permanently removed.</summary>
    [JsonPropertyName("deletionRequestedAt")]
    public DateTimeOffset? DeletionRequestedAt { get; init; }

    /// <summary>Whether the account is deactivated (e.g., pending deletion during grace period).</summary>
    [JsonPropertyName("isDeactivated")]
    public bool IsDeactivated { get; init; }
}
