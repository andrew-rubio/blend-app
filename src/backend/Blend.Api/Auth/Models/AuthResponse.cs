namespace Blend.Api.Auth.Models;
public sealed record AuthResponse(string AccessToken, string TokenType = "Bearer");
