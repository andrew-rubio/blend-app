namespace Blend.Api.Domain.Models;

/// <summary>Options for the SearchByIngredients endpoint.</summary>
public record SearchByIngredientsOptions
{
    public int? Number { get; init; } = 10;
    public int? Ranking { get; init; }
    public bool? IgnorePantry { get; init; }
}

/// <summary>Options for the ComplexSearch endpoint.</summary>
public record ComplexSearchOptions
{
    public string? Query { get; init; }
    public string? Cuisine { get; init; }
    public string? Diet { get; init; }
    public string? Intolerances { get; init; }
    public int? MaxReadyTime { get; init; }
    public int? Number { get; init; } = 10;
    public int? Offset { get; init; }
}
