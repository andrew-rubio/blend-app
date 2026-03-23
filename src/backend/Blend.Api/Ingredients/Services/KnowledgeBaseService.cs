using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Blend.Api.Ingredients.Models;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace Blend.Api.Ingredients.Services;

/// <summary>
/// Knowledge Base service that wraps Azure AI Search and the Cosmos DB
/// <c>ingredientPairings</c> container with a circuit-breaker pattern (per ADR 0005, PLAT-51).
/// </summary>
/// <remarks>
/// Registered as a <b>singleton</b> so that circuit-breaker state persists across requests.
/// </remarks>
public sealed class KnowledgeBaseService : IKnowledgeBaseService
{
    // ── Circuit-breaker constants ─────────────────────────────────────────────
    private const int FailureThreshold = 3;
    private static readonly TimeSpan CooldownPeriod = TimeSpan.FromSeconds(60);

    // ── Circuit-breaker state (thread-safe via Interlocked) ───────────────────
    private int _consecutiveFailures;
    private long _unavailableUntilTicks; // 0 = never tripped

    // ── Dependencies ──────────────────────────────────────────────────────────
    private readonly SearchClient? _searchClient;
    private readonly IngredientSearchOptions _options;
    private readonly IRepository<IngredientPairing>? _pairingRepository;
    private readonly ILogger<KnowledgeBaseService> _logger;

    public KnowledgeBaseService(
        ILogger<KnowledgeBaseService> logger,
        IOptions<IngredientSearchOptions> options,
        IRepository<IngredientPairing>? pairingRepository = null)
    {
        _logger = logger;
        _options = options.Value;
        _pairingRepository = pairingRepository;

        if (_options.IsConfigured)
        {
            _searchClient = new SearchClient(
                new Uri(_options.Endpoint!),
                _options.IndexName,
                new AzureKeyCredential(_options.ApiKey!));
        }
        else
        {
            _logger.LogWarning(
                "Azure AI Search is not configured (IngredientSearch:Endpoint or IngredientSearch:ApiKey missing). " +
                "Ingredient search and detail lookups will be unavailable.");
        }
    }

