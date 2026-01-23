# VoiceProcessor: .NET Technology Stack

**Date:** January 2026
**Target Developer:** .NET/C# background
**ML Strategy:** API-based (OpenAI, Azure Cognitive Services)

---

## Table of Contents

1. [Stack Overview](#1-stack-overview)
2. [Backend (.NET)](#2-backend-net)
3. [Database](#3-database)
4. [Background Jobs](#4-background-jobs)
5. [File Storage](#5-file-storage)
6. [ML & AI (API-Based)](#6-ml--ai-api-based)
7. [Authentication](#7-authentication)
8. [Infrastructure](#8-infrastructure)
9. [Data Collection Strategy](#9-data-collection-strategy)
10. [Monitoring](#10-monitoring)
11. [Project Structure](#11-project-structure)
12. [Cost Estimates](#12-cost-estimates)
13. [Getting Started](#13-getting-started)

> **Note:** Frontend documentation has been moved to the [voiceprocessor-web](https://github.com/waner11/voiceprocessor-web) repository.

---

## 1. Stack Overview

### 1.1 Technology Decisions

| Layer | Technology | Rationale |
|-------|------------|-----------|
| Backend | ASP.NET Core 8 | Your expertise, excellent performance |
| Language | C# 12 | Modern features, strong typing |
| Database | **PostgreSQL** | pgvector for ML embeddings, free hosting tiers, future-proof |
| Cache | Redis | Industry standard, great .NET support |
| Queue | Hangfire | Best .NET background job library |
| Frontend | Next.js + React | Best ecosystem (or Blazor to stay in C#) |
| Storage | Azure Blob or Cloudflare R2 | S3-compatible, .NET SDKs available |
| ML | OpenAI API + Azure Cognitive Services | No local training needed |
| Auth | ASP.NET Core Identity + JWT | Built-in, you know it |

### 1.2 Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                           CLIENTS                                    │
├─────────────────────────────────────────────────────────────────────┤
│     Next.js Web App     │     Mobile Apps     │     REST API        │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      ASP.NET CORE 8 API                             │
│         (Controllers, Services, Middleware, SignalR)                 │
└─────────────────────────────────────────────────────────────────────┘
            │                       │                       │
            ▼                       ▼                       ▼
┌───────────────────┐   ┌───────────────────┐   ┌───────────────────┐
│   PostgreSQL      │   │      Redis        │   │    Hangfire       │
│   (+ pgvector)    │   │   (Cache/Queue)   │   │   (Background)    │
└───────────────────┘   └───────────────────┘   └───────────────────┘
                                    │
                    ┌───────────────┼───────────────┐
                    ▼               ▼               ▼
            ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
            │ ElevenLabs  │ │   OpenAI    │ │   Azure     │
            │     API     │ │     API     │ │  Cognitive  │
            └─────────────┘ └─────────────┘ └─────────────┘
                                    │
                                    ▼
                        ┌─────────────────────┐
                        │  Azure Blob / R2    │
                        │   (Audio Storage)   │
                        └─────────────────────┘
```

---

## 2. Backend (.NET)

### 2.1 Framework & Version

```
ASP.NET Core 8 (LTS)
├── Minimal APIs OR Controllers (your preference)
├── Native AOT support (optional, for performance)
├── Built-in OpenAPI/Swagger
└── Excellent async/await support
```

### 2.2 Key NuGet Packages

```xml
<!-- VoiceProcessor.csproj -->
<ItemGroup>
    <!-- Web Framework -->
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.*" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.*" />

    <!-- Database (PostgreSQL) -->
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.*" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL.NodaTime" Version="8.0.*" />
    <PackageReference Include="Pgvector.EntityFrameworkCore" Version="0.2.*" /> <!-- For ML embeddings -->

    <!-- Background Jobs -->
    <PackageReference Include="Hangfire.Core" Version="1.8.*" />
    <PackageReference Include="Hangfire.PostgreSql" Version="1.20.*" />
    <PackageReference Include="Hangfire.Redis.StackExchange" Version="1.9.*" />

    <!-- Caching -->
    <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.*" />

    <!-- HTTP Clients (for TTS providers) -->
    <PackageReference Include="Microsoft.Extensions.Http.Polly" Version="8.0.*" />

    <!-- Authentication -->
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.*" />
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.*" />

    <!-- OpenAI -->
    <PackageReference Include="Azure.AI.OpenAI" Version="1.0.*" />
    <!-- OR community package: -->
    <PackageReference Include="OpenAI" Version="1.11.*" />

    <!-- Azure Cognitive Services -->
    <PackageReference Include="Azure.AI.TextAnalytics" Version="5.3.*" />

    <!-- Audio Processing -->
    <PackageReference Include="NAudio" Version="2.2.*" />

    <!-- Storage -->
    <PackageReference Include="Azure.Storage.Blobs" Version="12.19.*" />
    <!-- OR for S3-compatible (R2): -->
    <PackageReference Include="AWSSDK.S3" Version="3.7.*" />

    <!-- Validation -->
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.*" />

    <!-- Mapping -->
    <PackageReference Include="Mapster" Version="7.4.*" />

    <!-- Real-time -->
    <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.1.*" />
</ItemGroup>
```

### 2.3 Service Layer Architecture

```
CLEAN ARCHITECTURE LAYERS
│
├── VoiceProcessor.Api (ASP.NET Core)
│   ├── Controllers/
│   ├── Middleware/
│   ├── Hubs/ (SignalR)
│   └── Program.cs
│
├── VoiceProcessor.Application (Business Logic)
│   ├── Services/
│   │   ├── GenerationService.cs
│   │   ├── RoutingService.cs
│   │   ├── ChunkingService.cs
│   │   └── AnalysisService.cs
│   ├── Interfaces/
│   └── DTOs/
│
├── VoiceProcessor.Domain (Entities)
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Generation.cs
│   │   ├── Voice.cs
│   │   └── Feedback.cs
│   └── Enums/
│
├── VoiceProcessor.Infrastructure (External)
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   └── Migrations/
│   ├── Providers/ (TTS Adapters)
│   │   ├── ITTSProvider.cs
│   │   ├── ElevenLabsProvider.cs
│   │   ├── OpenAITTSProvider.cs
│   │   ├── GoogleTTSProvider.cs
│   │   └── AmazonPollyProvider.cs
│   ├── ML/ (API Wrappers)
│   │   ├── ITextAnalyzer.cs
│   │   ├── OpenAIAnalyzer.cs
│   │   └── AzureCognitiveAnalyzer.cs
│   └── Storage/
│       └── BlobStorageService.cs
│
└── VoiceProcessor.Jobs (Hangfire)
    ├── GenerationJob.cs
    ├── AudioMergeJob.cs
    └── AnalysisJob.cs
```

### 2.4 Provider Interface Example

```csharp
// VoiceProcessor.Infrastructure/Providers/ITTSProvider.cs
public interface ITTSProvider
{
    string Name { get; }
    IReadOnlyList<string> SupportedLanguages { get; }
    decimal CostPerThousandCharacters { get; }
    int MaxCharactersPerRequest { get; }

    Task<TTSResult> GenerateAsync(
        string text,
        string voiceId,
        TTSOptions options,
        CancellationToken cancellationToken = default);

    Task<Stream> GenerateStreamAsync(
        string text,
        string voiceId,
        TTSOptions options,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Voice>> GetVoicesAsync(
        CancellationToken cancellationToken = default);

    Task<HealthStatus> CheckHealthAsync(
        CancellationToken cancellationToken = default);
}

public record TTSResult(
    byte[] AudioData,
    string Format,
    TimeSpan Duration,
    int CharactersProcessed,
    decimal Cost);

public record TTSOptions(
    string Format = "mp3",
    double Stability = 1.0,
    double SimilarityBoost = 1.0,
    double Speed = 1.0);
```

### 2.5 ElevenLabs Provider Implementation

```csharp
// VoiceProcessor.Infrastructure/Providers/ElevenLabsProvider.cs
public class ElevenLabsProvider : ITTSProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ElevenLabsProvider> _logger;
    private readonly ElevenLabsOptions _options;

    public string Name => "ElevenLabs";
    public IReadOnlyList<string> SupportedLanguages =>
        ["en", "es", "fr", "de", "it", "pt", "pl", "hi", "ar"];
    public decimal CostPerThousandCharacters => 0.18m;
    public int MaxCharactersPerRequest => 5000;

    public ElevenLabsProvider(
        HttpClient httpClient,
        IOptions<ElevenLabsOptions> options,
        ILogger<ElevenLabsProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri("https://api.elevenlabs.io/v1/");
        _httpClient.DefaultRequestHeaders.Add("xi-api-key", _options.ApiKey);
    }

    public async Task<TTSResult> GenerateAsync(
        string text,
        string voiceId,
        TTSOptions options,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            text,
            model_id = "eleven_multilingual_v2",
            voice_settings = new
            {
                stability = options.Stability,
                similarity_boost = options.SimilarityBoost,
                style = 0.15,
                use_speaker_boost = true
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"text-to-speech/{voiceId}/stream",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var audioData = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var cost = text.Length / 1000m * CostPerThousandCharacters;

        return new TTSResult(
            AudioData: audioData,
            Format: "mp3",
            Duration: EstimateDuration(audioData),
            CharactersProcessed: text.Length,
            Cost: cost);
    }

    public async Task<Stream> GenerateStreamAsync(
        string text,
        string voiceId,
        TTSOptions options,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            text,
            model_id = "eleven_multilingual_v2",
            voice_settings = new
            {
                stability = options.Stability,
                similarity_boost = options.SimilarityBoost
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"text-to-speech/{voiceId}/stream",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Voice>> GetVoicesAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<ElevenLabsVoicesResponse>(
            "voices",
            cancellationToken);

        return response?.Voices.Select(v => new Voice(
            Id: v.VoiceId,
            Name: v.Name,
            Provider: Name,
            Language: v.Labels?.Language ?? "en",
            Gender: v.Labels?.Gender ?? "unknown"
        )).ToList() ?? [];
    }

    public async Task<HealthStatus> CheckHealthAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sw = Stopwatch.StartNew();
            var response = await _httpClient.GetAsync("user", cancellationToken);
            sw.Stop();

            return new HealthStatus(
                IsHealthy: response.IsSuccessStatusCode,
                LatencyMs: sw.ElapsedMilliseconds,
                Message: response.IsSuccessStatusCode ? "OK" : response.ReasonPhrase);
        }
        catch (Exception ex)
        {
            return new HealthStatus(false, 0, ex.Message);
        }
    }

    private static TimeSpan EstimateDuration(byte[] audioData)
    {
        // MP3 at 128kbps: ~1MB per minute
        var estimatedMinutes = audioData.Length / (128.0 * 1024 / 8 * 60);
        return TimeSpan.FromMinutes(estimatedMinutes);
    }
}

// Configuration
public class ElevenLabsOptions
{
    public string ApiKey { get; set; } = string.Empty;
}

// Response DTOs
internal record ElevenLabsVoicesResponse(List<ElevenLabsVoice> Voices);
internal record ElevenLabsVoice(
    string VoiceId,
    string Name,
    ElevenLabsVoiceLabels? Labels);
internal record ElevenLabsVoiceLabels(string? Language, string? Gender);
```

### 2.6 API Endpoints

```csharp
// VoiceProcessor.Api/Controllers/GenerationsController.cs
[ApiController]
[Route("api/v1/generations")]
[Authorize]
public class GenerationsController : ControllerBase
{
    private readonly IGenerationService _generationService;
    private readonly IBackgroundJobClient _backgroundJobs;

    public GenerationsController(
        IGenerationService generationService,
        IBackgroundJobClient backgroundJobs)
    {
        _generationService = generationService;
        _backgroundJobs = backgroundJobs;
    }

    /// <summary>
    /// Estimate cost for a generation without processing
    /// </summary>
    [HttpPost("estimate")]
    public async Task<ActionResult<CostEstimateResponse>> Estimate(
        [FromBody] GenerationRequest request)
    {
        var estimate = await _generationService.EstimateCostAsync(request);
        return Ok(estimate);
    }

    /// <summary>
    /// Generate a preview (first 500 characters)
    /// </summary>
    [HttpPost("preview")]
    public async Task<ActionResult<PreviewResponse>> Preview(
        [FromBody] GenerationRequest request)
    {
        var preview = await _generationService.GeneratePreviewAsync(request);
        return Ok(preview);
    }

    /// <summary>
    /// Start a full generation (async)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<GenerationStartedResponse>> Generate(
        [FromBody] GenerationRequest request)
    {
        var userId = User.GetUserId();

        // Create generation record
        var generation = await _generationService.CreateAsync(request, userId);

        // Queue background job
        var jobId = _backgroundJobs.Enqueue<GenerationJob>(
            job => job.ProcessAsync(generation.Id, CancellationToken.None));

        return Accepted(new GenerationStartedResponse(
            GenerationId: generation.Id,
            JobId: jobId,
            Status: "processing",
            EstimatedCompletionUtc: DateTime.UtcNow.AddMinutes(5),
            StatusUrl: Url.Action(nameof(GetStatus), new { id = generation.Id })));
    }

    /// <summary>
    /// Get generation status
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<GenerationResponse>> GetStatus(Guid id)
    {
        var generation = await _generationService.GetByIdAsync(id);

        if (generation is null)
            return NotFound();

        return Ok(generation);
    }

    /// <summary>
    /// Submit feedback for a generation
    /// </summary>
    [HttpPost("{id}/feedback")]
    public async Task<ActionResult> SubmitFeedback(
        Guid id,
        [FromBody] FeedbackRequest request)
    {
        await _generationService.AddFeedbackAsync(id, request);
        return Ok();
    }
}

// Request/Response DTOs
public record GenerationRequest(
    string Text,
    string? VoiceId,
    VoicePreferences? VoicePreferences,
    RoutingOptions? Routing,
    AudioOptions? Audio);

public record VoicePreferences(
    string? Gender,
    string? Style,
    string Language = "en");

public record RoutingOptions(
    string Strategy = "balanced",  // cost, quality, speed, balanced
    string? PreferredProvider = null,
    List<string>? ExcludeProviders = null,
    decimal? MaxCost = null);

public record AudioOptions(
    string Format = "mp3",
    int Bitrate = 128,
    bool Normalize = true);

public record GenerationStartedResponse(
    Guid GenerationId,
    string JobId,
    string Status,
    DateTime EstimatedCompletionUtc,
    string? StatusUrl);

public record CostEstimateResponse(
    int CharacterCount,
    string DetectedLanguage,
    string ContentType,
    List<ProviderEstimate> Options,
    ProviderEstimate Recommended);

public record ProviderEstimate(
    string Provider,
    decimal EstimatedCost,
    int QualityRating,
    TimeSpan EstimatedDuration);

public record FeedbackRequest(
    int Rating,
    List<string>? Tags,
    string? Comment);
```

---

## 3. Database

### 3.1 Recommendation: PostgreSQL

**PostgreSQL is the recommended database** for VoiceProcessor due to its ML capabilities, cost advantages, and future-proofing.

```
POSTGRESQL SETUP
│
├── Version: PostgreSQL 16+
├── ORM: Entity Framework Core 8 + Npgsql
├── Extensions: pgvector (for ML embeddings)
├── Hosting: Supabase (free tier), Neon (free tier), or Railway
│
├── Why PostgreSQL over SQL Server:
│   ├── pgvector extension for ML embeddings (similarity search)
│   ├── Free hosting tiers available (Supabase, Neon)
│   ├── Better JSONB support for flexible analytics storage
│   ├── No licensing costs
│   ├── Industry standard (more tutorials, community support)
│   └── EF Core code is nearly identical to SQL Server
│
└── Learning Curve: Minimal
    ├── EF Core abstracts most differences
    ├── LINQ queries are identical
    ├── Migrations work the same way
    └── GUI tools: pgAdmin, DBeaver, or DataGrip
```

### 3.2 Hosting Options

| Provider | Free Tier | Paid Tier | Best For |
|----------|-----------|-----------|----------|
| **Supabase** | 500MB, 2 projects | $25/mo | MVP, built-in auth option |
| **Neon** | 512MB, branching | $19/mo | Dev experience, branching |
| **Railway** | 1GB | $5+/mo | Simplicity |
| **Azure Database** | None | $15+/mo | Enterprise, Azure ecosystem |

**Recommendation:** Start with **Supabase** or **Neon** free tier for MVP.

### 3.3 PostgreSQL with pgvector (Future ML)

pgvector enables powerful ML features with simple SQL:

```sql
-- Enable pgvector extension
CREATE EXTENSION vector;

-- Add embedding column to generations table
ALTER TABLE generations ADD COLUMN embedding vector(1536);

-- Create index for fast similarity search
CREATE INDEX ON generations USING ivfflat (embedding vector_cosine_ops);

-- Find similar generations (for caching)
SELECT id, text_hash, audio_url
FROM generations
ORDER BY embedding <-> '[0.1, 0.2, ...]'::vector
LIMIT 5;
```

In C# with EF Core:

```csharp
// Entity with vector column
public class Generation
{
    public Guid Id { get; set; }
    // ... other properties
    public Vector? Embedding { get; set; } // pgvector type
}

// DbContext configuration
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasPostgresExtension("vector");

    modelBuilder.Entity<Generation>()
        .Property(g => g.Embedding)
        .HasColumnType("vector(1536)");
}

// Query similar generations
var similar = await _db.Generations
    .OrderBy(g => g.Embedding!.L2Distance(targetEmbedding))
    .Take(5)
    .ToListAsync();
```

### 3.4 Database Connection Setup

```csharp
// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.UseVector(); // Enable pgvector support
            npgsqlOptions.EnableRetryOnFailure(3);
        }));
```

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.xxx.supabase.co;Database=postgres;Username=postgres;Password=xxx;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

### 3.5 Entity Models

```csharp
// VoiceProcessor.Domain/Entities/Generation.cs
public class Generation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Status
    public GenerationStatus Status { get; set; }
    public string? ErrorMessage { get; set; }

    // Input
    public string TextHash { get; set; } = string.Empty; // SHA256 of text
    public int TextLength { get; set; }
    public string? OriginalText { get; set; } // Nullable, can delete after processing

    // Analysis (for future ML)
    public string Language { get; set; } = "en";
    public string ContentType { get; set; } = "general";
    public double EmotionScore { get; set; }
    public double DialogueRatio { get; set; }
    public int ParagraphCount { get; set; }

    // Routing Decision
    public string ProviderUsed { get; set; } = string.Empty;
    public string VoiceUsed { get; set; } = string.Empty;
    public string RoutingStrategy { get; set; } = "balanced";

    // Execution
    public decimal CostCents { get; set; }
    public int DurationMs { get; set; }
    public int ChunksCount { get; set; }
    public int LatencyMs { get; set; }
    public bool FallbackUsed { get; set; }

    // Output
    public string? AudioUrl { get; set; }
    public string AudioFormat { get; set; } = "mp3";
    public long FileSizeBytes { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public ICollection<GenerationChunk> Chunks { get; set; } = [];
    public GenerationFeedback? Feedback { get; set; }
}

public enum GenerationStatus
{
    Pending,
    Analyzing,
    Chunking,
    Generating,
    Merging,
    Completed,
    Failed
}

// VoiceProcessor.Domain/Entities/GenerationFeedback.cs
public class GenerationFeedback
{
    public Guid Id { get; set; }
    public Guid GenerationId { get; set; }
    public Generation Generation { get; set; } = null!;

    // Explicit feedback
    public int? Rating { get; set; } // 1-5
    public List<string> Tags { get; set; } = []; // ["too_fast", "pronunciation"]
    public string? Comment { get; set; }

    // Implicit feedback
    public bool Regenerated { get; set; }
    public bool Downloaded { get; set; }
    public bool ListenedFully { get; set; }
    public int ListenDurationSeconds { get; set; }

    public DateTime CreatedAt { get; set; }
}

// VoiceProcessor.Domain/Entities/GenerationChunk.cs
public class GenerationChunk
{
    public Guid Id { get; set; }
    public Guid GenerationId { get; set; }
    public Generation Generation { get; set; } = null!;

    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;
    public int CharacterCount { get; set; }

    public string ProviderUsed { get; set; } = string.Empty;
    public ChunkStatus Status { get; set; }
    public string? AudioUrl { get; set; }
    public int DurationMs { get; set; }
    public int LatencyMs { get; set; }

    public string? Error { get; set; }
    public int RetryCount { get; set; }
}

public enum ChunkStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}
```

---

## 4. Background Jobs

### 4.1 Hangfire Setup

Hangfire is the best .NET background job library — mature, reliable, with a built-in dashboard.

```csharp
// Program.cs
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
        options.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 2;
    options.Queues = new[] { "critical", "default", "low" };
});

// Dashboard (protected)
app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});
```

### 4.2 Generation Job

```csharp
// VoiceProcessor.Jobs/GenerationJob.cs
public class GenerationJob
{
    private readonly IGenerationService _generationService;
    private readonly IRoutingService _routingService;
    private readonly IChunkingService _chunkingService;
    private readonly IEnumerable<ITTSProvider> _providers;
    private readonly IAudioMerger _audioMerger;
    private readonly IStorageService _storage;
    private readonly IHubContext<GenerationHub> _hubContext;
    private readonly ILogger<GenerationJob> _logger;

    public GenerationJob(/* inject dependencies */) { }

    [Queue("default")]
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task ProcessAsync(Guid generationId, CancellationToken cancellationToken)
    {
        var generation = await _generationService.GetByIdAsync(generationId);
        if (generation is null) return;

        try
        {
            // 1. Update status
            await UpdateStatusAsync(generation, GenerationStatus.Analyzing);

            // 2. Analyze text (calls OpenAI API)
            var analysis = await _generationService.AnalyzeTextAsync(generation.OriginalText!);
            generation.Language = analysis.Language;
            generation.ContentType = analysis.ContentType;
            generation.EmotionScore = analysis.EmotionScore;
            generation.DialogueRatio = analysis.DialogueRatio;

            // 3. Route to provider
            await UpdateStatusAsync(generation, GenerationStatus.Chunking);
            var routing = await _routingService.RouteAsync(generation, analysis);
            generation.ProviderUsed = routing.Provider;
            generation.VoiceUsed = routing.VoiceId;

            // 4. Chunk text
            var chunks = await _chunkingService.ChunkAsync(
                generation.OriginalText!,
                routing.Provider);
            generation.ChunksCount = chunks.Count;

            // 5. Generate audio for each chunk
            await UpdateStatusAsync(generation, GenerationStatus.Generating);
            var provider = _providers.First(p => p.Name == routing.Provider);
            var audioChunks = new List<byte[]>();

            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var result = await provider.GenerateAsync(
                    chunk.Text,
                    routing.VoiceId,
                    new TTSOptions(),
                    cancellationToken);

                audioChunks.Add(result.AudioData);
                generation.CostCents += result.Cost * 100;

                // Notify progress
                await NotifyProgressAsync(generation, (i + 1.0) / chunks.Count);
            }

            // 6. Merge audio
            await UpdateStatusAsync(generation, GenerationStatus.Merging);
            var mergedAudio = await _audioMerger.MergeAsync(audioChunks);

            // 7. Upload to storage
            var audioUrl = await _storage.UploadAsync(
                $"generations/{generation.Id}.mp3",
                mergedAudio,
                "audio/mpeg");

            // 8. Complete
            generation.AudioUrl = audioUrl;
            generation.FileSizeBytes = mergedAudio.Length;
            generation.Status = GenerationStatus.Completed;
            generation.CompletedAt = DateTime.UtcNow;

            await _generationService.UpdateAsync(generation);
            await NotifyCompletedAsync(generation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generation {Id} failed", generationId);
            generation.Status = GenerationStatus.Failed;
            generation.ErrorMessage = ex.Message;
            await _generationService.UpdateAsync(generation);
            await NotifyFailedAsync(generation, ex.Message);
            throw;
        }
    }

    private async Task UpdateStatusAsync(Generation generation, GenerationStatus status)
    {
        generation.Status = status;
        await _generationService.UpdateAsync(generation);
        await _hubContext.Clients.User(generation.UserId.ToString())
            .SendAsync("StatusUpdate", new { generation.Id, Status = status.ToString() });
    }

    private async Task NotifyProgressAsync(Generation generation, double progress)
    {
        await _hubContext.Clients.User(generation.UserId.ToString())
            .SendAsync("Progress", new { generation.Id, Progress = progress });
    }

    private async Task NotifyCompletedAsync(Generation generation)
    {
        await _hubContext.Clients.User(generation.UserId.ToString())
            .SendAsync("Completed", new { generation.Id, generation.AudioUrl });
    }

    private async Task NotifyFailedAsync(Generation generation, string error)
    {
        await _hubContext.Clients.User(generation.UserId.ToString())
            .SendAsync("Failed", new { generation.Id, Error = error });
    }
}
```

---

## 5. File Storage

### 5.1 Azure Blob Storage

```csharp
// VoiceProcessor.Infrastructure/Storage/AzureBlobStorageService.cs
public class AzureBlobStorageService : IStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName = "audio";

    public AzureBlobStorageService(IOptions<AzureStorageOptions> options)
    {
        _blobServiceClient = new BlobServiceClient(options.Value.ConnectionString);
    }

    public async Task<string> UploadAsync(
        string path,
        byte[] data,
        string contentType)
    {
        var container = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blob = container.GetBlobClient(path);

        await blob.UploadAsync(
            new BinaryData(data),
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            });

        return blob.Uri.ToString();
    }

    public async Task<string> GetSignedUrlAsync(string path, TimeSpan expiry)
    {
        var container = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blob = container.GetBlobClient(path);

        var sasUri = blob.GenerateSasUri(
            BlobSasPermissions.Read,
            DateTimeOffset.UtcNow.Add(expiry));

        return sasUri.ToString();
    }

    public async Task DeleteAsync(string path)
    {
        var container = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blob = container.GetBlobClient(path);
        await blob.DeleteIfExistsAsync();
    }
}
```

### 5.2 S3-Compatible (Cloudflare R2)

```csharp
// For Cloudflare R2 or any S3-compatible storage
public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3StorageService(IOptions<S3Options> options)
    {
        var config = new AmazonS3Config
        {
            ServiceURL = options.Value.Endpoint, // R2 endpoint
            ForcePathStyle = true
        };

        _s3Client = new AmazonS3Client(
            options.Value.AccessKey,
            options.Value.SecretKey,
            config);

        _bucketName = options.Value.BucketName;
    }

    public async Task<string> UploadAsync(string path, byte[] data, string contentType)
    {
        using var stream = new MemoryStream(data);

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = path,
            InputStream = stream,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(request);

        return $"https://{_bucketName}.r2.cloudflarestorage.com/{path}";
    }

    public async Task<string> GetSignedUrlAsync(string path, TimeSpan expiry)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = path,
            Expires = DateTime.UtcNow.Add(expiry)
        };

        return await _s3Client.GetPreSignedURLAsync(request);
    }
}
```

---

## 6. ML & AI (API-Based)

### 6.1 OpenAI Integration for Text Analysis

```csharp
// VoiceProcessor.Infrastructure/ML/OpenAIAnalyzer.cs
public class OpenAIAnalyzer : ITextAnalyzer
{
    private readonly OpenAIClient _client;
    private readonly ILogger<OpenAIAnalyzer> _logger;

