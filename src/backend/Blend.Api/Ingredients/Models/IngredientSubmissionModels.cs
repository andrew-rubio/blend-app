using Blend.Domain.Entities;

namespace Blend.Api.Ingredients.Models;

/// <summary>Request body for submitting a new ingredient for review.</summary>
public sealed class IngredientSubmissionRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Category { get; init; }
    public string? Description { get; init; }
}

/// <summary>Response returned for a single ingredient submission.</summary>
public sealed class IngredientSubmissionResponse
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Category { get; init; }
    public string? Description { get; init; }
    public SubmissionStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    public static IngredientSubmissionResponse FromEntity(Content c) => new()
    {
        Id = c.Id,
        Name = c.Title,
        Category = c.Category,
        Description = c.Body,
        Status = c.SubmissionStatus ?? SubmissionStatus.Pending,
        CreatedAt = c.CreatedAt,
    };
}
