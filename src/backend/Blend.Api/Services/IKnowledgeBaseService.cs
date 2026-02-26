namespace Blend.Api.Services;

/// <summary>
/// Manages the ingredient Knowledge Base (Azure AI Search index).
/// </summary>
public interface IKnowledgeBaseService
{
    /// <summary>Adds an approved ingredient to the KB index.</summary>
    Task AddIngredientAsync(
        string ingredientName,
        string? description,
        string? category,
        CancellationToken cancellationToken = default);
}
