using System.Text.Json.Serialization;

namespace Blend.Api.Profile.Models;

/// <summary>Full profile returned to the authenticated user for their own account.</summary>
public sealed class MyProfileResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; init; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; init; }

    [JsonPropertyName("bio")]
    public string? Bio { get; init; }

    [JsonPropertyName("joinDate")]
    public DateTimeOffset JoinDate { get; init; }

    [JsonPropertyName("recipeCount")]
    public int RecipeCount { get; init; }

    [JsonPropertyName("likeCount")]
    public int LikeCount { get; init; }

    [JsonPropertyName("followerCount")]
    public int FollowerCount { get; init; }

    [JsonPropertyName("followingCount")]
    public int FollowingCount { get; init; }
}

/// <summary>Public profile returned when viewing another user's profile.</summary>
public sealed class PublicProfileResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; init; } = string.Empty;

    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; init; }

    [JsonPropertyName("bio")]
    public string? Bio { get; init; }

    [JsonPropertyName("joinDate")]
    public DateTimeOffset JoinDate { get; init; }

    [JsonPropertyName("recipeCount")]
    public int RecipeCount { get; init; }
}

/// <summary>Request body for updating the current user's profile.</summary>
public sealed class UpdateProfileRequest
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; init; } = string.Empty;

    [JsonPropertyName("bio")]
    public string? Bio { get; init; }

    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; init; }
}
