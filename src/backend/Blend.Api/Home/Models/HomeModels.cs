namespace Blend.Api.Home.Models;

/// <summary>The search bar section with a rotating placeholder prompt (HOME-01 through HOME-04).</summary>
public sealed class SearchSection
{
    public string Placeholder { get; init; } = string.Empty;
}

/// <summary>An enriched featured recipe item (HOME-05 through HOME-08).</summary>
public sealed class FeaturedRecipeResult
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string? Attribution { get; init; }
    public string? ShortDescription { get; init; }
}

/// <summary>An editorial story item (HOME-09 through HOME-12).</summary>
public sealed class FeaturedStoryResult
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? CoverImageUrl { get; init; }
    public string? Author { get; init; }
    public string? Excerpt { get; init; }
    public int? ReadingTimeMinutes { get; init; }
}

/// <summary>A featured video item (HOME-17 through HOME-20).</summary>
public sealed class FeaturedVideoResult
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public string? VideoUrl { get; init; }
    public string? Creator { get; init; }
}

/// <summary>The featured content section containing recipes, stories, and videos.</summary>
public sealed class FeaturedSection
{
    public IReadOnlyList<FeaturedRecipeResult> Recipes { get; init; } = [];
    public IReadOnlyList<FeaturedStoryResult> Stories { get; init; } = [];
    public IReadOnlyList<FeaturedVideoResult> Videos { get; init; } = [];
}

/// <summary>A community recipe summary (HOME-13 through HOME-16).</summary>
public sealed class CommunityRecipeResult
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string? AuthorId { get; init; }
    public string? CuisineType { get; init; }
    public int LikeCount { get; init; }
}

/// <summary>The community recipes section.</summary>
public sealed class CommunitySection
{
    public IReadOnlyList<CommunityRecipeResult> Recipes { get; init; } = [];
}

/// <summary>A single recently-viewed recipe entry (HOME-21 through HOME-24).</summary>
public sealed class RecentlyViewedRecipeResult
{
    public string RecipeId { get; init; } = string.Empty;
    public string ReferenceType { get; init; } = string.Empty;
    public DateTimeOffset ViewedAt { get; init; }
}

/// <summary>The recently-viewed section (authenticated users only).</summary>
public sealed class RecentlyViewedSection
{
    public IReadOnlyList<RecentlyViewedRecipeResult> Recipes { get; init; } = [];
}

/// <summary>Aggregated home page response (HOME-01 through HOME-24).</summary>
public sealed class HomeResponse
{
    public SearchSection Search { get; init; } = new();
    public FeaturedSection Featured { get; init; } = new();
    public CommunitySection Community { get; init; } = new();
    public RecentlyViewedSection RecentlyViewed { get; init; } = new();
}
