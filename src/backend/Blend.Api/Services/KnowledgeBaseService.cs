namespace Blend.Api.Services;

/// <summary>
/// Stub implementation of the Knowledge Base service.
/// Replace with Azure AI Search integration when ready.
/// </summary>
public class KnowledgeBaseService : IKnowledgeBaseService
{
    private readonly ILogger<KnowledgeBaseService> _logger;

    public KnowledgeBaseService(ILogger<KnowledgeBaseService> logger)
    {
        _logger = logger;
    }

    public Task AddIngredientAsync(
        string ingredientName,
        string? description,
        string? category,
        CancellationToken cancellationToken = default)
    {
        // TODO: Integrate with Azure AI Search to index the ingredient (task 009-task-media-upload dependency)
        _logger.LogInformation(
            "KB stub: would index ingredient '{Name}' (category: {Category})",
            ingredientName, category ?? "none");

        return Task.CompletedTask;
    }
}
