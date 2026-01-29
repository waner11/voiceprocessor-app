# PR #1 Fixes: Locale and Code Quality Improvements

## TL;DR

> **Quick Summary**: Fix locale-dependent tests and reduce code duplication in the credits display PR before merge.
> 
> **Deliverables**:
> - Fixed tests to use explicit `en-US` locale
> - Extracted shared `formatCostWithCredits` helper function
> - Updated both display locations to use the helper
> 
> **Estimated Effort**: Quick (< 30 min)
> **Parallel Execution**: NO - sequential
> **Critical Path**: Task 1 → Task 2 → Task 3

---

## Context

### Original Request
PR #1 review identified two issues that should be fixed before merge:
1. Tests use `toLocaleString()` without explicit locale - will fail on non-US CI servers
2. Formatting logic duplicated in two places in `page.tsx`

### Review Findings
- **Locale Issue**: `toLocaleString()` produces `"1.234"` in German locale vs `"1,234"` in US
- **DRY Violation**: Same formatting logic at lines 253-255 and 290-292
- **Test Isolation**: Test file duplicates the function instead of testing actual implementation

### Scope Decision
- WORKFLOW.md changes are APPROVED to stay in PR (user confirmed)
- Focus only on locale and DRY fixes

---

## Work Objectives

### Core Objective
Ensure PR #1 tests pass in any locale and reduce code duplication for maintainability.

### Concrete Deliverables
- `apps/web/src/app/(app)/generate/page.tsx` - Extract helper function, use explicit locale
- `apps/web/src/app/(app)/generate/__tests__/formatCredits.test.ts` - Use explicit locale

### Definition of Done
- [x] Tests pass with `LC_ALL=de_DE.UTF-8 pnpm test` (German locale)
- [x] Tests pass with `LC_ALL=en_US.UTF-8 pnpm test` (US locale)
- [x] Formatting logic exists in exactly ONE place (helper function)
- [x] Both display locations use the helper function
- [x] `pnpm build` succeeds

### Must NOT Have (Guardrails)
- Do NOT change the visual output format
- Do NOT create a separate utility file (keep inline in page.tsx)
- Do NOT modify WORKFLOW.md (already approved)
- Do NOT touch types.ts

---

## TODOs

- [x] 1. Extract `formatCostWithCredits` helper function in page.tsx

  **What to do**:
  - Add helper function inside the `GeneratePage` component (before the return statement)
  - Use explicit `'en-US'` locale for consistent formatting across all environments
  - Function signature: `(credits: number | undefined, cost: number) => string`

  **Implementation**:
  ```tsx
  // Add inside GeneratePage component, before return statement
  const formatCostWithCredits = (credits: number | undefined, cost: number) => {
    if (credits && credits > 0) {
      return `${credits.toLocaleString('en-US')} credits ($${cost.toFixed(4)})`;
    }
    return `$${cost.toFixed(4)}`;
  };
  ```

  **Location**: `apps/web/src/app/(app)/generate/page.tsx` - inside component, around line 100-110

  **References**:
  - Current duplicated logic at lines 253-255 and 290-292
  - Test file function at `formatCredits.test.ts:8-13`

  **Acceptance Criteria**:
  - [ ] Helper function defined inside component
  - [ ] Uses `toLocaleString('en-US')` for explicit locale
  - [ ] `pnpm build` succeeds

  **Commit**: NO (group with Task 2)

---

