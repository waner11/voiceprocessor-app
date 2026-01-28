# Display Credits Alongside Dollar Cost

## TL;DR

> **Quick Summary**: Add credits display to generation page cost estimate, showing "X,XXX credits ($Y.YY)" format alongside existing dollar cost. Includes type regeneration and unit tests.
> 
> **Deliverables**:
> - Regenerated TypeScript types with `creditsRequired` field
> - Updated generation page sidebar showing credits + dollars (top-level)
> - Added per-provider breakdown section to generation page with credits
> - Unit tests for formatting logic
> 
> **Estimated Effort**: Quick (< 2 hours)
> **Parallel Execution**: YES - 2 waves
> **Critical Path**: Task 1 (types) → Task 2+3 (parallel UI) → Task 4 (tests)

---

## Context

### Original Request
Display credits alongside dollar cost on the generation page in format "X,XXX credits ($Y.YY)".

### Interview Summary
**Key Discussions**:
- Format confirmed: "X,XXX credits ($Y.YY)" with comma separators
- Both top-level cost display and per-provider breakdown need credits
- User chose to regenerate types from API rather than manual edit
- User wants unit tests with Vitest

**Research Findings**:
- API already returns `creditsRequired` (verified in `CostEstimateResponse.cs`)
- Frontend `types.ts` is auto-generated via `openapi-typescript`
- Current display: `$${costEstimate.estimatedCost.toFixed(4)}` - no credits
- Existing test patterns in `apps/web/src/app/(app)/payment/__tests__/`

### Metis Review
**Identified Gaps** (addressed):
- Edge case: Handle null/undefined/0 `creditsRequired` → fallback to cost-only display
- Formatting consistency: Use `.toLocaleString()` for credits, match existing `.toFixed()` for dollars
- Test coverage: Include normal, zero, null, and large number cases
- Risk: Verify API spec includes field before type generation

### Momus Review (High Accuracy)
**Issues Found & Fixed**:
- CostEstimate component not used on generation page → Changed Task 3 to add inline per-provider breakdown
- Line number references corrected: cost display at lines 249-257
- Data flow clarified: explained how `costEstimate` response maps to UI
- Implementation patterns provided with concrete code snippets

---

## Work Objectives

### Core Objective
Show credits alongside dollar cost on the generation page so users understand the cost in both currencies (credits they purchased, dollars for reference).

### Concrete Deliverables
- `apps/web/src/lib/api/types.ts` - Regenerated with `creditsRequired` field
- `apps/web/src/app/(app)/generate/page.tsx` - Sidebar shows credits + dollars (top-level) AND per-provider breakdown
- `apps/web/src/app/(app)/generate/__tests__/formatCredits.test.ts` - Unit tests

### Definition of Done
- [x] Generation page sidebar shows: "X,XXX credits ($Y.YYYY)" as main cost display
- [x] Generation page has expandable per-provider breakdown showing credits for each provider
- [x] Falls back to "$Y.YYYY" when creditsRequired is null/undefined/0
- [x] Large numbers format correctly (1,234,567 credits)
- [x] All unit tests pass (`pnpm test`)
- [x] `pnpm build` succeeds with no type errors

### Must Have
- Credits formatted with comma separators (1,234 not 1234)
- Dollar cost maintains existing 4 decimal precision
- Graceful fallback when credits unavailable
- Unit test coverage for formatting edge cases

### Must NOT Have (Guardrails)
- Do NOT modify API code (already complete)
- Do NOT manually edit types.ts (regenerate from spec)
- Do NOT change other pages beyond generation
- Do NOT create separate utility files (inline formatting)
- Do NOT add E2E tests (manual browser verification sufficient)
- Do NOT change existing dollar display format

---

## Verification Strategy (MANDATORY)

### Test Decision
- **Infrastructure exists**: YES (Vitest)
- **User wants tests**: YES (unit tests)
- **Framework**: Vitest

### Test Coverage
Each TODO includes verification via:
1. Unit tests for formatting logic
2. Manual browser verification for visual correctness

---

## Execution Strategy

### Parallel Execution Waves

