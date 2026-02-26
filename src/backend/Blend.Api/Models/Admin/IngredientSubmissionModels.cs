using System.ComponentModel.DataAnnotations;

namespace Blend.Api.Models.Admin;

// ─── Request models ──────────────────────────────────────────────────────────

public class RejectSubmissionRequest
{
    public string? Reason { get; set; }
}

// ─── Response models ──────────────────────────────────────────────────────────

public class IngredientSubmissionResponse
{
    public string Id { get; set; } = string.Empty;

    public string IngredientName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Category { get; set; }

    public string? Aliases { get; set; }

    public string SubmittedByUserId { get; set; } = string.Empty;

    public string SubmissionStatus { get; set; } = string.Empty;

    public DateTimeOffset SubmittedAt { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }

    public string? ReviewedByAdminId { get; set; }

    public string? RejectionReason { get; set; }
}

public class PagedIngredientSubmissionsResponse
{
    public IReadOnlyList<IngredientSubmissionResponse> Items { get; set; } = [];

    public string? ContinuationToken { get; set; }

    public bool HasMore { get; set; }
}