- [x] 2. Update both display locations to use the helper

  **What to do**:
  - Replace inline ternary at line 253-255 with `formatCostWithCredits(costEstimate.creditsRequired, costEstimate.estimatedCost)`
  - Replace inline ternary at line 290-292 with `formatCostWithCredits(estimate.creditsRequired, estimate.cost)`

  **Before** (line 253-255):
  ```tsx
  costEstimate.creditsRequired && costEstimate.creditsRequired > 0
    ? `${costEstimate.creditsRequired.toLocaleString()} credits ($${costEstimate.estimatedCost.toFixed(4)})`
    : `$${costEstimate.estimatedCost.toFixed(4)}`
  ```

  **After**:
  ```tsx
  formatCostWithCredits(costEstimate.creditsRequired, costEstimate.estimatedCost)
  ```

  **Before** (line 290-292):
  ```tsx
  estimate.creditsRequired && estimate.creditsRequired > 0
    ? `${estimate.creditsRequired.toLocaleString()} credits ($${estimate.cost.toFixed(4)})`
    : `$${estimate.cost.toFixed(4)}`
  ```

  **After**:
  ```tsx
  formatCostWithCredits(estimate.creditsRequired, estimate.cost)
  ```

  **References**:
  - `apps/web/src/app/(app)/generate/page.tsx:253-255` - Main cost display
  - `apps/web/src/app/(app)/generate/page.tsx:290-292` - Per-provider display

  **Acceptance Criteria**:
  - [ ] Main cost display uses helper function
  - [ ] Per-provider display uses helper function
  - [ ] No duplicate formatting logic remains
  - [ ] Visual output unchanged (verify in browser)
  - [ ] `pnpm build` succeeds

  **Commit**: YES
  - Message: `fix(web): use explicit locale for credits formatting`
  - Files: `apps/web/src/app/(app)/generate/page.tsx`

---

- [x] 3. Update test file to use explicit locale

  **What to do**:
  - Update the `formatCostDisplay` function in test file to use `'en-US'` locale
  - This ensures tests pass regardless of CI server locale

  **Before**:
  ```typescript
  const formatCostDisplay = (credits: number | undefined, cost: number) => {
    if (credits && credits > 0) {
      return `${credits.toLocaleString()} credits ($${cost.toFixed(4)})`;
    }
    return `$${cost.toFixed(4)}`;
  };
  ```

  **After**:
  ```typescript
  const formatCostDisplay = (credits: number | undefined, cost: number) => {
    if (credits && credits > 0) {
      return `${credits.toLocaleString('en-US')} credits ($${cost.toFixed(4)})`;
    }
    return `$${cost.toFixed(4)}`;
  };
  ```

  **References**:
  - `apps/web/src/app/(app)/generate/__tests__/formatCredits.test.ts:8-13`

  **Acceptance Criteria**:
  - [ ] Test function uses `toLocaleString('en-US')`
  - [ ] All 4 tests still pass: `pnpm --filter @voiceprocessor/web test`
  - [ ] Tests pass in different locale: `LC_ALL=de_DE.UTF-8 pnpm --filter @voiceprocessor/web test`

  **Commit**: YES
  - Message: `fix(web): use explicit locale in credits formatting tests`
  - Files: `apps/web/src/app/(app)/generate/__tests__/formatCredits.test.ts`

---

## Verification Strategy

### Test Commands
```bash
# Standard test run
pnpm --filter @voiceprocessor/web test

# Verify locale independence (if de_DE locale available)
LC_ALL=de_DE.UTF-8 pnpm --filter @voiceprocessor/web test

# Build check
pnpm --filter @voiceprocessor/web build
```

### Manual Verification
1. Start dev server: `cd apps/web && pnpm dev`
2. Navigate to http://localhost:3000/generate
3. Enter text, verify credits display shows "1,234 credits ($0.0370)" format
4. Expand provider comparison, verify same format

---

## Commit Strategy

| After Task | Message | Files |
|------------|---------|-------|
| 1+2 | `fix(web): use explicit locale for credits formatting` | page.tsx |
| 3 | `fix(web): use explicit locale in credits formatting tests` | formatCredits.test.ts |

---

## Success Criteria

- [x] All unit tests pass
- [x] Tests are locale-independent
- [x] No duplicate formatting logic in page.tsx
- [x] Build succeeds
- [x] PR ready to merge
