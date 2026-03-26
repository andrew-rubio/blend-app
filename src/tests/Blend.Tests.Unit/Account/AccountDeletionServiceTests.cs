using Blend.Api.Account.Models;
using Blend.Api.Account.Services;
using Blend.Domain.Entities;
using Blend.Domain.Identity;
using Blend.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Blend.Tests.Unit.Account;

/// <summary>Unit tests for <see cref="AccountDeletionService"/> state machine.</summary>
public class AccountDeletionServiceTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Mock<UserManager<BlendUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<BlendUser>>();
        var queryableStore = store.As<IQueryableUserStore<BlendUser>>();
        queryableStore.Setup(s => s.Users).Returns(new List<BlendUser>().AsQueryable());
        return new Mock<UserManager<BlendUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static AccountDeletionService CreateService(
        Mock<UserManager<BlendUser>>? userManager = null,
        IRepository<User>? userRepo = null)
    {
        return new AccountDeletionService(
            NullLogger<AccountDeletionService>.Instance,
            userManager?.Object,
            userRepo);
    }

    private static User MakeUser(
        string id,
        DateTimeOffset? deletionRequestedAt = null,
        bool isDeactivated = false)
    {
        return new User
        {
            Id = id,
            Email = $"{id}@example.com",
            DisplayName = id,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-30),
            UpdatedAt = DateTimeOffset.UtcNow,
            DeletionRequestedAt = deletionRequestedAt,
            IsDeactivated = isDeactivated,
        };
    }

    // ── RequestDeletionAsync — service unavailable ────────────────────────────

    [Fact]
    public async Task RequestDeletion_WhenServicesNull_ReturnsNotFound()
    {
        var svc = CreateService();
        var (_, result) = await svc.RequestDeletionAsync("user-1", "password");
        Assert.Equal(AccountDeletionOpResult.NotFound, result);
    }

    // ── RequestDeletionAsync — re-authentication ──────────────────────────────

    [Fact]
    public async Task RequestDeletion_WhenPasswordRequiredButNotProvided_ReturnsReAuthRequired()
    {
        var userMgr = MockUserManager();
        var identityUser = new BlendUser { Id = "user-1" };

        userMgr.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(identityUser);
        userMgr.Setup(m => m.HasPasswordAsync(identityUser)).ReturnsAsync(true);

        var userRepoMock = new Mock<IRepository<User>>();

        var svc = CreateService(userMgr, userRepoMock.Object);
        var (_, result) = await svc.RequestDeletionAsync("user-1", null);

        Assert.Equal(AccountDeletionOpResult.ReAuthRequired, result);
    }

    [Fact]
    public async Task RequestDeletion_WhenPasswordWrong_ReturnsReAuthRequired()
    {
        var userMgr = MockUserManager();
        var identityUser = new BlendUser { Id = "user-1" };

        userMgr.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(identityUser);
        userMgr.Setup(m => m.HasPasswordAsync(identityUser)).ReturnsAsync(true);
        userMgr.Setup(m => m.CheckPasswordAsync(identityUser, "wrong")).ReturnsAsync(false);

        var userRepoMock = new Mock<IRepository<User>>();

        var svc = CreateService(userMgr, userRepoMock.Object);
        var (_, result) = await svc.RequestDeletionAsync("user-1", "wrong");

        Assert.Equal(AccountDeletionOpResult.ReAuthRequired, result);
    }

    // ── RequestDeletionAsync — not found ─────────────────────────────────────

    [Fact]
    public async Task RequestDeletion_WhenUserNotFound_ReturnsNotFound()
    {
        var userMgr = MockUserManager();
        userMgr.Setup(m => m.FindByIdAsync("missing")).ReturnsAsync((BlendUser?)null);

        var svc = CreateService(userMgr);
        var (_, result) = await svc.RequestDeletionAsync("missing", null);

        Assert.Equal(AccountDeletionOpResult.NotFound, result);
    }

    // ── RequestDeletionAsync — success ────────────────────────────────────────

    [Fact]
    public async Task RequestDeletion_WhenValid_DeactivatesUserAndReturnsSuccess()
    {
        var userMgr = MockUserManager();
        var identityUser = new BlendUser { Id = "user-1" };
        var cosmosUser = MakeUser("user-1");

        userMgr.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(identityUser);
        userMgr.Setup(m => m.HasPasswordAsync(identityUser)).ReturnsAsync(true);
        userMgr.Setup(m => m.CheckPasswordAsync(identityUser, "correct-password")).ReturnsAsync(true);

        User? savedUser = null;
        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock.Setup(r => r.GetByIdAsync("user-1", "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cosmosUser);
        userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), "user-1", "user-1", It.IsAny<CancellationToken>()))
            .Callback<User, string, string, CancellationToken>((u, _, _, _) => savedUser = u)
            .ReturnsAsync((User u, string _, string _, CancellationToken _) => u);

        var svc = CreateService(userMgr, userRepoMock.Object);
        var (scheduledAt, result) = await svc.RequestDeletionAsync("user-1", "correct-password");

        Assert.Equal(AccountDeletionOpResult.Success, result);
        Assert.NotNull(scheduledAt);
        Assert.NotNull(savedUser);
        Assert.True(savedUser!.IsDeactivated);
        Assert.NotNull(savedUser.DeletionRequestedAt);

        // Grace period should be ~30 days
        var expectedScheduledAt = savedUser.DeletionRequestedAt!.Value.Add(AccountDeletionService.GracePeriod);
        Assert.Equal(expectedScheduledAt, scheduledAt!.Value);
    }

    // ── RequestDeletionAsync — already requested ──────────────────────────────

    [Fact]
    public async Task RequestDeletion_WhenAlreadyRequested_ReturnsAlreadyRequested()
    {
        var userMgr = MockUserManager();
        var identityUser = new BlendUser { Id = "user-1" };
        var cosmosUser = MakeUser("user-1", deletionRequestedAt: DateTimeOffset.UtcNow.AddDays(-1), isDeactivated: true);

        userMgr.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(identityUser);
        userMgr.Setup(m => m.HasPasswordAsync(identityUser)).ReturnsAsync(true);
        userMgr.Setup(m => m.CheckPasswordAsync(identityUser, "password")).ReturnsAsync(true);

        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock.Setup(r => r.GetByIdAsync("user-1", "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cosmosUser);

        var svc = CreateService(userMgr, userRepoMock.Object);
        var (_, result) = await svc.RequestDeletionAsync("user-1", "password");

        Assert.Equal(AccountDeletionOpResult.AlreadyRequested, result);
    }

    // ── CancelDeletionAsync — service unavailable ─────────────────────────────

    [Fact]
    public async Task CancelDeletion_WhenRepoNull_ReturnsNotFound()
    {
        var svc = CreateService();
        var result = await svc.CancelDeletionAsync("user-1");
        Assert.Equal(AccountDeletionOpResult.NotFound, result);
    }

    // ── CancelDeletionAsync — no pending request ──────────────────────────────

    [Fact]
    public async Task CancelDeletion_WhenNoPendingRequest_ReturnsNoPendingRequest()
    {
        var cosmosUser = MakeUser("user-1");
        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock.Setup(r => r.GetByIdAsync("user-1", "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cosmosUser);

        var svc = CreateService(userRepo: userRepoMock.Object);
        var result = await svc.CancelDeletionAsync("user-1");

        Assert.Equal(AccountDeletionOpResult.NoPendingRequest, result);
    }

    // ── CancelDeletionAsync — grace period expired ────────────────────────────

    [Fact]
    public async Task CancelDeletion_WhenGracePeriodExpired_ReturnsNoPendingRequest()
    {
        // Requested 31 days ago — past the 30-day grace period
        var cosmosUser = MakeUser("user-1", deletionRequestedAt: DateTimeOffset.UtcNow.AddDays(-31), isDeactivated: true);
        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock.Setup(r => r.GetByIdAsync("user-1", "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cosmosUser);

        var svc = CreateService(userRepo: userRepoMock.Object);
        var result = await svc.CancelDeletionAsync("user-1");

        Assert.Equal(AccountDeletionOpResult.NoPendingRequest, result);
    }

    // ── CancelDeletionAsync — success ─────────────────────────────────────────

    [Fact]
    public async Task CancelDeletion_WhenWithinGracePeriod_ReactivatesAccountAndReturnsSuccess()
    {
        // Requested 5 days ago — within the 30-day grace period
        var cosmosUser = MakeUser("user-1", deletionRequestedAt: DateTimeOffset.UtcNow.AddDays(-5), isDeactivated: true);

        User? savedUser = null;
        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock.Setup(r => r.GetByIdAsync("user-1", "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cosmosUser);
        userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), "user-1", "user-1", It.IsAny<CancellationToken>()))
            .Callback<User, string, string, CancellationToken>((u, _, _, _) => savedUser = u)
            .ReturnsAsync((User u, string _, string _, CancellationToken _) => u);

        var svc = CreateService(userRepo: userRepoMock.Object);
        var result = await svc.CancelDeletionAsync("user-1");

        Assert.Equal(AccountDeletionOpResult.Success, result);
        Assert.NotNull(savedUser);
        Assert.False(savedUser!.IsDeactivated);
        Assert.Null(savedUser.DeletionRequestedAt);
    }

    // ── RequestDeletion without password (OAuth user) ─────────────────────────

    [Fact]
    public async Task RequestDeletion_WhenNoPassword_AndOAuthUser_SkipsPasswordCheck()
    {
        var userMgr = MockUserManager();
        var identityUser = new BlendUser { Id = "user-oauth" };
        var cosmosUser = MakeUser("user-oauth");

        userMgr.Setup(m => m.FindByIdAsync("user-oauth")).ReturnsAsync(identityUser);
        userMgr.Setup(m => m.HasPasswordAsync(identityUser)).ReturnsAsync(false);

        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock.Setup(r => r.GetByIdAsync("user-oauth", "user-oauth", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cosmosUser);
        userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), "user-oauth", "user-oauth", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, string _, string _, CancellationToken _) => u);

        var svc = CreateService(userMgr, userRepoMock.Object);
        var (_, result) = await svc.RequestDeletionAsync("user-oauth", null);

        Assert.Equal(AccountDeletionOpResult.Success, result);
    }
}
