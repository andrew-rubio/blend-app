namespace Blend.Api.Configuration;

public class JwtSettings
{
    public string Issuer { get; set; } = "blend-app";
    public string Audience { get; set; } = "blend-app-users";
    public string SecretKey { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 20;
    public int RefreshTokenExpirationDays { get; set; } = 7;
    public string PasswordResetBaseUrl { get; set; } = "https://localhost:3000/reset-password";
}
