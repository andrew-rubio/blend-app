namespace Blend.Api.Ingredients.Services;

/// <summary>
/// Configuration options for the Azure AI Search ingredient index.
/// Bound from the <c>IngredientSearch</c> configuration section.
/// </summary>
public sealed class IngredientSearchOptions
{
    public const string SectionName = "IngredientSearch";

    /// <summary>Azure AI Search service endpoint URI (e.g., https://my-service.search.windows.net).</summary>
    public string? Endpoint { get; init; }

    /// <summary>Azure AI Search admin API key.</summary>
    public string? ApiKey { get; init; }

    /// <summary>Name of the ingredients search index.</summary>
    public string IndexName { get; init; } = "ingredients";

    /// <summary>Name of the suggester configured on the index for autocomplete.</summary>
    public string SuggesterName { get; init; } = "ingredient-suggester";

    /// <summary>Returns true when enough configuration is present to connect to Azure AI Search.</summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Endpoint) && !string.IsNullOrWhiteSpace(ApiKey);
}
