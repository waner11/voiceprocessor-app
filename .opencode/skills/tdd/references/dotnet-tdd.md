# .NET TDD Reference

## Test Stack

| Library | Version | Purpose |
|---------|---------|---------|
| xUnit | 2.9.3 | Test framework |
| Moq | 4.20.72 | Mocking |
| FluentAssertions | 8.8.0 | Readable assertions |
| Microsoft.NET.Test.Sdk | 17.14.1 | Test runner |

Target: `net10.0`

## Test Projects

```
apps/api/tests/
├── VoiceProcessor.Managers.Tests/    # Orchestration layer tests
│   └── Generation/
│       └── GenerationManagerTests.cs
└── VoiceProcessor.Engines.Tests/     # Business logic layer tests
    ├── Chunking/
    │   └── ChunkingEngineTests.cs
    └── Pricing/
        └── PricingEngineTests.cs
```

**Where to create new tests:**
- Testing a Manager → `VoiceProcessor.Managers.Tests/{Domain}/{ClassName}Tests.cs`
- Testing an Engine → `VoiceProcessor.Engines.Tests/{Domain}/{ClassName}Tests.cs`
- New Accessor tests → Create `VoiceProcessor.Accessors.Tests` project if needed

## Commands

```bash
# Run ALL tests
dotnet test

# Run single test project
dotnet test tests/VoiceProcessor.Managers.Tests

# Run single test class
dotnet test --filter "FullyQualifiedName~VoiceProcessor.Managers.Tests.Generation.GenerationManagerTests"

# Run single test method (most useful for TDD)
dotnet test --filter "FullyQualifiedName~GenerationManagerTests.CreateGeneration_WithValidInput_ReturnsPendingGeneration"

# Verbose output (see assertion details on failure)
dotnet test --verbosity detailed
```

**TDD loop command** (run after each change):
```bash
dotnet test --filter "FullyQualifiedName~<YourTestClass>.<YourTestMethod>"
```

## Test Class Structure

```csharp
namespace VoiceProcessor.Managers.Tests.Generation;

public class GenerationManagerTests
{
    // 1. Mock fields (private readonly)
    private readonly Mock<IGenerationAccessor> _mockGenerationAccessor;
    private readonly Mock<IVoiceAccessor> _mockVoiceAccessor;
    private readonly Mock<IChunkingEngine> _mockChunkingEngine;
    private readonly Mock<IPricingEngine> _mockPricingEngine;
    private readonly Mock<IRoutingEngine> _mockRoutingEngine;
    private readonly Mock<ILogger<GenerationManager>> _mockLogger;

    // 2. Constructor initializes all mocks
    public GenerationManagerTests()
    {
        _mockGenerationAccessor = new Mock<IGenerationAccessor>();
        _mockVoiceAccessor = new Mock<IVoiceAccessor>();
        _mockChunkingEngine = new Mock<IChunkingEngine>();
        _mockPricingEngine = new Mock<IPricingEngine>();
        _mockRoutingEngine = new Mock<IRoutingEngine>();
        _mockLogger = new Mock<ILogger<GenerationManager>>();
    }

    // 3. Factory method creates the system under test
    private GenerationManager CreateManager()
    {
        return new GenerationManager(
            _mockGenerationAccessor.Object,
            _mockVoiceAccessor.Object,
            _mockChunkingEngine.Object,
            _mockPricingEngine.Object,
            _mockRoutingEngine.Object,
            _mockLogger.Object
        );
    }

    // 4. Test methods
    [Fact]
    public async Task MethodName_Scenario_ExpectedResult()
    {
        // Arrange
        var manager = CreateManager();
        // ... setup

        // Act
        var result = await manager.SomeMethodAsync(...);

        // Assert
        result.Should().NotBeNull();
    }
}
```

## Test Naming

Pattern: `MethodName_Scenario_ExpectedResult`

```csharp
[Fact]
public async Task CreateGeneration_WithValidInput_ReturnsPendingGeneration()

[Fact]
public async Task CreateGeneration_WhenVoiceNotFound_ThrowsInvalidOperation()

[Fact]
public async Task CreateGeneration_WhenInsufficientCredits_ThrowsInvalidOperation()

[Fact]
public async Task EstimateCost_WithLargeText_ReturnsCorrectChunkCount()

[Fact]
public void SplitText_EmptyString_ReturnsEmptyList()

[Fact]
public void CalculateEstimate_WithProviderOverride_UsesOverrideRate()
```

## Arrange-Act-Assert Pattern

### Arrange: Setup Mocks

```csharp
// Return a value
_mockVoiceAccessor
    .Setup(x => x.GetByIdAsync(voiceId, It.IsAny<CancellationToken>()))
    .ReturnsAsync(voice);

// Return null (not found)
_mockVoiceAccessor
    .Setup(x => x.GetByIdAsync(voiceId, It.IsAny<CancellationToken>()))
    .ReturnsAsync((Voice?)null);

// Throw exception
_mockRoutingEngine
    .Setup(x => x.SelectProviderAsync(It.IsAny<RoutingContext>(), It.IsAny<CancellationToken>()))
    .ThrowsAsync(new InvalidOperationException("Service unavailable"));

// Capture argument for later assertion
Domain.Entities.Generation? captured = null;
_mockGenerationAccessor
    .Setup(x => x.CreateAsync(It.IsAny<Generation>(), It.IsAny<CancellationToken>()))
    .Callback<Generation, CancellationToken>((g, ct) => captured = g)
    .ReturnsAsync((Generation g, CancellationToken ct) => g);

// Match specific arguments
_mockPricingEngine
    .Setup(x => x.CalculateEstimate(It.Is<PricingContext>(c => c.CharacterCount == 500)))
    .Returns(new PriceEstimate { EstimatedCost = 0.05m });
```

