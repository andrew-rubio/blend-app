using System.Text.Json.Serialization;

namespace Blend.Api.Recipes.Models;

public sealed class UpdateRecipeRequest
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("ingredients")]
    public IReadOnlyList<RecipeIngredientRequest> Ingredients { get; init; } = [];

    [JsonPropertyName("directions")]
    public IReadOnlyList<RecipeDirectionRequest> Directions { get; init; } = [];

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

    [JsonPropertyName("isPublic")]
    public bool IsPublic { get; init; }
}
