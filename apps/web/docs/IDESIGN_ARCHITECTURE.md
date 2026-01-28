# iDesign Architecture Guide

This document defines the architectural patterns and guidelines for VoiceProcessor API based on the iDesign Method by Juval Löwy.

## Overview

The iDesign Method uses **Volatility-Based Decomposition** rather than functional decomposition. Instead of creating services that map directly to requirements, we identify areas of volatility (things likely to change) and encapsulate them in isolated components.

> "Every dependency that is tightly integrated and might change should be isolated from the architecture."

## Service Layers

```
┌─────────────────────────────────────────────────────────────┐
│                        CLIENTS                               │
│         (Web App, API Controllers, CLI, Mobile)              │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                       MANAGERS                               │
│     (Workflow orchestration, use case coordination)          │
└─────────────────────────────────────────────────────────────┘
                              │
                    ┌─────────┴─────────┐
                    ▼                   ▼
┌───────────────────────────┐ ┌───────────────────────────────┐
│         ENGINES           │ │        ACCESSORS              │
│   (Business rules/logic)  │ │  (Resource/data access)       │
└───────────────────────────┘ └───────────────────────────────┘
                    │                   │
                    └─────────┬─────────┘
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                       UTILITIES                              │
│        (Cross-cutting: logging, caching, config)             │
└─────────────────────────────────────────────────────────────┘
```

## Layer Responsibilities

### Clients
- User-facing components (controllers, UI, CLI)
- Handle request/response transformation
- Validate input format (not business rules)
- Should NOT contain business logic
- Should call only ONE Manager per operation

### Managers
- **Orchestrate workflows** for a specific use case
- Define the sequence of operations (the "what")
- Coordinate calls to Engines and Accessors
- Own the public API contracts
- Handle transactions when needed
- Stateful when managing workflow context

Example: `GenerationManager` orchestrates the TTS generation workflow - analyzing text, routing to provider, chunking, generating audio, merging results.

### Engines
- **Encapsulate business rules** (the "how")
- Stateless, reusable logic
- Pure calculations and transformations
- No direct resource access
- Can be swapped with alternative implementations

Example: `RoutingEngine` calculates which TTS provider to use based on cost/quality/speed preferences. `ChunkingEngine` splits text into optimal segments.

### Accessors (Resource Access)
- **Hide external dependencies** from the system
- Encapsulate data stores, APIs, file systems
- Provide atomic operations on domain concepts
- Own their data contracts
- Handle connection management, retries, caching

Example: `GenerationAccessor` handles database operations for generations. `ElevenLabsAccessor` wraps the ElevenLabs API.

### Utilities
- **Cross-cutting concerns** not specific to business domain
- Logging, metrics, configuration, serialization
- Could be used in any application
- No business logic

Example: `LoggingUtility`, `CacheUtility`, `ConfigurationUtility`

## Calling Rules

**Flow is TOP to BOTTOM only. Never sideways or upward.**

```
✓ Client → Manager
✓ Manager → Engine
✓ Manager → Accessor
✓ Engine → Accessor
✓ Any layer → Utility

✗ Manager → Manager (sync)     Use async messaging instead
✗ Engine → Engine              Combine logic or use Manager
✗ Accessor → Engine            Accessors are leaf nodes
✗ Accessor → Manager           Never call upward
✗ Engine → Manager             Never call upward
```

### Manager-to-Manager Communication
Managers should NOT call other Managers synchronously. If coordination is needed:
- Use async messaging/events
- Or refactor to have a higher-level Manager orchestrate both

## Project Structure

