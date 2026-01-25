# PR #2 Review Fixes: Stripe Checkout Integration

## Context

### Original Request
Review and fix issues found in PR #2 (feat(billing): Stripe checkout integration for credit packs).

### PR Reference
- URL: https://github.com/waner11/voiceprocessor-web/pull/2
- Branch: `voiceprocessor-web-hbm-stripe-checkout`
- Files: 7 changed (+191/-130)

### Review Summary
PR review identified 11 issues across security, bugs, and code quality categories.

---

## Work Objectives

### Core Objective
Fix all critical and high-priority issues identified in PR #2 review before merge.

### Concrete Deliverables
- Fixed `verifyPayment` function (remove mock, implement real API or stub safely)
- Type-safe API calls (remove `any` casts)
- Correct loading state behavior (per-button)
- Error display working in CreditPackCard
- Clean code (no duplicate types, consistent formatting)

### Definition of Done
- [ ] `pnpm build` succeeds
- [ ] `pnpm exec tsc --noEmit` passes with no errors
- [ ] No `eslint-disable` comments for `any` types
- [ ] All buttons work correctly (only clicked button shows loading)
- [ ] Error states display properly

### Must Have
- Remove or secure the mock `verifyPayment` function
- Fix loading state bug (per-pack tracking)
- Wire up error prop in CreditPackCard

### Must NOT Have (Guardrails)
- Do NOT add new `any` type assertions
- Do NOT break existing functionality
- Do NOT change API endpoint paths
- Do NOT modify unrelated files

---

## Verification Strategy

### Test Decision
- **Infrastructure exists**: NO (no test files for payment)
- **User wants tests**: Manual verification
- **Framework**: N/A for this fix PR

### Manual Execution Verification
Each fix verified via:
1. TypeScript compilation: `pnpm exec tsc --noEmit`
2. Build check: `pnpm build`
3. Visual verification in browser (loading states, error display)

---

## Task Flow

```
Task 1 (Types) → Task 2 (Service) → Task 3 (Loading State) → Task 4 (Error Prop)
                                  ↘ Task 5 (Cleanup) [parallel with 3,4]
```

---

## TODOs

### CRITICAL FIXES

- [x] 1. Remove or Secure Mock `verifyPayment` Function

  **What to do**:
  - Option A: Remove `verifyPayment` entirely if not used
  - Option B: Add `throw new Error("Not implemented")` to prevent accidental use
  - Option C: Implement real API call to `/api/v1/payments/verify/{sessionId}`
  
  **Why this matters**: Current code ALWAYS returns `success: true` - potential payment bypass.

  **Must NOT do**:
  - Leave function returning hardcoded `success: true`

  **Parallelizable**: NO (foundational security fix)

  **References**:
  - `src/lib/api/payment/service.ts:42-52` - Current mock implementation
  - `src/hooks/usePayment.ts:19-36` - Where verifyPayment is called

  **Acceptance Criteria**:
  - [ ] `verifyPayment` no longer returns hardcoded success
  - [ ] If stub, throws descriptive error
  - [ ] If real API, calls backend endpoint

  **Commit**: YES
  - Message: `fix(payment): secure verifyPayment to prevent bypass`
  - Files: `src/lib/api/payment/service.ts`

---

- [ ] 2. Add Payment Types to OpenAPI and Remove `any` Casts

  **What to do**:
  - Check if payment endpoints exist in OpenAPI spec
  - If not in spec, add manual type definitions that integrate with openapi-fetch
  - Remove `eslint-disable` comments and `as any` casts
  - Ensure proper typing for POST/GET calls

  **Must NOT do**:
  - Add new `any` casts elsewhere
  - Break existing API client setup

  **Parallelizable**: NO (depends on understanding current type setup)

  **References**:
  - `src/lib/api/payment/service.ts:14-18, 33-34` - Current `any` casts
  - `src/lib/api/client.ts` - API client setup
  - `src/lib/api/types.ts` - Generated types location

  **Acceptance Criteria**:
  - [ ] No `eslint-disable-next-line @typescript-eslint/no-explicit-any` in service.ts
  - [ ] No `as any` type assertions
  - [ ] `pnpm exec tsc --noEmit` passes

  **Commit**: YES
  - Message: `fix(payment): add proper types for payment API calls`
  - Files: `src/lib/api/payment/service.ts`, possibly `src/lib/api/payment/types.ts`

---

### HIGH PRIORITY FIXES

- [ ] 3. Fix Loading State to Track Per-Pack

  **What to do**:
  - Add state to track which pack ID is being processed
  - Pass pack-specific loading state to each CreditPackCard
  - Update `handleBuyPack` to set the processing pack ID
  - Clear processing state on success/error

  **Must NOT do**:
  - Break the redirect flow on successful checkout

  **Parallelizable**: YES (with 4, 5)

  **References**:
  - `src/app/(app)/settings/billing/page.tsx:51-52` - Current handleBuyPack
  - `src/app/(app)/settings/billing/page.tsx:82-86` - Current CreditPackCard usage
  - `src/hooks/usePayment.ts:11-17` - Checkout mutation

  **Acceptance Criteria**:
  - [ ] Only clicked button shows "Processing..."
  - [ ] Other buttons remain clickable (or disabled but not showing "Processing...")
  - [ ] State clears properly after redirect/error

  **Verification**:
  - Using playwright or manual browser:
    - Navigate to `/settings/billing`
    - Click "Buy Now" on first pack
    - Verify only that button shows "Processing..."
    - Verify other buttons show "Buy Now"

  **Commit**: YES
  - Message: `fix(billing): track loading state per credit pack`
  - Files: `src/app/(app)/settings/billing/page.tsx`

