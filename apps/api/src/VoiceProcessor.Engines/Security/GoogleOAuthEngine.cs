using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoiceProcessor.Engines.Contracts;

namespace VoiceProcessor.Engines.Security;

public class GoogleOAuthEngine : IOAuthEngine
{
    private readonly GoogleOAuthOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleOAuthEngine> _logger;

    public string Provider => "Google";

    public GoogleOAuthEngine(
        IOptions<OAuthOptions> options,
        HttpClient httpClient,
        ILogger<GoogleOAuthEngine> logger)
    {
        _options = options.Value.Google;
        _httpClient = httpClient;
        _logger = logger;
    }

    public string GetAuthorizationUrl(string state, string redirectUri)
    {
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        queryParams["client_id"] = _options.ClientId;
        queryParams["redirect_uri"] = redirectUri;
        queryParams["response_type"] = "code";
        queryParams["scope"] = _options.Scope;
        queryParams["state"] = state;
        queryParams["access_type"] = "offline";
        queryParams["prompt"] = "consent";

        return $"{_options.AuthorizationEndpoint}?{queryParams}";
    }

    public async Task<OAuthUserInfo> ExchangeCodeAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        // Exchange code for tokens
        var tokenRequest = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code"
        };

        var tokenResponse = await _httpClient.PostAsync(
            _options.TokenEndpoint,
            new FormUrlEncodedContent(tokenRequest),
            cancellationToken);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            var error = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Google token exchange failed: {Error}", error);
            throw new InvalidOperationException("Failed to exchange authorization code");
        }

        var tokens = await tokenResponse.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Invalid token response from Google");

        // Get user info
        using var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, _options.UserInfoEndpoint);
        userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var userInfoResponse = await _httpClient.SendAsync(userInfoRequest, cancellationToken);

        if (!userInfoResponse.IsSuccessStatusCode)
        {
            var error = await userInfoResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Google user info request failed: {Error}", error);
            throw new InvalidOperationException("Failed to get user info from Google");
        }

        var userInfo = await userInfoResponse.Content.ReadFromJsonAsync<GoogleUserInfo>(cancellationToken)
            ?? throw new InvalidOperationException("Invalid user info response from Google");

        if (string.IsNullOrEmpty(userInfo.Email))
        {
            throw new InvalidOperationException("Email not provided by Google");
        }

        _logger.LogInformation("Google OAuth successful for user {Email}", userInfo.Email);

        return new OAuthUserInfo(
            ProviderUserId: userInfo.Sub,
            Email: userInfo.Email,
            Name: userInfo.Name
        );
    }

    private class GoogleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }
    }

    private class GoogleUserInfo
    {
        [JsonPropertyName("sub")]
        public string Sub { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("picture")]
        public string? Picture { get; set; }
    }
}