```
VoiceProcessor/
├── src/
│   ├── VoiceProcessor.Clients/
│   │   └── Api/
│   │       ├── Controllers/
│   │       │   ├── GenerationsController.cs
│   │       │   └── VoicesController.cs
│   │       ├── Hubs/
│   │       └── Program.cs
│   │
│   ├── VoiceProcessor.Managers/
│   │   ├── Contracts/                    # Public interfaces
│   │   │   ├── IGenerationManager.cs
│   │   │   └── IVoiceManager.cs
│   │   ├── Generation/
│   │   │   └── GenerationManager.cs
│   │   ├── Voice/
│   │   │   └── VoiceManager.cs
│   │   └── Notification/
│   │       └── NotificationManager.cs
│   │
│   ├── VoiceProcessor.Engines/
│   │   ├── Routing/
│   │   │   ├── IRoutingEngine.cs
│   │   │   └── RoutingEngine.cs
│   │   ├── Chunking/
│   │   │   ├── IChunkingEngine.cs
│   │   │   └── ChunkingEngine.cs
│   │   ├── Analysis/
│   │   │   ├── ITextAnalysisEngine.cs
│   │   │   └── TextAnalysisEngine.cs
│   │   ├── Pricing/
│   │   │   ├── IPricingEngine.cs
│   │   │   └── PricingEngine.cs
│   │   └── AudioProcessing/
│   │       ├── IAudioMergeEngine.cs
│   │       └── AudioMergeEngine.cs
│   │
│   ├── VoiceProcessor.Accessors/
│   │   ├── Data/                         # Database accessors
│   │   │   ├── IGenerationAccessor.cs
│   │   │   ├── GenerationAccessor.cs
│   │   │   ├── IUserAccessor.cs
│   │   │   ├── UserAccessor.cs
│   │   │   └── DbContext/
│   │   │       └── VoiceProcessorDbContext.cs
│   │   ├── Providers/                    # External TTS APIs
│   │   │   ├── ITTSProviderAccessor.cs
│   │   │   ├── ElevenLabsAccessor.cs
│   │   │   ├── OpenAITTSAccessor.cs
│   │   │   ├── GoogleTTSAccessor.cs
│   │   │   └── AmazonPollyAccessor.cs
│   │   ├── Storage/                      # Blob/file storage
│   │   │   ├── IStorageAccessor.cs
│   │   │   └── BlobStorageAccessor.cs
│   │   └── AI/                           # ML/AI services
│   │       ├── IOpenAIAccessor.cs
│   │       └── OpenAIAccessor.cs
│   │
│   ├── VoiceProcessor.Utilities/
│   │   ├── Logging/
│   │   ├── Caching/
│   │   ├── Configuration/
│   │   └── Serialization/
│   │
│   └── VoiceProcessor.Domain/            # Shared entities/DTOs
│       ├── Entities/
│       ├── DTOs/
│       └── Enums/
│
└── tests/
    ├── VoiceProcessor.Managers.Tests/
    ├── VoiceProcessor.Engines.Tests/
    └── VoiceProcessor.Accessors.Tests/
```

## Naming Conventions

| Component | Pattern | Example |
|-----------|---------|---------|
| Manager Interface | `I{Domain}Manager` | `IGenerationManager` |
| Manager Class | `{Domain}Manager` | `GenerationManager` |
| Engine Interface | `I{Function}Engine` | `IRoutingEngine` |
| Engine Class | `{Function}Engine` | `RoutingEngine` |
| Accessor Interface | `I{Resource}Accessor` | `IGenerationAccessor` |
| Accessor Class | `{Resource}Accessor` | `GenerationAccessor` |
| Utility Interface | `I{Function}Utility` | `ICacheUtility` |

## Example: Generation Workflow

```
GenerationsController (Client)
    │
    ▼
GenerationManager (Manager)
    │
    ├──► TextAnalysisEngine ──► OpenAIAccessor
    │
    ├──► ChunkingEngine
    │
    ├──► RoutingEngine
    │
    ├──► PricingEngine
    │
    ├──► [For each chunk]
    │       └──► TTSProviderAccessor (ElevenLabs/OpenAI/etc)
    │
    ├──► AudioMergeEngine
    │
    ├──► StorageAccessor
    │
    └──► GenerationAccessor (save to DB)
```

## Volatility Encapsulation

Identify what's likely to change and isolate it:

| Volatility | Component | Rationale |
|------------|-----------|-----------|
| TTS Providers | `Accessors/Providers/*` | New providers, API changes |
| Pricing rules | `PricingEngine` | Business model changes |
| Routing logic | `RoutingEngine` | Optimization improvements |
| Chunking strategy | `ChunkingEngine` | Algorithm improvements |
| Storage backend | `StorageAccessor` | S3 → R2 → Azure Blob |
| AI/ML services | `Accessors/AI/*` | OpenAI → Azure → local |

## Dependency Injection

