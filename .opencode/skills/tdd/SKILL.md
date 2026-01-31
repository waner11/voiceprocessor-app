---
name: tdd
description: >
  Test-Driven Development methodology enforcer. MUST USE when implementing any new
  feature, bug fix, or behavior change in both backend (.NET) and frontend (React/Next.js).
  Enforces strict Red→Green→Refactor cycle: write a failing test first, write minimal
  code to pass, then refactor. Triggers: "implement", "add feature", "fix bug", "build",
  "create endpoint", "add component", "new hook", any implementation task. Does NOT
  apply to: research, reviews, documentation, configuration-only changes.
compatibility: opencode
metadata:
  author: voiceprocessor-team
---

# Test-Driven Development

You follow strict TDD. Every behavior change starts with a failing test. No exceptions.

## The Iron Rule

> **Never write production code without a failing test that demands it.**

If you catch yourself writing implementation code first, STOP. Delete it. Write the test.

## The Cycle: Red → Green → Refactor

### RED: Write a Failing Test

1. **Think about the behavior** you need, not the implementation
2. **Write one test** that describes that behavior
3. **Run it** — it MUST fail (if it passes, you're testing something that already works)
4. **Verify it fails for the RIGHT reason** — missing method, wrong return value, not a compilation error

```
Test written → Run → FAIL (red) → Proceed to GREEN
Test written → Run → PASS → STOP. You're not testing new behavior. Rethink.
Test written → Run → COMPILE ERROR → Fix only the compilation, not the logic. Re-run → FAIL → Proceed.
```

### GREEN: Make It Pass (Minimal Code)

1. **Write the simplest code** that makes the test pass
2. **Do not generalize** — solve only what the test asks for
3. **Hardcode if needed** — the next test will force you to generalize
4. **Run the test** — it MUST pass now
5. **Run ALL tests** — nothing else should break

```
Write minimal code → Run failing test → PASS (green) → Proceed to REFACTOR
Write minimal code → Run failing test → STILL FAILS → Fix. Don't add more. Re-run.
Write minimal code → Run ALL tests → SOMETHING ELSE BREAKS → Fix regression first.
```

### REFACTOR: Clean Up (Tests Stay Green)

1. **Improve the code** — extract methods, rename variables, reduce duplication
2. **Do NOT add new behavior** — refactoring changes structure, not behavior
3. **Run ALL tests after every change** — if anything goes red, undo and try again
4. **Improve tests too** — remove duplication, improve naming, extract helpers

```
Refactor → Run all tests → ALL GREEN → Done. Pick next behavior.
Refactor → Run all tests → SOMETHING RED → Undo last change. Try different approach.
```

## Practical Workflow

### Step 1: Identify Behaviors

Before coding, list the behaviors you need to implement:

```markdown
Feature: Create Generation
Behaviors:
1. Returns generation response with pending status
2. Rejects when voice ID doesn't exist
3. Rejects when user has insufficient credits
4. Splits text into chunks using chunking engine
5. Routes to optimal provider
6. Queues background processing job
```

### Step 2: Order by Complexity

Start with the simplest behavior. Each test builds on previous ones.

**Good order**: Happy path basics → Input validation → Edge cases → Error handling → Integration

### Step 3: One Test at a Time

```
For each behavior:
  1. Write ONE test (RED)
  2. Make it pass (GREEN)  
  3. Refactor if needed
  4. Commit if the behavior is complete
  5. Move to next behavior
```

### Step 4: Commit Cadence

Commit after each Green→Refactor cycle when the behavior is meaningful:
- `test: add generation creation with pending status`
- `feat: implement generation creation`
- `test: reject generation when voice not found`
- `feat: handle missing voice validation`

Small, atomic commits. Each one leaves tests green.

## What to Test (and What Not To)

### ALWAYS Test

| Layer | What to Test |
|-------|-------------|
| **Engine/Hook logic** | Business rules, calculations, state transitions |
| **Manager orchestration** | Correct calls in correct order, error propagation |
| **Component behavior** | What the user sees and does (not implementation) |
| **API contracts** | Request/response shapes, status codes |
| **Edge cases** | Null, empty, boundary values, concurrent access |
| **Error paths** | What happens when things fail |

### NEVER Test

- Framework internals (don't test that React renders)
- Private methods directly (test through public API)
- Implementation details (don't assert internal state)
- Third-party library behavior (trust their tests)
- Trivial code (simple property assignment, constructors with no logic)

## TDD Anti-Patterns

| Anti-Pattern | Why It's Wrong | Fix |
|-------------|---------------|-----|
| Writing code first, tests after | Tests become verification, not specification | Delete code, write test first |
| Testing implementation, not behavior | Brittle tests that break on refactor | Test inputs → outputs |
| Writing multiple tests before any GREEN | Lost focus, unclear which behavior you're building | One test at a time |
| Making test pass with over-engineered solution | Violates "simplest code" rule | Strip back to minimum |
| Skipping REFACTOR step | Technical debt accumulates | Always assess after GREEN |
| Not running ALL tests | Regressions hide | Run full suite after every GREEN |
| Testing private internals | Couples tests to implementation | Test through public interface |
| Giant test methods | Hard to understand what's being tested | One assertion per concept |

## Naming Tests

### Backend (.NET)
```
MethodName_Condition_ExpectedResult
```
Examples:
- `CreateGeneration_WithValidInput_ReturnsPendingGeneration`
- `CreateGeneration_WhenVoiceNotFound_ThrowsInvalidOperation`
- `CalculateEstimate_WithLargeText_ReturnsCorrectChunkCount`

### Frontend (React/TypeScript)
```
describe('ComponentOrHook', () => {
  it('does expected thing when condition', () => { ... })
})
```
Examples:
- `it('displays loading spinner while fetching generations')`
- `it('shows error toast when creation fails')`
- `it('disables submit button when text is empty')`

## Stack-Specific References

- **Backend (.NET)**: See [dotnet-tdd.md](references/dotnet-tdd.md) for xUnit patterns, mocking, test commands
- **Frontend (React)**: See [frontend-tdd.md](references/frontend-tdd.md) for Vitest/Testing Library patterns, test commands

## Decision: Unit vs Integration vs E2E

| Test Type | When | Speed | Confidence |
|-----------|------|-------|------------|
| **Unit** | Pure logic, engines, hooks, utilities | Fast | Logic correct |
| **Integration** | Manager orchestration, component + hook together | Medium | Parts work together |
| **E2E** | Critical user flows (auth, generation, payment) | Slow | System works end-to-end |

**TDD applies primarily to Unit and Integration tests.** E2E tests are written after implementation to verify complete flows.

**The Testing Trophy (preferred balance):**
```
    /  E2E  \        ← Few, critical paths only
   / Integration \   ← Some, verify layer interaction
  /    Unit Tests   \ ← Many, fast, cover business logic
```
