using System.Text.Json.Serialization;

namespace Blend.Domain.Entities;

/// <summary>Type of notification sent to a user.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationType
{
    FriendRequestReceived,
    FriendRequestAccepted,
    RecipeLiked,
    RecipePublished,
    NewFollower,
    System,
    IngredientApproved,
    IngredientRejected,
}

/// <summary>A notification delivered to a user's inbox.</summary>
public sealed class Notification
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("recipientUserId")]
    public string RecipientUserId { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public NotificationType Type { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("sourceUserId")]
    public string? SourceUserId { get; init; }

    [JsonPropertyName("referenceId")]
    public string? ReferenceId { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("actionUrl")]
    public string? ActionUrl { get; init; }

    [JsonPropertyName("read")]
    public bool Read { get; init; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Time-to-live in seconds. Cosmos DB will auto-expire the document when &gt; 0.</summary>
    [JsonPropertyName("ttl")]
    public int Ttl { get; init; } = -1;
}
