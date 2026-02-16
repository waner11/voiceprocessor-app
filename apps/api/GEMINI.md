# GEMINI.md

This file provides guidance to Google Gemini when working with code in this repository.

## Project Overview

VoiceProcessor API is a multi-provider Text-to-Speech SaaS platform built with ASP.NET Core 8 / C# 12. It abstracts multiple TTS providers (ElevenLabs, OpenAI, Google Cloud TTS, Amazon Polly, Fish Audio, Cartesia) behind a unified API with intelligent routing, text chunking, and cost optimization.

**Current Status:** Design phase - comprehensive documentation exists but source code has not yet been implemented.

## Architecture

This project uses **iDesign Method** (Volatility-Based Decomposition) by Juval Löwy. See `../../docs/IDESIGN_ARCHITECTURE.md` for full details.

### Service Layers
```
Clients    → Controllers, SignalR hubs (no business logic)
Managers   → Workflow orchestration (GenerationManager, VoiceManager)
Engines    → Business rules/logic (RoutingEngine, ChunkingEngine, PricingEngine)
Accessors  → External resources (DB, TTS APIs, Storage, OpenAI)
Utilities  → Cross-cutting (Logging, Caching, Config)
Domain     → Shared entities, DTOs, enums
```

### Project Structure
```
src/
├── VoiceProcessor.Clients.Api/      # Controllers, hubs, middleware
├── VoiceProcessor.Managers/         # Workflow orchestration
├── VoiceProcessor.Engines/          # Business rules (stateless)
├── VoiceProcessor.Accessors/        # DB, TTS providers, storage, AI
├── VoiceProcessor.Utilities/        # Logging, caching, config
└── VoiceProcessor.Domain/           # Entities, DTOs, enums
```

### Calling Rules
```
✓ Client → Manager → Engine → Accessor → Utility
✗ Never call sideways (Manager→Manager sync, Engine→Engine)
✗ Never call upward (Accessor→Engine, Engine→Manager)
```

### Key Components
- **GenerationManager** - Orchestrates TTS workflow (analyze → chunk → route → generate → merge)
- **RoutingEngine** - Selects optimal provider based on cost/quality/speed
- **ChunkingEngine** - Splits text into optimal segments
- **TextAnalysisEngine** - Content classification, emotion detection
- **TTSProviderAccessor** - Wraps external TTS APIs (ElevenLabs, OpenAI, etc.)

## Development Commands

```bash
# Start infrastructure (PostgreSQL + Redis)
docker-compose up -d db redis

# Restore packages
dotnet restore

# Run the API
dotnet run --project src/VoiceProcessor.Clients.Api

# Run with hot reload
dotnet watch --project src/VoiceProcessor.Clients.Api

# Build
dotnet build

# Run tests
dotnet test

# Add EF migration
dotnet ef migrations add <name> --project src/VoiceProcessor.Accessors --startup-project src/VoiceProcessor.Clients.Api

# Apply migrations
dotnet ef database update --project src/VoiceProcessor.Accessors --startup-project src/VoiceProcessor.Clients.Api
```

## Git Guidelines

- **No AI attribution** - Do not add "Co-Authored-By: Gemini" or any AI co-author tags to commits
- **Write natural commit messages** - Write like a human developer, not like AI-generated text
- Keep messages concise and direct
- Use lowercase, no periods at the end of subject lines
- Avoid overly formal or verbose language

Good examples:
```
fix null check in chunking service
add elevenlabs provider implementation
update routing logic for cost optimization
```

Bad examples (too AI-sounding):
```
Implement robust error handling mechanism for the chunking service
Add comprehensive ElevenLabs provider with full feature support
Refactor and enhance the routing logic for improved cost optimization
```

### Branching Workflow

See **AGENTS.md Section 4** for the complete git workflow including:
- Starting work (sync main, create branch, claim issue)
- During work (commit conventions)
- Finishing work (rebase, PR creation, close issue)

**Reference**: See `../../WORKFLOW.md` for detailed workflow instructions

## Technology Stack

| Component | Technology |
|-----------|------------|
| Backend | ASP.NET Core 8 / C# 12 (LTS until Nov 2026) |
| Database | PostgreSQL 16 with pgvector extension |
| Cache | Redis 7 |
| Background Jobs | Hangfire |
| Storage | Azure Blob / Cloudflare R2 (S3-compatible) |
| ML/AI | OpenAI API (GPT-4o-mini for text analysis) |
| Real-time | SignalR |

## Logging & Observability

**Every operation must be traceable in production.** Use structured logging with Serilog.

