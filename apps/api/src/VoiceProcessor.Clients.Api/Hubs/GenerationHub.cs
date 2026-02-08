using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using VoiceProcessor.Domain.Contracts.Hubs;

namespace VoiceProcessor.Clients.Api.Hubs;

[Authorize]
public class GenerationHub : Hub<IGenerationClient>
{
    private readonly ILogger<GenerationHub> _logger;

    public GenerationHub(ILogger<GenerationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected to GenerationHub. ConnectionId: {ConnectionId}, UserId: {UserId}",
            Context.ConnectionId, Context.UserIdentifier);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected from GenerationHub. ConnectionId: {ConnectionId}, UserId: {UserId}",
            Context.ConnectionId, Context.UserIdentifier);
        await base.OnDisconnectedAsync(exception);
    }
}
