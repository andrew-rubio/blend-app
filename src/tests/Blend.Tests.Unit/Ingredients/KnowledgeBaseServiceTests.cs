using Blend.Api.Ingredients.Models;
using Blend.Api.Ingredients.Services;
using Blend.Domain.Entities;
using Blend.Domain.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Blend.Tests.Unit.Ingredients;

/// <summary>
/// Unit tests for <see cref="KnowledgeBaseService"/>.
/// Azure AI Search calls are exercised only through the circuit-breaker path
/// (no real HTTP calls are made); Cosmos DB is mocked via <see cref="IRepository{T}"/>.
/// </summary>
public class KnowledgeBaseServiceTests
{
    // ── Factory helpers ───────────────────────────────────────────────────────

    private static KnowledgeBaseService CreateService(
        IngredientSearchOptions? opts = null,
        IRepository<IngredientPairing>? pairingRepo = null)
    {
        var options = Options.Create(opts ?? new IngredientSearchOptions());
        return new KnowledgeBaseService(
            NullLogger<KnowledgeBaseService>.Instance,
            options,
            pairingRepo);
    }

    private static Mock<IRepository<IngredientPairing>> CreatePairingRepoMock(
        IReadOnlyList<IngredientPairing>? pairings = null)
    {
        var mock = new Mock<IRepository<IngredientPairing>>();
        mock.Setup(r => r.GetByQueryAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pairings ?? []);
        mock.Setup(r => r.GetByIdAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IngredientPairing?)null);
        return mock;
    }

    // ── IsAvailableAsync — no search config ───────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_WhenNotConfigured_ReturnsFalse()
    {
        var svc = CreateService(); // no Endpoint/ApiKey
        var available = await svc.IsAvailableAsync();
        Assert.False(available);
    }

    // ── IsAvailableAsync — circuit breaker fresh ──────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_WhenConfigured_ReturnsTrue_Initially()
    {
        var opts = new IngredientSearchOptions
        {
            Endpoint = "https://test.search.windows.net",
            ApiKey = "test-key",
        };
        var svc = CreateService(opts);
        var available = await svc.IsAvailableAsync();
        Assert.True(available);
    }

    // ── SearchIngredientsAsync — not configured ───────────────────────────────

    [Fact]
    public async Task SearchIngredientsAsync_WhenNotConfigured_ReturnsEmpty()
    {
        var svc = CreateService();
        var results = await svc.SearchIngredientsAsync("tomato");
        Assert.Empty(results);
    }

    // ── GetIngredientAsync — not configured ───────────────────────────────────

    [Fact]
    public async Task GetIngredientAsync_WhenNotConfigured_ReturnsNull()
    {
        var svc = CreateService();
        var ingredient = await svc.GetIngredientAsync("ing-tomato");
        Assert.Null(ingredient);
    }

    // ── GetSubstitutesAsync — not configured ──────────────────────────────────

    [Fact]
    public async Task GetSubstitutesAsync_WhenNotConfigured_ReturnsEmpty()
    {
        var svc = CreateService();
        var substitutes = await svc.GetSubstitutesAsync("ing-tomato");
        Assert.Empty(substitutes);
    }

    // ── GetPairingsAsync — no repository ─────────────────────────────────────

    [Fact]
    public async Task GetPairingsAsync_WhenRepositoryNull_ReturnsEmpty()
    {
        var svc = CreateService(pairingRepo: null);
        var pairings = await svc.GetPairingsAsync("ing-tomato");
        Assert.Empty(pairings);
    }

    // ── GetPairingsAsync — repository returns pairings ───────────────────────

    [Fact]
    public async Task GetPairingsAsync_WithPairings_ReturnsSortedByScoreDescending()
    {
        var pairing1 = new IngredientPairing
        {
            Id = "ing-tomato:ing-basil",
            IngredientId = "ing-tomato",
            PairedIngredientId = "ing-basil",
            Score = 0.95,
            SourceType = PairingSourceType.Reference,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        var pairing2 = new IngredientPairing
        {
            Id = "ing-tomato:ing-garlic",
            IngredientId = "ing-tomato",
            PairedIngredientId = "ing-garlic",
            Score = 0.97,
            SourceType = PairingSourceType.Reference,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        var pairing3 = new IngredientPairing
        {
            Id = "ing-tomato:ing-onion",
            IngredientId = "ing-tomato",
            PairedIngredientId = "ing-onion",
            Score = 0.88,
            SourceType = PairingSourceType.Reference,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var mock = CreatePairingRepoMock([pairing1, pairing2, pairing3]);
        var svc = CreateService(pairingRepo: mock.Object);

        var pairings = await svc.GetPairingsAsync("ing-tomato");

        Assert.Equal(3, pairings.Count);
        Assert.True(pairings[0].Score >= pairings[1].Score,
            "Results should be sorted by score descending.");
        Assert.True(pairings[1].Score >= pairings[2].Score,
            "Results should be sorted by score descending.");
    }

    [Fact]
    public async Task GetPairingsAsync_WithPairings_ReturnsPairedIngredientId()
    {
        var pairing = new IngredientPairing
        {
            Id = "ing-tomato:ing-garlic",
            IngredientId = "ing-tomato",
            PairedIngredientId = "ing-garlic",
            Score = 0.97,
            SourceType = PairingSourceType.Reference,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var mock = CreatePairingRepoMock([pairing]);
        var svc = CreateService(pairingRepo: mock.Object);

        var pairings = await svc.GetPairingsAsync("ing-tomato");

        Assert.Single(pairings);
        Assert.Equal("ing-garlic", pairings[0].PairedIngredientId);
        Assert.Equal(0.97, pairings[0].Score);
        Assert.Equal(PairingSourceType.Reference, pairings[0].SourceType);
    }

    [Fact]
    public async Task GetPairingsAsync_WithCommunityPairing_ReturnsCommunitySourceType()
    {
        var pairing = new IngredientPairing
        {
            Id = "ing-tomato:ing-garlic",
            IngredientId = "ing-tomato",
            PairedIngredientId = "ing-garlic",
            Score = 0.85,
            SourceType = PairingSourceType.Community,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var mock = CreatePairingRepoMock([pairing]);
        var svc = CreateService(pairingRepo: mock.Object);

        var pairings = await svc.GetPairingsAsync("ing-tomato");

        Assert.Single(pairings);
        Assert.Equal(PairingSourceType.Community, pairings[0].SourceType);
    }

    // ── Circuit breaker ───────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_AfterThreeFailures_ReturnsFalse()
    {
        // Use a real (but unreachable) endpoint so the service attempts calls.
        var opts = new IngredientSearchOptions
        {
            Endpoint = "https://nonexistent-test-host.search.windows.net",
            ApiKey = "test-key",
        };
        var svc = CreateService(opts);

        // Force three failures via SearchIngredientsAsync (which calls Azure AI Search).
        // Each call will throw because the endpoint is unreachable.
        for (var i = 0; i < 3; i++)
        {
            var results = await svc.SearchIngredientsAsync("test");
            Assert.Empty(results); // degraded, not throwing
        }

        // After 3 failures the circuit should be open.
        var available = await svc.IsAvailableAsync();
        Assert.False(available);
    }

    [Fact]
    public async Task SearchIngredientsAsync_AfterCircuitOpen_ReturnsEmpty()
    {
        var opts = new IngredientSearchOptions
        {
            Endpoint = "https://nonexistent-test-host.search.windows.net",
            ApiKey = "test-key",
        };
        var svc = CreateService(opts);

        // Open the circuit.
        for (var i = 0; i < 3; i++)
        {
            await svc.SearchIngredientsAsync("test");
        }

        Assert.False(await svc.IsAvailableAsync());

        // Further calls return empty without hitting the service.
        var results = await svc.SearchIngredientsAsync("tomato");
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetIngredientAsync_AfterCircuitOpen_ReturnsNull()
    {
        var opts = new IngredientSearchOptions
        {
            Endpoint = "https://nonexistent-test-host.search.windows.net",
            ApiKey = "test-key",
        };
        var svc = CreateService(opts);

        // Open the circuit.
        for (var i = 0; i < 3; i++)
        {
            await svc.SearchIngredientsAsync("test");
        }

        var ingredient = await svc.GetIngredientAsync("ing-tomato");
        Assert.Null(ingredient);
    }

    // ── Limit clamping (via GetPairingsAsync) ─────────────────────────────────

    [Fact]
    public async Task GetPairingsAsync_LimitRespected()
    {
        var pairings = Enumerable.Range(1, 10)
            .Select(i => new IngredientPairing
            {
                Id = $"ing-a:ing-{i}",
                IngredientId = "ing-a",
                PairedIngredientId = $"ing-{i}",
                Score = 0.5 + i * 0.01,
                SourceType = PairingSourceType.Reference,
                UpdatedAt = DateTimeOffset.UtcNow,
            })
            .ToList();

        var mock = new Mock<IRepository<IngredientPairing>>();
        mock.Setup(r => r.GetByQueryAsync(
                It.Is<string>(q => q.Contains("TOP 3")),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pairings.Take(3).ToList());

        var svc = CreateService(pairingRepo: mock.Object);
        var result = await svc.GetPairingsAsync("ing-a", limit: 3);

        Assert.True(result.Count <= 3);
    }

    // ── IngredientPairing entity ──────────────────────────────────────────────

    [Fact]
    public void IngredientPairing_DefaultSourceType_IsReference()
    {
        var pairing = new IngredientPairing();
        Assert.Equal(PairingSourceType.Reference, pairing.SourceType);
    }

    [Fact]
    public void PairingSourceType_Constants_HaveExpectedValues()
    {
        Assert.Equal("reference", PairingSourceType.Reference);
        Assert.Equal("community", PairingSourceType.Community);
    }

    // ── UpdatePairingScoreAsync — pairing score aggregation ───────────────────

    [Fact]
    public async Task UpdatePairingScoreAsync_WhenRepositoryNull_DoesNotThrow()
    {
        var svc = CreateService(pairingRepo: null);

        // Should complete gracefully without throwing
        await svc.UpdatePairingScoreAsync("ing-tomato", "ing-basil", 0.8);
    }

    [Fact]
    public async Task UpdatePairingScoreAsync_WhenPairingExists_UpdatesScoreByWeightedAverage()
    {
        var existingPairing = new IngredientPairing
        {
            Id = "ing-tomato:ing-basil",
            IngredientId = "ing-tomato",
            PairedIngredientId = "ing-basil",
            Score = 0.5,
            CoOccurrenceCount = 2,
            SourceType = PairingSourceType.Reference,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        IngredientPairing? capturedUpdate = null;
        var mock = new Mock<IRepository<IngredientPairing>>();
        mock.Setup(r => r.GetByIdAsync("ing-tomato:ing-basil", "ing-tomato", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPairing);
        mock.Setup(r => r.UpdateAsync(It.IsAny<IngredientPairing>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<IngredientPairing, string, string, CancellationToken>((p, _, _, _) => capturedUpdate = p)
            .ReturnsAsync((IngredientPairing p, string _, string _, CancellationToken _) => p);
        // Reverse direction
        mock.Setup(r => r.GetByIdAsync("ing-basil:ing-tomato", "ing-basil", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IngredientPairing?)null);
        mock.Setup(r => r.CreateAsync(It.IsAny<IngredientPairing>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IngredientPairing p, CancellationToken _) => p);

        var svc = CreateService(pairingRepo: mock.Object);

        // Submit rating of 5 → normalised = 1.0
        // Expected new score = (0.5 * 2 + 1.0) / 3 = 2.0/3 ≈ 0.6667
        await svc.UpdatePairingScoreAsync("ing-tomato", "ing-basil", 1.0);

        Assert.NotNull(capturedUpdate);
        Assert.Equal(PairingSourceType.Community, capturedUpdate.SourceType);
        Assert.Equal(3, capturedUpdate.CoOccurrenceCount);
        Assert.Equal(2.0 / 3.0, capturedUpdate.Score, precision: 10);
    }

    [Fact]
    public async Task UpdatePairingScoreAsync_WhenPairingDoesNotExist_CreatesNewCommunityPairing()
    {
        var created = new List<IngredientPairing>();
        var mock = new Mock<IRepository<IngredientPairing>>();
        mock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IngredientPairing?)null);
        mock.Setup(r => r.CreateAsync(It.IsAny<IngredientPairing>(), It.IsAny<CancellationToken>()))
            .Callback<IngredientPairing, CancellationToken>((p, _) => created.Add(p))
            .ReturnsAsync((IngredientPairing p, CancellationToken _) => p);

        var svc = CreateService(pairingRepo: mock.Object);
        await svc.UpdatePairingScoreAsync("ing-new-a", "ing-new-b", 0.6);

        // Forward direction
        var forward = created.FirstOrDefault(p => p.Id == "ing-new-a:ing-new-b");
        Assert.NotNull(forward);
        Assert.Equal("ing-new-a", forward.IngredientId);
        Assert.Equal("ing-new-b", forward.PairedIngredientId);
        Assert.Equal(0.6, forward.Score, precision: 10);
        Assert.Equal(1, forward.CoOccurrenceCount);
        Assert.Equal(PairingSourceType.Community, forward.SourceType);
    }

    [Fact]
    public async Task UpdatePairingScoreAsync_UpdatesBothDirections()
    {
        var mock = new Mock<IRepository<IngredientPairing>>();
        mock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IngredientPairing?)null);
        mock.Setup(r => r.CreateAsync(It.IsAny<IngredientPairing>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IngredientPairing p, CancellationToken _) => p);

        var svc = CreateService(pairingRepo: mock.Object);
        await svc.UpdatePairingScoreAsync("ing-a", "ing-b", 0.8);

        // Both A→B and B→A should be created
        mock.Verify(r => r.CreateAsync(
            It.Is<IngredientPairing>(p => p.Id == "ing-a:ing-b"),
            It.IsAny<CancellationToken>()), Times.Once);
        mock.Verify(r => r.CreateAsync(
            It.Is<IngredientPairing>(p => p.Id == "ing-b:ing-a"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePairingScoreAsync_ClampedToValidRange()
    {
        var created = new List<IngredientPairing>();
        var mock = new Mock<IRepository<IngredientPairing>>();
        mock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IngredientPairing?)null);
        mock.Setup(r => r.CreateAsync(It.IsAny<IngredientPairing>(), It.IsAny<CancellationToken>()))
            .Callback<IngredientPairing, CancellationToken>((p, _) => created.Add(p))
            .ReturnsAsync((IngredientPairing p, CancellationToken _) => p);

        var svc = CreateService(pairingRepo: mock.Object);
        // Rating above 1.0 should be clamped
        await svc.UpdatePairingScoreAsync("ing-a", "ing-b", 1.5);

        Assert.NotEmpty(created);
        Assert.All(created, p =>
        {
            Assert.True(p.Score <= 1.0, "Score should be clamped to 1.0.");
            Assert.True(p.Score >= 0.0, "Score should be >= 0.0.");
        });
    }
}