    // ── IKnowledgeBaseService ─────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<IReadOnlyList<IngredientSearchResult>> SearchIngredientsAsync(
        string query,
        int limit = 10,
        CancellationToken ct = default)
    {
        if (_searchClient is null || !IsCircuitClosed())
        {
            return [];
        }

        try
        {
            var suggestOptions = new SuggestOptions
            {
                Size = limit,
                UseFuzzyMatching = true,
                Select = { "ingredientId", "name", "category", "flavourProfile" },
            };

            var response = await _searchClient.SuggestAsync<IngredientDocument>(
                query,
                _options.SuggesterName,
                suggestOptions,
                ct);

            RecordSuccess();

            return response.Value.Results
                .Select(r => new IngredientSearchResult(
                    r.Document.IngredientId,
                    r.Document.Name,
                    r.Document.Category,
                    r.Document.FlavourProfile))
                .ToList();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            RecordFailure();
            _logger.LogWarning(ex, "Azure AI Search suggest call failed for query '{Query}'.", query);
            return [];
        }
    }

    /// <inheritdoc/>
    public async Task<IngredientDocument?> GetIngredientAsync(string id, CancellationToken ct = default)
    {
        if (_searchClient is null || !IsCircuitClosed())
        {
            return null;
        }

        try
        {
            var response = await _searchClient.GetDocumentAsync<IngredientDocument>(id, cancellationToken: ct);
            RecordSuccess();
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            RecordSuccess(); // A 404 is a valid response, not a service failure.
            return null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            RecordFailure();
            _logger.LogWarning(ex, "Azure AI Search GetDocument call failed for id '{Id}'.", id);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PairingSuggestion>> GetPairingsAsync(
        string ingredientId,
        string? category = null,
        int limit = 20,
        CancellationToken ct = default)
    {
        if (_pairingRepository is null)
        {
            return [];
        }

        if (!IsValidIngredientId(ingredientId))
        {
            _logger.LogWarning("Invalid ingredientId format: '{IngredientId}'.", ingredientId);
            return [];
        }

        try
        {
            var clampedLimit = Math.Clamp(limit, 1, 100);
            // Category filtering is applied during enrichment (category data lives in Azure AI Search, not Cosmos DB).
            var query = $"SELECT TOP {clampedLimit} * FROM c WHERE c.ingredientId = '{ingredientId}' ORDER BY c.score DESC";

            var pairings = await _pairingRepository.GetByQueryAsync(query, ingredientId, ct);

            // Enrich with ingredient names when the search client is available.
            var suggestions = new List<PairingSuggestion>(pairings.Count);
            foreach (var p in pairings)
            {
                var pairedName = p.PairedIngredientId;
                if (_searchClient is not null && IsCircuitClosed())
                {
                    var doc = await GetIngredientAsync(p.PairedIngredientId, ct);
                    if (doc is not null)
                    {
                        pairedName = doc.Name;
                        if (category is not null &&
                            !string.Equals(doc.Category, category, StringComparison.OrdinalIgnoreCase))
                        {
                            continue; // filter by category
                        }
                    }
                }

                suggestions.Add(new PairingSuggestion(
                    p.PairedIngredientId,
                    pairedName,
                    p.Score,
                    null,
                    p.SourceType));
            }

            return suggestions.OrderByDescending(s => s.Score).ToList();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to retrieve pairings for ingredient '{IngredientId}'.", ingredientId);
            return [];
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SubstituteSuggestion>> GetSubstitutesAsync(
        string ingredientId,
        CancellationToken ct = default)
    {
        if (_searchClient is null || !IsCircuitClosed())
        {
            return [];
        }

        try
        {
            var doc = await GetIngredientAsync(ingredientId, ct);
            if (doc is null || doc.Substitutes.Length == 0)
            {
                return [];
            }

            var suggestions = new List<SubstituteSuggestion>(doc.Substitutes.Length);
            foreach (var substituteId in doc.Substitutes)
            {
                var substituteName = substituteId;
                var substituteDoc = await GetIngredientAsync(substituteId, ct);
                if (substituteDoc is not null)
                {
                    substituteName = substituteDoc.Name;
                }

                suggestions.Add(new SubstituteSuggestion(substituteId, substituteName, null));
            }

            return suggestions;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to retrieve substitutes for ingredient '{IngredientId}'.", ingredientId);
            return [];
        }
    }

    /// <inheritdoc/>
    public Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        if (_searchClient is null)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(IsCircuitClosed());
    }

    /// <inheritdoc/>
    public async Task UpdatePairingScoreAsync(
        string ingredientId1,
        string ingredientId2,
        double normalizedRating,
        CancellationToken ct = default)
    {
        if (_pairingRepository is null)
        {
            _logger.LogWarning("Pairing repository unavailable; cannot update pairing score for '{Id1}':'{Id2}'.",
                ingredientId1, ingredientId2);
            return;
        }

        await UpdateOnePairingAsync(ingredientId1, ingredientId2, normalizedRating, ct);
        await UpdateOnePairingAsync(ingredientId2, ingredientId1, normalizedRating, ct);
    }

    /// <summary>Updates (or creates) a single directional pairing entry in the <c>ingredientPairings</c> container.</summary>
    private async Task UpdateOnePairingAsync(
        string primaryId,
        string pairedId,
        double normalizedRating,
        CancellationToken ct)
    {
        var pairingId = $"{primaryId}:{pairedId}";
        IngredientPairing? existing = null;
        try
        {
            existing = await _pairingRepository!.GetByIdAsync(pairingId, primaryId, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to retrieve pairing '{PairingId}' before update.", pairingId);
        }

        IngredientPairing updated;
        if (existing is not null)
        {
            // Weighted average: newScore = (oldScore * count + newRating) / (count + 1)
            var newCount = existing.CoOccurrenceCount + 1;
            var newScore = Math.Clamp(
                (existing.Score * existing.CoOccurrenceCount + normalizedRating) / newCount,
                0.0,
                1.0);

            updated = new IngredientPairing
            {
                Id = existing.Id,
                IngredientId = existing.IngredientId,
                PairedIngredientId = existing.PairedIngredientId,
                Score = newScore,
                CoOccurrenceCount = newCount,
                SourceType = PairingSourceType.Community,
                UpdatedAt = DateTimeOffset.UtcNow,
            };

            try
            {
                await _pairingRepository!.UpdateAsync(updated, pairingId, primaryId, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Failed to update pairing '{PairingId}'.", pairingId);
            }
        }
        else
        {
            // Create a new community pairing entry
            updated = new IngredientPairing
            {
                Id = pairingId,
                IngredientId = primaryId,
                PairedIngredientId = pairedId,
                Score = Math.Clamp(normalizedRating, 0.0, 1.0),
                CoOccurrenceCount = 1,
                SourceType = PairingSourceType.Community,
                UpdatedAt = DateTimeOffset.UtcNow,
            };

            try
            {
                await _pairingRepository!.CreateAsync(updated, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Failed to create pairing '{PairingId}'.", pairingId);
            }
        }
    }

    // ── Circuit-breaker helpers ───────────────────────────────────────────────

    /// <summary>
    /// Returns <c>true</c> when the circuit is closed (KB is accepting calls).
    /// Automatically resets the circuit after the cooldown period has elapsed.
    /// </summary>
    private bool IsCircuitClosed()
    {
        var unavailableTicks = Interlocked.Read(ref _unavailableUntilTicks);
        if (unavailableTicks == 0)
        {
            return true; // Circuit has never been tripped.
        }

        if (DateTimeOffset.UtcNow.UtcTicks >= unavailableTicks)
        {
            // Cooldown has elapsed — reset and allow a probe request.
            Interlocked.CompareExchange(ref _unavailableUntilTicks, 0, unavailableTicks);
            Interlocked.Exchange(ref _consecutiveFailures, 0);
            return true;
        }

        return false;
    }

    private void RecordSuccess()
    {
        Interlocked.Exchange(ref _consecutiveFailures, 0);
        Interlocked.Exchange(ref _unavailableUntilTicks, 0);
    }

    private void RecordFailure()
    {
        var failures = Interlocked.Increment(ref _consecutiveFailures);
        if (failures >= FailureThreshold)
        {
            var until = DateTimeOffset.UtcNow.Add(CooldownPeriod).UtcTicks;
            // Only set if not already set (avoid repeatedly extending the deadline).
            Interlocked.CompareExchange(ref _unavailableUntilTicks, until, 0);
            _logger.LogWarning(
                "Ingredient Knowledge Base circuit breaker OPEN after {Failures} consecutive failures. " +
                "Will retry after {Until:O}.",
                failures,
                new DateTimeOffset(until, TimeSpan.Zero));
        }
    }

    /// <summary>
    /// Validates that an ingredient ID contains only safe characters to prevent SQL injection
    /// in Cosmos DB queries. Accepts lowercase letters, digits, and hyphens — the format used
    /// throughout the ingredient knowledge base (e.g., "ing-tomato").
    /// </summary>
    private static bool IsValidIngredientId(string id) =>
        !string.IsNullOrWhiteSpace(id)
        && id.Length <= 128
        && System.Text.RegularExpressions.Regex.IsMatch(id, @"^[a-zA-Z0-9\-_]+$");
}
