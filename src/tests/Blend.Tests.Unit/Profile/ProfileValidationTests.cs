using Blend.Api.Profile.Models;
using Blend.Api.Profile.Services;
using Blend.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Blend.Tests.Unit.Profile;

public class ProfileValidationTests
{
    private static ProfileService CreateService(Mock<UserManager<BlendUser>>? userManagerMock = null)
    {
        userManagerMock ??= MockUserManager();
        return new ProfileService(userManagerMock.Object, NullLogger<ProfileService>.Instance);
    }

    private static Mock<UserManager<BlendUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<BlendUser>>();
        return new Mock<UserManager<BlendUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    // ── Display name validation ────────────────────────────────────────────────

    [Fact]
    public void ValidateUpdateProfile_EmptyDisplayName_ReturnsError()
    {
        var svc = CreateService();
        var errors = svc.ValidateUpdateProfile(new UpdateProfileRequest { DisplayName = "" });
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void ValidateUpdateProfile_DisplayNameTooShort_ReturnsError()
    {
        var svc = CreateService();
        var errors = svc.ValidateUpdateProfile(new UpdateProfileRequest { DisplayName = "A" });
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void ValidateUpdateProfile_DisplayNameExactlyMin_NoError()
    {
        var svc = CreateService();
        var errors = svc.ValidateUpdateProfile(new UpdateProfileRequest { DisplayName = "AB" });
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateUpdateProfile_DisplayNameTooLong_ReturnsError()
    {
        var svc = CreateService();
        var errors = svc.ValidateUpdateProfile(new UpdateProfileRequest { DisplayName = new string('A', 51) });
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void ValidateUpdateProfile_DisplayNameExactlyMax_NoError()
    {
        var svc = CreateService();
        var errors = svc.ValidateUpdateProfile(new UpdateProfileRequest { DisplayName = new string('A', 50) });
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateUpdateProfile_DisplayNameWithSpecialChars_ReturnsError()
    {
        var svc = CreateService();
        var errors = svc.ValidateUpdateProfile(new UpdateProfileRequest { DisplayName = "Bad!Name@" });
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void ValidateUpdateProfile_DisplayNameAllowsLettersNumbersSpacesUnderscoresHyphensDots_NoError()
    {
        var svc = CreateService();
        var errors = svc.ValidateUpdateProfile(new UpdateProfileRequest { DisplayName = "John_Doe-2.0" });
        Assert.Empty(errors);
    }

    // ── Bio validation ────────────────────────────────────────────────────────

    [Fact]
    public void ValidateUpdateProfile_NullBio_NoError()
    {
        var svc = CreateService();
        var errors = svc.ValidateUpdateProfile(new UpdateProfileRequest { DisplayName = "ValidName", Bio = null });
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateUpdateProfile_BioExactlyMax_NoError()
    {
        var svc = CreateService();
        var errors = svc.ValidateUpdateProfile(new UpdateProfileRequest
        {
            DisplayName = "ValidName",
            Bio = new string('x', 500),
        });
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateUpdateProfile_BioTooLong_ReturnsError()
    {
        var svc = CreateService();
        var errors = svc.ValidateUpdateProfile(new UpdateProfileRequest
        {
            DisplayName = "ValidName",
            Bio = new string('x', 501),
        });
        Assert.NotEmpty(errors);
    }

    // ── GetMyProfileAsync authorisation ──────────────────────────────────────

    [Fact]
    public async Task GetMyProfileAsync_ReturnsPrivateData()
    {
        var userManagerMock = MockUserManager();
        var user = new BlendUser
        {
            Id = "user-1",
            DisplayName = "Test User",
            Email = "test@example.com",
            ProfilePhotoUrl = null,
            Bio = "My bio",
            CreatedAt = DateTimeOffset.UtcNow,
            RecipeCount = 5,
            FollowerCount = 10,
            FollowingCount = 3,
        };

        userManagerMock.Setup(m => m.FindByIdAsync("user-1"))
            .ReturnsAsync(user);

        var svc = CreateService(userManagerMock);
        var profile = await svc.GetMyProfileAsync("user-1");

        Assert.NotNull(profile);
        Assert.Equal("test@example.com", profile.Email);
        Assert.Equal(10, profile.FollowerCount);
        Assert.Equal(3, profile.FollowingCount);
        Assert.Equal(5, profile.RecipeCount);
        Assert.Equal("My bio", profile.Bio);
    }

    [Fact]
    public async Task GetMyProfileAsync_UserNotFound_ReturnsNull()
    {
        var userManagerMock = MockUserManager();
        userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((BlendUser?)null);

        var svc = CreateService(userManagerMock);
        var profile = await svc.GetMyProfileAsync("nonexistent");

        Assert.Null(profile);
    }

    // ── GetPublicProfileAsync authorisation ───────────────────────────────────

    [Fact]
    public async Task GetPublicProfileAsync_DoesNotExposeEmail()
    {
        var userManagerMock = MockUserManager();
        var user = new BlendUser
        {
            Id = "user-2",
            DisplayName = "Public User",
            Email = "private@example.com",
            CreatedAt = DateTimeOffset.UtcNow,
            RecipeCount = 2,
        };

        userManagerMock.Setup(m => m.FindByIdAsync("user-2"))
            .ReturnsAsync(user);

        var svc = CreateService(userManagerMock);
        var profile = await svc.GetPublicProfileAsync("user-2");

        Assert.NotNull(profile);
        Assert.Equal("Public User", profile.DisplayName);
        Assert.Equal(2, profile.RecipeCount);
        // PublicProfileResponse has no Email property — compile-time guarantee
    }

    [Fact]
    public async Task GetPublicProfileAsync_UserNotFound_ReturnsNull()
    {
        var userManagerMock = MockUserManager();
        userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((BlendUser?)null);

        var svc = CreateService(userManagerMock);
        var profile = await svc.GetPublicProfileAsync("nonexistent");

        Assert.Null(profile);
    }
}
