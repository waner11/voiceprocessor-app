# Stripe Checkout Integration for Credit Packs

## Context

### Original Request
Integrate Stripe checkout for credit pack purchases on the billing page (issue `voiceprocessor-web-hbm`). Work in a new branch.

### Interview Summary
**Key Discussions**:
- P0 priority feature - highest priority work item
- Backend endpoints are ready and tested
- User confirmed working in a new branch

**Research Findings**:
- Billing page at `/src/app/(app)/settings/billing/page.tsx` - currently shows subscription model with mock data
- Payment service at `/src/lib/api/payment/service.ts` - mock implementation needs real API calls
- `usePayment` hook exists with `startCheckout()` and `verifyTransaction()` - uses React Query mutations
- Uses `openapi-fetch` client at `/src/lib/api/client.ts` with auth middleware
- UI uses Tailwind CSS v4 with skeleton loading patterns (animate-pulse)
- Credit packs defined in `/src/lib/api/payment/constants.ts` - can serve as fallback
- Branch naming convention: `voiceprocessor-web-{suffix}`

### Metis Review
**Identified Gaps** (addressed):
- API types may not be generated - added step to verify/regenerate
- Success/cancel URLs need coordination with dependent issue - using `/payment/success` and `/payment/cancel`
- Mock service should use `api` client for auth middleware - updating implementation
- Remove obsolete subscription UI - included in refactor
- Need error display without toast library - use inline error states

---

## Work Objectives

### Core Objective
Refactor the billing page from a subscription model to a credit-based model, displaying credit packs that users can purchase through Stripe checkout.

### Concrete Deliverables
- Updated billing page showing credit balance and purchasable credit packs
- Real API integration for fetching packs and creating checkout sessions
- Working checkout flow that redirects to Stripe

### Definition of Done
- [x] `pnpm build` succeeds
- [x] `pnpm lint` passes
- [x] Billing page loads and displays credit packs from API (or fallback)
- [x] Clicking "Buy" initiates checkout and redirects to Stripe
- [x] Loading states visible during API calls
- [x] Error states display for failed requests

### Must Have
- Credit balance displayed prominently
- Credit pack cards with name, credits, price, and Buy button
- Loading state during checkout initiation
- Error handling for failed API calls
- Graceful fallback if packs API unavailable