    public OpenAIAnalyzer(IOptions<OpenAIOptions> options, ILogger<OpenAIAnalyzer> logger)
    {
        _client = new OpenAIClient(options.Value.ApiKey);
        _logger = logger;
    }

    public async Task<TextAnalysisResult> AnalyzeAsync(string text)
    {
        // Use GPT-4o-mini for cost efficiency
        var chatClient = _client.GetChatClient("gpt-4o-mini");

        var prompt = $"""
            Analyze the following text and return a JSON object with these fields:
            - language: ISO 639-1 code (e.g., "en", "es")
            - content_type: one of [fiction_narrative, fiction_dialogue, technical, educational, news, conversational, poetry]
            - emotion_score: float 0-1 indicating emotional intensity
            - dialogue_ratio: float 0-1 indicating proportion of dialogue
            - complexity_score: float 0-1 indicating vocabulary/sentence complexity
            - suggested_voice_style: one of [narrative, dramatic, conversational, professional, warm]

            Text to analyze:
            {text.Substring(0, Math.Min(text.Length, 2000))}

            Respond only with valid JSON.
            """;

        var response = await chatClient.CompleteChatAsync(prompt);
        var json = response.Value.Content[0].Text;

        try
        {
            var result = JsonSerializer.Deserialize<OpenAIAnalysisResponse>(json);

            return new TextAnalysisResult(
                Language: result?.Language ?? "en",
                ContentType: result?.ContentType ?? "general",
                EmotionScore: result?.EmotionScore ?? 0.5,
                DialogueRatio: result?.DialogueRatio ?? 0,
                ComplexityScore: result?.ComplexityScore ?? 0.5,
                SuggestedVoiceStyle: result?.SuggestedVoiceStyle ?? "narrative"
            );
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse OpenAI response: {Json}", json);
            return TextAnalysisResult.Default;
        }
    }
}

