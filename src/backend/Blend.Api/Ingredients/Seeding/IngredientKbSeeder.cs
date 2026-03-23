using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Blend.Api.Ingredients.Models;
using Blend.Api.Ingredients.Services;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace Blend.Api.Ingredients.Seeding;

/// <summary>
/// Idempotent seeder that populates:
/// <list type="bullet">
///   <item>The Azure AI Search <c>ingredients</c> index with reference ingredient documents.</item>
///   <item>The Cosmos DB <c>ingredientPairings</c> container with reference pairing scores.</item>
/// </list>
/// Safe to run multiple times — documents are upserted, not duplicated.
/// </summary>
public sealed class IngredientKbSeeder
{
    private readonly IngredientSearchOptions _options;
    private readonly IRepository<IngredientPairing>? _pairingRepository;
    private readonly ILogger<IngredientKbSeeder> _logger;

    public IngredientKbSeeder(
        IOptions<IngredientSearchOptions> options,
        ILogger<IngredientKbSeeder> logger,
        IRepository<IngredientPairing>? pairingRepository = null)
    {
        _options = options.Value;
        _logger = logger;
        _pairingRepository = pairingRepository;
    }

    /// <summary>Runs all seeding operations idempotently.</summary>
    public async Task SeedAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting Ingredient Knowledge Base seed...");

        if (_options.IsConfigured)
        {
            await SeedSearchIndexAsync(ct);
        }
        else
        {
            _logger.LogWarning(
                "Azure AI Search is not configured — skipping ingredient index seeding.");
        }

        if (_pairingRepository is not null)
        {
            await SeedPairingsAsync(ct);
        }
        else
        {
            _logger.LogWarning(
                "Ingredient pairing repository is not available — skipping pairing seed.");
        }

        _logger.LogInformation("Ingredient Knowledge Base seed complete.");
    }

    // ── Azure AI Search seeding ───────────────────────────────────────────────

    private async Task SeedSearchIndexAsync(CancellationToken ct)
    {
        var indexClient = new SearchIndexClient(
            new Uri(_options.Endpoint!),
            new AzureKeyCredential(_options.ApiKey!));

        await EnsureIndexExistsAsync(indexClient, ct);

        var searchClient = new SearchClient(
            new Uri(_options.Endpoint!),
            _options.IndexName,
            new AzureKeyCredential(_options.ApiKey!));

        var documents = IngredientSeedData.Ingredients
            .Select(s => new IngredientDocument
            {
                IngredientId = s.IngredientId,
                Name = s.Name,
                Aliases = s.Aliases,
                Category = s.Category,
                FlavourProfile = s.FlavourProfile,
                Substitutes = s.Substitutes,
                NutritionSummary = s.NutritionSummary,
            })
            .ToList();

        // MergeOrUpload is idempotent — existing documents are updated, new ones are added.
        var batch = IndexDocumentsBatch.MergeOrUpload(documents);
        var response = await searchClient.IndexDocumentsAsync(batch, cancellationToken: ct);

        var succeeded = response.Value.Results.Count(r => r.Succeeded);
        _logger.LogInformation(
            "Azure AI Search seeding: {Succeeded}/{Total} ingredient documents upserted.",
            succeeded,
            documents.Count);
    }

    private async Task EnsureIndexExistsAsync(SearchIndexClient indexClient, CancellationToken ct)
    {
        var fieldBuilder = new FieldBuilder();
        var searchFields = fieldBuilder.Build(typeof(IngredientDocument));

        var scoringProfile = new ScoringProfile("boost-exact")
        {
            TextWeights = new TextWeights(
                new Dictionary<string, double>
                {
                    ["name"] = 5.0,
                    ["aliases"] = 2.0,
                }),
        };

        var suggester = new SearchSuggester(_options.SuggesterName, ["name"]);

        var index = new SearchIndex(_options.IndexName, searchFields)
        {
            ScoringProfiles = { scoringProfile },
            Suggesters = { suggester },
            DefaultScoringProfile = "boost-exact",
        };

        try
        {
            await indexClient.CreateOrUpdateIndexAsync(index, cancellationToken: ct);
            _logger.LogInformation("Azure AI Search index '{Index}' ensured.", _options.IndexName);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex,
                "Failed to create or update Azure AI Search index '{Index}'.", _options.IndexName);
            throw;
        }
    }

    // ── Cosmos DB pairing seeding ─────────────────────────────────────────────

    private async Task SeedPairingsAsync(CancellationToken ct)
    {
        var seeded = 0;
        var skipped = 0;

        foreach (var pairing in IngredientSeedData.Pairings)
        {
            ct.ThrowIfCancellationRequested();

            // Idempotency check: only create if the document does not already exist.
            var existing = await _pairingRepository!.GetByIdAsync(
                pairing.Id, pairing.IngredientId, ct);

            if (existing is not null)
            {
                skipped++;
                continue;
            }

            await _pairingRepository.CreateAsync(pairing, ct);
            seeded++;
        }

        _logger.LogInformation(
            "Ingredient pairing seed: {Seeded} created, {Skipped} already existed.",
            seeded,
            skipped);
    }
}
