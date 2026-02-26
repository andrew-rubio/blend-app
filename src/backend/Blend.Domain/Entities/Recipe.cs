namespace Blend.Domain.Entities;

/// <summary>
/// User-generated recipe document.
/// Partition key: /authorId
/// </summary>
public class Recipe : CosmosEntity
{
    public string AuthorId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public List<RecipeIngredient> Ingredients { get; set; } = [];

    public List<RecipeStep> Steps { get; set; } = [];

    public RecipeMetadata Metadata { get; set; } = new();

    public RecipeStats Stats { get; set; } = new();

    public bool IsPublished { get; set; } = false;

    public bool IsDeleted { get; set; } = false;

    public List<string> Tags { get; set; } = [];

    public string? SpoonacularId { get; set; }
}

public class RecipeIngredient
{
    public string Name { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public string Unit { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public int? SpoonacularIngredientId { get; set; }
}

public class RecipeStep
{
    public int Order { get; set; }

    public string Instruction { get; set; } = string.Empty;

    public int? TimerMinutes { get; set; }

    public List<string> ImageUrls { get; set; } = [];
}

public class RecipeMetadata
{
    public int PrepTimeMinutes { get; set; }

    public int CookTimeMinutes { get; set; }

    public int TotalTimeMinutes => PrepTimeMinutes + CookTimeMinutes;

    public int Servings { get; set; } = 2;

    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Easy;

    public string Cuisine { get; set; } = string.Empty;

    public List<string> DietaryLabels { get; set; } = [];

    public NutritionInfo? Nutrition { get; set; }
}

public class NutritionInfo
{
    public int CaloriesPerServing { get; set; }

    public decimal ProteinGrams { get; set; }

    public decimal CarbsGrams { get; set; }

    public decimal FatGrams { get; set; }

    public decimal FiberGrams { get; set; }
}

public class RecipeStats
{
    public int ViewCount { get; set; }

    public int SaveCount { get; set; }

    public int CookCount { get; set; }

    public double AverageRating { get; set; }

    public int RatingCount { get; set; }
}

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard,
    Expert
}
