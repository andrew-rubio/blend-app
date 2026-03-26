using Blend.Api.Account.Models;

namespace Blend.Api.Account.Services;

/// <summary>
/// Manages account deletion lifecycle (SETT-17 through SETT-24, REQ-61).
/// </summary>
public interface IAccountDeletionService
{
    /// <summary>
    /// Initiates a deletion request for the given user after verifying their password.
    /// The account is deactivated immediately and permanently deleted after 30 days.
    /// </summary>
    /// <returns>The scheduled deletion time, or an operation failure result.</returns>
    Task<(DateTimeOffset? ScheduledAt, AccountDeletionOpResult Result)> RequestDeletionAsync(
        string userId,
        string? password,
        CancellationToken ct = default);

    /// <summary>
    /// Cancels a pending deletion request within the 30-day grace period, reactivating the account.
    /// </summary>
    Task<AccountDeletionOpResult> CancelDeletionAsync(
        string userId,
        CancellationToken ct = default);
}