### Must NOT Have (Guardrails)
- **NO** payment success/cancel pages (that's issue `voiceprocessor-web-snu`)
- **NO** payment history fetching (that's issue `voiceprocessor-web-e07`)
- **NO** new dependencies (toast libraries, etc.) without explicit approval
- **NO** invoice download functionality
- **NO** analytics or tracking code
- **NO** complex retry logic - single request, show error, user retries manually
- **NO** "Best Value" badges or promotional elements
- **NO** modifying `authStore` interface beyond using existing `setCredits()`

---

## Verification Strategy

### Test Decision
- **Infrastructure exists**: YES (Vitest configured)
- **User wants tests**: Manual verification (no existing test patterns to follow)
- **Framework**: Vitest available but not used yet

### Manual QA Procedures

Each task includes manual verification steps using browser inspection and network monitoring.

---

## Task Flow

```
Task 0 (Branch Setup)
    |
    v
Task 1 (Verify/Generate API Types)
    |
    v
Task 2 (Update Payment Service)
    |
    v
Task 3 (Create CreditPackCard Component)
    |
    v
Task 4 (Refactor Billing Page)
    |
    v
Task 5 (Integration Testing & Polish)
```

## Parallelization

| Task | Depends On | Reason |
|------|------------|--------|
| 1 | 0 | Needs branch created |
| 2 | 1 | Needs API types |
| 3 | 1 | Needs API types for CreditPack interface |
| 4 | 2, 3 | Needs service and component |
| 5 | 4 | End-to-end verification |

---

## TODOs

- [x] 0. Create feature branch from main

  **What to do**:
  - Create new branch named `voiceprocessor-web-hbm-stripe-checkout`
  - Push branch to remote

  **Must NOT do**:
  - Don't commit any changes yet - just create the branch

  **Parallelizable**: NO (first task)

  **References**:
  - Branch naming convention from existing branches: `voiceprocessor-web-{issue-id}-{description}`

  **Acceptance Criteria**:

  **Manual Execution Verification:**
  - [ ] Command: `git checkout -b voiceprocessor-web-hbm-stripe-checkout`
  - [ ] Command: `git push -u origin voiceprocessor-web-hbm-stripe-checkout`
  - [ ] Verify: `git branch --show-current` → `voiceprocessor-web-hbm-stripe-checkout`

  **Commit**: NO (no code changes)

---

- [x] 1. Verify and prepare API types for payment endpoints

  **What to do**:
  - Check if payment endpoints exist in `/src/lib/api/types.ts`
  - If not present, run `pnpm generate:api` to regenerate from backend spec
  - If still not present, add manual type definitions to `/src/lib/api/payment/types.ts`
  - Ensure types exist for:
    - `CreditPack` (id, name, credits, price, priceId, description)
    - `CheckoutRequest` (priceId: string)
    - `CheckoutResponse` (checkoutUrl: string)

  **Must NOT do**:
  - Don't modify backend types file if regeneration works
  - Don't create duplicate type definitions

  **Parallelizable**: NO (depends on 0)

  **References**:

  **Pattern References**:
  - `/src/lib/api/types.ts` - Existing OpenAPI generated types (check for payment endpoints)
  - `/src/lib/api/payment/types.ts:1-15` - Existing manual payment types to extend

  **API/Type References**:
  - Backend endpoint: `GET /api/v1/payments/packs` - Response shape needed
  - Backend endpoint: `POST /api/v1/payments/checkout` - Request/response shapes needed

  **Documentation References**:
  - `/package.json` scripts section - `generate:api` command

  **Acceptance Criteria**:

  **Manual Execution Verification:**
  - [ ] Command: `grep -r "payments" src/lib/api/types.ts | head -5`
  - [ ] If no results: `pnpm generate:api`
  - [ ] Verify types exist: `grep "CreditPack\|CheckoutRequest\|checkoutUrl" src/lib/api/payment/types.ts`
  - [ ] TypeScript check: `pnpm exec tsc --noEmit` → no payment-related errors

  **Commit**: YES
  - Message: `feat(payment): add/verify API types for payment endpoints`
  - Files: `src/lib/api/payment/types.ts` (if modified)
  - Pre-commit: `pnpm exec tsc --noEmit`

---

- [x] 2. Update payment service to use real API endpoints

  **What to do**:
  - Replace mock implementation in `paymentService.createCheckoutSession()` with real API call
  - Use `api.POST('/api/v1/payments/checkout', { body: { priceId } })` pattern
  - Add `fetchCreditPacks()` method using `api.GET('/api/v1/payments/packs')`
  - Implement fallback to `CREDIT_PACKS` constant if API fails
  - Remove artificial delays (setTimeout)
  - Keep `verifyPayment` as-is (not needed for this issue - webhooks handle it)

  **Must NOT do**:
  - Don't implement payment history fetching (issue e07)
  - Don't add retry logic beyond single attempt
  - Don't modify verifyPayment logic significantly

  **Parallelizable**: NO (depends on 1)

  **References**:

  **Pattern References**:
  - `/src/lib/api/client.ts:1-50` - API client setup with auth middleware (use this pattern)
  - `/src/hooks/useGenerations.ts:25-40` - Example of api.POST with error handling
  - `/src/hooks/useVoices.ts:10-25` - Example of api.GET with error handling

  **API/Type References**:
  - `/src/lib/api/payment/types.ts` - CheckoutSessionResponse, CreditPack types
  - `/src/lib/api/payment/constants.ts` - CREDIT_PACKS fallback data

  **External References**:
  - Backend contract: `POST /api/v1/payments/checkout` expects `{ priceId: string }`, returns `{ checkoutUrl: string }`
  - Backend contract: `GET /api/v1/payments/packs` returns `{ packs: CreditPack[] }`

  **Acceptance Criteria**:

  **Manual Execution Verification:**
  - [ ] TypeScript check: `pnpm exec tsc --noEmit` → no errors
  - [ ] Verify no setTimeout remains: `grep -n "setTimeout" src/lib/api/payment/service.ts` → no results
  - [ ] Verify api client used: `grep -n "api.POST\|api.GET" src/lib/api/payment/service.ts` → shows usages

  **Commit**: YES
  - Message: `feat(payment): integrate real API endpoints for checkout and packs`
  - Files: `src/lib/api/payment/service.ts`
  - Pre-commit: `pnpm exec tsc --noEmit`

---

- [x] 3. Create CreditPackCard component

  **What to do**:
  - Create `/src/components/CreditPackCard/CreditPackCard.tsx`
  - Create `/src/components/CreditPackCard/index.ts` for export
  - Display: pack name, credits count (formatted with commas), price, description
  - Buy button that calls `onBuy(packId)` callback
  - Loading state: button shows spinner/disabled when `isLoading` prop is true
  - Error state: accept optional `error` prop to display inline error
  - Follow existing card patterns (rounded-lg border p-6)
  - Support dark mode (dark: classes)

  **Must NOT do**:
  - Don't add "Best Value" or "Popular" badges
  - Don't create skeleton loading variant (use button disabled state)
  - Don't add animations beyond Tailwind defaults

  **Parallelizable**: YES (can parallel with task 2 if types ready)

  **References**:

  **Pattern References**:
  - `/src/app/(app)/settings/billing/page.tsx:92-101` - Stats card pattern (rounded-lg bg-gray-50 p-4)
  - `/src/components/CostEstimate/CostEstimate.tsx:1-50` - Card component structure with sections
  - `/src/components/GenerationStatus/GenerationStatus.tsx:40-60` - Badge styling patterns

  **API/Type References**:
  - `/src/lib/api/payment/types.ts:CreditPack` - Props interface source
  - `/src/lib/api/payment/constants.ts:CREDIT_PACKS` - Example data shape

  **Documentation References**:
  - Card pattern: `rounded-lg border border-gray-200 dark:border-gray-700 p-6`
  - Button pattern: `rounded-lg bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 disabled:opacity-50`

  **Acceptance Criteria**:

  **Manual Execution Verification:**
  - [ ] File exists: `ls src/components/CreditPackCard/CreditPackCard.tsx` → exists
  - [ ] TypeScript check: `pnpm exec tsc --noEmit` → no errors
  - [ ] Export works: `grep "CreditPackCard" src/components/CreditPackCard/index.ts` → export found

  **Commit**: YES
  - Message: `feat(components): add CreditPackCard component for billing page`
  - Files: `src/components/CreditPackCard/CreditPackCard.tsx`, `src/components/CreditPackCard/index.ts`
  - Pre-commit: `pnpm exec tsc --noEmit`

---

- [x] 4. Refactor billing page for credit packs model

  **What to do**:
  - Remove "Current Plan" section (subscription model)
  - Remove "Cancel Subscription" and "Change Plan" buttons
  - Add "Credit Balance" section at top showing `creditsRemaining` from authStore
  - Add "Buy Credits" section with grid of CreditPackCard components
  - Fetch packs on mount using `paymentService.fetchCreditPacks()`
  - Wire up Buy buttons to `usePayment().startCheckout(priceId)`
  - Add loading state while fetching packs (skeleton or spinner)
  - Add error state if packs fetch fails (show fallback packs from constants)
  - Keep "Usage This Month" section (statistics are useful)
  - Keep "Payment Method" section (existing UI)
  - STUB "Billing History" section with "Coming soon" or link to future feature

  **Must NOT do**:
  - Don't implement payment history fetching (issue e07)
  - Don't create success/cancel pages (issue snu)
  - Don't modify authStore
  - Don't add invoice download

  **Parallelizable**: NO (depends on 2, 3)

  **References**:

  **Pattern References**:
  - `/src/app/(app)/settings/billing/page.tsx` - Current page structure to refactor
  - `/src/components/VoiceSelector/VoiceSelector.tsx:80-120` - Grid layout pattern for cards
  - `/src/hooks/useVoices.ts:10-30` - Data fetching pattern with useQuery

  **API/Type References**:
  - `/src/hooks/usePayment.ts` - usePayment hook interface (startCheckout, isProcessing, error)
  - `/src/stores/authStore.ts` - creditsRemaining selector

  **Test References**:
  - Manual test: Load page, verify packs display, click Buy, verify redirect

  **Acceptance Criteria**:

  **Manual Execution Verification:**
  - [ ] Start dev server: `pnpm dev`
  - [ ] Using browser: Navigate to `http://localhost:3000/settings/billing`
  - [ ] Verify: "Current Plan" section is GONE
  - [ ] Verify: Credit balance displays at top
  - [ ] Verify: Credit pack cards are visible (3 packs)
  - [ ] Verify: Each card has name, credits, price, Buy button
  - [ ] Using browser DevTools Network tab: Verify `GET /api/v1/payments/packs` is called
  - [ ] Click a Buy button: Verify redirect occurs (to Stripe or mock URL)
  - [ ] Verify dark mode: Toggle system theme, verify colors adapt

  **Commit**: YES
  - Message: `feat(billing): refactor page for credit packs with Stripe checkout`
  - Files: `src/app/(app)/settings/billing/page.tsx`
  - Pre-commit: `pnpm build`

---

- [x] 5. Integration testing and polish

  **What to do**:
  - Run full build to catch any issues: `pnpm build`
  - Run linter: `pnpm lint`
  - Test complete checkout flow manually
  - Test error states (disable network, verify fallback)
  - Test loading states (slow network simulation)
  - Verify responsive layout (mobile/tablet/desktop)
  - Fix any issues found
  - Update issue status in beads

  **Must NOT do**:
  - Don't add automated tests (no existing patterns)
  - Don't add features beyond scope
  - Don't push to main branch

  **Parallelizable**: NO (final task)

  **References**:

  **Pattern References**:
  - `/package.json` scripts - build, lint commands

  **Acceptance Criteria**:

  **Manual Execution Verification:**
  - [ ] Command: `pnpm build` → succeeds
  - [ ] Command: `pnpm lint` → passes (or only pre-existing warnings)
  - [ ] Using browser: Full checkout flow works
  - [ ] Using browser DevTools: Throttle to "Slow 3G", verify loading states appear
  - [ ] Using browser DevTools: Block `/api/v1/payments/packs`, verify fallback packs display
  - [ ] Responsive: Resize window to mobile width, verify layout adapts
  - [ ] Command: `bd update voiceprocessor-web-hbm --status in_progress`

  **Commit**: YES (if any fixes)
  - Message: `fix(billing): polish and fixes from integration testing`
  - Files: Any files with fixes
  - Pre-commit: `pnpm build && pnpm lint`

---

## Commit Strategy

| After Task | Message | Files | Verification |
|------------|---------|-------|--------------|
| 1 | `feat(payment): add/verify API types for payment endpoints` | `src/lib/api/payment/types.ts` | `tsc --noEmit` |
| 2 | `feat(payment): integrate real API endpoints for checkout and packs` | `src/lib/api/payment/service.ts` | `tsc --noEmit` |
| 3 | `feat(components): add CreditPackCard component for billing page` | `src/components/CreditPackCard/*` | `tsc --noEmit` |
| 4 | `feat(billing): refactor page for credit packs with Stripe checkout` | `src/app/(app)/settings/billing/page.tsx` | `pnpm build` |
| 5 | `fix(billing): polish and fixes from integration testing` | Various | `pnpm build && pnpm lint` |

---

## Success Criteria

### Verification Commands
```bash
pnpm build          # Expected: Build succeeds
pnpm lint           # Expected: No new errors
```

### Final Checklist
- [x] Credit balance displayed on billing page
- [x] Credit pack cards shown with prices from API (or fallback)
- [x] Buy button triggers checkout flow
- [x] User redirected to Stripe checkout URL
- [x] Loading states during API calls
- [x] Error handling for failed requests
- [x] Old subscription UI removed (Current Plan, Cancel Subscription)
- [x] Dark mode works correctly
- [x] Responsive layout maintained
- [x] All commits follow conventional format
