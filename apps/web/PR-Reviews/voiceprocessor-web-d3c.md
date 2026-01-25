# Code Review: Branch `voiceprocessor-web-d3c` (Revision 2)

**Reviewer:** Senior Frontend Engineer
**Date:** 2026-01-23
**Commits:**
- `9141e2d` - feat: implement mock payment integration
- `5dfc59e` - fix: refactor payment api and hook based on review

---

## Summary

The branch implements a mock payment integration for a credit-pack monetization system. **All critical issues from the initial review have been addressed.** The code now follows established codebase patterns and is ready for merge.

| Files Changed | Additions | Deletions |
|---------------|-----------|-----------|
| 7 | 121 | 1 |

---

## Previous Issues - Resolution Status

### Critical Issues

| Issue | Status |
|-------|--------|
| Missing `"use client"` directive | ✅ Fixed |
| Not using TanStack Query | ✅ Fixed |
| SSR-unsafe `window.location.origin` | ✅ Fixed |
| Bypasses API client architecture | ✅ Acceptable (mock) |

### Medium Issues

| Issue | Status |
|-------|--------|
| Error state not properly reset | ✅ Fixed (handled by React Query) |
| Mixed concerns in single file | ✅ Fixed (split into 3 files) |

---

## Changes Review

### `src/hooks/usePayment.ts` ✅

```typescript
"use client";

import { useMutation } from "@tanstack/react-query";
import { paymentService } from "@/lib/api";
import { useAuthStore } from "@/stores";
```

**What's good:**
- Now uses `useMutation` from TanStack Query (consistent with codebase)
- Has `"use client"` directive
- Imports service from barrel export
- Integrates with auth store to update credits on success
- Properly exposes `isPending` and `error` from mutation state
- Provides both `mutate` (fire-and-forget) and `mutateAsync` (awaitable) patterns

---

### `src/lib/api/payment/types.ts` ✅

Clean type definitions, properly separated from implementation.

---

### `src/lib/api/payment/constants.ts` ✅

Credit pack definitions isolated for easy modification and potential future API integration.

---

### `src/lib/api/payment/service.ts` ✅

```typescript
const origin =
  typeof window !== "undefined"
    ? window.location.origin
    : process.env.NEXT_PUBLIC_APP_URL || "http://localhost:3000";
```

**What's good:**
- SSR-safe with proper `typeof window` guard
- Fallback to environment variable
- Clear mock comments indicating future real API integration points

---

### `src/lib/api/index.ts` ✅

```typescript
export * from "./payment/types";
export * from "./payment/constants";
export { paymentService } from "./payment/service";
```

Proper barrel exports allowing clean imports throughout the app.

---

## Minor Observations (Non-blocking)

### 1. Credit calculation in `usePayment.ts:31-33`

```typescript
if (data.success && data.creditsAdded > 0) {
  setCredits(creditsRemaining + data.creditsAdded);
}
```

The code computes the new balance client-side. The response type already includes `newBalance` which should be used instead when the real API is integrated. The current approach is fine for the mock since concurrent payments are unlikely during testing.

**Recommendation:** Add a comment noting that the real implementation should use `data.newBalance` from the backend response.

---

### 2. Unused `sessionId` parameter

In `service.ts:43`, the `sessionId` parameter is accepted but not used in the mock logic. This is expected for a mock and correctly mirrors the real API signature.

---

## What's Good

1. **Consistent patterns** - Now uses TanStack Query like all other hooks
2. **Clean architecture** - Types, constants, and service properly separated
3. **SSR safety** - Proper window guards with env variable fallback
4. **State management** - Integrates correctly with Zustand auth store
5. **Developer experience** - Clear comments indicating mock vs real behavior
6. **Export patterns** - Clean barrel exports in `lib/api/index.ts`

---

## Verdict

**✅ Approved for merge.** All critical and medium issues have been addressed. The implementation now follows established codebase patterns and conventions.

---

## Checklist

- [x] Add `"use client"` directive to `usePayment.ts`
- [x] Refactor to use TanStack Query `useMutation`
- [x] Fix SSR-unsafe `window.location.origin` usage
- [x] Add store integration for credit updates after payment
- [x] Split `payment.ts` into types/constants/service files
- [x] Update barrel exports

---

## Post-Merge Recommendations

1. Add TODO comments for transitioning mock to real API
2. Consider adding integration tests for the payment flow
3. When backend is ready, switch `paymentService` to use `api.POST/GET` from openapi-fetch
