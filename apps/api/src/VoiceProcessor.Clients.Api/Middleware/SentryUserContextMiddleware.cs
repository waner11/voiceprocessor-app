using Sentry;
using VoiceProcessor.Clients.Api.Services;

namespace VoiceProcessor.Clients.Api.Middleware;

public class SentryUserContextMiddleware
{
    private readonly RequestDelegate _next;

    public SentryUserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var currentUserService = context.RequestServices.GetRequiredService<ICurrentUserService>();

        SentrySdk.ConfigureScope(scope =>
        {
            if (currentUserService.IsAuthenticated)
            {
                scope.User = new SentryUser
                {
                    Id = currentUserService.UserId?.ToString(),
                    Email = currentUserService.Email,
                    Other = new Dictionary<string, string>
                    {
                        { "Tier", currentUserService.Tier ?? "unknown" },
                        { "AuthMethod", currentUserService.AuthMethod ?? "unknown" }
                    }
                };
            }
            else
            {
                scope.User = null;
            }
        });

        await _next(context);
    }
}