public record TextAnalysisResult(
    string Language,
    string ContentType,
    double EmotionScore,
    double DialogueRatio,
    double ComplexityScore,
    string SuggestedVoiceStyle)
{
    public static TextAnalysisResult Default => new(
        "en", "general", 0.5, 0, 0.5, "narrative");
}

internal record OpenAIAnalysisResponse(
    [property: JsonPropertyName("language")] string? Language,
    [property: JsonPropertyName("content_type")] string? ContentType,
    [property: JsonPropertyName("emotion_score")] double? EmotionScore,
    [property: JsonPropertyName("dialogue_ratio")] double? DialogueRatio,
    [property: JsonPropertyName("complexity_score")] double? ComplexityScore,
    [property: JsonPropertyName("suggested_voice_style")] string? SuggestedVoiceStyle
);
```

### 6.2 Azure Cognitive Services Alternative

```csharp
// VoiceProcessor.Infrastructure/ML/AzureCognitiveAnalyzer.cs
public class AzureCognitiveAnalyzer : ITextAnalyzer
{
    private readonly TextAnalyticsClient _client;

    public AzureCognitiveAnalyzer(IOptions<AzureCognitiveOptions> options)
    {
        _client = new TextAnalyticsClient(
            new Uri(options.Value.Endpoint),
            new AzureKeyCredential(options.Value.ApiKey));
    }

