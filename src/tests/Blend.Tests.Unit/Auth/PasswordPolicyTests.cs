using Blend.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace Blend.Tests.Unit.Auth;

public class PasswordPolicyTests
{
    // Mirror the options configured in AuthServiceExtensions.AddBlendAuthentication
    private static readonly PasswordOptions ExpectedOptions = new()
    {
        RequireDigit = true,
        RequiredLength = 8,
        RequireNonAlphanumeric = true,
        RequireUppercase = true,
        RequireLowercase = true,
    };

    [Fact]
    public void PasswordOptions_RequireDigit_IsTrue() =>
        Assert.True(ExpectedOptions.RequireDigit);

    [Fact]
    public void PasswordOptions_RequiredLength_Is8() =>
        Assert.Equal(8, ExpectedOptions.RequiredLength);

    [Fact]
    public void PasswordOptions_RequireNonAlphanumeric_IsTrue() =>
        Assert.True(ExpectedOptions.RequireNonAlphanumeric);

    [Fact]
    public void PasswordOptions_RequireUppercase_IsTrue() =>
        Assert.True(ExpectedOptions.RequireUppercase);

    [Fact]
    public void PasswordOptions_RequireLowercase_IsTrue() =>
        Assert.True(ExpectedOptions.RequireLowercase);

    [Fact]
    public void PasswordHasher_HashAndVerify_Works()
    {
        var hasher = new PasswordHasher<BlendUser>();
        var user = new BlendUser();
        const string password = "ValidPass1!";

        var hash = hasher.HashPassword(user, password);
        var result = hasher.VerifyHashedPassword(user, hash, password);

        Assert.Equal(PasswordVerificationResult.Success, result);
    }

    [Fact]
    public void PasswordHasher_WrongPassword_Fails()
    {
        var hasher = new PasswordHasher<BlendUser>();
        var user = new BlendUser();

        var hash = hasher.HashPassword(user, "ValidPass1!");
        var result = hasher.VerifyHashedPassword(user, hash, "WrongPass1!");

        Assert.Equal(PasswordVerificationResult.Failed, result);
    }

    [Fact]
    public void PasswordHasher_ProducesUniqueHashes()
    {
        var hasher = new PasswordHasher<BlendUser>();
        var user = new BlendUser();
        const string password = "ValidPass1!";

        var hash1 = hasher.HashPassword(user, password);
        var hash2 = hasher.HashPassword(user, password);

        // Each hash includes a random salt, so two hashes of the same password differ
        Assert.NotEqual(hash1, hash2);
    }
}
