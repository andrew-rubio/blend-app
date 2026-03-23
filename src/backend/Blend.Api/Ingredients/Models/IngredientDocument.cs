using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace Blend.Api.Ingredients.Models;

/// <summary>
/// Represents an ingredient document in the Azure AI Search "ingredients" index (per ADR 0005).
/// </summary>
public sealed class IngredientDocument
{
    /// <summary>Unique ingredient identifier (index key).</summary>
    [SimpleField(IsKey = true, IsFilterable = true)]
    public string IngredientId { get; set; } = string.Empty;

    /// <summary>Display name of the ingredient (e.g., "tomato").</summary>
    [SearchableField(IsSortable = true, IsFilterable = true, AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Alternative names (e.g., "aubergine" ↔ "eggplant").</summary>
    [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.EnLucene)]
    public string[] Aliases { get; set; } = [];

    /// <summary>Broad category (e.g., "vegetable", "protein", "spice").</summary>
    [SimpleField(IsFilterable = true, IsFacetable = true)]
    public string? Category { get; set; }

    /// <summary>Flavour profile descriptor (e.g., "sweet", "savoury", "umami").</summary>
    [SimpleField(IsFilterable = true)]
    public string? FlavourProfile { get; set; }

    /// <summary>Common substitution ingredient IDs.</summary>
    [SimpleField]
    public string[] Substitutes { get; set; } = [];

    /// <summary>Basic nutritional summary string.</summary>
    [SimpleField]
    public string? NutritionSummary { get; set; }
}