    public async Task<TextAnalysisResult> AnalyzeAsync(string text)
    {
        // Language detection
        var languageResult = await _client.DetectLanguageAsync(text);
        var language = languageResult.Value.Iso6391Name;

        // Sentiment analysis
        var sentimentResult = await _client.AnalyzeSentimentAsync(text, language);
        var emotionScore = sentimentResult.Value.ConfidenceScores.Positive;

        // For content type, still use OpenAI (Azure doesn't have this)
        // Or implement rule-based classification

        return new TextAnalysisResult(
            Language: language,
            ContentType: ClassifyContentType(text), // Rule-based fallback
            EmotionScore: emotionScore,
            DialogueRatio: CalculateDialogueRatio(text),
            ComplexityScore: CalculateComplexity(text),
            SuggestedVoiceStyle: "narrative"
        );
    }

    private string ClassifyContentType(string text)
    {
        // Simple rule-based classification
        var dialogueMarkers = text.Count(c => c == '"') / 2;
        var dialogueRatio = dialogueMarkers * 50.0 / text.Length;

        if (dialogueRatio > 0.1) return "fiction_dialogue";
        if (text.Contains("Chapter") || text.Length > 10000) return "fiction_narrative";
        if (text.Contains("function") || text.Contains("install")) return "technical";
        return "general";
    }

