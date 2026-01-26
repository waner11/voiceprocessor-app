using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VoiceProcessor.Clients.Api.Extensions;
using VoiceProcessor.Clients.Api.Services;
using VoiceProcessor.Domain.DTOs.Requests.Auth;
using VoiceProcessor.Domain.DTOs.Responses.Auth;
using VoiceProcessor.Engines.Security;
using VoiceProcessor.Managers.Contracts;

namespace VoiceProcessor.Clients.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthManager _authManager;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AuthController> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly JwtOptions _jwtOptions;

    public AuthController(
        IAuthManager authManager,
        ICurrentUserService currentUser,
        ILogger<AuthController> logger,
        IWebHostEnvironment environment,
        IOptions<JwtOptions> jwtOptions)
    {
        _authManager = authManager;
        _currentUser = currentUser;
        _logger = logger;
        _environment = environment;
        _jwtOptions = jwtOptions.Value;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authManager.RegisterAsync(request, cancellationToken);
            
            Response.SetAuthCookies(
                response.AccessToken,
                response.RefreshToken,
                _jwtOptions.AccessTokenExpirationMinutes,
                _jwtOptions.RefreshTokenExpirationDays,
                _environment.IsDevelopment()
            );
            
            return CreatedAtAction(nameof(GetCurrentUser), response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already registered"))
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var ipAddress = GetClientIpAddress();
            var response = await _authManager.LoginAsync(request, ipAddress, cancellationToken);
            
            Response.SetAuthCookies(
                response.AccessToken,
                response.RefreshToken,
                _jwtOptions.AccessTokenExpirationMinutes,
                _jwtOptions.RefreshTokenExpirationDays,
                _environment.IsDevelopment()
            );
            
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Refresh access token using a refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Try to get refresh token from request body first
            var refreshToken = request.RefreshToken;
            
            // If empty, try to get from cookie
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                Request.Cookies.TryGetValue(
                    Extensions.AuthCookieExtensions.RefreshTokenCookieName, 
                    out refreshToken);
            }
            
            // If still empty, return 400 Bad Request
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(new { error = "Refresh token is required" });
            }
            
            var ipAddress = GetClientIpAddress();
            var response = await _authManager.RefreshTokenAsync(
                refreshToken, ipAddress, cancellationToken);
            
            Response.SetAuthCookies(
                response.AccessToken,
                response.RefreshToken,
                _jwtOptions.AccessTokenExpirationMinutes,
                _jwtOptions.RefreshTokenExpirationDays,
                _environment.IsDevelopment()
            );
            
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Logout and revoke refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenRequest? request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        await _authManager.LogoutAsync(userId, request?.RefreshToken, cancellationToken);
        
        Response.ClearAuthCookies();
        
        return NoContent();
    }

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
        {
            return Unauthorized();
        }

        var response = new UserInfoResponse
        {
            Id = _currentUser.UserId.Value,
            Email = _currentUser.Email ?? string.Empty,
            Name = _currentUser.Name,
            Tier = Enum.TryParse<VoiceProcessor.Domain.Enums.SubscriptionTier>(
                _currentUser.Tier, out var tier) ? tier : Domain.Enums.SubscriptionTier.Free,
            CreditsRemaining = 0 // This would need to be fetched from the user manager
        };

        return Ok(response);
    }

    /// <summary>
    /// Create a new API key
    /// </summary>
    [HttpPost("api-keys")]
    [Authorize]
    [ProducesResponseType(typeof(ApiKeyCreatedResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateApiKey(
        [FromBody] CreateApiKeyRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var response = await _authManager.CreateApiKeyAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetApiKeys), new { id = response.Id }, response);
    }

    /// <summary>
    /// Get all API keys for the current user
    /// </summary>
    [HttpGet("api-keys")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<ApiKeyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetApiKeys(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var keys = await _authManager.GetUserApiKeysAsync(userId, cancellationToken);
        return Ok(keys);
    }

    /// <summary>
    /// Revoke an API key
    /// </summary>
    [HttpDelete("api-keys/{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeApiKey(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        try
        {
            await _authManager.RevokeApiKeyAsync(userId, id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { error = "API key not found" });
        }
    }

    #region OAuth Endpoints

    /// <summary>
    /// Get OAuth authorization URL for a provider
    /// </summary>
    [HttpGet("oauth/{provider}/url")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OAuthUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetOAuthUrl(
        string provider,
        [FromQuery] string redirectUri)
    {
        if (string.IsNullOrWhiteSpace(redirectUri))
        {
            return BadRequest(new { error = "redirectUri is required" });
        }

        try
        {
            var response = _authManager.GetOAuthUrl(provider, redirectUri);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Exchange OAuth authorization code for JWT tokens
    /// </summary>
    [HttpPost("oauth/{provider}/callback")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OAuthLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> OAuthCallback(
        string provider,
        [FromBody] OAuthLoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var ipAddress = GetClientIpAddress();
            var response = await _authManager.OAuthLoginAsync(
                provider, request.Code, request.RedirectUri, ipAddress, cancellationToken);
            
            Response.SetAuthCookies(
                response.AccessToken,
                response.RefreshToken,
                _jwtOptions.AccessTokenExpirationMinutes,
                _jwtOptions.RefreshTokenExpirationDays,
                _environment.IsDevelopment()
            );
            
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "OAuth provider error for {Provider}", provider);
            return Unauthorized(new { error = "Failed to authenticate with provider" });
        }
    }

    /// <summary>
    /// Get list of OAuth providers linked to the current user
    /// </summary>
    [HttpGet("oauth/providers")]
    [Authorize]
    [ProducesResponseType(typeof(LinkedProvidersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLinkedProviders(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var response = await _authManager.GetLinkedProvidersAsync(userId, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Link an OAuth provider to the current user's account
    /// </summary>
    [HttpPost("oauth/{provider}/link")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> LinkOAuthProvider(
        string provider,
        [FromBody] OAuthLoginRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        try
        {
            await _authManager.LinkOAuthAsync(
                userId, provider, request.Code, request.RedirectUri, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already linked"))
        {
            return Conflict(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "OAuth provider error while linking {Provider}", provider);
            return BadRequest(new { error = "Failed to authenticate with provider" });
        }
    }

    /// <summary>
    /// Unlink an OAuth provider from the current user's account
    /// </summary>
    [HttpDelete("oauth/{provider}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkOAuthProvider(
        string provider,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        try
        {
            await _authManager.UnlinkOAuthAsync(userId, provider, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not linked"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    #endregion

    private string? GetClientIpAddress()
    {
        // Check for forwarded header first (behind proxy/load balancer)
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').First().Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}
