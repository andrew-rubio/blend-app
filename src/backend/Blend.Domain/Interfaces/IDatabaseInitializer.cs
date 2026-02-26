namespace Blend.Domain.Interfaces;

/// <summary>
/// Marker interface for the database initializer.
/// Ensures containers are created and seed data is loaded on startup.
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>Creates containers and applies seed data if needed.</summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