    private double CalculateDialogueRatio(string text)
    {
        var inQuotes = false;
        var quotedChars = 0;

        foreach (var c in text)
        {
            if (c == '"') inQuotes = !inQuotes;
            else if (inQuotes) quotedChars++;
        }

        return (double)quotedChars / text.Length;
    }

    private double CalculateComplexity(string text)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var avgWordLength = words.Average(w => w.Length);
        // Normalize to 0-1 (avg word length 4-8 maps to 0-1)
        return Math.Clamp((avgWordLength - 4) / 4, 0, 1);
    }
}
```

### 6.3 Hybrid Approach (Recommended)

```csharp
// Use both: Azure for language/sentiment, OpenAI for classification
public class HybridTextAnalyzer : ITextAnalyzer
{
    private readonly TextAnalyticsClient _azureClient;
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<HybridTextAnalyzer> _logger;

    public async Task<TextAnalysisResult> AnalyzeAsync(string text)
    {
        // Run in parallel
        var languageTask = DetectLanguageAsync(text);
        var sentimentTask = AnalyzeSentimentAsync(text);
        var classificationTask = ClassifyContentAsync(text);

        await Task.WhenAll(languageTask, sentimentTask, classificationTask);

        return new TextAnalysisResult(
            Language: await languageTask,
            ContentType: (await classificationTask).ContentType,
            EmotionScore: await sentimentTask,
            DialogueRatio: CalculateDialogueRatio(text),
            ComplexityScore: (await classificationTask).Complexity,
            SuggestedVoiceStyle: (await classificationTask).VoiceStyle
        );
    }