```
Wave 1 (Start Immediately):
└── Task 1: Regenerate TypeScript types [no dependencies]

Wave 2 (After Wave 1):
├── Task 2: Update generation page main cost display [depends: 1]
└── Task 3: Add per-provider breakdown to generation page [depends: 1]

Wave 3 (After Wave 2):
└── Task 4: Add unit tests [depends: 2, 3]

Critical Path: Task 1 → Task 2 → Task 4
Parallel Speedup: Tasks 2+3 run in parallel (~30% faster)
```

### Dependency Matrix

| Task | Depends On | Blocks | Can Parallelize With |
|------|------------|--------|---------------------|
| 1 | None | 2, 3 | None |
| 2 | 1 | 4 | 3 |
| 3 | 1 | 4 | 2 |
| 4 | 2, 3 | None | None (final) |

---

## TODOs

- [x] 1. Regenerate TypeScript types from API

  **What to do**:
  - Start the API locally: `cd apps/api && dotnet run --project src/VoiceProcessor.Clients.Api`
  - Regenerate types: `cd apps/web && pnpm generate:api --spec=http://localhost:5000/swagger/v1/swagger.json`
  - Verify `creditsRequired` appears in `CostEstimateResponse` and `ProviderEstimate` types

  **Must NOT do**:
  - Do NOT manually edit types.ts
  - Do NOT commit if creditsRequired is missing (indicates API issue)

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: Single command execution with verification
  - **Skills**: `[]`
    - No specialized skills needed

  **Parallelization**:
  - **Can Run In Parallel**: NO (prerequisite for all other tasks)
  - **Parallel Group**: Wave 1 (solo)
  - **Blocks**: Tasks 2, 3
  - **Blocked By**: None

  **References**:
  
  **Pattern References**:
  - `apps/web/package.json:scripts.generate:api` - Type generation command

  **API/Type References**:
  - `apps/api/src/VoiceProcessor.Domain/DTOs/Responses/CostEstimateResponse.cs` - Source DTO with CreditsRequired

  **Acceptance Criteria**:
  
  **Verification:**
  - [ ] Run: `grep -n "creditsRequired" apps/web/src/lib/api/types.ts`
  - [ ] Expected: Multiple matches showing `creditsRequired` in CostEstimateResponse and ProviderEstimate
  - [ ] Run: `pnpm --filter @voiceprocessor/web build`
  - [ ] Expected: Build succeeds with no type errors

  **Commit**: YES
  - Message: `chore(web): regenerate api types with creditsRequired field`
  - Files: `apps/web/src/lib/api/types.ts`
  - Pre-commit: `pnpm --filter @voiceprocessor/web build`

---

- [x] 2. Update generation page sidebar to show credits

  **What to do**:
  - Locate cost display in `apps/web/src/app/(app)/generate/page.tsx` (~line 252-257)
  - Change from: `$${costEstimate.estimatedCost.toFixed(4)}`
  - Change to: `${creditsRequired?.toLocaleString()} credits ($${estimatedCost.toFixed(4)})` when creditsRequired > 0
  - Fallback to cost-only when creditsRequired is null/undefined/0

  **Implementation pattern**:
  ```tsx
  // Inline formatting - no utility file
  const formatCostDisplay = (credits: number | undefined, cost: number) => {
    if (credits && credits > 0) {
      return `${credits.toLocaleString()} credits ($${cost.toFixed(4)})`;
    }
    return `$${cost.toFixed(4)}`;
  };
  ```

  **Must NOT do**:
  - Do NOT create separate utility files
  - Do NOT change the existing cost calculation logic
  - Do NOT modify other sections of the page

  **Recommended Agent Profile**:
  - **Category**: `visual-engineering`
    - Reason: Frontend UI modification with display formatting
  - **Skills**: `["frontend-ui-ux"]`
    - `frontend-ui-ux`: Display formatting and user-facing text

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 2 (with Task 3)
  - **Blocks**: Task 4
  - **Blocked By**: Task 1

  **References**:
  
  **Pattern References**:
  - `apps/web/src/app/(app)/generate/page.tsx:249-257` - Current cost display: `$${costEstimate.estimatedCost.toFixed(4)}`
  - `apps/web/src/components/CostEstimate/CostEstimate.tsx:39-46` - Reference: existing `formatCost` function using Intl.NumberFormat

  **API/Type References**:
  - `apps/web/src/lib/api/types.ts:CostEstimateResponse` - Type with `creditsRequired` field (after regeneration)

  **Data Flow**:
  - `useEstimateCost` hook is called on text/voice change (line ~170)
  - Response stored in `costEstimate` variable (line ~175)
  - Access: `costEstimate.creditsRequired` and `costEstimate.estimatedCost`

  **Acceptance Criteria**:
  
  **Manual Browser Verification:**
  - [ ] Navigate to: `http://localhost:3000/generate`
  - [ ] Enter text in the generation input
  - [ ] Verify sidebar shows: "X,XXX credits ($Y.YYYY)" format
  - [ ] Verify credits update as text changes
  - [ ] Screenshot: Save to `.sisyphus/evidence/task-2-credits-display.png`

  **Commit**: YES (groups with Task 3)
  - Message: `feat(web): display credits alongside cost on generation page`
  - Files: `apps/web/src/app/(app)/generate/page.tsx`
  - Pre-commit: `pnpm --filter @voiceprocessor/web build`

