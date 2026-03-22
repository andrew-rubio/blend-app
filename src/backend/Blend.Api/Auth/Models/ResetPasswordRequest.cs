namespace Blend.Api.Auth.Models;
public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);
