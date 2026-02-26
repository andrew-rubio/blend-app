namespace Blend.Domain.Entities;

/// <summary>
/// Represents an interactive cooking session tied to a recipe.
/// Partition key: /userId (stored in the activity container)
/// </summary>
public class CookingSession : CosmosEntity
{
    public string UserId { get; set; } = string.Empty;

    public string RecipeId { get; set; } = string.Empty;

    public string RecipeTitle { get; set; } = string.Empty;

    public CookingSessionStatus Status { get; set; } = CookingSessionStatus.InProgress;

    public int CurrentStepIndex { get; set; } = 0;

    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CompletedAt { get; set; }

    public int? ActualServings { get; set; }

    public string? Notes { get; set; }

    public int? Rating { get; set; }

    public List<StepCompletion> StepCompletions { get; set; } = [];

    public List<SubstitutionUsed> SubstitutionsUsed { get; set; } = [];
}

public enum CookingSessionStatus
{
    InProgress,
    Paused,
    Completed,
    Abandoned
}

public class StepCompletion
{
    public int StepOrder { get; set; }

    public DateTimeOffset CompletedAt { get; set; }

    public int? DurationSeconds { get; set; }
}

public class SubstitutionUsed
{
    public string OriginalIngredient { get; set; } = string.Empty;

    public string SubstitutedWith { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
