using System.Text.Json.Serialization;

namespace Blend.Api.CookSessions.Models;

/// <summary>A single ingredient pairing rating submitted by the user (COOK-31 through COOK-35).</summary>
public sealed class PairingFeedbackItem
{
    [JsonPropertyName("ingredientId1")]
    public string IngredientId1 { get; init; } = string.Empty;

    [JsonPropertyName("ingredientId2")]
    public string IngredientId2 { get; init; } = string.Empty;

    /// <summary>Star rating from 1 (poor) to 5 (excellent).</summary>
    [JsonPropertyName("rating")]
    public int Rating { get; init; }

    [JsonPropertyName("comment")]
    public string? Comment { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/cook-sessions/{id}/feedback</c>.</summary>
public sealed class SubmitFeedbackRequest
{
    [JsonPropertyName("feedback")]
    public IReadOnlyList<PairingFeedbackItem> Feedback { get; init; } = [];
}
