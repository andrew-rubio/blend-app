using Blend.Api.CookSessions.Models;
using Blend.Api.Ingredients.Services;
using Blend.Api.Preferences.Services;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Blend.Api.CookSessions.Services;

/// <summary>
/// Manages Cook Mode sessions in the Cosmos DB 'activity' container
/// (per ADR 0003 § container definitions).
/// </summary>
public sealed class CookSessionService : ICookSessionService
{
    /// <summary>
    /// TTL in seconds for paused sessions (24 hours). Configurable via DI if needed.
    /// </summary>
    private const int PausedSessionTtlSeconds = 86400;

    private readonly IRepository<CookingSession>? _sessionRepository;
    private readonly IRepository<Recipe>? _recipeRepository;
    private readonly IKnowledgeBaseService? _kb;
    private readonly IPreferenceService? _preferenceService;
    private readonly ILogger<CookSessionService> _logger;

    public CookSessionService(
        ILogger<CookSessionService> logger,
        IRepository<CookingSession>? sessionRepository = null,
        IRepository<Recipe>? recipeRepository = null,
        IKnowledgeBaseService? kb = null,
        IPreferenceService? preferenceService = null)
    {
        _logger = logger;
        _sessionRepository = sessionRepository;
        _recipeRepository = recipeRepository;
        _kb = kb;
        _preferenceService = preferenceService;
    }

