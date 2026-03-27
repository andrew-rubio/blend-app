using System.Text.Json.Serialization;

namespace Blend.Domain.Entities;

/// <summary>A structured ingredient reference within a recipe.</summary>
public sealed class RecipeIngredient
{
    [JsonPropertyName("quantity")]
    public double Quantity { get; init; }

    [JsonPropertyName("unit")]
    public string Unit { get; init; } = string.Empty;

    [JsonPropertyName("ingredientName")]
    public string IngredientName { get; init; } = string.Empty;

    [JsonPropertyName("ingredientId")]
    public string? IngredientId { get; init; }
}

/// <summary>A single step in a recipe's directions.</summary>
public sealed class RecipeDirection
{
    [JsonPropertyName("stepNumber")]
    public int StepNumber { get; init; }

    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;

    [JsonPropertyName("mediaUrl")]
    public string? MediaUrl { get; init; }
}

/// <summary>Aggregate nutritional information for a recipe.</summary>
public sealed class RecipeNutritionInfo
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

/// <summary>A user-generated recipe.</summary>
public sealed class Recipe
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("authorId")]
    public string AuthorId { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("ingredients")]
    public IReadOnlyList<RecipeIngredient> Ingredients { get; init; } = [];

    [JsonPropertyName("directions")]
    public IReadOnlyList<RecipeDirection> Directions { get; init; } = [];

    [JsonPropertyName("prepTime")]
    public int PrepTime { get; init; }

    [JsonPropertyName("cookTime")]
    public int CookTime { get; init; }

    [JsonPropertyName("servings")]
    public int Servings { get; init; }

    [JsonPropertyName("cuisineType")]
    public string? CuisineType { get; init; }

    [JsonPropertyName("dishType")]
    public string? DishType { get; init; }

    [JsonPropertyName("tags")]
    public IReadOnlyList<string> Tags { get; init; } = [];

    [JsonPropertyName("featuredPhotoUrl")]
    public string? FeaturedPhotoUrl { get; init; }

    [JsonPropertyName("photos")]
    public IReadOnlyList<string> Photos { get; init; } = [];

    [JsonPropertyName("nutritionInfo")]
    public RecipeNutritionInfo? NutritionInfo { get; init; }

    [JsonPropertyName("isPublic")]
    public bool IsPublic { get; init; }

    [JsonPropertyName("likeCount")]
    public int LikeCount { get; init; }

    [JsonPropertyName("viewCount")]
    public int ViewCount { get; init; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; init; }

    [JsonPropertyName("deletedAt")]
    public DateTimeOffset? DeletedAt { get; init; }
}
