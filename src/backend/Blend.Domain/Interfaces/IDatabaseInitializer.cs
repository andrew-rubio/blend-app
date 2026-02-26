namespace Blend.Domain.Interfaces;

/// <summary>
/// Initialises the Cosmos DB database and containers on application startup.
/// </summary>
public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
