using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoiceProcessor.Clients.Api.Services;
using VoiceProcessor.Domain.DTOs.Responses;
using VoiceProcessor.Managers.Contracts;

namespace VoiceProcessor.Clients.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsageController : ControllerBase
{
    private readonly IAuthManager _authManager;
    private readonly ICurrentUserService _currentUser;

    public UsageController(
        IAuthManager authManager,
        ICurrentUserService currentUser)
    {
        _authManager = authManager;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Get current user's usage statistics
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(UsageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUsage(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        try
        {
            var response = await _authManager.GetUsageAsync(userId, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new ErrorResponse { Code = "USER_NOT_FOUND", Message = ex.Message });
        }
    }
}