    private async Task<string> DetectLanguageAsync(string text)
    {
        // Azure is very accurate and cheap for this
        var result = await _azureClient.DetectLanguageAsync(text.Substring(0, Math.Min(500, text.Length)));
        return result.Value.Iso6391Name;
    }

    private async Task<double> AnalyzeSentimentAsync(string text)
    {
        // Azure sentiment analysis
        var result = await _azureClient.AnalyzeSentimentAsync(text.Substring(0, Math.Min(5000, text.Length)));
        return result.Value.ConfidenceScores.Positive;
    }

    private async Task<(string ContentType, double Complexity, string VoiceStyle)> ClassifyContentAsync(string text)
    {
        // OpenAI for nuanced classification
        // ... (use GPT-4o-mini as shown earlier)
    }
}
```

### 6.4 Cost Comparison

| Service | Task | Cost per 1K requests |
|---------|------|---------------------|
| OpenAI GPT-4o-mini | Classification | ~$0.15 |
| Azure Language | Language detection | ~$1.00 |
| Azure Language | Sentiment | ~$1.00 |
| OpenAI GPT-4o | Classification (better) | ~$2.50 |

**Recommendation:** OpenAI GPT-4o-mini for everything — it's cheaper and does all tasks in one call.

---

## 7. Authentication

### 7.1 ASP.NET Core Identity + JWT

```csharp
// Program.cs
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
    };
});
```

### 7.2 API Key Authentication (For Developer API)

```csharp
// Custom API Key authentication handler
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IApiKeyService _apiKeyService;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
            return AuthenticateResult.NoResult();

        var apiKey = apiKeyHeader.ToString();
        var keyInfo = await _apiKeyService.ValidateKeyAsync(apiKey);

        if (keyInfo is null)
            return AuthenticateResult.Fail("Invalid API key");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, keyInfo.UserId.ToString()),
            new Claim("ApiKeyId", keyInfo.KeyId.ToString()),
            new Claim("Scope", string.Join(",", keyInfo.Scopes))
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
```

---

## 8. Infrastructure

### 8.1 Hosting Recommendations

| Phase | Frontend | Backend | Database | Cost |
|-------|----------|---------|----------|------|
| MVP | Vercel | Railway or Render | Supabase/Neon (free) | ~$10-25/mo |
| Growth | Vercel Pro | Railway | Supabase Pro | ~$100-150/mo |
| Scale | Vercel | Azure Container Apps | Azure PostgreSQL | ~$400-500/mo |

### 8.2 Alternative: All-Azure Stack

```
AZURE-NATIVE STACK
│
├── Frontend: Azure Static Web Apps (free tier)
├── Backend: Azure App Service or Container Apps
├── Database: Azure Database for PostgreSQL
├── Cache: Azure Cache for Redis
├── Storage: Azure Blob Storage
├── Queue: Azure Service Bus (or Hangfire with PostgreSQL)
├── ML: Azure Cognitive Services + OpenAI
└── Monitoring: Application Insights
```

### 8.3 Recommended: Budget Stack (MVP)

```
BUDGET STACK (~$10-25/month) ← RECOMMENDED FOR MVP
│
├── Frontend: Vercel (free)
├── Backend: Railway ($5-20)
├── Database: Supabase free tier (PostgreSQL + pgvector)
├── Cache: Upstash free tier (Redis)
├── Storage: Cloudflare R2 (cheap, no egress fees)
├── ML: OpenAI API (pay per use)
└── Monitoring: Sentry free tier
```

### 8.4 Docker Configuration

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["VoiceProcessor.Api/VoiceProcessor.Api.csproj", "VoiceProcessor.Api/"]
COPY ["VoiceProcessor.Application/VoiceProcessor.Application.csproj", "VoiceProcessor.Application/"]
COPY ["VoiceProcessor.Domain/VoiceProcessor.Domain.csproj", "VoiceProcessor.Domain/"]
COPY ["VoiceProcessor.Infrastructure/VoiceProcessor.Infrastructure.csproj", "VoiceProcessor.Infrastructure/"]
RUN dotnet restore "VoiceProcessor.Api/VoiceProcessor.Api.csproj"
COPY . .
WORKDIR "/src/VoiceProcessor.Api"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VoiceProcessor.Api.dll"]
```

