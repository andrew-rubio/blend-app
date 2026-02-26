namespace Blend.Domain.Entities;

/// <summary>
/// Ingredient pairing score used by the recommendation engine.
/// Partition key: /ingredientId
/// </summary>
public class IngredientPairing : CosmosEntity
{
    public string IngredientId { get; set; } = string.Empty;

    public string IngredientName { get; set; } = string.Empty;

    public string PairedIngredientId { get; set; } = string.Empty;

    public string PairedIngredientName { get; set; } = string.Empty;

    /// <summary>Score between 0.0 and 1.0 indicating pairing affinity.</summary>
    public double Score { get; set; }

    public int SampleSize { get; set; }

    public string? Rationale { get; set; }

    public List<string> SupportingRecipeIds { get; set; } = [];
}