---

- [x] 3. Add per-provider breakdown section to generation page

  **What to do**:
  - Add an expandable `<details>` section below the main cost display in `apps/web/src/app/(app)/generate/page.tsx`
  - The section should show ALL provider estimates with credits: "X,XXX credits ($Y.YY)"
  - Use the `providerEstimates` array from `costEstimate` response
  - Follow the existing CostEstimate component pattern (lines 135-158) for the `<details>` UI structure

  **Implementation pattern**:
  ```tsx
  {/* Add below the main cost display (~line 275) */}
  {costEstimate?.providerEstimates && costEstimate.providerEstimates.length > 1 && (
    <details className="mt-4 text-sm">
      <summary className="cursor-pointer text-blue-400 hover:underline">
        Compare all providers
      </summary>
      <div className="mt-2 space-y-2">
        {costEstimate.providerEstimates.map((estimate) => (
          <div
            key={estimate.provider}
            className="flex items-center justify-between rounded p-2 bg-gray-700/50"
          >
            <span>{estimate.provider}</span>
            <span>
              {estimate.creditsRequired && estimate.creditsRequired > 0
                ? `${estimate.creditsRequired.toLocaleString()} credits ($${estimate.cost.toFixed(4)})`
                : `$${estimate.cost.toFixed(4)}`}
            </span>
          </div>
        ))}
      </div>
    </details>
  )}
  ```

  **Location**: Insert this AFTER the existing cost display section (after line 274, before the closing `</div>` of the cost estimate card)

  **Must NOT do**:
  - Do NOT modify CostEstimate component (it's not used on this page)
  - Do NOT change the main cost display (that's Task 2)
  - Do NOT add complex styling - match existing dark theme

  **Recommended Agent Profile**:
  - **Category**: `visual-engineering`
    - Reason: Frontend UI addition with display formatting
  - **Skills**: `["frontend-ui-ux"]`
    - `frontend-ui-ux`: Component display patterns

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 2 (with Task 2)
  - **Blocks**: Task 4
  - **Blocked By**: Task 1

  **References**:
  
  **Pattern References**:
  - `apps/web/src/app/(app)/generate/page.tsx:242-281` - Cost estimate card where this will be added
  - `apps/web/src/components/CostEstimate/CostEstimate.tsx:135-158` - Reference pattern for `<details>` provider comparison UI

  **API/Type References**:
  - `apps/web/src/lib/api/types.ts:CostEstimateResponse.providerEstimates` - Array of provider estimates
  - `apps/web/src/lib/api/types.ts:ProviderEstimate` - Type with per-provider `creditsRequired` field

  **Data Flow**:
  - `useEstimateCost` hook returns `CostEstimateResponse`
  - Access `costEstimate.providerEstimates` array
  - Each item has `provider`, `cost`, `creditsRequired`, etc.

  **Acceptance Criteria**:
  
  **Manual Browser Verification:**
  - [ ] Navigate to: `http://localhost:3000/generate`
  - [ ] Enter text to trigger cost estimation
  - [ ] Verify "Compare all providers" link appears below main cost
  - [ ] Click to expand and verify each provider shows: "X,XXX credits ($Y.YY)"
  - [ ] Verify credits fall back to cost-only when creditsRequired is 0/null
  - [ ] Screenshot: Save to `.sisyphus/evidence/task-3-provider-credits.png`

  **Commit**: YES (groups with Task 2)
  - Message: `feat(web): display credits alongside cost on generation page`
  - Files: `apps/web/src/app/(app)/generate/page.tsx`
  - Pre-commit: `pnpm --filter @voiceprocessor/web build`

---

- [x] 4. Add unit tests for credits formatting

  **What to do**:
  - Create test file: `apps/web/src/app/(app)/generate/__tests__/formatCredits.test.ts`
  - Test the inline formatting logic for edge cases:
    - Normal case: `1234` credits → `"1,234 credits ($0.0370)"`
    - Zero credits: `0` → `"$0.0000"` (cost only)
    - Null/undefined: → `"$0.0000"` (cost only)
    - Large number: `1234567` → `"1,234,567 credits ($37.0370)"`

  **Test structure**:
  ```typescript
  import { describe, it, expect } from 'vitest';

  // Extract or duplicate the formatting function for testing
  const formatCostDisplay = (credits: number | undefined, cost: number) => {
    if (credits && credits > 0) {
      return `${credits.toLocaleString()} credits ($${cost.toFixed(4)})`;
    }
    return `$${cost.toFixed(4)}`;
  };

  describe('formatCostDisplay', () => {
    it('formats credits and cost together', () => {
      expect(formatCostDisplay(1234, 0.037)).toBe('1,234 credits ($0.0370)');
    });

    it('shows cost only when credits is zero', () => {
      expect(formatCostDisplay(0, 0.037)).toBe('$0.0370');
    });

    it('shows cost only when credits is undefined', () => {
      expect(formatCostDisplay(undefined, 0.037)).toBe('$0.0370');
    });

    it('formats large credit numbers with commas', () => {
      expect(formatCostDisplay(1234567, 37.037)).toBe('1,234,567 credits ($37.0370)');
    });
  });
  ```

  **Must NOT do**:
  - Do NOT add E2E tests
  - Do NOT mock API calls (pure formatting tests)
  - Do NOT test component rendering (just logic)

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: Simple test file creation
  - **Skills**: `[]`
    - No specialized skills needed

  **Parallelization**:
  - **Can Run In Parallel**: NO (final task)
  - **Parallel Group**: Wave 3 (solo)
  - **Blocks**: None
  - **Blocked By**: Tasks 2, 3

  **References**:
  
  **Test References**:
  - `apps/web/src/app/(app)/payment/__tests__/success.test.tsx` - Existing test patterns
  - `apps/web/vitest.config.ts` - Test configuration

  **Acceptance Criteria**:
  
  **Automated Verification:**
  - [ ] Run: `pnpm --filter @voiceprocessor/web test`
  - [ ] Expected: All tests pass including new formatCredits tests
  - [ ] Run: `pnpm --filter @voiceprocessor/web test --coverage`
  - [ ] Expected: formatCostDisplay function has 100% coverage

  **Commit**: YES
  - Message: `test(web): add unit tests for credits display formatting`
  - Files: `apps/web/src/app/(app)/generate/__tests__/formatCredits.test.ts`
  - Pre-commit: `pnpm --filter @voiceprocessor/web test`

---

## Commit Strategy

| After Task | Message | Files | Verification |
|------------|---------|-------|--------------|
| 1 | `chore(web): regenerate api types with creditsRequired field` | types.ts | pnpm build |
| 2+3 | `feat(web): display credits alongside cost on generation page` | page.tsx | pnpm build |
| 4 | `test(web): add unit tests for credits display formatting` | formatCredits.test.ts | pnpm test |

---

## Success Criteria

### Verification Commands
```bash
# Type check
pnpm --filter @voiceprocessor/web build  # Expected: success

# Unit tests
pnpm --filter @voiceprocessor/web test   # Expected: all pass

# Type verification
grep -n "creditsRequired" apps/web/src/lib/api/types.ts  # Expected: matches found
```

### Final Checklist
- [x] Generation page main display shows "X,XXX credits ($Y.YYYY)"
- [x] Generation page has expandable per-provider breakdown with credits
- [x] Fallback works when credits unavailable (shows cost only)
- [x] All unit tests pass
- [x] Build succeeds with no errors
- [x] Issue `voiceprocessor-web-0mt` can be closed
