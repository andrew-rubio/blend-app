using System.Text.Json.Serialization;

namespace Blend.Domain.Entities;

/// <summary>
/// A pairing score between two ingredients (per ADR 0005).
/// Mutable – scores are updated as user data accumulates.
/// </summary>
public sealed class IngredientPairing
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>The primary ingredient (partition key).</summary>
    [JsonPropertyName("ingredientId")]
    public string IngredientId { get; init; } = string.Empty;

    /// <summary>The secondary ingredient being paired with the primary.</summary>
    [JsonPropertyName("pairedIngredientId")]
    public string PairedIngredientId { get; init; } = string.Empty;

    /// <summary>A normalised score in [0, 1] indicating how well the two ingredients pair together.</summary>
    [JsonPropertyName("score")]
    public double Score { get; init; }

    [JsonPropertyName("coOccurrenceCount")]
    public int CoOccurrenceCount { get; init; }

    /// <summary>
    /// Origin of the pairing score.
    /// <c>reference</c> — from curated static data; <c>community</c> — aggregated from user feedback.
    /// </summary>
    [JsonPropertyName("sourceType")]
    public string SourceType { get; init; } = PairingSourceType.Reference;

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>Well-known values for <see cref="IngredientPairing.SourceType"/>.</summary>
public static class PairingSourceType
{
    /// <summary>Pairing score originates from curated reference data.</summary>
    public const string Reference = "reference";

    /// <summary>Pairing score is derived from aggregated community (user) feedback.</summary>
    public const string Community = "community";
}
