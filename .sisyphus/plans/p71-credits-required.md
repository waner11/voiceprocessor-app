# Add CreditsRequired to CostEstimateResponse

## Context

### Original Request
Bug fix: The `PricingEngine` calculates `CreditsRequired` for cost estimates, but this value is not exposed to API clients. Frontend cannot display credits alongside dollar cost.

### Interview Summary
**Key Discussions**:
- Confirmed this is a P1 bug blocking frontend
- User initially wanted unit tests, but no test infrastructure exists
- Deferred test setup to separate issue - just fix the bug now

**Research Findings**:
- `ProviderPriceEstimate` in `IPricingEngine.cs:37` already has `CreditsRequired` field
- `PricingEngine.CalculateAllProviderEstimates()` returns estimates with `CreditsRequired`
- `GenerationManager.EstimateCostAsync()` maps at lines 100-115 but drops the credits field
- `tests/` directory is empty - no test project exists

### Metis Review
**Identified Gaps** (addressed):
- Metis suggested only per-provider credits, but issue requires both top-level AND per-provider (following issue spec)
- Test infrastructure missing - deferred to separate issue

---

## Work Objectives

### Core Objective
Expose `CreditsRequired` field in the API cost estimate response so frontend can display credits to users.

### Concrete Deliverables
- `CostEstimateResponse` record with top-level `CreditsRequired` property
- `ProviderEstimate` record with `CreditsRequired` property  
- `GenerationManager.EstimateCostAsync()` mapping both fields

### Definition of Done
- [x] `GET /api/generations/estimate` returns `creditsRequired` at top level
- [x] Each provider estimate includes `creditsRequired`
- [x] `dotnet build` succeeds with no errors
- [x] API response verified via curl

### Must Have
- Top-level `CreditsRequired` on `CostEstimateResponse`
- Per-provider `CreditsRequired` on `ProviderEstimate`
- Mapping from `ProviderPriceEstimate.CreditsRequired`

### Must NOT Have (Guardrails)
- Do NOT modify `IPricingEngine.cs` or `PricingEngine.cs` (source already correct)
- Do NOT create test project (deferred)
- Do NOT change existing property types
- Do NOT add nullable types (use `int`, not `int?`)

---

## Verification Strategy (MANDATORY)

### Test Decision
- **Infrastructure exists**: NO
- **User wants tests**: DEFERRED to separate issue
- **Framework**: N/A

### Manual QA Verification
Each TODO includes specific curl commands to verify changes.

---

## Task Flow

```
Task 1 (DTO) → Task 2 (Mapping) → Task 3 (Verify)
```

## Parallelization

| Task | Depends On | Reason |
|------|------------|--------|
| 1 | None | Independent DTO change |
| 2 | 1 | Needs new properties to exist |
| 3 | 2 | Needs mapping complete |

---

## TODOs

- [x] 1. Add CreditsRequired properties to DTOs

  **What to do**:
  - Add `public required int CreditsRequired { get; init; }` to `CostEstimateResponse` record
  - Add `public int CreditsRequired { get; init; }` to `ProviderEstimate` record (not required - defaults to 0)

  **Must NOT do**:
  - Do not make nullable (`int?`)
  - Do not change other existing properties

  **Parallelizable**: NO (first task)

  **References**:
  
  **Pattern References**:
  - `src/VoiceProcessor.Domain/DTOs/Responses/CostEstimateResponse.cs:5-13` - Existing CostEstimateResponse structure
  - `src/VoiceProcessor.Domain/DTOs/Responses/CostEstimateResponse.cs:15-22` - Existing ProviderEstimate structure

  **API/Type References**:
  - `src/VoiceProcessor.Engines/Contracts/IPricingEngine.cs:28` - Source `CreditsRequired` field type (int)
  - `src/VoiceProcessor.Engines/Contracts/IPricingEngine.cs:37` - Per-provider `CreditsRequired` field type (int)

  **Acceptance Criteria**:

  **Build Verification**:
  - [ ] `dotnet build` → SUCCESS (will fail until Task 2 completes - missing mapping)

  **Commit**: NO (wait for Task 2)

---

- [x] 2. Update GenerationManager mapping to include CreditsRequired

  **What to do**:
  - In `EstimateCostAsync()`, add `CreditsRequired = bestEstimate.CreditsRequired` to CostEstimateResponse
  - In the ProviderEstimates LINQ Select, add `CreditsRequired = pe.CreditsRequired`

  **Must NOT do**:
  - Do not change any other mapping logic
  - Do not modify method signature

  **Parallelizable**: NO (depends on Task 1)

  **References**:

  **Pattern References**:
  - `src/VoiceProcessor.Managers/Generation/GenerationManager.cs:100-115` - Current mapping (add CreditsRequired here)
  - `src/VoiceProcessor.Managers/Generation/GenerationManager.cs:64-70` - Where `providerEstimates` comes from

  **API/Type References**:
  - `src/VoiceProcessor.Engines/Contracts/IPricingEngine.cs:33-38` - `ProviderPriceEstimate` record with CreditsRequired

  **Acceptance Criteria**:

  **Build Verification**:
  - [ ] `dotnet build` → SUCCESS (0 errors, 0 warnings expected)

  **Commit**: YES
  - Message: `fix(api): expose CreditsRequired in cost estimate response`
  - Files: `CostEstimateResponse.cs`, `GenerationManager.cs`
  - Pre-commit: `dotnet build`

---

- [x] 3. Verify API response includes CreditsRequired

  **What to do**:
  - Start the API server
  - Make a test request to the estimate endpoint
  - Verify JSON response contains `creditsRequired` at both levels

  **Must NOT do**:
  - Do not modify any code in this step
  - Do not commit anything

  **Parallelizable**: NO (depends on Task 2)

  **References**:

  **Documentation References**:
  - `src/VoiceProcessor.Clients.Api/Controllers/GenerationsController.cs` - Endpoint location

  **Acceptance Criteria**:

  **Manual API Verification**:
  - [ ] Start server: `dotnet run --project src/VoiceProcessor.Clients.Api`
  - [ ] Request: `curl -X POST http://localhost:5000/api/generations/estimate -H "Content-Type: application/json" -d '{"text": "Hello world test", "routingPreference": "Cost"}'`
  - [ ] Response contains: `"creditsRequired":` at top level
  - [ ] Response contains: `"creditsRequired":` in each providerEstimate object

  **Evidence Required**:
  - [ ] Copy-paste actual curl response showing creditsRequired fields

  **Commit**: NO (verification only)

---

## Commit Strategy

| After Task | Message | Files | Verification |
|------------|---------|-------|--------------|
| 2 | `fix(api): expose CreditsRequired in cost estimate response` | CostEstimateResponse.cs, GenerationManager.cs | dotnet build |

---

## Success Criteria

### Verification Commands
```bash
dotnet build  # Expected: Build succeeded
```

### Final Checklist
- [x] Top-level `creditsRequired` in API response
- [x] Per-provider `creditsRequired` in each estimate
- [x] Build passes
- [x] No changes to PricingEngine (already correct)

---

## Beads Reference
- **Issue**: `voiceprocessor-api-p71`
- **Type**: Bug
- **Priority**: P1
