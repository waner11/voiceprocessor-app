using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoiceProcessor.Engines.Contracts;

namespace VoiceProcessor.Engines.Security;

public class GitHubOAuthEngine : IOAuthEngine
{
    private readonly GitHubOAuthOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubOAuthEngine> _logger;

    public string Provider => "GitHub";

    public GitHubOAuthEngine(
        IOptions<OAuthOptions> options,
        HttpClient httpClient,
        ILogger<GitHubOAuthEngine> logger)
    {
        _options = options.Value.GitHub;
        _httpClient = httpClient;
        _logger = logger;
    }

    public string GetAuthorizationUrl(string state, string redirectUri)
    {
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams["client_id"] = _options.ClientId;
        queryParams["redirect_uri"] = redirectUri;
        queryParams["scope"] = _options.Scope;
        queryParams["state"] = state;

        return $"{_options.AuthorizationEndpoint}?{queryParams}";
    }

    public async Task<OAuthUserInfo> ExchangeCodeAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        // Exchange code for access token
        var tokenRequest = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = redirectUri
        };

        using var tokenRequestMessage = new HttpRequestMessage(HttpMethod.Post, _options.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(tokenRequest)
        };
        tokenRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var tokenResponse = await _httpClient.SendAsync(tokenRequestMessage, cancellationToken);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            var error = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("GitHub token exchange failed: {Error}", error);
            throw new InvalidOperationException("Failed to exchange authorization code");
        }

        var tokens = await tokenResponse.Content.ReadFromJsonAsync<GitHubTokenResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Invalid token response from GitHub");

        if (!string.IsNullOrEmpty(tokens.Error))
        {
            _logger.LogError("GitHub OAuth error: {Error} - {Description}", tokens.Error, tokens.ErrorDescription);
            throw new InvalidOperationException(tokens.ErrorDescription ?? tokens.Error);
        }

        // Get user info
        using var userRequest = new HttpRequestMessage(HttpMethod.Get, _options.UserEndpoint);
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        userRequest.Headers.Add("User-Agent", "VoiceProcessor-API");

        var userResponse = await _httpClient.SendAsync(userRequest, cancellationToken);

        if (!userResponse.IsSuccessStatusCode)
        {
            var error = await userResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("GitHub user request failed: {Error}", error);
            throw new InvalidOperationException("Failed to get user info from GitHub");
        }

        var userInfo = await userResponse.Content.ReadFromJsonAsync<GitHubUserInfo>(cancellationToken)
            ?? throw new InvalidOperationException("Invalid user info response from GitHub");

        // Get primary email if not public
        var email = userInfo.Email;
        if (string.IsNullOrEmpty(email))
        {
            email = await GetPrimaryEmailAsync(tokens.AccessToken, cancellationToken);
        }

        if (string.IsNullOrEmpty(email))
        {
            throw new InvalidOperationException("Email not provided by GitHub. Please make your email public or grant email permission.");
        }

        _logger.LogInformation("GitHub OAuth successful for user {Email}", email);

        return new OAuthUserInfo(
            ProviderUserId: userInfo.Id.ToString(),
            Email: email,
            Name: userInfo.Name ?? userInfo.Login
        );
    }

    private async Task<string?> GetPrimaryEmailAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var emailRequest = new HttpRequestMessage(HttpMethod.Get, _options.UserEmailsEndpoint);
        emailRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        emailRequest.Headers.Add("User-Agent", "VoiceProcessor-API");

        var emailResponse = await _httpClient.SendAsync(emailRequest, cancellationToken);

        if (!emailResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var emails = await emailResponse.Content.ReadFromJsonAsync<List<GitHubEmail>>(cancellationToken);
        return emails?.FirstOrDefault(e => e.Primary)?.Email
            ?? emails?.FirstOrDefault(e => e.Verified)?.Email;
    }

    private class GitHubTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("error_description")]
        public string? ErrorDescription { get; set; }
    }

    private class GitHubUserInfo
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("login")]
        public string Login { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; }
    }

    private class GitHubEmail
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("primary")]
        public bool Primary { get; set; }

        [JsonPropertyName("verified")]
        public bool Verified { get; set; }
    }
}
