using System.Text.Json.Serialization;

namespace Blend.Domain.Entities;

/// <summary>Type of user activity recorded.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ActivityType
{
    Viewed,
    Cooked,
    Liked,
}

/// <summary>A single activity event for a user (view, cook, like).</summary>
public sealed class Activity
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("userId")]
    public string UserId { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public ActivityType Type { get; init; }

    [JsonPropertyName("referenceId")]
    public string ReferenceId { get; init; } = string.Empty;

    [JsonPropertyName("referenceType")]
    public string ReferenceType { get; init; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; init; }
}
