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

## 2. Development Methodology

This project follows **TDD (Test-Driven Development)**. Use `load_skills=["tdd"]` for all implementation tasks.
Write a failing test first, make it pass with minimal code, then refactor. See the `tdd` skill for the full
Red→Green→Refactor workflow, xUnit/Moq/FluentAssertions patterns, and test commands.

## 3. Build, Test, & Run

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

## 4. Code Style & Conventions

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

## 5. Work Session Workflow

This section covers the complete git workflow from starting work to finishing and creating a pull request.

### 5.1 Starting Work

Before you begin coding, follow these steps to set up your feature branch:

1. **Check for clean working directory**:
   ```bash
   git status
   ```
   If you have uncommitted changes, commit or stash them before proceeding.

2. **Switch to main and sync**:
   ```bash
   git checkout main
   git pull --rebase origin main
   ```

3. **Create feature branch from issue ID**:
   ```bash
   git checkout -b beads-xxx-short-description
   ```
   Example: `git checkout -b beads-p71-add-credits-field`
   
   **Edge case**: If the branch already exists locally, delete it first:
   ```bash
   git branch -D beads-xxx-short-description
   git checkout -b beads-xxx-short-description
   ```

4. **Claim the issue**:
   ```bash
   bd update <id> --status in_progress
   ```

### 5.2 During Work

While working on your feature:

- **Commit frequently** with meaningful messages
- **Follow commit conventions**:
  - Use lowercase, imperative mood, no period
  - **NO AI attribution** in messages
  - Examples:
    - `implement routing engine logic`
    - `add elevenlabs provider implementation`
    - `fix null check in chunking service`

### 5.3 Finishing Work ("Landing the Plane")

When your work is complete, you **MUST** follow this sequence. Work is not done until it is pushed and a PR is created.

1. **Quality Check**: Run `dotnet build` and `dotnet test`. Fix any errors.

2. **Cleanup**: Remove unused usings, temp comments.

3. **Commit final changes**:
   ```bash
   git add -A
   git commit -m "implement feature description"
   ```

4. **Sync main and rebase**:
   ```bash
   git checkout main
   git pull --rebase origin main
   git checkout beads-xxx-short-description
   git rebase main
   ```
   
   **If conflicts occur during rebase**:
   - Open the conflicted files and resolve conflicts manually
   - Stage the resolved files: `git add <file>`
   - Continue the rebase: `git rebase --continue`
   - Repeat until rebase completes
   - If you need to abort: `git rebase --abort`

5. **Push and create PR**:
   ```bash
   git push -u origin beads-xxx-short-description
   gh pr create --base main --fill
   ```
   
   **Edge case**: If PR already exists, check with:
   ```bash
   gh pr list
   ```

6. **Sync beads and close issue**:
   ```bash
   bd sync
   bd close <id> --reason="PR created"
   ```

---

## 6. Environment & Configuration

Ensure these environment variables are set (or use `appsettings.Development.json`):

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection |
| `Redis__ConnectionString` | Redis connection |
| `ElevenLabs__ApiKey` | API Key for ElevenLabs |
| `OpenAI__ApiKey` | API Key for OpenAI |

### Local Development Setup

For local development, use `appsettings.Development.json` to manage sensitive configuration without modifying `appsettings.json`.

#### Stripe Configuration

1. Copy the template:
   ```bash
   cp src/VoiceProcessor.Clients.Api/appsettings.Development.json.example \
      src/VoiceProcessor.Clients.Api/appsettings.Development.json
   ```

2. Edit `appsettings.Development.json` with your Stripe keys:
   - `SecretKey`: From [Stripe Dashboard](https://dashboard.stripe.com) (Test mode)
   - `WebhookSecret`: From `stripe listen` output when running Stripe CLI

3. Start the API - it will automatically load Development settings:
   ```bash
   dotnet run --project src/VoiceProcessor.Clients.Api
   ```

**Note**: `appsettings.Development.json` is in `.gitignore` and should never be committed. Use `appsettings.Development.json.example` as a template for new developers.

## 7. Key Libraries
- **Serilog**: Logging.
- **FluentValidation**: Input validation.
- **Entity Framework Core**: Data access.
- **Hangfire**: Background jobs.
- **Polly**: Resilience and retries (HTTP calls).
