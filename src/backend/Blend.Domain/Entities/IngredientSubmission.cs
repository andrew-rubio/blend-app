namespace Blend.Domain.Entities;

/// <summary>
/// A user-submitted ingredient awaiting admin review.
/// Stored in the content container with contentType = "ingredient-submission".
/// Partition key: /contentType
/// </summary>
public class IngredientSubmission : CosmosEntity
{
    /// <summary>Fixed partition key value.</summary>
    public string ContentType { get; set; } = "ingredient-submission";

    public string IngredientName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Category { get; set; }

    public string? Aliases { get; set; }

    public string SubmittedByUserId { get; set; } = string.Empty;

    public IngredientSubmissionStatus SubmissionStatus { get; set; } = IngredientSubmissionStatus.Pending;

    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ReviewedAt { get; set; }

    public string? ReviewedByAdminId { get; set; }

    public string? RejectionReason { get; set; }
}

public enum IngredientSubmissionStatus
{
    Pending,
    Approved,
    Rejected
}
