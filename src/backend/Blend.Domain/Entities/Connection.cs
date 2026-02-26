namespace Blend.Domain.Entities;

/// <summary>
/// Represents a connection (friendship) between two users.
/// Partition key: /userId
/// </summary>
public class Connection : CosmosEntity
{
    public string UserId { get; set; } = string.Empty;

    public string ConnectedUserId { get; set; } = string.Empty;

    public ConnectionStatus Status { get; set; } = ConnectionStatus.Pending;

    public DateTimeOffset? AcceptedAt { get; set; }

    public string? InitiatedByUserId { get; set; }
}

public enum ConnectionStatus
{
    Pending,
    Accepted,
    Blocked,
    Declined
}