### Act: Call the Method

```csharp
// Normal call
var result = await manager.CreateGenerationAsync(userId, request);

// For exception testing, capture the act
Func<Task> act = () => manager.CreateGenerationAsync(userId, request);
```

### Assert: Verify Results

```csharp
// FluentAssertions on return values
result.Should().NotBeNull();
result.Status.Should().Be(GenerationStatus.Pending);
result.CharacterCount.Should().Be(500);
result.EstimatedCost.Should().Be(0.05m);
result.ProviderEstimates.Should().HaveCount(2);
result.ProviderEstimates.Should().Contain(x => x.Provider == Provider.ElevenLabs);

// FluentAssertions on exceptions
await act.Should().ThrowAsync<InvalidOperationException>()
    .WithMessage("*not found*");

await act.Should().ThrowAsync<InvalidOperationException>()
    .WithMessage("Insufficient credits");

// Verify mock interactions
_mockGenerationAccessor.Verify(
    x => x.CreateAsync(It.IsAny<Generation>(), It.IsAny<CancellationToken>()),
    Times.Once);

_mockGenerationAccessor.Verify(
    x => x.CreateAsync(It.IsAny<Generation>(), It.IsAny<CancellationToken>()),
    Times.Never);  // Should NOT have been called

// Verify captured arguments
captured.Should().NotBeNull();
captured!.UserId.Should().Be(userId);
captured.Status.Should().Be(GenerationStatus.Pending);
```

## TDD by iDesign Layer

### Testing Engines (Pure Logic — Easiest to TDD)

Engines are stateless with no I/O. Perfect TDD candidates.

```csharp
public class ChunkingEngineTests
{
    private readonly ChunkingEngine _engine = new();

    [Fact]
    public void SplitText_ShortText_ReturnsSingleChunk()
    {
        // Arrange
        var text = "Hello world";

        // Act
        var result = _engine.SplitText(text);

        // Assert
        result.Should().HaveCount(1);
        result[0].Text.Should().Be(text);
    }

    [Fact]
    public void SplitText_TextExceedingMaxSize_SplitsAtSentenceBoundary()
    {
        // Arrange
        var text = "First sentence. Second sentence. Third sentence.";
        var options = new ChunkingOptions { MaxChunkSize = 20 };

        // Act
        var result = _engine.SplitText(text, options);

        // Assert
        result.Should().HaveCountGreaterThan(1);
        result.All(c => c.Text.Length <= 20).Should().BeTrue();
    }
}
```

No mocks needed. Input → Output. This is where TDD shines brightest.

### Testing Managers (Orchestration — Mock Dependencies)

Managers coordinate Engines and Accessors. Mock all dependencies.

```csharp
public class GenerationManagerTests
{
    // ... mocks in constructor ...

    [Fact]
    public async Task CreateGeneration_WithValidInput_CallsChunkingEngineThenCreatesRecord()
    {
        // Arrange
        var manager = CreateManager();
        SetupValidVoice();
        SetupValidUser(creditsRemaining: 1000);
        SetupChunkingEngine(chunkCount: 3);
        SetupPricingEngine(cost: 0.05m, credits: 50);
        SetupRoutingEngine(provider: Provider.ElevenLabs);

        // Act
        var result = await manager.CreateGenerationAsync(userId, request);

        // Assert — verify orchestration order and result
        result.Status.Should().Be(GenerationStatus.Pending);
        _mockChunkingEngine.Verify(x => x.SplitText(request.Text, null), Times.Once);
        _mockGenerationAccessor.Verify(x => x.CreateAsync(
            It.Is<Generation>(g => g.ChunkCount == 3),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // Helper methods to reduce setup duplication
    private void SetupValidVoice() { /* ... */ }
    private void SetupValidUser(int creditsRemaining) { /* ... */ }
}
```

Focus on: **correct calls in correct order**, not internal logic.

### Testing Accessors (I/O — Integration Tests)

Accessors touch databases/APIs. Usually integration tests, not pure TDD.
For unit testing Accessors, use in-memory database:

```csharp
public class GenerationAccessorTests
{
    private VoiceProcessorDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<VoiceProcessorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new VoiceProcessorDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_ValidGeneration_PersistsToDatabase()
    {
        // Arrange
        using var db = CreateDbContext();
        var accessor = new GenerationAccessor(db, _mockLogger.Object);
        var generation = new Generation { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };

        // Act
        var result = await accessor.CreateAsync(generation);

        // Assert
        var saved = await db.Generations.FindAsync(generation.Id);
        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(generation.UserId);
    }
}
```

## TDD Cycle Example: New Engine Method

Goal: Add `EstimateChunkCount` to `ChunkingEngine`.

```
Step 1 (RED):
  Write: ChunkingEngineTests.EstimateChunkCount_ShortText_ReturnsOne
  Run: dotnet test --filter "EstimateChunkCount_ShortText_ReturnsOne"
  Result: FAIL (method doesn't exist)

Step 2 (GREEN):
  Add method: public int EstimateChunkCount(string text) => 1;
  Run: PASS ✓

Step 3 (RED):
  Write: EstimateChunkCount_LongText_ReturnsCorrectCount
  Run: FAIL (returns 1, expected 5)

Step 4 (GREEN):
  Implement: actual character counting logic
  Run: PASS ✓

Step 5 (RED):
  Write: EstimateChunkCount_EmptyText_ReturnsZero
  Run: FAIL (returns 1)

Step 6 (GREEN):
  Add: if (string.IsNullOrEmpty(text)) return 0;
  Run: PASS ✓

Step 7 (REFACTOR):
  Extract magic numbers, improve readability
  Run ALL tests: ALL PASS ✓

Step 8: Commit
  git commit -m "add chunk count estimation to chunking engine"
```
