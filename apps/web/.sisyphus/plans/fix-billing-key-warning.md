# Fix React Key Warning in BillingSettingsPage

## Context

### Original Request
Fix React warning: "Each child in a list should have a unique 'key' prop" on the billing page.

### Root Cause
The API returns credit packs that may not have an `id` field, causing `pack.id` to be `undefined`. When multiple items have `undefined` as their key, React warns about duplicate keys.

---

## Work Objectives

### Core Objective
Add fallback key to prevent React warning when API returns packs without `id` field.

### Concrete Deliverables
- Updated `src/app/(app)/settings/billing/page.tsx` with robust key handling

### Definition of Done
- [x] No React key warning appears on billing page
- [x] Credit packs still render correctly

### Must NOT Have (Guardrails)
- Do NOT change API integration logic
- Do NOT modify CreditPackCard component

---

## TODOs

- [x] 1. Add fallback key to packs.map()

  **What to do**:
  - Change line 145 from `{packs.map((pack) => (` to `{packs.map((pack, index) => (`
  - Change line 147 from `key={pack.id}` to `key={pack.id || pack.priceId || \`pack-${index}\`}`

  **File**: `src/app/(app)/settings/billing/page.tsx`

  **Exact change**:
  ```diff
  - {packs.map((pack) => (
  -   <CreditPackCard
  -     key={pack.id}
  + {packs.map((pack, index) => (
  +   <CreditPackCard
  +     key={pack.id || pack.priceId || `pack-${index}`}
  ```

  **Parallelizable**: NO (single file change)

  **Acceptance Criteria**:
  - [x] Refresh billing page - no React key warning in console
  - [x] Credit packs still display correctly

  **Commit**: YES
  - Message: `fix(billing): add fallback key for credit pack list`
  - Files: `src/app/(app)/settings/billing/page.tsx`

---

## Success Criteria

### Verification Commands
```bash
# Open browser console on billing page - should show no key warnings
```

### Final Checklist
- [x] No React key warning in browser console
- [x] Credit packs render as expected
