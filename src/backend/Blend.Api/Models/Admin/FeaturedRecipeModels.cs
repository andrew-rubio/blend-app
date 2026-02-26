using System.ComponentModel.DataAnnotations;

namespace Blend.Api.Models.Admin;

// ─── Request models ──────────────────────────────────────────────────────────

public class CreateFeaturedRecipeRequest
{
    [Required]
    public string RecipeId { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(spoonacular|community)$", ErrorMessage = "Source must be 'spoonacular' or 'community'.")]
    public string Source { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int DisplayOrder { get; set; } = 0;
}

public class UpdateFeaturedRecipeRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int? DisplayOrder { get; set; }
}

// ─── Response model ───────────────────────────────────────────────────────────

public class FeaturedRecipeResponse
{
    public string Id { get; set; } = string.Empty;

    public string RecipeId { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int DisplayOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
