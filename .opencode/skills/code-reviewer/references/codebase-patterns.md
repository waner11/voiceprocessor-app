# VoiceProcessor Codebase Patterns

Enforced conventions specific to this project. Deviations require justification.

## Project Structure

```
apps/api/src/
├── VoiceProcessor.Clients.Api/     # Controllers, Hubs, Program.cs
├── VoiceProcessor.Managers/        # Orchestration layer
├── VoiceProcessor.Engines/         # Business logic layer
├── VoiceProcessor.Accessors/       # Resource access layer
├── VoiceProcessor.Utilities/       # Cross-cutting concerns
└── VoiceProcessor.Domain/          # Shared types (Entities, DTOs, Enums)
```

Each layer is a separate .NET project with explicit dependency references.

## Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Namespace | File-scoped | `namespace VoiceProcessor.Managers;` |
| Interface | `I{Domain}{Layer}` | `IGenerationManager`, `IRoutingEngine` |
| Class | `{Domain}{Layer}` | `GenerationManager`, `RoutingEngine` |
| Private field | `_camelCase` | `_generationAccessor`, `_logger` |
| Async method | `{Verb}{Noun}Async` | `CreateGenerationAsync`, `GetByIdAsync` |
| Request DTO | `{Verb}{Noun}Request` | `CreateGenerationRequest` |
| Response DTO | `{Noun}Response` | `GenerationResponse` |
| Options class | `{Feature}Options` | `PricingOptions`, `JwtOptions` |
| Options section | `const string SectionName` | `public const string SectionName = "Pricing";` |

## DI Registration Patterns

Each layer has its own `ServiceCollectionExtensions.cs` in a `DependencyInjection/` folder:

```csharp
// Pattern: Extension method per layer
public static class ServiceCollectionExtensions
{
    public static IServiceCollection Add{Layer}s(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register components
        return services;
    }
}
```

**Registration in Program.cs** (order matters):
```csharp
builder.Services.AddAccessors(builder.Configuration);   // First: no dependencies
builder.Services.AddEngines(builder.Configuration);      // Second: may depend on Accessors
builder.Services.AddManagers(builder.Configuration);     // Third: depends on both
```

**Lifetimes**:
- Stateless Engines: `Singleton`
- Stateful Engines (with `IOptions<>`): `Scoped`
- Managers: `Scoped`
- Accessors (DB): `Scoped`
- Accessors (HTTP): `AddHttpClient<>` with Polly policies

## Constructor Injection Pattern

```csharp
public class GenerationManager : IGenerationManager
{
    private readonly IGenerationAccessor _generationAccessor;
    private readonly IChunkingEngine _chunkingEngine;
    private readonly ILogger<GenerationManager> _logger;

    public GenerationManager(
        IGenerationAccessor generationAccessor,
        IChunkingEngine chunkingEngine,
        ILogger<GenerationManager> logger)
    {
        _generationAccessor = generationAccessor;
        _chunkingEngine = chunkingEngine;
        _logger = logger;
    }
}
```

Rules:
- Always `private readonly`
- Constructor assigns only, no logic
- Order: Accessors, Engines, Infrastructure (logger, options)

## Async / CancellationToken Pattern

```csharp
// Interface: default parameter
Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

// Implementation: propagate everywhere
public async Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
{
    return await _dbContext.Entities
        .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
}
```

Rules:
- Every async method accepts `CancellationToken` as last parameter
- Default value is `default` (not `CancellationToken.None`)
- Always propagate to downstream calls
- Engines can be synchronous when doing pure computation

## DTO Pattern

```csharp
// Records for immutability
public record CreateGenerationRequest
{
    public required string Text { get; init; }        // Required fields: 'required' keyword
    public required Guid VoiceId { get; init; }
    public string? AudioFormat { get; init; } = "mp3"; // Optional with defaults
}

// Context records for inter-layer communication
public record PricingContext
{
    public required int CharacterCount { get; init; }
    public Provider? Provider { get; init; }
}
```

Rules:
- Use `record` types for DTOs
- Use `required` for mandatory fields
- Use `init` setters for immutability
- Optional fields use nullable (`?`) with sensible defaults
- Request/Response DTOs in `VoiceProcessor.Domain/DTOs/`
- Context records in Engine `Contracts/` alongside their interface

