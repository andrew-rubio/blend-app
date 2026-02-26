using System.Text.Json.Serialization;

namespace Blend.Api.Services.Spoonacular;

// --- Raw Spoonacular API response shapes ---

internal sealed class SpoonacularRecipeSummaryDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("image")] public string? Image { get; set; }
    [JsonPropertyName("usedIngredientCount")] public int? UsedIngredientCount { get; set; }
    [JsonPropertyName("missedIngredientCount")] public int? MissedIngredientCount { get; set; }
    [JsonPropertyName("likes")] public double? Likes { get; set; }
}

internal sealed class SpoonacularComplexSearchResponse
{
    [JsonPropertyName("results")] public List<SpoonacularRecipeSummaryDto> Results { get; set; } = [];
    [JsonPropertyName("totalResults")] public int TotalResults { get; set; }
}

internal sealed class SpoonacularIngredientDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("amount")] public double Amount { get; set; }
    [JsonPropertyName("unit")] public string Unit { get; set; } = string.Empty;
    [JsonPropertyName("image")] public string? Image { get; set; }
}

internal sealed class SpoonacularStepDto
{
    [JsonPropertyName("number")] public int Number { get; set; }
    [JsonPropertyName("step")] public string Step { get; set; } = string.Empty;
}

internal sealed class SpoonacularInstructionDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("steps")] public List<SpoonacularStepDto> Steps { get; set; } = [];
}

internal sealed class SpoonacularRecipeDetailDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("image")] public string? Image { get; set; }
    [JsonPropertyName("readyInMinutes")] public int? ReadyInMinutes { get; set; }
    [JsonPropertyName("servings")] public int? Servings { get; set; }
    [JsonPropertyName("summary")] public string? Summary { get; set; }
    [JsonPropertyName("cuisines")] public List<string> Cuisines { get; set; } = [];
    [JsonPropertyName("dishTypes")] public List<string> DishTypes { get; set; } = [];
    [JsonPropertyName("extendedIngredients")] public List<SpoonacularIngredientDto> ExtendedIngredients { get; set; } = [];
    [JsonPropertyName("analyzedInstructions")] public List<SpoonacularInstructionDto> AnalyzedInstructions { get; set; } = [];
    [JsonPropertyName("vegetarian")] public bool? Vegetarian { get; set; }
    [JsonPropertyName("vegan")] public bool? Vegan { get; set; }
    [JsonPropertyName("glutenFree")] public bool? GlutenFree { get; set; }
    [JsonPropertyName("dairyFree")] public bool? DairyFree { get; set; }
}

internal sealed class SpoonacularSubstituteResponse
{
    [JsonPropertyName("ingredient")] public string Ingredient { get; set; } = string.Empty;
    [JsonPropertyName("substitutes")] public List<string> Substitutes { get; set; } = [];
    [JsonPropertyName("message")] public string? Message { get; set; }
}
