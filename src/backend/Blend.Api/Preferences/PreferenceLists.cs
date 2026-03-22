namespace Blend.Api.Preferences;

/// <summary>
/// Predefined lists of supported preference values aligned with Spoonacular's
/// supported categories (PREF-09). These values are used for validation and
/// as reference data exposed via the public list endpoints.
/// </summary>
public static class PreferenceLists
{
    /// <summary>
    /// Supported cuisine types aligned with Spoonacular's cuisine filter values.
    /// </summary>
    public static readonly IReadOnlyList<string> Cuisines =
    [
        "African",
        "Asian",
        "American",
        "British",
        "Cajun",
        "Caribbean",
        "Chinese",
        "Eastern European",
        "European",
        "French",
        "German",
        "Greek",
        "Indian",
        "Irish",
        "Italian",
        "Japanese",
        "Jewish",
        "Korean",
        "Latin American",
        "Mediterranean",
        "Mexican",
        "Middle Eastern",
        "Nordic",
        "Southern",
        "Spanish",
        "Thai",
        "Vietnamese",
    ];

    /// <summary>
    /// Supported dish types aligned with Spoonacular's type filter values.
    /// </summary>
    public static readonly IReadOnlyList<string> DishTypes =
    [
        "main course",
        "side dish",
        "dessert",
        "appetizer",
        "salad",
        "bread",
        "breakfast",
        "soup",
        "beverage",
        "sauce",
        "marinade",
        "fingerfood",
        "snack",
        "drink",
    ];

    /// <summary>
    /// Supported dietary plans aligned with Spoonacular's diet filter values.
    /// </summary>
    public static readonly IReadOnlyList<string> Diets =
    [
        "gluten free",
        "ketogenic",
        "vegetarian",
        "lacto-vegetarian",
        "ovo-vegetarian",
        "vegan",
        "pescetarian",
        "paleo",
        "primal",
        "low FODMAP",
        "whole30",
    ];

    /// <summary>
    /// Supported intolerances aligned with Spoonacular's intolerances filter values.
    /// These are applied as strict exclusion filters (PREF-07).
    /// </summary>
    public static readonly IReadOnlyList<string> Intolerances =
    [
        "dairy",
        "egg",
        "gluten",
        "grain",
        "peanut",
        "seafood",
        "sesame",
        "shellfish",
        "soy",
        "sulfite",
        "tree nut",
        "wheat",
    ];

    // Case-insensitive lookup sets for efficient validation
    private static readonly HashSet<string> CuisineSet =
        new(Cuisines, StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> DishTypeSet =
        new(DishTypes, StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> DietSet =
        new(Diets, StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> IntoleranceSet =
        new(Intolerances, StringComparer.OrdinalIgnoreCase);

    /// <summary>Returns invalid cuisine values from the given list.</summary>
    public static IReadOnlyList<string> GetInvalidCuisines(IEnumerable<string> values) =>
        values.Where(v => !CuisineSet.Contains(v)).ToList();

    /// <summary>Returns invalid dish type values from the given list.</summary>
    public static IReadOnlyList<string> GetInvalidDishTypes(IEnumerable<string> values) =>
        values.Where(v => !DishTypeSet.Contains(v)).ToList();

    /// <summary>Returns invalid diet values from the given list.</summary>
    public static IReadOnlyList<string> GetInvalidDiets(IEnumerable<string> values) =>
        values.Where(v => !DietSet.Contains(v)).ToList();

    /// <summary>Returns invalid intolerance values from the given list.</summary>
    public static IReadOnlyList<string> GetInvalidIntolerances(IEnumerable<string> values) =>
        values.Where(v => !IntoleranceSet.Contains(v)).ToList();
}