### Log Levels
| Level | When to use |
|-------|-------------|
| `Debug` | Detailed diagnostic info (disabled in prod) |
| `Information` | Key business events: generation started, completed, provider selected |
| `Warning` | Recoverable issues: retry triggered, fallback used, rate limit hit |
| `Error` | Failures requiring attention: provider error, chunk failed, DB error |
| `Fatal` | App cannot continue: startup failure, critical config missing |

### What to Log
**Always include:**
- `GenerationId`, `UserId`, `ChunkId` for correlation
- Operation name and duration
- Provider name for external calls
- Error details with stack trace

**Managers:** Log workflow start/end, key decisions, total duration
```csharp
_logger.LogInformation("Generation {GenerationId} started for user {UserId}, {CharCount} chars",
    request.Id, request.UserId, request.Text.Length);
```

**Engines:** Log inputs, outputs, and computation results
```csharp
_logger.LogInformation("Routing selected {Provider} for {GenerationId} (score: {Score})",
    result.Provider, generationId, result.Score);
```

**Accessors:** Log external calls with timing, retries, and responses
```csharp
_logger.LogInformation("TTS request to {Provider} completed in {ElapsedMs}ms, {AudioBytes} bytes",
    ProviderName, sw.ElapsedMilliseconds, result.AudioData.Length);
```

### Exception Handling

**Layer-specific exceptions:**
```
VoiceProcessorException (base)
├── ProviderException        - TTS provider failures
├── ValidationException      - Invalid input
├── RateLimitException       - User/provider rate limits
├── InsufficientCreditsException
└── GenerationException      - Processing failures
```

**Rules:**
- Catch specific exceptions, not `Exception`
- Log at the point of failure with full context
- Rethrow or wrap with additional context
- Let global handler catch unhandled exceptions
- Never swallow exceptions silently

**Global exception middleware logs:**
- Full exception with stack trace
- Request path, method, user ID
- Correlation ID for tracing

### Metrics to Track
- Generation count, success/failure rate
- Provider latency (p50, p95, p99)
- Chunk processing time
- Cost per generation
- Queue depth and processing lag
- Active users, concurrent generations

## Key NuGet Packages

- `Serilog.AspNetCore` - Structured logging
- `Serilog.Sinks.Console` - Dev logging
- `Serilog.Sinks.Seq` - Production log aggregation (or use cloud provider)
- `Npgsql.EntityFrameworkCore.PostgreSQL` - PostgreSQL EF Core provider
- `Pgvector.EntityFrameworkCore` - Vector embeddings for ML features
- `Hangfire.PostgreSql` - Background job storage
- `Azure.AI.OpenAI` - OpenAI integration
- `FFMpegCore` + `ffmpeg` - Audio processing/merging
- `FluentValidation.AspNetCore` - Request validation

## Environment Variables

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Redis__ConnectionString` | Redis connection (default: `localhost:6379`) |
| `ElevenLabs__ApiKey` | ElevenLabs TTS API key |
| `OpenAI__ApiKey` | OpenAI API key (for analysis + TTS) |
| `Google__CredentialsPath` | Google Cloud credentials JSON path |

## API Endpoints (Planned)

```
POST /api/v1/generations/estimate  - Cost estimation
POST /api/v1/generations/preview   - Preview (first 500 chars)
POST /api/v1/generations           - Start async generation
GET  /api/v1/generations/{id}      - Get status/result
POST /api/v1/generations/{id}/feedback - Submit rating
GET  /api/v1/voices                - List available voices
```

## Accessor Interface Pattern

All TTS provider accessors implement:
```csharp
public interface ITTSProviderAccessor
{
    string ProviderName { get; }
    Task<TTSResult> GenerateAsync(string text, string voiceId, TTSOptions options, CancellationToken ct);
    Task<IReadOnlyList<Voice>> GetVoicesAsync(CancellationToken ct);
    Task<HealthStatus> CheckHealthAsync(CancellationToken ct);
}
```

## Database Schema (Core Tables)

- `users` - User accounts, subscription tier, credits
- `generations` - Generation jobs with status, provider used, cost, audio URL
- `generation_chunks` - Individual chunk processing records
- `feedback` - User ratings and implicit feedback (downloads, playback)
- `voices` - Universal voice catalog with provider mappings

## Issue Tracking

This project uses **bd** (beads) for issue tracking:
```bash
bd ready              # Find available work
bd show <id>          # View issue details
bd update <id> --status in_progress
bd close <id>         # Complete work
bd sync               # Sync with git
```

### Always Add Context to Issues

**When creating issues**, always include a description:
```bash
bd create --title="Implement Manager classes" --type=task --priority=2 \
  --description="Implement IGenerationManager, IVoiceManager, IUserManager following iDesign patterns."