```yaml
# docker-compose.yml
version: '3.8'

services:
  api:
    build: .
    ports:
      - "5000:8080"
    environment:
      - ConnectionStrings__DefaultConnection=Host=db;Database=voiceprocessor;Username=postgres;Password=postgres
      - Redis__ConnectionString=redis:6379
      - OpenAI__ApiKey=${OPENAI_API_KEY}
      - ElevenLabs__ApiKey=${ELEVENLABS_API_KEY}
    depends_on:
      - db
      - redis

  hangfire:
    build: .
    command: ["dotnet", "VoiceProcessor.Api.dll", "--hangfire-server"]
    environment:
      - ConnectionStrings__DefaultConnection=Host=db;Database=voiceprocessor;Username=postgres;Password=postgres
      - Redis__ConnectionString=redis:6379
    depends_on:
      - db
      - redis

  db:
    image: pgvector/pgvector:pg16
    environment:
      - POSTGRES_DB=voiceprocessor
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

volumes:
  pgdata:
```

---

## 9. Data Collection Strategy

### 9.1 What to Store (For Future ML)

```csharp
// Store comprehensive data from day one
public class GenerationAnalytics
{
    public Guid Id { get; set; }
    public Guid GenerationId { get; set; }

    // === INPUT FEATURES (for routing ML) ===
    public int TextLength { get; set; }
    public string Language { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public double EmotionScore { get; set; }
    public double DialogueRatio { get; set; }
    public double ComplexityScore { get; set; }
    public int ParagraphCount { get; set; }
    public int SentenceCount { get; set; }
    public double AvgSentenceLength { get; set; }

    // === ROUTING DECISION ===
    public string ProviderSelected { get; set; } = string.Empty;
    public string VoiceSelected { get; set; } = string.Empty;
    public string RoutingStrategy { get; set; } = string.Empty;
    public string RoutingReason { get; set; } = string.Empty;
    public List<string> ProvidersConsidered { get; set; } = [];

    // === EXECUTION METRICS ===
    public decimal ActualCostCents { get; set; }
    public decimal EstimatedCostCents { get; set; }
    public int TotalLatencyMs { get; set; }
    public int ProviderLatencyMs { get; set; }
    public int ChunksProcessed { get; set; }
    public int ChunksFailed { get; set; }
    public int RetriesCount { get; set; }
    public bool FallbackUsed { get; set; }
    public string? FallbackProvider { get; set; }

    // === QUALITY METRICS ===
    public int AudioDurationMs { get; set; }
    public long FileSizeBytes { get; set; }
    public double? AudioQualityScore { get; set; } // If we add automated quality check

    // === USER FEEDBACK (The ML Label) ===
    public int? UserRating { get; set; }
    public bool Regenerated { get; set; }
    public string? RegeneratedWithProvider { get; set; }
    public bool Downloaded { get; set; }
    public bool PlayedFully { get; set; }
    public int PlayDurationSeconds { get; set; }
    public List<string> FeedbackTags { get; set; } = [];

    // === CONTEXT ===
    public Guid UserId { get; set; }
    public string UserTier { get; set; } = string.Empty; // free, pro, enterprise
    public DateTime CreatedAt { get; set; }
    public string? UserAgent { get; set; }
    public string? Country { get; set; }
}
```

### 9.2 Analytics Service

```csharp
public class AnalyticsService : IAnalyticsService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<AnalyticsService> _logger;

    public async Task RecordGenerationAsync(
        Generation generation,
        TextAnalysisResult analysis,
        RoutingDecision routing,
        ExecutionMetrics metrics)
    {
        var analytics = new GenerationAnalytics
        {
            GenerationId = generation.Id,

            // Input features
            TextLength = generation.TextLength,
            Language = analysis.Language,
            ContentType = analysis.ContentType,
            EmotionScore = analysis.EmotionScore,
            DialogueRatio = analysis.DialogueRatio,
            ComplexityScore = analysis.ComplexityScore,

            // Routing
            ProviderSelected = routing.Provider,
            VoiceSelected = routing.VoiceId,
            RoutingStrategy = routing.Strategy,
            RoutingReason = routing.Reason,
            ProvidersConsidered = routing.Alternatives,

            // Execution
            ActualCostCents = metrics.Cost,
            TotalLatencyMs = metrics.TotalLatencyMs,
            ChunksProcessed = metrics.ChunksProcessed,

            // Context
            UserId = generation.UserId,
            CreatedAt = DateTime.UtcNow
        };

        _db.GenerationAnalytics.Add(analytics);
        await _db.SaveChangesAsync();
    }

    public async Task RecordFeedbackAsync(Guid generationId, FeedbackRequest feedback)
    {
        var analytics = await _db.GenerationAnalytics
            .FirstOrDefaultAsync(a => a.GenerationId == generationId);

        if (analytics is null) return;

        analytics.UserRating = feedback.Rating;
        analytics.FeedbackTags = feedback.Tags ?? [];

        await _db.SaveChangesAsync();
    }

    public async Task RecordPlaybackAsync(Guid generationId, int durationSeconds, bool completed)
    {
        var analytics = await _db.GenerationAnalytics
            .FirstOrDefaultAsync(a => a.GenerationId == generationId);

        if (analytics is null) return;

        analytics.PlayDurationSeconds = durationSeconds;
        analytics.PlayedFully = completed;

        await _db.SaveChangesAsync();
    }
}
```

### 9.3 Future ML Training Query

When you have enough data, training a routing model is straightforward:

```sql
-- Export training data for routing optimization (PostgreSQL)
SELECT
    -- Features
    text_length,
    language,
    content_type,
    emotion_score,
    dialogue_ratio,
    complexity_score,

    -- Label (what we want to predict)
    provider_selected,

    -- Outcome (for weighting)
    user_rating,
    regenerated,
    played_fully
FROM generation_analytics
WHERE user_rating IS NOT NULL  -- Only use rated generations
  AND created_at > NOW() - INTERVAL '6 months'  -- Last 6 months
```

---

## 10. Monitoring

### 10.1 Application Insights (Azure)

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();

// Custom telemetry
public class GenerationTelemetry
{
    private readonly TelemetryClient _telemetry;

