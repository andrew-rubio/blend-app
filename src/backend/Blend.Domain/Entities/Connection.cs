using System.Text.Json.Serialization;

namespace Blend.Domain.Entities;

/// <summary>Status of a friend connection between two users.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConnectionStatus
{
    Pending,
    Accepted,
    Declined,
}

/// <summary>A friend connection or friend request between two users.</summary>
public sealed class Connection
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("userId")]
    public string UserId { get; init; } = string.Empty;

    [JsonPropertyName("friendUserId")]
    public string FriendUserId { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public ConnectionStatus Status { get; init; } = ConnectionStatus.Pending;

    [JsonPropertyName("initiatedBy")]
    public string InitiatedBy { get; init; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; init; }
}