```

**When closing issues**, add a reason and update notes:
```bash
bd close <id> --reason="Implemented with full test coverage"
bd update <id> --notes="Completed: All managers implemented and registered in DI."
```

This ensures future sessions have context about what was done and why.

## Gemini-Specific Guidance

### Code Generation Best Practices

When generating C# code for this project:

1. **Always use file-scoped namespaces**:
   ```csharp
   namespace VoiceProcessor.Managers;
   
   public class GenerationManager : IGenerationManager
   {
       // Implementation
   }
   ```

2. **Enable nullable reference types** and handle nulls explicitly:
   ```csharp
   public async Task<Generation?> GetGenerationAsync(Guid id, CancellationToken ct)
   {
       var generation = await _context.Generations.FindAsync(id, ct);
       return generation; // Can be null
   }
   ```

3. **Use constructor injection** with private readonly fields:
   ```csharp
   private readonly ILogger<GenerationManager> _logger;
   private readonly IGenerationAccessor _generationAccessor;
   
   public GenerationManager(
       ILogger<GenerationManager> logger,
       IGenerationAccessor generationAccessor)
   {
       _logger = logger;
       _generationAccessor = generationAccessor;
   }
   ```

4. **Always pass CancellationToken** to async methods:
   ```csharp
   public async Task<Generation> CreateGenerationAsync(
       CreateGenerationRequest request,
       CancellationToken ct)
   {
       // Implementation
   }
   ```

5. **Use structured logging** (never string interpolation):
   ```csharp
   // GOOD
   _logger.LogInformation("Processing chunk {ChunkId} for generation {GenerationId}", 
       chunkId, generationId);
   
   // BAD
   _logger.LogInformation($"Processing chunk {chunkId} for generation {generationId}");
   ```

### Testing Patterns

When generating tests:

1. **Use xUnit** with clear test names:
   ```csharp
   [Fact]
   public async Task CreateGeneration_WithValidRequest_ShouldSucceed()
   {
       // Arrange
       var request = new CreateGenerationRequest { /* ... */ };
       
       // Act
       var result = await _manager.CreateGenerationAsync(request, CancellationToken.None);
       
       // Assert
       Assert.NotNull(result);
       Assert.Equal(GenerationStatus.Pending, result.Status);
   }
   ```

2. **Mock dependencies** using Moq or NSubstitute:
   ```csharp
   private readonly Mock<IGenerationAccessor> _mockAccessor;
   private readonly Mock<ILogger<GenerationManager>> _mockLogger;
   
   public GenerationManagerTests()
   {
       _mockAccessor = new Mock<IGenerationAccessor>();
       _mockLogger = new Mock<ILogger<GenerationManager>>();
   }
   ```

### Common Pitfalls to Avoid

1. **Don't violate iDesign calling rules**:
   - ❌ Manager calling another Manager directly
   - ❌ Engine calling another Engine directly
   - ❌ Accessor calling Engine or Manager
   - ✅ Use dependency injection and interfaces

2. **Don't put business logic in Controllers**:
   - ❌ Controllers should only validate, call Managers, and return responses
   - ✅ All business logic belongs in Engines

3. **Don't catch generic Exception**:
   - ❌ `catch (Exception ex)`
   - ✅ `catch (ProviderException ex)` or specific exceptions

4. **Don't use async void**:
   - ❌ `public async void ProcessChunk()`
   - ✅ `public async Task ProcessChunkAsync(CancellationToken ct)`

### Multi-Step Tasks

When working on complex features:

1. **Read documentation first**: Check `../../docs/IDESIGN_ARCHITECTURE.md` and `docs/TECHNICAL_DESIGN.md`
2. **Plan the layers**: Identify which Managers, Engines, and Accessors are needed
3. **Start from the bottom up**: Implement Accessors → Engines → Managers → Controllers
4. **Write tests alongside**: Don't wait until the end to add tests
5. **Verify iDesign compliance**: Ensure calling rules are followed

### Documentation

- `../../docs/IDESIGN_ARCHITECTURE.md` - **iDesign method guide, calling rules, project structure**
- `docs/TECHNICAL_DESIGN.md` - System architecture, API design, data flows
- `docs/TECHNOLOGY_STACK_DOTNET.md` - Implementation patterns, code examples
- `../../docs/MARKET_ANALYSIS.md` - Business model, target markets
