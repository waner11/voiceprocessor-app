using Microsoft.AspNetCore.Mvc;

namespace VoiceProcessor.Clients.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    // Common functionality for all controllers can be added here
    // For example: getting current user ID, handling common responses, etc.
}
