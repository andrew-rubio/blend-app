namespace Blend.Api.Auth.Models;
public sealed record RegisterRequest(string DisplayName, string Email, string Password);
