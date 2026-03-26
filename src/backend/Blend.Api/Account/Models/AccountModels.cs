namespace Blend.Api.Account.Models;

/// <summary>Request body for initiating account deletion.</summary>
public sealed class DeleteAccountRequest
{
    /// <summary>Current password for re-authentication confirmation.</summary>
    public string? Password { get; init; }
}

/// <summary>Response returned after a deletion request is created.</summary>
/// <param name="DeletionScheduledAt">When the account will be permanently deleted.</param>
/// <param name="GracePeriodEndsAt">The last moment at which the deletion can be cancelled.</param>
public sealed record DeleteAccountResponse(
    DateTimeOffset DeletionScheduledAt,
    DateTimeOffset GracePeriodEndsAt);

/// <summary>Result of an account deletion operation.</summary>
public enum AccountDeletionOpResult
{
    Success,
    NoPendingRequest,
    AlreadyRequested,
    ReAuthRequired,
    NotFound,
}
