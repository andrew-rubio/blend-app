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

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonPropertyName("unreadNotificationCount")]
    public int UnreadNotificationCount { get; init; }

    [JsonPropertyName("role")]
    public UserRole Role { get; init; } = UserRole.User;
}
