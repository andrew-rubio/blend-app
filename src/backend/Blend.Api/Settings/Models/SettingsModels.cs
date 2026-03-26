using Blend.Domain.Entities;

namespace Blend.Api.Settings.Models;

/// <summary>Request body for updating app settings.</summary>
public sealed class UpdateSettingsRequest
{
    public MeasurementUnit? UnitSystem { get; init; }
    public ThemePreference? Theme { get; init; }
    public UpdateNotificationPreferencesRequest? Notifications { get; init; }
}

/// <summary>Notification preferences sub-request.</summary>
public sealed class UpdateNotificationPreferencesRequest
{
    public bool? FriendRequests { get; init; }
    public bool? RecipeLikes { get; init; }
    public bool? RecipePublished { get; init; }
    public bool? SystemAnnouncements { get; init; }
}

/// <summary>App settings response returned by GET /api/v1/settings.</summary>
public sealed class AppSettingsResponse
{
    public MeasurementUnit UnitSystem { get; init; }
    public ThemePreference Theme { get; init; }
    public NotificationPreferencesResponse Notifications { get; init; } = new();

    public static AppSettingsResponse FromEntity(AppSettings s) => new()
    {
        UnitSystem = s.UnitSystem,
        Theme = s.Theme,
        Notifications = new NotificationPreferencesResponse
        {
            FriendRequests = s.Notifications.FriendRequests,
            RecipeLikes = s.Notifications.RecipeLikes,
            RecipePublished = s.Notifications.RecipePublished,
            SystemAnnouncements = s.Notifications.SystemAnnouncements,
        },
    };
}

/// <summary>Notification preferences response.</summary>
public sealed class NotificationPreferencesResponse
{
    public bool FriendRequests { get; init; }
    public bool RecipeLikes { get; init; }
    public bool RecipePublished { get; init; }
    public bool SystemAnnouncements { get; init; }
}
