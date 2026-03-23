using Blend.Domain.Entities;

namespace Blend.Api.Ingredients.Seeding;

/// <summary>
/// Static seed data for the Ingredient Knowledge Base.
/// Used to populate the Azure AI Search index and Cosmos DB <c>ingredientPairings</c> container.
/// </summary>
internal static class IngredientSeedData
{
    /// <summary>Initial ingredient documents to index in Azure AI Search.</summary>
    public static IReadOnlyList<IngredientSeedDocument> Ingredients { get; } =
    [
        new("ing-tomato",    "Tomato",       ["Love Apple"],              "vegetable", "sweet,savoury",  ["ing-cherry-tomato"],        "Rich in lycopene and vitamin C. ~18 kcal/100 g."),
        new("ing-garlic",    "Garlic",        ["Stinking Rose"],           "allium",    "pungent,savoury", [],                           "Contains allicin. ~149 kcal/100 g."),
        new("ing-onion",     "Onion",         ["Bulb Onion"],              "allium",    "pungent,sweet",   ["ing-shallot", "ing-leek"],   "~40 kcal/100 g."),
        new("ing-basil",     "Basil",         ["Sweet Basil"],             "herb",      "sweet,anise",     ["ing-oregano"],               "Good source of vitamin K. ~23 kcal/100 g."),
        new("ing-oregano",   "Oregano",       [],                          "herb",      "earthy,warm",     ["ing-basil", "ing-marjoram"],  "High in antioxidants. ~265 kcal/100 g."),
        new("ing-olive-oil", "Olive Oil",     ["Extra Virgin Olive Oil"],  "fat",       "fruity,peppery",  ["ing-sunflower-oil"],         "High in monounsaturated fats. ~884 kcal/100 g."),
        new("ing-lemon",     "Lemon",         ["Citrus Limon"],            "citrus",    "sour,bright",     ["ing-lime"],                  "Rich in vitamin C. ~29 kcal/100 g."),
        new("ing-chicken",   "Chicken",       ["Hen"],                     "protein",   "mild,savoury",    ["ing-turkey", "ing-tofu"],    "~165 kcal/100 g (breast)."),
        new("ing-rice",      "Rice",          ["White Rice"],              "grain",     "neutral,starchy", ["ing-quinoa", "ing-couscous"], "~130 kcal/100 g (cooked)."),
        new("ing-pasta",     "Pasta",         ["Noodles"],                 "grain",     "neutral,starchy", ["ing-rice", "ing-couscous"],  "~131 kcal/100 g (cooked)."),
        new("ing-potato",    "Potato",        ["Spud"],                    "vegetable", "starchy,earthy",  ["ing-sweet-potato"],          "~77 kcal/100 g."),
        new("ing-carrot",    "Carrot",        ["Daucus Carota"],           "vegetable", "sweet,earthy",    [],                            "Rich in beta-carotene. ~41 kcal/100 g."),
        new("ing-spinach",   "Spinach",       ["Baby Spinach"],            "vegetable", "mild,earthy",     ["ing-kale", "ing-chard"],     "High in iron and folate. ~23 kcal/100 g."),
        new("ing-egg",       "Egg",           ["Hen Egg"],                 "protein",   "rich,mild",       ["ing-tofu"],                  "Complete protein. ~155 kcal/100 g."),
        new("ing-butter",    "Butter",        ["Dairy Butter"],            "fat",       "rich,creamy",     ["ing-olive-oil", "ing-ghee"], "~717 kcal/100 g."),
        new("ing-milk",      "Milk",          ["Whole Milk"],              "dairy",     "mild,creamy",     ["ing-oat-milk", "ing-soy-milk"], "~61 kcal/100 g."),
        new("ing-cheese",    "Cheese",        ["Cheddar"],                 "dairy",     "rich,tangy",      [],                            "Good source of calcium. ~402 kcal/100 g."),
        new("ing-shallot",   "Shallot",       [],                          "allium",    "mild,sweet",      ["ing-onion"],                 "~72 kcal/100 g."),
        new("ing-leek",      "Leek",          [],                          "allium",    "mild,sweet",      ["ing-onion", "ing-shallot"],   "~61 kcal/100 g."),
        new("ing-lime",      "Lime",          ["Persian Lime"],            "citrus",    "sour,tart",       ["ing-lemon"],                 "Rich in vitamin C. ~30 kcal/100 g."),
        new("ing-tofu",      "Tofu",          ["Bean Curd"],               "protein",   "neutral,mild",    ["ing-chicken", "ing-egg"],    "~76 kcal/100 g."),
        new("ing-kale",      "Kale",          ["Curly Kale"],              "vegetable", "earthy,bitter",   ["ing-spinach"],               "High in vitamins K and C. ~49 kcal/100 g."),
        new("ing-couscous",  "Couscous",      [],                          "grain",     "neutral",         ["ing-rice", "ing-pasta"],     "~112 kcal/100 g (cooked)."),
        new("ing-quinoa",    "Quinoa",        [],                          "grain",     "nutty",           ["ing-rice"],                  "Complete protein. ~120 kcal/100 g (cooked)."),
        new("ing-sweet-potato", "Sweet Potato", ["Kumara"],                "vegetable", "sweet,starchy",   ["ing-potato"],                "Rich in beta-carotene. ~86 kcal/100 g."),
        new("ing-cherry-tomato", "Cherry Tomato", ["Grape Tomato"],       "vegetable", "sweet",           ["ing-tomato"],                "~18 kcal/100 g."),
        new("ing-marjoram",  "Marjoram",      [],                          "herb",      "earthy,warm",     ["ing-oregano"],               "~271 kcal/100 g."),
        new("ing-turkey",    "Turkey",        [],                          "protein",   "mild,savoury",    ["ing-chicken"],               "~135 kcal/100 g (breast)."),
        new("ing-ghee",      "Ghee",          ["Clarified Butter"],        "fat",       "rich,nutty",      ["ing-butter"],                "~900 kcal/100 g."),
        new("ing-sunflower-oil", "Sunflower Oil", [],                      "fat",       "neutral",         ["ing-olive-oil"],             "~884 kcal/100 g."),
    ];

