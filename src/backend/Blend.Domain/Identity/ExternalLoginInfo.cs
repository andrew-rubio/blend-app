using System.Text.Json.Serialization;

namespace Blend.Domain.Identity;

/// <summary>Represents a social/external login provider entry stored within a <see cref="BlendUser"/>.</summary>
public sealed class ExternalLoginInfo
{
    /// <summary>The login provider name, e.g. "Google" or "Facebook".</summary>
    [JsonPropertyName("loginProvider")]
    public string LoginProvider { get; set; } = string.Empty;

    /// <summary>The unique key issued by the provider for this user.</summary>
    [JsonPropertyName("providerKey")]
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>Human-readable provider display name, e.g. "Google".</summary>
    [JsonPropertyName("providerDisplayName")]
    public string? ProviderDisplayName { get; set; }
}