---

- [ ] 4. Wire Up Error Prop in CreditPackCard

  **What to do**:
  - Track checkout errors per pack in billing page state
  - Pass error to corresponding CreditPackCard
  - Clear error when user retries

  **Must NOT do**:
  - Show same error on all cards

  **Parallelizable**: YES (with 3, 5)

  **References**:
  - `src/components/CreditPackCard/CreditPackCard.tsx:10,41-43` - Error prop and display
  - `src/app/(app)/settings/billing/page.tsx:82-86` - Missing error prop
  - `src/hooks/usePayment.ts:43` - Error from mutation

  **Acceptance Criteria**:
  - [ ] Error displays under the specific pack that failed
  - [ ] Error clears on retry
  - [ ] TypeScript compiles without errors

  **Commit**: YES (can combine with Task 3)
  - Message: `fix(billing): display checkout errors per pack`
  - Files: `src/app/(app)/settings/billing/page.tsx`

---

### MEDIUM PRIORITY FIXES

- [ ] 5. Code Cleanup: Remove Duplicate Types and Fix Formatting

  **What to do**:
  - Remove duplicate `CheckoutResponse` interface (keep `CheckoutSessionResponse`)
  - Fix indentation in `fetchCreditPacks` function (use 2-space indent)
  - Ensure consistent error handling pattern

  **Must NOT do**:
  - Change any logic, only formatting/cleanup

  **Parallelizable**: YES (with 3, 4)

  **References**:
  - `src/lib/api/payment/types.ts:12-14` - Duplicate CheckoutResponse
  - `src/lib/api/payment/service.ts:32-40` - Inconsistent indentation

  **Acceptance Criteria**:
  - [ ] Only one checkout response type exists
  - [ ] Consistent 2-space indentation throughout service.ts
  - [ ] No linting errors

  **Commit**: YES
  - Message: `chore(payment): clean up duplicate types and formatting`
  - Files: `src/lib/api/payment/types.ts`, `src/lib/api/payment/service.ts`

---

### LOW PRIORITY (OPTIONAL)

- [ ] 6. Add Error Toast for Checkout Failures

  **What to do**:
  - Add `onError` handler to checkout mutation in usePayment hook
  - Show toast notification on failure
  - Consider using existing toast system if available, or add one

  **Parallelizable**: YES (independent)

  **References**:
  - `src/hooks/usePayment.ts:11-17` - Checkout mutation without onError

  **Acceptance Criteria**:
  - [ ] User sees notification when checkout fails
  - [ ] Error message is user-friendly

  **Commit**: YES
  - Message: `feat(payment): add toast notification for checkout errors`
  - Files: `src/hooks/usePayment.ts`

---

- [ ] 7. Add Retry Button for Failed Pack Fetch

  **What to do**:
  - Add "Try Again" button in error state UI
  - Button triggers re-fetch of credit packs

  **Parallelizable**: YES (independent)

  **References**:
  - `src/app/(app)/settings/billing/page.tsx:73-78` - Error display without retry

  **Acceptance Criteria**:
  - [ ] "Try Again" button visible when pack fetch fails
  - [ ] Clicking button re-fetches packs
  - [ ] Loading state shown during retry

  **Commit**: YES
  - Message: `feat(billing): add retry button for failed pack fetch`
  - Files: `src/app/(app)/settings/billing/page.tsx`

---

## Commit Strategy

| After Task | Message | Files |
|------------|---------|-------|
| 1 | `fix(payment): secure verifyPayment to prevent bypass` | service.ts |
| 2 | `fix(payment): add proper types for payment API calls` | service.ts, types.ts |
| 3+4 | `fix(billing): fix per-pack loading and error states` | page.tsx |
| 5 | `chore(payment): clean up duplicate types and formatting` | types.ts, service.ts |
| 6 | `feat(payment): add toast notification for checkout errors` | usePayment.ts |
| 7 | `feat(billing): add retry button for failed pack fetch` | page.tsx |

---

## Success Criteria

### Verification Commands
```bash
pnpm exec tsc --noEmit  # Expected: no errors
pnpm build              # Expected: success
pnpm lint               # Expected: no errors (or only unrelated warnings)
```

### Final Checklist
- [ ] No `any` type assertions in payment code
- [ ] `verifyPayment` doesn't return hardcoded success
- [ ] Loading states work per-pack
- [ ] Errors display correctly
- [ ] All commits follow conventional commit format
- [ ] PR ready for re-review
