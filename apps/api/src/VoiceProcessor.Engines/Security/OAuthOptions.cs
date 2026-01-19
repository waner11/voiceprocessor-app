namespace VoiceProcessor.Engines.Security;

public class OAuthOptions
{
    public const string SectionName = "OAuth";

    public GoogleOAuthOptions Google { get; set; } = new();
    public GitHubOAuthOptions GitHub { get; set; } = new();
}

public class GoogleOAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = "https://accounts.google.com/o/oauth2/v2/auth";
    public string TokenEndpoint { get; set; } = "https://oauth2.googleapis.com/token";
    public string UserInfoEndpoint { get; set; } = "https://openidconnect.googleapis.com/v1/userinfo";
    public string Scope { get; set; } = "openid email profile";
}

public class GitHubOAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = "https://github.com/login/oauth/authorize";
    public string TokenEndpoint { get; set; } = "https://github.com/login/oauth/access_token";
    public string UserEndpoint { get; set; } = "https://api.github.com/user";
    public string UserEmailsEndpoint { get; set; } = "https://api.github.com/user/emails";
    public string Scope { get; set; } = "user:email";
}