    /// <summary>Reference pairing scores for the Cosmos DB <c>ingredientPairings</c> container.</summary>
    public static IReadOnlyList<IngredientPairing> Pairings { get; } =
    [
        Pair("ing-tomato",    "ing-garlic",    0.97),
        Pair("ing-tomato",    "ing-basil",     0.95),
        Pair("ing-tomato",    "ing-olive-oil", 0.90),
        Pair("ing-tomato",    "ing-onion",     0.88),
        Pair("ing-garlic",    "ing-olive-oil", 0.93),
        Pair("ing-garlic",    "ing-onion",     0.91),
        Pair("ing-garlic",    "ing-lemon",     0.82),
        Pair("ing-garlic",    "ing-chicken",   0.89),
        Pair("ing-basil",     "ing-olive-oil", 0.88),
        Pair("ing-basil",     "ing-tomato",    0.95),
        Pair("ing-chicken",   "ing-lemon",     0.87),
        Pair("ing-chicken",   "ing-garlic",    0.89),
        Pair("ing-chicken",   "ing-onion",     0.84),
        Pair("ing-rice",      "ing-egg",       0.80),
        Pair("ing-rice",      "ing-chicken",   0.86),
        Pair("ing-pasta",     "ing-tomato",    0.92),
        Pair("ing-pasta",     "ing-garlic",    0.90),
        Pair("ing-pasta",     "ing-cheese",    0.88),
        Pair("ing-spinach",   "ing-garlic",    0.85),
        Pair("ing-spinach",   "ing-cheese",    0.82),
        Pair("ing-egg",       "ing-butter",    0.88),
        Pair("ing-egg",       "ing-cheese",    0.86),
        Pair("ing-potato",    "ing-butter",    0.90),
        Pair("ing-potato",    "ing-cheese",    0.85),
        Pair("ing-carrot",    "ing-onion",     0.80),
        Pair("ing-carrot",    "ing-garlic",    0.79),
        Pair("ing-lemon",     "ing-garlic",    0.82),
        Pair("ing-lemon",     "ing-chicken",   0.87),
        Pair("ing-lemon",     "ing-olive-oil", 0.84),
        Pair("ing-olive-oil", "ing-garlic",    0.93),
        Pair("ing-olive-oil", "ing-lemon",     0.84),
    ];

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static IngredientPairing Pair(string ingredientId, string pairedId, double score) =>
        new()
        {
            Id = $"{ingredientId}:{pairedId}",
            IngredientId = ingredientId,
            PairedIngredientId = pairedId,
            Score = score,
            SourceType = PairingSourceType.Reference,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
}

/// <summary>
/// Lightweight DTO used only during seeding; distinct from <see cref="Blend.Api.Ingredients.Models.IngredientDocument"/>
/// to avoid coupling seed data to the Azure Search index model.
/// </summary>
internal sealed record IngredientSeedDocument(
    string IngredientId,
    string Name,
    string[] Aliases,
    string Category,
    string FlavourProfile,
    string[] Substitutes,
    string NutritionSummary);
