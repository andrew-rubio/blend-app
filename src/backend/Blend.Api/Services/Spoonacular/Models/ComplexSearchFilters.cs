namespace Blend.Api.Services.Spoonacular.Models;

/// <summary>
/// Filter parameters for the <c>complexSearch</c> Spoonacular endpoint.
/// All properties are optional; omitted filters are not sent to the API.
/// </summary>
public sealed class ComplexSearchFilters
{
    /// <summary>Cuisine type, e.g. "Italian", "Mexican".</summary>
    public string? Cuisine { get; init; }

    /// <summary>Diet type, e.g. "vegetarian", "vegan", "paleo".</summary>
    public string? Diet { get; init; }

    /// <summary>Comma-separated intolerances, e.g. "gluten,dairy".</summary>
    public string? Intolerances { get; init; }

    /// <summary>Maximum ready-in time in minutes.</summary>
    public int? MaxReadyTime { get; init; }

    /// <summary>Maximum number of results to return (default 10).</summary>
    public int Number { get; init; } = 10;
}
