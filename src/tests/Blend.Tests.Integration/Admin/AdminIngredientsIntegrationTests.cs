using Blend.Domain.Entities;
using Blend.Domain.Interfaces;
using Blend.Infrastructure.Cosmos.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Blend.Tests.Integration.Admin;

/// <summary>
/// Integration tests for ingredient submission approval workflow against the Cosmos DB emulator.
/// Set COSMOS_EMULATOR_CONNECTION_STRING to run.
/// </summary>
[Trait("Category", "Integration")]
public class AdminIngredientsIntegrationTests : IAsyncLifetime, IDisposable
{
    private CosmosClient? _client;
    private IRepository<IngredientSubmission>? _submissionsRepository;
    private readonly string _databaseName = $"blend-test-{Guid.NewGuid():N}";
    private bool _emulatorAvailable;

    public async Task InitializeAsync()
    {
        _emulatorAvailable = await CosmosEmulatorFixture.IsEmulatorAvailableAsync();
        if (!_emulatorAvailable) return;

        _client = new CosmosClient(CosmosEmulatorFixture.EmulatorEndpoint, new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                IgnoreNullValues = true
            }
        });

        var db = await _client.CreateDatabaseIfNotExistsAsync(_databaseName);
        await db.Database.CreateContainerIfNotExistsAsync(
            new ContainerProperties("content", "/contentType"));

        var container = _client.GetContainer(_databaseName, "content");
        _submissionsRepository = new CosmosRepository<IngredientSubmission>(
            container,
            NullLogger<CosmosRepository<IngredientSubmission>>.Instance);
    }

    public async Task DisposeAsync()
    {
        if (_client is not null && _emulatorAvailable)
        {
            try { await _client.GetDatabase(_databaseName).DeleteAsync(); }
            catch { /* best-effort */ }
        }
    }

    public void Dispose() => _client?.Dispose();

    [SkippableFact]
    public async Task IngredientSubmission_SubmitApprove_Workflow()
    {
        Skip.IfNot(_emulatorAvailable, "Cosmos DB emulator not available");

        // User submits an ingredient
        var submission = new IngredientSubmission
        {
            IngredientName = "Fenugreek",
            Description = "A spice used in Indian cuisine.",
            Category = "spice",
            SubmittedByUserId = "user-42",
            SubmissionStatus = IngredientSubmissionStatus.Pending
        };

        var created = await _submissionsRepository!.CreateAsync(submission);
        Assert.Equal(IngredientSubmissionStatus.Pending, created.SubmissionStatus);

        // Admin approves
        created.SubmissionStatus = IngredientSubmissionStatus.Approved;
        created.ReviewedAt = DateTimeOffset.UtcNow;
        created.ReviewedByAdminId = "admin-1";

        var approved = await _submissionsRepository.UpdateAsync(created, "ingredient-submission");
        Assert.Equal(IngredientSubmissionStatus.Approved, approved.SubmissionStatus);
        Assert.NotNull(approved.ReviewedAt);
    }

    [SkippableFact]
    public async Task IngredientSubmission_SubmitRejectWithReason_Workflow()
    {
        Skip.IfNot(_emulatorAvailable, "Cosmos DB emulator not available");

        var submission = new IngredientSubmission
        {
            IngredientName = "Moonbeam Extract",
            Description = "A fictional ingredient.",
            SubmittedByUserId = "user-99",
            SubmissionStatus = IngredientSubmissionStatus.Pending
        };

        var created = await _submissionsRepository!.CreateAsync(submission);

        // Admin rejects with reason
        created.SubmissionStatus = IngredientSubmissionStatus.Rejected;
        created.ReviewedAt = DateTimeOffset.UtcNow;
        created.ReviewedByAdminId = "admin-1";
        created.RejectionReason = "Not a real ingredient.";

        var rejected = await _submissionsRepository.UpdateAsync(created, "ingredient-submission");
        Assert.Equal(IngredientSubmissionStatus.Rejected, rejected.SubmissionStatus);
        Assert.Equal("Not a real ingredient.", rejected.RejectionReason);
    }

    [SkippableFact]
    public async Task IngredientSubmissions_FilterByStatus_ReturnsOnlyMatching()
    {
        Skip.IfNot(_emulatorAvailable, "Cosmos DB emulator not available");

        await _submissionsRepository!.CreateAsync(new IngredientSubmission
        {
            IngredientName = "Pending Herb",
            SubmittedByUserId = "u1",
            SubmissionStatus = IngredientSubmissionStatus.Pending
        });
        await _submissionsRepository.CreateAsync(new IngredientSubmission
        {
            IngredientName = "Approved Spice",
            SubmittedByUserId = "u2",
            SubmissionStatus = IngredientSubmissionStatus.Approved
        });

        var result = await _submissionsRepository.QueryAsync(
            "SELECT * FROM c WHERE c.contentType = 'ingredient-submission' AND c.submissionStatus = @status",
            parameters: new Dictionary<string, object> { ["@status"] = (int)IngredientSubmissionStatus.Pending },
            partitionKey: "ingredient-submission");

        Assert.All(result.Items, item => Assert.Equal(IngredientSubmissionStatus.Pending, item.SubmissionStatus));
    }

    private static async Task<bool> IsEmulatorAvailableAsync() =>
        await CosmosEmulatorFixture.IsEmulatorAvailableAsync();
}
