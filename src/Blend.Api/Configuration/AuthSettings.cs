namespace Blend.Api.Configuration;

public class AuthSettings
{
    public GoogleOAuthSettings Google { get; set; } = new();
    public FacebookOAuthSettings Facebook { get; set; } = new();
    public TwitterSettings Twitter { get; set; } = new();
}

public class GoogleOAuthSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class FacebookOAuthSettings
{
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
}

public class TwitterSettings
{
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
}