    public void TrackGeneration(Generation generation, TimeSpan duration)
    {
        _telemetry.TrackEvent("GenerationCompleted", new Dictionary<string, string>
        {
            ["Provider"] = generation.ProviderUsed,
            ["Language"] = generation.Language,
            ["ContentType"] = generation.ContentType
        }, new Dictionary<string, double>
        {
            ["Duration"] = duration.TotalMilliseconds,
            ["Cost"] = (double)generation.CostCents,
            ["Characters"] = generation.TextLength
        });
    }
}
```

### 10.2 Sentry (Alternative/Addition)

```csharp
// Program.cs
builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"];
    options.TracesSampleRate = 0.2; // 20% of transactions
});
```

---

## 11. Project Structure

```
VoiceProcessor/
├── src/
│   ├── VoiceProcessor.Api/
│   │   ├── Controllers/
│   │   │   ├── GenerationsController.cs
│   │   │   ├── VoicesController.cs
│   │   │   ├── UsersController.cs
│   │   │   └── WebhooksController.cs
│   │   ├── Hubs/
│   │   │   └── GenerationHub.cs
│   │   ├── Middleware/
│   │   │   ├── RateLimitingMiddleware.cs
│   │   │   └── ApiKeyMiddleware.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── VoiceProcessor.Api.csproj
│   │
│   ├── VoiceProcessor.Application/
│   │   ├── Services/
│   │   │   ├── GenerationService.cs
│   │   │   ├── RoutingService.cs
│   │   │   ├── ChunkingService.cs
│   │   │   ├── AnalysisService.cs
│   │   │   └── AudioMergeService.cs
│   │   ├── Interfaces/
│   │   │   ├── IGenerationService.cs
│   │   │   ├── IRoutingService.cs
│   │   │   └── ITTSProvider.cs
│   │   ├── DTOs/
│   │   └── VoiceProcessor.Application.csproj
│   │
│   ├── VoiceProcessor.Domain/
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Generation.cs
│   │   │   ├── GenerationChunk.cs
│   │   │   ├── GenerationFeedback.cs
│   │   │   ├── GenerationAnalytics.cs
│   │   │   ├── Voice.cs
│   │   │   └── ApiKey.cs
│   │   ├── Enums/
│   │   └── VoiceProcessor.Domain.csproj
│   │
│   ├── VoiceProcessor.Infrastructure/
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   └── Migrations/
│   │   ├── Providers/
│   │   │   ├── ElevenLabsProvider.cs
│   │   │   ├── OpenAITTSProvider.cs
│   │   │   ├── GoogleTTSProvider.cs
│   │   │   └── AmazonPollyProvider.cs
│   │   ├── ML/
│   │   │   ├── OpenAIAnalyzer.cs
│   │   │   └── AzureCognitiveAnalyzer.cs
│   │   ├── Storage/
│   │   │   └── BlobStorageService.cs
│   │   └── VoiceProcessor.Infrastructure.csproj
│   │
│   └── VoiceProcessor.Jobs/
│       ├── GenerationJob.cs
│       ├── AudioMergeJob.cs
│       ├── CleanupJob.cs
│       └── VoiceProcessor.Jobs.csproj
│
├── tests/
│   ├── VoiceProcessor.UnitTests/
│   ├── VoiceProcessor.IntegrationTests/
│   └── VoiceProcessor.E2ETests/
│
├── docker-compose.yml
├── Dockerfile
├── VoiceProcessor.sln
└── README.md
```

---

## 12. Cost Estimates

### 12.1 MVP Phase

| Service | Provider | Monthly Cost |
|---------|----------|--------------|
| Backend | Railway | $5-10 |
| Database | Supabase Free (PostgreSQL) | $0 |
| Redis | Upstash Free | $0 |
| Storage | Cloudflare R2 | $5 |
| Frontend | Vercel Free | $0 |
| OpenAI API | ~10K classifications | $2 |
| Monitoring | Sentry Free | $0 |
| **Total** | | **~$12-17/month** |

### 12.2 Growth Phase

| Service | Provider | Monthly Cost |
|---------|----------|--------------|
| Backend | Railway (scaled) | $20-50 |
| Database | Supabase Pro (PostgreSQL) | $25 |
| Redis | Upstash Pro | $10 |
| Storage | Cloudflare R2 | $20 |
| Frontend | Vercel Pro | $20 |
| OpenAI API | ~100K classifications | $15 |
| Monitoring | Sentry Team | $26 |
| **Total** | | **~$135-165/month** |

---

## 13. Getting Started

### 13.1 Prerequisites

```bash
# Required
- .NET 8 SDK
- Docker Desktop
- Visual Studio 2022 or VS Code with C# extension
- Node.js 20+ (for frontend)

# Accounts needed
- OpenAI API key
- ElevenLabs API key (free tier available)
- Azure account (free tier available)
```

### 13.2 Setup Checklist

```
□ Create solution and projects
  dotnet new sln -n VoiceProcessor
  dotnet new webapi -n VoiceProcessor.Api
  dotnet new classlib -n VoiceProcessor.Application
  dotnet new classlib -n VoiceProcessor.Domain
  dotnet new classlib -n VoiceProcessor.Infrastructure

□ Add project references
□ Install NuGet packages
□ Configure EF Core and create initial migration
□ Set up Docker Compose for local development
□ Implement ITTSProvider interface
□ Create ElevenLabs provider (port your existing code)
□ Set up OpenAI analyzer
□ Create Generation endpoint
□ Set up Hangfire for background jobs
□ Test end-to-end generation
□ Set up frontend (Next.js or Blazor)
□ Deploy to Azure/Railway
```

### 13.3 First API Call Test

```bash
# After running locally
curl -X POST http://localhost:5000/api/v1/generations \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "text": "Hello, this is a test of the voice processor system.",
    "routing": {
      "strategy": "balanced"
    }
  }'
```

---

## Summary

This .NET stack gives you:

- **Familiar territory** — C#, ASP.NET Core (your expertise)
- **Future-proof database** — PostgreSQL with pgvector for ML embeddings
- **No ML complexity** — APIs handle all AI tasks (OpenAI, Azure)
- **Data ready for future** — Analytics stored from day one
- **Scalable architecture** — Clean separation, background jobs
- **Cost effective** — Start at ~$12-17/month with free tiers

### Final Technology Stack

```
VOICEPROCESSOR - FINAL STACK
│
├── Backend: ASP.NET Core 8 + C# 12
├── Database: PostgreSQL 16 + pgvector (Supabase)
├── Cache: Redis (Upstash)
├── Queue: Hangfire + PostgreSQL
├── Storage: Cloudflare R2
├── ML: OpenAI API + Azure Cognitive Services
├── Frontend: Next.js (or Blazor)
└── Hosting: Railway + Vercel
```

You can build and maintain this yourself without depending on Python expertise. When you're ready, I can help you scaffold the initial project structure.