    // ── Session CRUD ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<CookingSession?> CreateSessionAsync(
        string userId,
        CreateCookSessionRequest request,
        CancellationToken ct = default)
    {
        if (_sessionRepository is null)
        {
            _logger.LogWarning("Session repository unavailable; cannot create session for user {UserId}.", userId);
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var dishes = new List<CookingSessionDish>();

        // Pre-populate from a recipe if requested
        if (!string.IsNullOrWhiteSpace(request.RecipeId) && _recipeRepository is not null)
        {
            var recipe = await _recipeRepository.GetByIdAsync(request.RecipeId, request.RecipeId, ct);
            if (recipe is not null)
            {
                var dishIngredients = recipe.Ingredients
                    .Select(ri => new SessionIngredient
                    {
                        IngredientId = ri.IngredientId ?? ri.IngredientName,
                        Name = ri.IngredientName,
                        AddedAt = now,
                    })
                    .ToList();

                dishes.Add(new CookingSessionDish
                {
                    DishId = Guid.NewGuid().ToString(),
                    Name = recipe.Title,
                    CuisineType = recipe.CuisineType,
                    Ingredients = dishIngredients,
                });
            }
        }

        if (dishes.Count == 0)
        {
            dishes.Add(new CookingSessionDish
            {
                DishId = Guid.NewGuid().ToString(),
                Name = string.IsNullOrWhiteSpace(request.InitialDishName)
                    ? "My Dish"
                    : request.InitialDishName,
            });
        }

        var session = new CookingSession
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Status = CookingSessionStatus.Active,
            Dishes = dishes,
            AddedIngredients = [],
            CreatedAt = now,
            UpdatedAt = now,
        };

        return await _sessionRepository.CreateAsync(session, ct);
    }

    /// <inheritdoc/>
    public async Task<CookingSession?> GetSessionAsync(
        string sessionId,
        string userId,
        CancellationToken ct = default)
    {
        if (_sessionRepository is null)
        {
            return null;
        }

        var session = await _sessionRepository.GetByIdAsync(sessionId, userId, ct);
        if (session is null || session.UserId != userId)
        {
            return null;
        }

        return session;
    }

    /// <inheritdoc/>
    public async Task<CookingSession?> GetActiveSessionAsync(
        string userId,
        CancellationToken ct = default)
    {
        if (_sessionRepository is null)
        {
            return null;
        }

        // userId is extracted from a validated JWT claim — safe to embed in query.
        var paramQuery = $"SELECT * FROM c WHERE c.userId = '{EscapeSingleQuotes(userId)}' " +
                         "AND (c.status = 'Active' OR c.status = 'Paused') " +
                         "ORDER BY c.updatedAt DESC OFFSET 0 LIMIT 1";

        var results = await _sessionRepository.GetByQueryAsync(paramQuery, userId, ct);
        return results.Count > 0 ? results[0] : null;
    }

    /// <inheritdoc/>
    public async Task<bool> HasActiveSessionAsync(string userId, CancellationToken ct = default)
    {
        var session = await GetActiveSessionAsync(userId, ct);
        return session is { Status: CookingSessionStatus.Active };
    }

    // ── Ingredient management ─────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<CookingSession?> AddIngredientAsync(
        string sessionId,
        string userId,
        AddIngredientRequest request,
        CancellationToken ct = default)
    {
        if (_sessionRepository is null)
        {
            return null;
        }

        var session = await GetSessionAsync(sessionId, userId, ct);
        if (session is null)
        {
            return null;
        }

        var ingredient = new SessionIngredient
        {
            IngredientId = request.IngredientId,
            Name = request.Name,
            AddedAt = DateTimeOffset.UtcNow,
            Notes = request.Notes,
        };

        CookingSession updated;

        if (!string.IsNullOrWhiteSpace(request.DishId))
        {
            // Add to specific dish
            var dishes = session.Dishes
                .Select(d =>
                {
                    if (d.DishId == request.DishId)
                    {
                        return new CookingSessionDish
                        {
                            DishId = d.DishId,
                            Name = d.Name,
                            CuisineType = d.CuisineType,
                            Notes = d.Notes,
                            Ingredients = [.. d.Ingredients, ingredient],
                        };
                    }

                    return d;
                })
                .ToList();

            updated = CloneSession(session, dishes: dishes, updatedAt: DateTimeOffset.UtcNow);
        }
        else
        {
            // Add to session-level ingredients
            updated = CloneSession(session,
                addedIngredients: [.. session.AddedIngredients, ingredient],
                updatedAt: DateTimeOffset.UtcNow);
        }

        return await _sessionRepository.UpdateAsync(updated, sessionId, userId, ct);
    }

    /// <inheritdoc/>
    public async Task<CookingSession?> RemoveIngredientAsync(
        string sessionId,
        string userId,
        string ingredientId,
        string? dishId,
        CancellationToken ct = default)
    {
        if (_sessionRepository is null)
        {
            return null;
        }

        var session = await GetSessionAsync(sessionId, userId, ct);
        if (session is null)
        {
            return null;
        }

        CookingSession updated;

        if (!string.IsNullOrWhiteSpace(dishId))
        {
            // Remove from specific dish
            var dishes = session.Dishes
                .Select(d =>
                {
                    if (d.DishId == dishId)
                    {
                        return new CookingSessionDish
                        {
                            DishId = d.DishId,
                            Name = d.Name,
                            CuisineType = d.CuisineType,
                            Notes = d.Notes,
                            Ingredients = d.Ingredients
                                .Where(i => i.IngredientId != ingredientId)
                                .ToList(),
                        };
                    }

                    return d;
                })
                .ToList();

            updated = CloneSession(session, dishes: dishes, updatedAt: DateTimeOffset.UtcNow);
        }
        else
        {
            // Remove from session-level ingredients
            updated = CloneSession(session,
                addedIngredients: session.AddedIngredients
                    .Where(i => i.IngredientId != ingredientId)
                    .ToList(),
                updatedAt: DateTimeOffset.UtcNow);
        }

        return await _sessionRepository.UpdateAsync(updated, sessionId, userId, ct);
    }

    // ── Dish management ───────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<CookingSession?> AddDishAsync(
        string sessionId,
        string userId,
        AddDishRequest request,
        CancellationToken ct = default)
    {
        if (_sessionRepository is null)
        {
            return null;
        }

        var session = await GetSessionAsync(sessionId, userId, ct);
        if (session is null)
        {
            return null;
        }

        var dish = new CookingSessionDish
        {
            DishId = Guid.NewGuid().ToString(),
            Name = request.Name,
            CuisineType = request.CuisineType,
            Notes = request.Notes,
        };

        var updated = CloneSession(session,
            dishes: [.. session.Dishes, dish],
            updatedAt: DateTimeOffset.UtcNow);

        return await _sessionRepository.UpdateAsync(updated, sessionId, userId, ct);
    }

    /// <inheritdoc/>
    public async Task<CookingSession?> RemoveDishAsync(
        string sessionId,
        string userId,
        string dishId,
        CancellationToken ct = default)
    {
        if (_sessionRepository is null)
        {
            return null;
        }

        var session = await GetSessionAsync(sessionId, userId, ct);
        if (session is null)
        {
            return null;
        }

        var updated = CloneSession(session,
            dishes: session.Dishes.Where(d => d.DishId != dishId).ToList(),
            updatedAt: DateTimeOffset.UtcNow);

        return await _sessionRepository.UpdateAsync(updated, sessionId, userId, ct);
    }

    // ── Lifecycle transitions ─────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<CookingSession?> CompleteSessionAsync(
        string sessionId,
        string userId,
        CancellationToken ct = default)
    {
        if (_sessionRepository is null)
        {
            return null;
        }

        var session = await GetSessionAsync(sessionId, userId, ct);
        if (session is null)
        {
            return null;
        }

        var updated = CloneSession(session,
            status: CookingSessionStatus.Completed,
            updatedAt: DateTimeOffset.UtcNow,
            ttl: null,
            clearTtl: true);

        return await _sessionRepository.UpdateAsync(updated, sessionId, userId, ct);
    }

    /// <inheritdoc/>
    public async Task<CookingSession?> PauseSessionAsync(
        string sessionId,
        string userId,
        CancellationToken ct = default)
    {
        if (_sessionRepository is null)
        {
            return null;
        }

        var session = await GetSessionAsync(sessionId, userId, ct);
        if (session is null)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var updated = CloneSession(session,
            status: CookingSessionStatus.Paused,
            pausedAt: now,
            updatedAt: now,
            ttl: PausedSessionTtlSeconds);

        return await _sessionRepository.UpdateAsync(updated, sessionId, userId, ct);
    }

    // ── Smart Suggestions ─────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<SessionSuggestionsResult> GetSuggestionsAsync(
        string sessionId,
        string userId,
        string? dishId,
        int limit,
        CancellationToken ct = default)
    {
        if (_kb is null)
        {
            return new SessionSuggestionsResult { KbUnavailable = true };
        }

        var kbAvailable = await _kb.IsAvailableAsync(ct);
        if (!kbAvailable)
        {
            return new SessionSuggestionsResult { KbUnavailable = true };
        }

        if (_sessionRepository is null)
        {
            return new SessionSuggestionsResult { KbUnavailable = false };
        }

        var session = await GetSessionAsync(sessionId, userId, ct);
        if (session is null)
        {
            return new SessionSuggestionsResult();
        }

        // Collect all current ingredient IDs
        var currentIngredients = GetSessionIngredients(session, dishId);
        if (currentIngredients.Count == 0)
        {
            return new SessionSuggestionsResult();
        }

        // Build exclusion set: current session ingredients + user preferences
        var excludedIds = new HashSet<string>(
            currentIngredients.Select(i => i.IngredientId),
            StringComparer.OrdinalIgnoreCase);

        if (_preferenceService is not null)
        {
            var dislikedIds = await _preferenceService.GetExcludedIngredientIdsAsync(userId, ct);
            foreach (var id in dislikedIds)
            {
                excludedIds.Add(id);
            }
        }

        // Aggregate pairing scores across all session ingredients
        var aggregated = await AggregatePairingScoresAsync(
            currentIngredients.Select(i => i.IngredientId).ToList(),
            excludedIds,
            ct);

        var topN = aggregated
            .OrderByDescending(kvp => kvp.Value.Score)
            .Take(limit)
            .Select(kvp => new SmartSuggestion(
                kvp.Key,
                kvp.Value.Name,
                kvp.Value.Score,
                kvp.Value.Category,
                BuildPairingReason(kvp.Value.PairedWithNames)))
            .ToList();

        return new SessionSuggestionsResult { Suggestions = topN };
    }

    /// <inheritdoc/>
    public async Task<IngredientDetailResult?> GetIngredientDetailAsync(
        string sessionId,
        string userId,
        string ingredientId,
        CancellationToken ct = default)
    {
        if (_kb is null || _sessionRepository is null)
        {
            return null;
        }

        var session = await GetSessionAsync(sessionId, userId, ct);
        if (session is null)
        {
            return null;
        }

        var doc = await _kb.GetIngredientAsync(ingredientId, ct);
        if (doc is null)
        {
            return null;
        }

        // Collect co-occurring ingredient names to build a "why it pairs" explanation
        var currentIngredients = GetSessionIngredients(session, dishId: null);
        var coIngredientNames = currentIngredients
            .Where(i => i.IngredientId != ingredientId)
            .Select(i => i.Name)
            .Take(5)
            .ToList();

        var substitutes = await _kb.GetSubstitutesAsync(ingredientId, ct);

        string? whyItPairs = coIngredientNames.Count > 0
            ? $"Pairs well with {string.Join(", ", coIngredientNames)} based on complementary flavour profiles."
            : null;

        return new IngredientDetailResult
        {
            IngredientId = doc.IngredientId,
            Name = doc.Name,
            Category = doc.Category,
            FlavourProfile = doc.FlavourProfile,
            Substitutes = substitutes.Select(s => s.SubstituteIngredientName).ToList(),
            WhyItPairs = whyItPairs,
            NutritionSummary = doc.NutritionSummary,
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private sealed record AggregatedEntry(
        string Name,
        double Score,
        string? Category,
        IReadOnlyList<string> PairedWithNames);

    private async Task<Dictionary<string, AggregatedEntry>> AggregatePairingScoresAsync(
        IReadOnlyList<string> ingredientIds,
        HashSet<string> excludedIds,
        CancellationToken ct)
    {
        var scores = new Dictionary<string, (double Score, string Name, string? Category, List<string> PairedWithNames)>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var ingredientId in ingredientIds)
        {
            IReadOnlyList<Blend.Api.Ingredients.Models.PairingSuggestion> pairings;
            try
            {
                pairings = await _kb!.GetPairingsAsync(ingredientId, limit: 50, ct: ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Failed to get pairings for ingredient '{IngredientId}'.", ingredientId);
                continue;
            }

            // Look up the source ingredient name for the "paired with" reason text
            var sourceIngName = ingredientId;
            var sourceDoc = await _kb!.GetIngredientAsync(ingredientId, ct);
            if (sourceDoc is not null)
            {
                sourceIngName = sourceDoc.Name;
            }

            foreach (var pairing in pairings)
            {
                if (excludedIds.Contains(pairing.PairedIngredientId))
                {
                    continue;
                }

                if (scores.TryGetValue(pairing.PairedIngredientId, out var existing))
                {
                    var updatedNames = new List<string>(existing.PairedWithNames) { sourceIngName };
                    scores[pairing.PairedIngredientId] = (
                        existing.Score + pairing.Score,
                        existing.Name,
                        existing.Category,
                        updatedNames);
                }
                else
                {
                    scores[pairing.PairedIngredientId] = (
                        pairing.Score,
                        pairing.PairedIngredientName,
                        pairing.Category,
                        [sourceIngName]);
                }
            }
        }

        return scores.ToDictionary(
            kvp => kvp.Key,
            kvp => new AggregatedEntry(
                kvp.Value.Name,
                kvp.Value.Score,
                kvp.Value.Category,
                kvp.Value.PairedWithNames),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns all ingredients from the session, optionally scoped to a specific dish.
    /// When <paramref name="dishId"/> is null, returns all ingredients across all dishes
    /// plus session-level ingredients.
    /// </summary>
    private static IReadOnlyList<SessionIngredient> GetSessionIngredients(
        CookingSession session,
        string? dishId)
    {
        if (!string.IsNullOrWhiteSpace(dishId))
        {
            var dish = session.Dishes.FirstOrDefault(d => d.DishId == dishId);
            return dish?.Ingredients ?? [];
        }

        var all = new List<SessionIngredient>(session.AddedIngredients);
        foreach (var dish in session.Dishes)
        {
            all.AddRange(dish.Ingredients);
        }

        return all;
    }

    private static string BuildPairingReason(IReadOnlyList<string> pairedWithNames)
    {
        if (pairedWithNames.Count == 0)
        {
            return "Pairs well with your current ingredients.";
        }

        if (pairedWithNames.Count == 1)
        {
            return $"Pairs well with {pairedWithNames[0]}.";
        }

        var listed = string.Join(", ", pairedWithNames.Take(3));
        return $"Pairs well with {listed}.";
    }

    /// <summary>Escapes single quotes in a string for safe embedding in Cosmos SQL queries.</summary>
    private static string EscapeSingleQuotes(string value) =>
        value.Replace("'", "\\'", StringComparison.Ordinal);

    /// <summary>
    /// Creates a new <see cref="CookingSession"/> from an existing one, replacing only
    /// the provided fields (all others are copied from <paramref name="source"/>).
    /// </summary>
    private static CookingSession CloneSession(
        CookingSession source,
        IReadOnlyList<CookingSessionDish>? dishes = null,
        IReadOnlyList<SessionIngredient>? addedIngredients = null,
        CookingSessionStatus? status = null,
        DateTimeOffset? updatedAt = null,
        DateTimeOffset? pausedAt = null,
        int? ttl = null,
        bool clearTtl = false)
    {
        return new CookingSession
        {
            Id = source.Id,
            UserId = source.UserId,
            Dishes = dishes ?? source.Dishes,
            AddedIngredients = addedIngredients ?? source.AddedIngredients,
            Status = status ?? source.Status,
            CreatedAt = source.CreatedAt,
            UpdatedAt = updatedAt ?? source.UpdatedAt,
            PausedAt = pausedAt ?? source.PausedAt,
            Ttl = clearTtl ? null : (ttl ?? source.Ttl),
        };
    }
}
