using System.Text.Json.Serialization;

namespace Blend.Domain.Identity;

/// <summary>
/// ASP.NET Core Identity role used by the custom <c>CosmosRoleStore</c>.
/// Roles are predefined (User, Admin) and held in-memory — no Cosmos persistence needed.
/// </summary>
public sealed class BlendRole
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("normalizedName")]
    public string NormalizedName { get; set; } = string.Empty;

    [JsonPropertyName("concurrencyStamp")]
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
}
