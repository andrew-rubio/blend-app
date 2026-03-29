using System.Text.Json.Serialization;

namespace Blend.Api.Recipes.Models;

/// <summary>Response DTO for the full recipe detail, matching the frontend <c>Recipe</c> type.</summary>
public sealed class RecipeResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; init; }

    [JsonPropertyName("photos")]
    public IReadOnlyList<string> Photos { get; init; } = [];

    [JsonPropertyName("cuisines")]
    public IReadOnlyList<string> Cuisines { get; init; } = [];

    [JsonPropertyName("dishTypes")]
    public IReadOnlyList<string> DishTypes { get; init; } = [];

    [JsonPropertyName("diets")]
    public IReadOnlyList<string> Diets { get; init; } = [];

    [JsonPropertyName("intolerances")]
    public IReadOnlyList<string> Intolerances { get; init; } = [];

    [JsonPropertyName("readyInMinutes")]
    public int? ReadyInMinutes { get; init; }

    [JsonPropertyName("prepTimeMinutes")]
    public int? PrepTimeMinutes { get; init; }

    [JsonPropertyName("cookTimeMinutes")]
    public int? CookTimeMinutes { get; init; }

    [JsonPropertyName("servings")]
    public int Servings { get; init; }

    [JsonPropertyName("ingredients")]
    public IReadOnlyList<IngredientResponse> Ingredients { get; init; } = [];

    [JsonPropertyName("steps")]
    public IReadOnlyList<StepResponse> Steps { get; init; } = [];

    [JsonPropertyName("dataSource")]
    public string DataSource { get; init; } = "Community";

    [JsonPropertyName("author")]
    public AuthorResponse? Author { get; init; }

    [JsonPropertyName("nutritionInfo")]
    public NutritionResponse? NutritionInfo { get; init; }

    [JsonPropertyName("likeCount")]
    public int LikeCount { get; init; }

    [JsonPropertyName("isLiked")]
    public bool? IsLiked { get; init; }

    [JsonPropertyName("viewCount")]
    public int? ViewCount { get; init; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; init; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset? UpdatedAt { get; init; }
}

public sealed class IngredientResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("amount")]
    public double Amount { get; init; }

    [JsonPropertyName("unit")]
    public string Unit { get; init; } = string.Empty;

    [JsonPropertyName("original")]
    public string? Original { get; init; }
}

public sealed class StepResponse
{
    [JsonPropertyName("number")]
    public int Number { get; init; }

    [JsonPropertyName("step")]
    public string Step { get; init; } = string.Empty;
}

public sealed class AuthorResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; init; }
}

public sealed class NutritionResponse
{
    [JsonPropertyName("calories")]
    public double? Calories { get; init; }

    [JsonPropertyName("protein")]
    public double? Protein { get; init; }

    [JsonPropertyName("carbs")]
    public double? Carbs { get; init; }

    [JsonPropertyName("fat")]
    public double? Fat { get; init; }

    [JsonPropertyName("fiber")]
    public double? Fiber { get; init; }

    [JsonPropertyName("sugar")]
    public double? Sugar { get; init; }
}