## Error Handling Pattern

### Manager: Throw specific exceptions
```csharp
// Validation-style errors
throw new InvalidOperationException($"Voice {request.VoiceId} not found");
throw new InvalidOperationException("Insufficient credits");

// Graceful degradation for non-critical failures
try
{
    var routing = await _routingEngine.SelectProviderAsync(context, ct);
    provider = routing.SelectedProvider;
}
catch (Exception ex)
{
    _logger.LogWarning(ex, "Could not determine recommended provider");
    // Continue with fallback
}
```

### Controller: Map exceptions to HTTP status codes
```csharp
catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
{
    return NotFound(new ErrorResponse { Code = "NOT_FOUND", Message = ex.Message });
}
catch (InvalidOperationException ex) when (ex.Message.Contains("Insufficient credits"))
{
    return StatusCode(402, new ErrorResponse { Code = "INSUFFICIENT_CREDITS", Message = ex.Message });
}
```

### ErrorResponse format
```csharp
public record ErrorResponse
{
    public string Code { get; set; } = string.Empty;   // Machine-readable
    public string Message { get; set; } = string.Empty; // Human-readable
}
```

## Logging Pattern (Serilog)

```csharp
// CORRECT: Structured logging with named parameters
_logger.LogInformation("Creating generation for user {UserId}, {CharCount} characters",
    userId, request.Text.Length);

_logger.LogWarning(ex, "Could not determine recommended provider for {VoiceId}", voiceId);

_logger.LogError("ElevenLabs TTS failed: {StatusCode} - {Error}",
    response.StatusCode, error);

// WRONG: String interpolation
_logger.LogInformation($"Creating generation for user {userId}");
```

Rules:
- **Information**: Key business events (workflow start/end, state transitions)
- **Warning**: Recoverable issues (retries, fallbacks, degraded functionality)
- **Error**: Failures requiring attention (external API errors, unexpected state)
- Named parameters use `PascalCase` in templates
- Include relevant IDs for traceability

## Polly Resilience Pattern (Accessors)

```csharp
services.AddHttpClient<ElevenLabsAccessor>(client => { /* config */ })
    .AddPolicyHandler(GetRetryPolicy())           // 3x exponential backoff
    .AddPolicyHandler(GetCircuitBreakerPolicy());  // Break after 5 failures

private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
```

## Options Pattern

```csharp
public class PricingOptions
{
    public const string SectionName = "Pricing";  // Maps to appsettings section

    public string Currency { get; set; } = "USD";
    public decimal CostPerCredit { get; set; } = 0.01m;
}

// Registration
services.Configure<PricingOptions>(configuration.GetSection(PricingOptions.SectionName));

// Injection
public PricingEngine(IOptions<PricingOptions> options)
{
    _options = options.Value;
}
```

## Entity Pattern

```csharp
public class Generation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public GenerationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<GenerationChunk> Chunks { get; set; } = [];
}
```

Rules:
- Entities in `VoiceProcessor.Domain/Entities/`
- Enums in `VoiceProcessor.Domain/Enums/`
- Navigation properties initialized with `null!` or `[]`
- Use `DateTime.UtcNow` always (never `DateTime.Now`)

## Anti-Patterns to Flag

| Anti-Pattern | Severity |
|-------------|----------|
| `as any` / type suppression | BLOCKING |
| `catch (Exception) { }` empty catch | BLOCKING |
| Business logic in Controller | MAJOR |
| Business logic in Accessor | MAJOR |
| Manager iterating/computing over data | MAJOR |
| Engine calling another Engine | BLOCKING |
| Manager calling another Manager (sync) | BLOCKING |
| Accessor calling Engine/Manager | BLOCKING |
| Missing `CancellationToken` propagation | MAJOR |
| String interpolation in log templates | MINOR |
| `DateTime.Now` instead of `DateTime.UtcNow` | MAJOR |
| `Task.Run` in ASP.NET Core request pipeline | MAJOR |
| `.Result` or `.Wait()` on async code | BLOCKING |
| Missing `await` (fire-and-forget without intent) | BLOCKING |
| `public` fields on classes | MINOR |
| Mutable DTOs (non-record, with setters) | MINOR |
