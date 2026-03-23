using System.Text.Json.Serialization;
using Blend.Domain.Entities;

namespace Blend.Api.Friends.Models;

/// <summary>Response item representing a friend with their public profile data.</summary>
public sealed class FriendResponse
{
    [JsonPropertyName("userId")]
    public string UserId { get; init; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; init; } = string.Empty;

    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; init; }

    [JsonPropertyName("recipeCount")]
    public int RecipeCount { get; init; }

    [JsonPropertyName("connectedAt")]
    public DateTimeOffset ConnectedAt { get; init; }
}

/// <summary>Response item representing a pending friend request.</summary>
public sealed class FriendRequestResponse
{
    [JsonPropertyName("requestId")]
    public string RequestId { get; init; } = string.Empty;

    [JsonPropertyName("userId")]
    public string UserId { get; init; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; init; } = string.Empty;

    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; init; }

    [JsonPropertyName("sentAt")]
    public DateTimeOffset SentAt { get; init; }
}

/// <summary>Request body for sending a friend request.</summary>
public sealed class SendFriendRequestBody
{
    [JsonPropertyName("targetUserId")]
    public string TargetUserId { get; init; } = string.Empty;
}

/// <summary>Paginated list of friends.</summary>
public sealed class FriendsPageResponse
{
    [JsonPropertyName("items")]
    public IReadOnlyList<FriendResponse> Items { get; init; } = [];

    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; init; }

    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage => NextCursor is not null;
}

/// <summary>Paginated list of friend requests.</summary>
public sealed class FriendRequestsPageResponse
{
    [JsonPropertyName("items")]
    public IReadOnlyList<FriendRequestResponse> Items { get; init; } = [];

    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; init; }

    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage => NextCursor is not null;
}

/// <summary>Result of a user search with connection status indicator.</summary>
public sealed class UserSearchResult
{
    [JsonPropertyName("userId")]
    public string UserId { get; init; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; init; } = string.Empty;

    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; init; }

    [JsonPropertyName("recipeCount")]
    public int RecipeCount { get; init; }

    [JsonPropertyName("connectionStatus")]
    public string ConnectionStatus { get; init; } = "none";
}

/// <summary>Paginated list of user search results.</summary>
public sealed class UserSearchPageResponse
{
    [JsonPropertyName("items")]
    public IReadOnlyList<UserSearchResult> Items { get; init; } = [];

    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; init; }

    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage => NextCursor is not null;
}

/// <summary>Discriminated result type for friends service operations.</summary>
public enum FriendsOpResult
{
    Success,
    NotFound,
    Forbidden,
    AlreadyExists,
    CooldownActive,
    InvalidRequest,
    ServiceUnavailable,
}
