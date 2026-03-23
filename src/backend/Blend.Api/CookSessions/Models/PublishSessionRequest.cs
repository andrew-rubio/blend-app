using System.Text.Json.Serialization;
using Blend.Api.Recipes.Models;

namespace Blend.Api.CookSessions.Models;

/// <summary>Request body for <c>POST /api/v1/cook-sessions/{id}/publish</c> (COOK-40 through COOK-44).</summary>
public sealed class PublishSessionRequest
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>At least one direction step is required when publishing.</summary>
    [JsonPropertyName("directions")]
    public IReadOnlyList<RecipeDirectionRequest> Directions { get; init; } = [];

    [JsonPropertyName("photos")]
    public IReadOnlyList<string> Photos { get; init; } = [];

    [JsonPropertyName("cuisineType")]
    public string? CuisineType { get; init; }

    [JsonPropertyName("tags")]
    public IReadOnlyList<string> Tags { get; init; } = [];

    [JsonPropertyName("servings")]
    public int Servings { get; init; }

    [JsonPropertyName("prepTime")]
    public int PrepTime { get; init; }

    [JsonPropertyName("cookTime")]
    public int CookTime { get; init; }
}

/// <summary>Response returned by <c>POST /api/v1/cook-sessions/{id}/publish</c>.</summary>
public sealed class PublishSessionResult
{
    [JsonPropertyName("recipeId")]
    public string RecipeId { get; init; } = string.Empty;
}
