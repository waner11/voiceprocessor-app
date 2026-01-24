# Agent Instructions

This repository defines the **VoiceProcessor API**, a multi-provider Text-to-Speech platform built with **ASP.NET Core 10** and **C# 14**.
It strictly follows the **iDesign (Volatility-Based Decomposition)** architecture.

## 1. Quick Reference

### Issue Tracking (Beads)
Use `bd` to manage your work.
```bash
bd ready                  # Find available work
bd update <id> --status in_progress  # Claim work
bd show <id>              # View details
bd close <id>             # Complete work
bd sync                   # Sync with git
```

### Directory Structure
- `src/VoiceProcessor.Clients.Api`: **Entry Point**. Controllers, SignalR Hubs. No business logic.
- `src/VoiceProcessor.Managers`: **Orchestration**. Orchestrates work between Engines and Accessors.
- `src/VoiceProcessor.Engines`: **Business Logic**. Pure logic, rules, calculations. Stateless.
- `src/VoiceProcessor.Accessors`: **Resources**. Database, 3rd Party APIs, File System.
- `src/VoiceProcessor.Utilities`: **Cross-Cutting**. Logging, configuration, helpers.
- `src/VoiceProcessor.Domain`: **Shared Types**. Entities, DTOs, Enums, Interfaces.

---

## 2. Build, Test, & Run

Execute these commands from the repository root.

### Build
```bash
# Restore dependencies
dotnet restore

# Build the entire solution
dotnet build
```

### Test
```bash
# Run all tests
dotnet test

# Run a single test case (Recommended for TDD)
# Syntax: dotnet test --filter "FullyQualifiedName~Namespace.Class.Method"
dotnet test --filter "FullyQualifiedName~VoiceProcessor.UnitTests.Managers.GenerationManagerTests.CreateGeneration_ShouldSucceed"
```

### Run
```bash
# Start required infrastructure (PostgreSQL, Redis)
docker-compose up -d db redis

# Run the API
dotnet run --project src/VoiceProcessor.Clients.Api

# Watch mode (Hot Reload)
dotnet watch --project src/VoiceProcessor.Clients.Api
```

### Database Migrations
```bash
# Create a new migration
dotnet ef migrations add <MigrationName> --project src/VoiceProcessor.Accessors --startup-project src/VoiceProcessor.Clients.Api

# Apply migrations
dotnet ef database update --project src/VoiceProcessor.Accessors --startup-project src/VoiceProcessor.Clients.Api
```

---

## 3. Code Style & Conventions

### General Guidelines
- **Framework**: Use C# 14 features. Target `.net10.0`.
- **Namespaces**: Use file-scoped namespaces (e.g., `namespace VoiceProcessor.Managers;`).
- **Nullability**: Enable nullable reference types. Explicitly handle `null`.
- **Async/Await**: Use `async` all the way down. Always pass `CancellationToken` to async methods.
- **Dependency Injection**: Use constructor injection. Assign to `private readonly` fields.

### Naming Conventions
- **Classes/Methods/Properties**: `PascalCase`.
- **Private Fields**: `_camelCase` (e.g., `_logger`, `_generationAccessor`).
- **Interfaces**: Prefix with `I` (e.g., `IGenerationManager`).
- **Async Methods**: Suffix with `Async` (e.g., `CreateGenerationAsync`).
- **DTOs**: Suffix with `Request` or `Response`.

### Architectural Rules (iDesign)
1.  **Call Chain**: `Client -> Manager -> Engine -> Accessor -> Utility`.
2.  **No Sideways Calls**: Managers cannot call Managers. Engines cannot call Engines.
3.  **No Upward Calls**: Accessors cannot call Engines. Engines cannot call Managers.
4.  **Manager Responsibility**: Orchestration only. "Do this, then do that." No complex loops or rules.
5.  **Engine Responsibility**: "How" to do something. Algorithms, strategies, validation rules.
6.  **Accessor Responsibility**: encapsulate "Where" data comes from. specific SQL, HTTP calls.

### Error Handling
- **Exceptions**: Throw specific exceptions (`VoiceProcessorException`, `ProviderException`).
- **Catching**: Do not catch generic `Exception` unless in a top-level handler/middleware.
- **Validation**: Use FluentValidation for request validation.
- **HTTP Status**: Controllers map exceptions to status codes (400, 402, 404).

### Logging (Serilog)
Use structured logging. Do not use string interpolation for log arguments.
```csharp
// GOOD
_logger.LogInformation("Processing chunk {ChunkId} for generation {GenerationId}", chunkId, genId);

// BAD
_logger.LogInformation($"Processing chunk {chunkId} for generation {genId}");
```
- **Information**: Key business events (Start/End of workflows).
- **Warning**: Recoverable issues (Retries, Fallbacks).
- **Error**: Exceptions and failures requiring attention.

---

## 4. Work Session Workflow ("Landing the Plane")

When finishing a task, you **MUST** follow this sequence. Work is not done until it is pushed.

1.  **Quality Check**: Run `dotnet build` and `dotnet test`. Fix any errors.
2.  **Cleanup**: Remove unused usings, temp comments.
3.  **Commit**:
    - Use lowercase, imperative mood, no period.
    - **NO AI attribution** in messages.
    - Example: `implement routing engine logic`
4.  **Push Sequence (MANDATORY)**:
    ```bash
    git pull --rebase
    bd sync
    git push
    git status # Must be clean and up to date
    ```
5.  **Close Issue**:
    ```bash
    bd close <id> --reason="Completed implementation"
    ```

---

## 5. Environment & Configuration

Ensure these environment variables are set (or use `appsettings.Development.json`):

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection |
| `Redis__ConnectionString` | Redis connection |
| `ElevenLabs__ApiKey` | API Key for ElevenLabs |
| `OpenAI__ApiKey` | API Key for OpenAI |

## 6. Key Libraries
- **Serilog**: Logging.
- **FluentValidation**: Input validation.
- **Entity Framework Core**: Data access.
- **Hangfire**: Background jobs.
- **Polly**: Resilience and retries (HTTP calls).
