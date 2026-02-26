namespace Blend.Api.Domain;

public class ExternalLogin
{
    public string Provider { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
