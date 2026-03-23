using System.Text.Json.Serialization;

namespace Blend.Domain.Entities;

/// <summary>Status of a cooking session.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CookingSessionStatus
{
    Active,
    Paused,
    Completed,
}

/// <summary>An ingredient added to a cooking session or dish during Cook Mode.</summary>
public sealed class SessionIngredient
{
    [JsonPropertyName("ingredientId")]
    public string IngredientId { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("addedAt")]
    public DateTimeOffset AddedAt { get; init; }

    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}

/// <summary>A dish being cooked as part of a cooking session.</summary>
public sealed class CookingSessionDish
{
    [JsonPropertyName("dishId")]
    public string DishId { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("cuisineType")]
    public string? CuisineType { get; init; }

    [JsonPropertyName("ingredients")]
    public IReadOnlyList<SessionIngredient> Ingredients { get; init; } = [];

    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}

/// <summary>A cooking session tracking one or more dishes being cooked simultaneously.</summary>
public sealed class CookingSession
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("userId")]
    public string UserId { get; init; } = string.Empty;

    [JsonPropertyName("dishes")]
    public IReadOnlyList<CookingSessionDish> Dishes { get; init; } = [];

    /// <summary>Session-level ingredients not scoped to any specific dish.</summary>
    [JsonPropertyName("addedIngredients")]
    public IReadOnlyList<SessionIngredient> AddedIngredients { get; init; } = [];

    [JsonPropertyName("status")]
    public CookingSessionStatus Status { get; init; } = CookingSessionStatus.Active;

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonPropertyName("pausedAt")]
    public DateTimeOffset? PausedAt { get; init; }

    /// <summary>
    /// Cosmos DB TTL in seconds. Set to expire paused sessions after 24 hours.
    /// Null disables TTL for active and completed sessions.
    /// </summary>
    [JsonPropertyName("ttl")]
    public int? Ttl { get; init; }
}
