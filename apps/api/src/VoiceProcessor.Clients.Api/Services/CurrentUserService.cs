using System.Security.Claims;

namespace VoiceProcessor.Clients.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var userIdClaim = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public string? Tier => User?.FindFirstValue("tier");

    public string? Name => User?.FindFirstValue("name");

    public string? AuthMethod => User?.FindFirstValue("auth_method");

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
