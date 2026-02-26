namespace Blend.Tests.Integration;

/// <summary>
/// Shared constants and helpers for integration tests that use the Cosmos DB emulator.
/// Set COSMOS_EMULATOR_CONNECTION_STRING to run against a custom endpoint.
/// </summary>
public static class CosmosEmulatorFixture
{
    public const string EmulatorKey =
        "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMssZaR8r/8ZbT1A==";

    public static readonly string EmulatorEndpoint =
        Environment.GetEnvironmentVariable("COSMOS_EMULATOR_CONNECTION_STRING")
        ?? $"AccountEndpoint=https://localhost:8081/;AccountKey={EmulatorKey}";

    public static async Task<bool> IsEmulatorAvailableAsync()
    {
        try
        {
            using var client = new Microsoft.Azure.Cosmos.CosmosClient(
                EmulatorEndpoint,
                new Microsoft.Azure.Cosmos.CosmosClientOptions
                {
                    MaxRetryAttemptsOnRateLimitedRequests = 0
                });
            await client.ReadAccountAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