```csharp
// Program.cs - Register by layer

// Utilities (no dependencies)
services.AddSingleton<ICacheUtility, RedisCacheUtility>();
services.AddSingleton<ILoggingUtility, SerilogUtility>();

// Accessors (depend on utilities)
services.AddScoped<IGenerationAccessor, GenerationAccessor>();
services.AddScoped<IUserAccessor, UserAccessor>();
services.AddHttpClient<ITTSProviderAccessor, ElevenLabsAccessor>();
services.AddHttpClient<IOpenAIAccessor, OpenAIAccessor>();
services.AddScoped<IStorageAccessor, BlobStorageAccessor>();

// Engines (depend on accessors, utilities)
services.AddScoped<IRoutingEngine, RoutingEngine>();
services.AddScoped<IChunkingEngine, ChunkingEngine>();
services.AddScoped<ITextAnalysisEngine, TextAnalysisEngine>();
services.AddScoped<IPricingEngine, PricingEngine>();
services.AddScoped<IAudioMergeEngine, AudioMergeEngine>();

// Managers (depend on engines, accessors)
services.AddScoped<IGenerationManager, GenerationManager>();
services.AddScoped<IVoiceManager, VoiceManager>();
```

## Interface Examples

### Manager Interface
```csharp
public interface IGenerationManager
{
    Task<GenerationResult> GenerateAsync(GenerationRequest request, CancellationToken ct);
    Task<CostEstimate> EstimateAsync(EstimateRequest request, CancellationToken ct);
    Task<Generation> GetStatusAsync(Guid generationId, CancellationToken ct);
    Task SubmitFeedbackAsync(Guid generationId, FeedbackRequest feedback, CancellationToken ct);
}
```

### Engine Interface
```csharp
public interface IRoutingEngine
{
    RoutingDecision SelectProvider(
        TextAnalysis analysis,
        RoutingPreferences preferences,
        IReadOnlyList<ProviderHealth> providerStatus);
}

public interface IChunkingEngine
{
    IReadOnlyList<TextChunk> Chunk(
        string text,
        ChunkingStrategy strategy,
        int maxChunkSize);
}
```

### Accessor Interface
```csharp
public interface IGenerationAccessor
{
    Task<Generation> CreateAsync(Generation generation, CancellationToken ct);
    Task<Generation?> GetByIdAsync(Guid id, CancellationToken ct);
    Task UpdateAsync(Generation generation, CancellationToken ct);
    Task<IReadOnlyList<Generation>> GetByUserAsync(Guid userId, int limit, CancellationToken ct);
}

public interface ITTSProviderAccessor
{
    string ProviderName { get; }
    Task<TTSResult> GenerateAsync(string text, string voiceId, TTSOptions options, CancellationToken ct);
    Task<IReadOnlyList<Voice>> GetVoicesAsync(CancellationToken ct);
    Task<HealthStatus> CheckHealthAsync(CancellationToken ct);
}
```

## Anti-Patterns to Avoid

### 1. Functional Decomposition
```
❌ Bad: One service per requirement
   - GeneratePreviewService
   - GenerateFullService
   - GetStatusService

✓ Good: Services based on volatility
   - GenerationManager (orchestrates all generation workflows)
   - RoutingEngine (encapsulates routing logic)
```

### 2. Sideways Calls
```
❌ Bad: Engine calling another Engine
   RoutingEngine → ChunkingEngine

✓ Good: Manager coordinates both
   GenerationManager → RoutingEngine
   GenerationManager → ChunkingEngine
```

### 3. Upward Calls
```
❌ Bad: Accessor calling Engine
   GenerationAccessor → PricingEngine

✓ Good: Keep accessors as leaf nodes
   Manager → PricingEngine → result
   Manager → GenerationAccessor (with price)
```

### 4. Business Logic in Accessors
```
❌ Bad: Pricing logic in database accessor
   GenerationAccessor.Create() { cost = CalculatePrice(); }

✓ Good: Engine handles logic, Accessor just persists
   PricingEngine.Calculate() → cost
   GenerationAccessor.Create(generation with cost)
```

### 5. Multiple Manager Calls from Client
```
❌ Bad: Controller calls multiple managers
   Controller → GenerationManager.Start()
   Controller → NotificationManager.Send()

✓ Good: Single manager orchestrates
   Controller → GenerationManager.Start()
   GenerationManager → (internally triggers notification)
```

## References

- [Righting Software](https://rightingsoftware.org/) by Juval Löwy
- [IDesign Official Site](https://www.idesign.net/)
- [Software Architecture with the IDesign Method](https://medium.com/nmc-techblog/software-architecture-with-the-idesign-method-63716a8329ec) - Tal Joffe
- [Volatility Based Decomposition](http://www.waynecliffordbarker.co.za/2019/03/23/volatility-based-decomposition-for-microservices/) - Wayne Barker
- [IDesign VirtualTradeMe Sample](https://github.com/joerglang/IDesign-VirtualTradeMe)
- [iDesign Layer Patterns](https://spencerfarley.com/tags/idesign/) - Spencer Farley
