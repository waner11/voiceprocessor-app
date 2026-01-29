# Quick Wins Cleanup + Generation Detail Page

## TL;DR

> **Quick Summary**: Clean up ESLint errors, console.logs, and missing eslint-disable across the web app (3 issues), then wire up the existing generation detail page stub with AudioPlayer, GenerationStatus, and FeedbackForm components using existing hooks and TDD approach.
> 
> **Deliverables**:
> - Zero ESLint errors/warnings in `pnpm lint`
> - 4 console.log statements removed from auth code
> - eslint-disable comment added for intentional empty deps
> - Fully functional `/generations/[id]` detail page with audio playback, status display, and feedback submission
> - Vitest test suite for the detail page
> 
> **Estimated Effort**: Medium (cleanup ~1hr, detail page ~4-5hrs TDD)
> **Parallel Execution**: YES — 2 waves
> **Critical Path**: Housekeeping → Wave 1 (cleanup) → Wave 2 (detail page tasks in sequence via TDD)

---

## Context

### Original Request
User chose to: (1) batch 3 quick-win cleanup tasks, then (2) build the P1 generation detail page with audio player. TDD approach for the feature work.

### Interview Summary
**Key Discussions**:
- JWT security issue (`voiceprocessor-web-p9f`) was already ported to monorepo — closed it
- Quick wins: 3 beads issues for console.logs, eslint-disable, and ESLint errors
- Generation detail page: stub exists at `/generations/[id]/page.tsx`, all components and hooks ready
- User confirmed TDD with Vitest

**Research Findings**:
- **FeedbackForm already exists** (`components/FeedbackForm/FeedbackForm.tsx`, 182 lines) — star rating, quick tags, comment, success state. Has fake API call (setTimeout) that needs replacing.
- **GenerationStatus type mismatch**: API uses PascalCase 8 states (`"Pending"`, `"Processing"`, `"Completed"`, etc.) vs SignalR uses lowercase 4 states (`"queued"`, `"processing"`, `"completed"`, `"failed"`). GenerationStatus component imports the SignalR type.
- **SignalR backend does not exist** — `useGenerationHub()` is a no-op. `useGeneration(id)` already polls every 2s for in-progress items.
- **Backend FeedbackAccessor has TODO** — `useSubmitFeedback()` will hit the endpoint but backend doesn't persist yet.
- **FeedbackForm tags not supported by backend** — `SubmitFeedbackRequest` only accepts `{rating?, comment?}`, no tags field.
- **Detail page is a server component** — needs conversion to client component with `"use client"` + `useParams()`.
- **16 total console.* statements** but only 4 are `console.log` (the rest are `console.error`/`console.warn`). Issue `voiceprocessor-web-k3k` scoped to just the 4 `console.log`s.

### Metis Review
**Identified Gaps** (addressed):
- FeedbackForm already exists — changed from "build" to "wire existing component"
- GenerationStatus type mismatch — added explicit mapping function task
- SignalR backend missing — rely on useGeneration polling, don't call useGenerationHub
- Tags gap — append selected tags to comment string as workaround
- Server-to-client component conversion — explicit task for page conversion
- Voice name not in API response — show provider name instead

---

## Work Objectives

### Core Objective
Clean up ESLint issues and console.log statements across the web app, then wire up the generation detail page stub with existing components (AudioPlayer, GenerationStatus, FeedbackForm) and hooks (useGeneration, useSubmitFeedback) using TDD.

### Concrete Deliverables
- `apps/web/src/components/AuthProvider.tsx` — no console.logs, eslint-disable added
- `apps/web/src/stores/authStore.ts` — no console.log
- 11 files with ESLint fixes (4 errors + 12 warnings resolved)
- `apps/web/src/app/(app)/generations/[id]/page.tsx` — fully wired client component
- `apps/web/src/app/(app)/generations/[id]/__tests__/page.test.tsx` — Vitest test suite
- `apps/web/src/lib/utils/mapGenerationStatus.ts` — status mapping utility

### Definition of Done
- [ ] `pnpm lint` returns 0 errors, 0 warnings (in `apps/web/`)
- [ ] `pnpm build` exits 0 (in `apps/web/`)
- [ ] `pnpm test -- --run` all tests pass (in `apps/web/`)
- [ ] `grep -r "console\.log" apps/web/src/ --include="*.ts" --include="*.tsx"` returns 0 results
- [ ] `/generations/[id]` page renders with generation data, audio player, status, and feedback form

### Must Have
- Console.log removal (4 statements)
- ESLint-disable for intentional empty deps in AuthProvider
- ESLint error fixes (4 errors + 12 warnings)
- Detail page with loading, error, and data states
- AudioPlayer conditional rendering (only when audioUrl exists)
- FeedbackForm conditional rendering (only when status is Completed)
- Status mapping from API PascalCase to component format
- Details section populated (provider, duration, cost, timestamps)
- TDD test suite

### Must NOT Have (Guardrails)
- DO NOT replace console.error/console.warn with no-ops — those serve a purpose; defer to a logger utility task
- DO NOT build a new star rating or feedback component — FeedbackForm already exists
- DO NOT implement backend SignalR hub — rely on useGeneration() 2s polling
- DO NOT add backend FeedbackAccessor — backend TODO stays deferred
- DO NOT add voice name to API response — show provider name in Voice field
- DO NOT refactor AudioPlayer, GenerationStatus, or FeedbackForm internals
- DO NOT add chapters data wiring — chapters section stays as placeholder
- DO NOT add dark mode fixes, navigation chrome, breadcrumbs, or loading skeletons per section
- DO NOT restructure NavigationProgress or Header components when fixing setState-in-effect — use eslint-disable with justification
- DO NOT change behavioral logic during ESLint cleanup
- DO NOT "discover" additional cleanup while in files — fix only the listed issues
- DO NOT touch console.error/console.warn statements (12 total) — out of scope for this plan

---

## Verification Strategy (MANDATORY)

### Test Decision
- **Infrastructure exists**: YES (Vitest already configured from credits display work)
- **User wants tests**: TDD (RED → GREEN → REFACTOR)
- **Framework**: Vitest + @testing-library/react

### TDD Structure

Each detail page task follows RED-GREEN-REFACTOR:

1. **RED**: Write failing test first
   - Test file: `apps/web/src/app/(app)/generations/[id]/__tests__/page.test.tsx`
   - Test command: `pnpm test -- --run --reporter=verbose`
   - Expected: FAIL (test exists, implementation doesn't)
2. **GREEN**: Implement minimum code to pass
   - Command: `pnpm test -- --run --reporter=verbose`
   - Expected: PASS
3. **REFACTOR**: Clean up while keeping green
   - Command: `pnpm test -- --run --reporter=verbose`
   - Expected: PASS (still)

### Test Patterns to Follow
- Mock hooks with `vi.mock("@/hooks/useGenerations")`
- Use `@testing-library/react` with `render`, `screen`, `waitFor`
- Follow existing pattern in `apps/web/src/app/(app)/generate/__tests__/formatCredits.test.ts`

---

## Execution Strategy

### Pre-Work: Housekeeping (Before Wave 1)

Before any task execution, the executor MUST:
1. Switch to main: `git checkout main && git pull --rebase origin main`
2. Verify clean working directory

### Parallel Execution Waves

```
Housekeeping (switch to main, sync):
└── Required before any work begins

Wave 1 (Cleanup — single branch):
├── Task 1: Remove console.log statements (voiceprocessor-web-k3k)
├── Task 2: Add eslint-disable comment (voiceprocessor-web-a1v)
└── Task 3: Fix ESLint errors and warnings (voiceprocessor-web-cum)
    → All three batched on ONE branch, ONE commit, ONE PR

Wave 2 (Detail Page — sequential TDD on separate branch):
├── Task 4: Convert page to client component + wire useGeneration
├── Task 5: Create status mapping utility + wire GenerationStatus
├── Task 6: Wire AudioPlayer (conditional on audioUrl)
├── Task 7: Wire FeedbackForm to useSubmitFeedback (conditional on Completed)
├── Task 8: Populate details section from GenerationResponse
└── Task 9: Handle edge cases (404, invalid UUID, cancelled)

Critical Path: Housekeeping → Wave 1 → Wave 2 (Tasks 4→5→6→7→8→9)
```

### Dependency Matrix

| Task | Depends On | Blocks | Can Parallelize With |
|------|------------|--------|---------------------|
| 1 | Housekeeping | 4 (via clean main) | 2, 3 |
| 2 | Housekeeping | 4 | 1, 3 |
| 3 | Housekeeping | 4 | 1, 2 |
| 4 | 1, 2, 3 (merged) | 5, 6, 7, 8, 9 | None |
| 5 | 4 | 6 | None |
| 6 | 5 | 7 | None |
| 7 | 6 | 8 | None |
| 8 | 7 | 9 | None |
| 9 | 8 | None | None |

### Agent Dispatch Summary

| Wave | Tasks | Recommended Agents |
|------|-------|-------------------|
| 1 | 1, 2, 3 (batched) | `delegate_task(category="quick", load_skills=["git-master"])` |
| 2 | 4, 5, 6, 7, 8, 9 | `delegate_task(category="visual-engineering", load_skills=["frontend-ui-ux"])` |

---

## TODOs

### WAVE 1: Quick Wins Cleanup (Single Branch)

> All 3 cleanup tasks are batched on one branch. Create branch, do all fixes, one commit, one PR.
> Branch name: Use first issue ID as branch name (voiceprocessor-web-k3k) or create a combined branch.
> Close all 3 issues (voiceprocessor-web-k3k, voiceprocessor-web-a1v, voiceprocessor-web-cum) after PR merge.

- [x] 1. Remove console.log statements from auth code

  **What to do**:
  - Remove 4 `console.log` statements:
    - `apps/web/src/components/AuthProvider.tsx:38` — `console.log('Session validated')`
    - `apps/web/src/components/AuthProvider.tsx:40` — `console.log('Session invalid, logging out')`
    - `apps/web/src/components/AuthProvider.tsx:44` — `console.log('Session validation failed, logging out')`
    - `apps/web/src/stores/authStore.ts:68` — `console.log('Legacy token cleaned up')`
  - Simply delete the lines. Do not replace with logger calls (defer to separate task).

  **Must NOT do**:
  - Do NOT touch `console.error` or `console.warn` statements anywhere
  - Do NOT add a logger utility — that's a separate task
  - Do NOT change any logic in these files

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: Single-file changes, <10 lines, trivial deletion
  - **Skills**: [`git-master`]
    - `git-master`: Needed for proper branch creation, commit, PR workflow

  **Parallelization**:
  - **Can Run In Parallel**: YES (batched with Tasks 2, 3 on same branch)
  - **Parallel Group**: Wave 1 (with Tasks 2, 3)
  - **Blocks**: Task 4 (needs clean main after merge)
  - **Blocked By**: Housekeeping (switch to main)

  **References**:

  **Pattern References**:
  - `apps/web/src/components/AuthProvider.tsx:26-52` — Full useEffect containing the 3 console.logs to remove
  - `apps/web/src/stores/authStore.ts:59-75` — onRehydrateStorage callback containing the console.log to remove

  **Documentation References**:
  - `apps/web/AGENTS.md:Logging Standards` — Project logging standards (console.log is explicitly prohibited in production code)

  **WHY Each Reference Matters**:
  - AuthProvider.tsx: Lines 38, 40, 44 are the exact console.log lines inside the validateSession async function
  - authStore.ts: Line 68 is inside the onRehydrateStorage callback's legacy token cleanup

  **Acceptance Criteria**:
  ```bash
  # Agent runs:
  grep -rn "console\.log" apps/web/src/ --include="*.ts" --include="*.tsx"
  # Assert: 0 results (no console.log anywhere in src/)
  
  # Agent runs:
  cd apps/web && pnpm build 2>&1 | tail -1
  # Assert: Exit code 0
  ```

  **Commit**: YES (groups with Tasks 2, 3)
  - Message: `fix(web): remove console.logs, fix eslint errors, add eslint-disable for auth deps`
  - Files: All files modified in Tasks 1, 2, 3
  - Pre-commit: `cd apps/web && pnpm lint && pnpm build && pnpm test -- --run`

---

- [x] 2. Add eslint-disable comment for intentional empty deps

  **What to do**:
  - Add eslint-disable comment before the empty dependency array in AuthProvider.tsx:
    ```typescript
    // Session validation runs once on mount — adding deps would cause infinite loops
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);
    ```
  - The comment MUST explain WHY deps are empty (not just suppress the warning)

  **Must NOT do**:
  - Do NOT add the missing dependencies (isAuthenticated, login, logout, setLoading) — they would cause infinite re-validation loops
  - Do NOT restructure the useEffect or move to useCallback

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: Single line addition, trivial change
  - **Skills**: [`git-master`]
    - `git-master`: Needed for commit workflow

  **Parallelization**:
  - **Can Run In Parallel**: YES (batched with Tasks 1, 3 on same branch)
  - **Parallel Group**: Wave 1 (with Tasks 1, 3)
  - **Blocks**: Task 4
  - **Blocked By**: Housekeeping

  **References**:

  **Pattern References**:
  - `apps/web/src/components/AuthProvider.tsx:26-52` — The useEffect in question. Line 52 is `}, []);` which needs the eslint-disable above it

  **WHY Each Reference Matters**:
  - The useEffect runs `validateSession()` which uses `isAuthenticated`, `login`, `logout`, `setLoading` from Zustand. These are intentionally excluded because adding them would trigger re-validation on every state change, creating an infinite loop.

  **Acceptance Criteria**:
  ```bash
  # Agent runs:
  cd apps/web && pnpm lint 2>&1 | grep "react-hooks/exhaustive-deps"
  # Assert: 0 results (warning suppressed)
  ```

  **Commit**: YES (groups with Tasks 1, 3)

---

- [x] 3. Fix ESLint errors and warnings across web app

  **What to do**:
  - Fix 4 ESLint **errors**:
    1. `apps/web/src/app/(auth)/login/page.tsx:32` — Replace `any` type with `ApiError` or specific error type
    2. `apps/web/src/app/(auth)/signup/page.tsx:45` — Replace `any` type with `ApiError` or specific error type
    3. `apps/web/src/components/NavigationProgress.tsx:14` — Add `// eslint-disable-next-line react-hooks/set-state-in-effect` with justification comment: "Intentional: setIsNavigating(false) on route change is safe in this context"
    4. `apps/web/src/components/layout/Header.tsx:29` — Add `// eslint-disable-next-line react-hooks/set-state-in-effect` with justification comment: "Intentional: setIsMounted(true) on mount is the standard hydration pattern"
  - Fix 12 ESLint **warnings** (all `@typescript-eslint/no-unused-vars`):
    5. `apps/web/src/app/(app)/settings/connections/page.tsx:95` — Prefix unused `isConnected` with `_` or remove
    6. `apps/web/src/app/(app)/voices/page.tsx:5` — Remove unused `cn` import
    7. `apps/web/src/app/(auth)/forgot-password/page.tsx:28` — Prefix unused `data` with `_`
    8. `apps/web/src/app/(auth)/forgot-password/page.tsx:39` — Prefix unused `err` with `_`
    9. `apps/web/src/app/(auth)/login/page.tsx:18` — Prefix unused `error` with `_` or remove
    10. `apps/web/src/components/AuthProvider.tsx:43` — Prefix unused `error` with `_`
    11. `apps/web/src/components/GenerationStatus/GenerationStatus.tsx:3` — Remove unused `useEffect` import
    12. `apps/web/src/components/NavigationProgress.tsx:37` — Remove unused `handleComplete` variable or prefix with `_`
    13. `apps/web/src/lib/api/payment/service.ts:76` — Prefix unused `sessionId` with `_`
    14. `apps/web/src/lib/api/payment/service.ts:77` — Prefix unused `packId` with `_`
    15. `apps/web/src/stores/authStore.ts:70` — Prefix unused `e` with `_`

  **Must NOT do**:
  - Do NOT restructure NavigationProgress or Header components — only add eslint-disable comments
  - Do NOT change behavioral logic in any file
  - Do NOT refactor adjacent code while fixing ESLint issues
  - Do NOT add a logger utility

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: Mechanical fixes across known files, all locations identified
  - **Skills**: [`git-master`]
    - `git-master`: Needed for commit workflow

  **Parallelization**:
  - **Can Run In Parallel**: YES (batched with Tasks 1, 2 on same branch)
  - **Parallel Group**: Wave 1 (with Tasks 1, 2)
  - **Blocks**: Task 4
  - **Blocked By**: Housekeeping

  **References**:

  **Pattern References**:
  - `apps/web/src/hooks/useAuth.ts:30-35` — `ApiError` interface definition to use for `any` type replacements in login/signup pages
  - `apps/web/src/app/(auth)/login/page.tsx:32` — The `any` type to fix (use ApiError from useAuth)
  - `apps/web/src/app/(auth)/signup/page.tsx:45` — The `any` type to fix (use ApiError from useAuth)
  - `apps/web/src/components/NavigationProgress.tsx:14` — setState-in-effect to eslint-disable
  - `apps/web/src/components/layout/Header.tsx:29` — setState-in-effect to eslint-disable (hydration pattern)

  **WHY Each Reference Matters**:
  - useAuth.ts has the `ApiError` interface that login/signup pages should reference instead of `any`
  - NavigationProgress and Header setState-in-effect are intentional patterns (route change handling and hydration) — suppression is correct, not refactoring
  - All unused vars are straightforward: prefix with `_` or remove unused imports

  **Acceptance Criteria**:
  ```bash
  # Agent runs:
  cd apps/web && pnpm lint 2>&1
  # Assert: 0 errors, 0 warnings
  
  # Agent runs:
  cd apps/web && pnpm build 2>&1 | tail -1
  # Assert: Exit code 0
  
  # Agent runs:
  cd apps/web && pnpm test -- --run 2>&1
  # Assert: All tests pass
  ```

  **Commit**: YES (groups with Tasks 1, 2)
  - Message: `fix(web): remove console.logs, fix eslint errors, add eslint-disable for auth deps`
  - Files: All files listed in Tasks 1, 2, 3
  - Pre-commit: `cd apps/web && pnpm lint && pnpm build && pnpm test -- --run`

---

### WAVE 2: Generation Detail Page (TDD, Sequential)

> Create new branch (voiceprocessor-web-9dj). TDD approach: write tests first, then implement.
> Each task builds on the previous. Commit after each RED→GREEN→REFACTOR cycle.

- [x] 4. Convert detail page to client component + wire useGeneration hook

  **What to do**:
  - **RED**: Write test file `apps/web/src/app/(app)/generations/[id]/__tests__/page.test.tsx`:
    - Test: "renders loading state when data is fetching" — mock `useGeneration` with `isLoading: true`, assert loading indicator visible
    - Test: "renders generation data when loaded" — mock `useGeneration` with complete GenerationResponse, assert key data visible
    - Test: "renders error state when fetch fails" — mock `useGeneration` with `error`, assert error message visible
  - **GREEN**: Convert `generations/[id]/page.tsx`:
    - Add `"use client"` directive at top
    - Replace `async function GenerationPage({ params })` with regular function using `useParams()`
    - Import and call `useGeneration(id)` hook
    - Render three states: loading (spinner/skeleton), error (error message + back link), data (existing layout)
    - Add a "Back to Generations" link at the top
  - **REFACTOR**: Clean up, ensure no unnecessary re-renders

  **Must NOT do**:
  - Do NOT wire AudioPlayer, GenerationStatus, or FeedbackForm yet — just render the page shell with data
  - Do NOT add per-section loading skeletons — one page-level loading state

  **Recommended Agent Profile**:
  - **Category**: `visual-engineering`
    - Reason: Frontend component work with React hooks and client-side rendering
  - **Skills**: [`frontend-ui-ux`]
    - `frontend-ui-ux`: React component patterns, client-side routing, hook integration

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential (first in Wave 2)
  - **Blocks**: Tasks 5, 6, 7, 8, 9
  - **Blocked By**: Tasks 1, 2, 3 (clean main after Wave 1 merge)

  **References**:

  **Pattern References**:
  - `apps/web/src/app/(app)/generations/[id]/page.tsx:1-88` — The existing stub page to convert (server component with placeholder layout)
  - `apps/web/src/hooks/useGenerations.ts:48-73` — `useGeneration(id)` hook: returns `{ data, error, isLoading }` with 2s polling for in-progress statuses
  - `apps/web/src/app/(app)/generate/__tests__/formatCredits.test.ts` — Existing test pattern to follow (vi.mock, describe/it structure)

  **API/Type References**:
  - `apps/web/src/lib/api/types.ts:648-691` — `GenerationResponse` type with all fields (status, audioUrl, progress, provider, cost, timestamps)
  - `apps/web/src/lib/api/types.ts:691` — `GenerationStatus` enum: `"Pending" | "Analyzing" | "Chunking" | "Processing" | "Merging" | "Completed" | "Failed" | "Cancelled"`

  **Test References**:
  - `apps/web/src/app/(app)/generate/__tests__/formatCredits.test.ts` — Test file structure, vi.mock usage, describe/it pattern

  **WHY Each Reference Matters**:
  - The stub page (lines 1-88) provides the HTML structure/layout to preserve when converting to client component
  - useGeneration hook signature shows what data/loading/error states to handle
  - GenerationResponse type shows all available fields for the details section
  - formatCredits test shows the project's testing conventions

  **Acceptance Criteria**:

  ```bash
  # Agent runs:
  cd apps/web && pnpm test -- --run --reporter=verbose 2>&1 | grep -E "PASS|FAIL|Tests:"
  # Assert: All tests pass including new tests for loading, data, and error states
  
  # Agent runs:
  cd apps/web && pnpm build 2>&1 | tail -1
  # Assert: Exit code 0
  ```

  **Commit**: YES
  - Message: `feat(web): convert generation detail page to client component with data fetching`
  - Files: `apps/web/src/app/(app)/generations/[id]/page.tsx`, `apps/web/src/app/(app)/generations/[id]/__tests__/page.test.tsx`
  - Pre-commit: `cd apps/web && pnpm test -- --run`

---

- [x] 5. Create status mapping utility + wire GenerationStatus component

  **What to do**:
  - **RED**: Write tests:
    - Test `mapGenerationStatus` utility: `"Completed"` → `"completed"`, `"Pending"` → `"queued"`, `"Processing"` → `"processing"`, `"Failed"` → `"failed"`, `"Cancelled"` → `"failed"`, `"Analyzing"` → `"processing"`, `"Chunking"` → `"processing"`, `"Merging"` → `"processing"`
    - Test detail page: "renders GenerationStatus with mapped status" — mock useGeneration with `status: "Processing"`, assert GenerationStatus component receives `status: "processing"`
  - **GREEN**: Create `apps/web/src/lib/utils/mapGenerationStatus.ts`:
    ```typescript
    import type { components } from "@/lib/api/types";
    import type { GenerationStatus as SignalRStatus } from "@/lib/signalr/connection";

    type ApiStatus = components["schemas"]["GenerationStatus"];

    const STATUS_MAP: Record<ApiStatus, SignalRStatus> = {
      Pending: "queued",
      Analyzing: "processing",
      Chunking: "processing",
      Processing: "processing",
      Merging: "processing",
      Completed: "completed",
      Failed: "failed",
      Cancelled: "failed",
    };

    export function mapGenerationStatus(apiStatus: ApiStatus): SignalRStatus {
      return STATUS_MAP[apiStatus] ?? "queued";
    }
    ```
  - Wire GenerationStatus component into the detail page's Status section, passing mapped status, progress, and errorMessage

  **Must NOT do**:
  - Do NOT modify GenerationStatus component internals
  - Do NOT call useGenerationHub() — backend SignalR hub doesn't exist; rely on useGeneration polling

  **Recommended Agent Profile**:
  - **Category**: `visual-engineering`
    - Reason: Frontend utility + component integration
  - **Skills**: [`frontend-ui-ux`]
    - `frontend-ui-ux`: Type-safe React component wiring

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential
  - **Blocks**: Task 6
  - **Blocked By**: Task 4

  **References**:

  **Pattern References**:
  - `apps/web/src/lib/signalr/connection.ts:6` — SignalR `GenerationStatus` type: `"queued" | "processing" | "completed" | "failed"` — the TARGET type for mapping
  - `apps/web/src/components/GenerationStatus/GenerationStatus.tsx` — The component to wire. Check its props interface for what it accepts (status, progress, errorMessage, etc.)

  **API/Type References**:
  - `apps/web/src/lib/api/types.ts:691` — API `GenerationStatus` enum: `"Pending" | "Analyzing" | "Chunking" | "Processing" | "Merging" | "Completed" | "Failed" | "Cancelled"` — the SOURCE type for mapping

  **WHY Each Reference Matters**:
  - The SignalR connection.ts defines the 4 lowercase status values that GenerationStatus component expects
  - The API types.ts defines the 8 PascalCase status values that useGeneration returns
  - The mapping function bridges these two incompatible type systems

  **Acceptance Criteria**:

  ```bash
  # Agent runs:
  cd apps/web && pnpm test -- --run --reporter=verbose 2>&1 | grep -E "mapGenerationStatus|GenerationStatus"
  # Assert: All mapping tests pass (8 status mappings verified)
  
  # Agent runs:
  cd apps/web && pnpm build 2>&1 | tail -1
  # Assert: Exit code 0
  ```

  **Commit**: YES
  - Message: `feat(web): add status mapping utility and wire GenerationStatus to detail page`
  - Files: `apps/web/src/lib/utils/mapGenerationStatus.ts`, `apps/web/src/app/(app)/generations/[id]/page.tsx`, test files
  - Pre-commit: `cd apps/web && pnpm test -- --run`

---

- [x] 6. Wire AudioPlayer component (conditional on audioUrl)

  **What to do**:
  - **RED**: Write tests:
    - Test: "renders AudioPlayer when generation is completed and has audioUrl" — mock useGeneration with `status: "Completed"`, `audioUrl: "https://storage/audio.mp3"`, assert AudioPlayer is rendered
    - Test: "does NOT render AudioPlayer when audioUrl is null" — mock with `status: "Processing"`, `audioUrl: null`, assert AudioPlayer is NOT in DOM, assert placeholder text shown instead
    - Test: "does NOT render AudioPlayer when status is Pending" — mock with `status: "Pending"`, assert placeholder
  - **GREEN**: In the detail page's Audio Player section:
    - Import `AudioPlayer` from `@/components/AudioPlayer/AudioPlayer`
    - Conditionally render: `generation.audioUrl ? <AudioPlayer audioUrl={generation.audioUrl} /> : <PlaceholderMessage />`
    - Placeholder should say "Audio will appear here once generation completes" (or similar) with generation progress if in-progress
  - Remove the static placeholder HTML (fake play button, fake select, etc.)

  **Must NOT do**:
  - Do NOT modify AudioPlayer component internals
  - Do NOT pass chapters prop (no data source yet)
  - Do NOT implement download separately — AudioPlayer already has download buttons

  **Recommended Agent Profile**:
  - **Category**: `visual-engineering`
    - Reason: Frontend conditional rendering with component integration
  - **Skills**: [`frontend-ui-ux`]
    - `frontend-ui-ux`: React conditional rendering patterns

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential
  - **Blocks**: Task 7
  - **Blocked By**: Task 5

  **References**:

  **Pattern References**:
  - `apps/web/src/components/AudioPlayer/AudioPlayer.tsx` — Full AudioPlayer component. Check props interface: likely accepts `audioUrl: string`, `chapters?: Chapter[]`, possibly format-related props

  **API/Type References**:
  - `apps/web/src/lib/api/types.ts:661` — `audioUrl?: string | null` field in GenerationResponse
  - `apps/web/src/lib/api/types.ts:662` — `audioFormat?: string | null` field
  - `apps/web/src/lib/api/types.ts:664` — `audioDurationMs?: number | null` field

  **WHY Each Reference Matters**:
  - AudioPlayer.tsx props interface tells you exactly what props to pass (audioUrl is the minimum)
  - The API types show audioUrl is nullable — requires conditional rendering guard
  - audioDurationMs might be useful for AudioPlayer's duration display

  **Acceptance Criteria**:

  ```bash
  # Agent runs:
  cd apps/web && pnpm test -- --run --reporter=verbose 2>&1 | grep -E "AudioPlayer"
  # Assert: All AudioPlayer conditional rendering tests pass
  
  # Agent runs:
  cd apps/web && pnpm build 2>&1 | tail -1
  # Assert: Exit code 0
  ```

  **Commit**: YES
  - Message: `feat(web): wire AudioPlayer to generation detail page`
  - Files: `apps/web/src/app/(app)/generations/[id]/page.tsx`, test files
  - Pre-commit: `cd apps/web && pnpm test -- --run`

---

- [ ] 7. Wire FeedbackForm to useSubmitFeedback (conditional on Completed)

  **What to do**:
  - **RED**: Write tests:
    - Test: "renders FeedbackForm when generation status is Completed" — mock useGeneration with `status: "Completed"`, assert FeedbackForm is rendered
    - Test: "does NOT render FeedbackForm when status is Processing" — assert FeedbackForm is NOT in DOM
    - Test: "does NOT render FeedbackForm when status is Failed" — assert FeedbackForm is NOT in DOM
    - Test: "calls useSubmitFeedback when FeedbackForm submits" — mock useSubmitFeedback mutation, simulate form submission, assert mutate called with `{ id, rating, comment }` (tags appended to comment)
  - **GREEN**: In the detail page's Feedback section:
    - Import `FeedbackForm` from `@/components/FeedbackForm/FeedbackForm`
    - Import `useSubmitFeedback` from `@/hooks/useGenerations`
    - Conditionally render: only when `generation.status === "Completed"`
    - Wire FeedbackForm's `onSubmit` callback:
      ```typescript
      const submitFeedback = useSubmitFeedback();
      
      const handleFeedbackSubmit = (feedback: FeedbackData) => {
        // Append tags to comment since backend doesn't support tags field
        const tagPrefix = feedback.tags.length > 0
          ? `[Tags: ${feedback.tags.join(", ")}] `
          : "";
        submitFeedback.mutate({
          id: generation.id,
          rating: feedback.rating,
          comment: tagPrefix + feedback.comment,
        });
      };
      ```
  - Replace the static "Rate this generation..." placeholder

  **Must NOT do**:
  - Do NOT modify FeedbackForm internals
  - Do NOT add a tags field to the backend SubmitFeedbackRequest
  - Do NOT add error handling beyond what FeedbackForm already has

  **Recommended Agent Profile**:
  - **Category**: `visual-engineering`
    - Reason: Frontend hook-to-component wiring with data transformation
  - **Skills**: [`frontend-ui-ux`]
    - `frontend-ui-ux`: React hook integration and callback patterns

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential
  - **Blocks**: Task 8
  - **Blocked By**: Task 6

  **References**:

  **Pattern References**:
  - `apps/web/src/components/FeedbackForm/FeedbackForm.tsx:6-17` — Props interface: `{ generationId: string, onSubmit?: (feedback: FeedbackData) => void, className?: string }` and `FeedbackData` type with `{ generationId, rating, tags, comment }`
  - `apps/web/src/components/FeedbackForm/FeedbackForm.tsx:48-72` — Current `handleSubmit` with fake API call (setTimeout) that needs to be replaced via the onSubmit callback

  **API/Type References**:
  - `apps/web/src/hooks/useGenerations.ts:131-149` — `useSubmitFeedback()` hook: accepts `{ id: string, rating?: number, comment?: string }`
  - `apps/web/src/lib/api/types.ts:707-710` — `SubmitFeedbackRequest`: `{ rating?: number | null, comment?: string | null }` — NO tags field

  **WHY Each Reference Matters**:
  - FeedbackForm props show it accepts an `onSubmit` callback — this is how we inject the real API call
  - FeedbackData includes `tags: string[]` but backend doesn't support it — we append tags to comment string
  - useSubmitFeedback shows the exact mutation shape `{id, rating, comment}` — must transform FeedbackData to match

  **Acceptance Criteria**:

  ```bash
  # Agent runs:
  cd apps/web && pnpm test -- --run --reporter=verbose 2>&1 | grep -E "FeedbackForm|feedback"
  # Assert: All FeedbackForm rendering and submission tests pass
  
  # Agent runs:
  cd apps/web && pnpm build 2>&1 | tail -1
  # Assert: Exit code 0
  ```

  **Commit**: YES
  - Message: `feat(web): wire FeedbackForm to useSubmitFeedback on generation detail page`
  - Files: `apps/web/src/app/(app)/generations/[id]/page.tsx`, test files
  - Pre-commit: `cd apps/web && pnpm test -- --run`

---

- [ ] 8. Populate details section from GenerationResponse

  **What to do**:
  - **RED**: Write tests:
    - Test: "displays provider name" — mock with `provider: "ElevenLabs"`, assert "ElevenLabs" visible
    - Test: "displays audio duration formatted" — mock with `audioDurationMs: 125000`, assert "2:05" visible
    - Test: "displays actual cost when available" — mock with `actualCost: 0.25`, assert "$0.25" visible
    - Test: "displays estimated cost when actual not available" — mock with `estimatedCost: 0.30, actualCost: null`, assert "~$0.30" visible
    - Test: "displays creation date" — mock with `createdAt: "2026-01-28T12:00:00Z"`, assert formatted date visible
    - Test: "displays character count" — mock with `characterCount: 1500`, assert "1,500 characters" visible
  - **GREEN**: Populate the Details section `<dl>` in the detail page:
    - Voice → Show `generation.provider ?? "--"` (voice name not in API response)
    - Provider → Show `generation.provider ?? "--"`
    - Duration → Format `generation.audioDurationMs` as mm:ss (or "--" if null)
    - Cost → Show `generation.actualCost ?? generation.estimatedCost` formatted as currency (or "--")
    - Add additional fields: Character Count, Status, Created At, Completed At
  - Use the existing `formatCostWithCredits` helper from `apps/web/src/app/(app)/generate/page.tsx` if cost+credits formatting is needed, or create a simple `formatDuration` utility

  **Must NOT do**:
  - Do NOT fetch voice name from a separate API endpoint
  - Do NOT add backend changes

  **Recommended Agent Profile**:
  - **Category**: `visual-engineering`
    - Reason: Frontend data display with formatting
  - **Skills**: [`frontend-ui-ux`]
    - `frontend-ui-ux`: Data formatting and display patterns

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential
  - **Blocks**: Task 9
  - **Blocked By**: Task 7

  **References**:

  **Pattern References**:
  - `apps/web/src/app/(app)/generations/[id]/page.tsx:56-76` — The Details `<dl>` section with Voice, Provider, Duration, Cost placeholders to populate
  - `apps/web/src/app/(app)/generate/page.tsx` — `formatCostWithCredits` helper function if cost formatting is needed

  **API/Type References**:
  - `apps/web/src/lib/api/types.ts:648-676` — Full GenerationResponse with all fields to display:
    - `provider?: Provider` (line 660)
    - `audioDurationMs?: number | null` (line 664)
    - `estimatedCost?: number | null` (line 666)
    - `actualCost?: number | null` (line 668)
    - `characterCount: number` (line 653)
    - `createdAt: string` (line 671)
    - `completedAt?: string | null` (line 675)

  **WHY Each Reference Matters**:
  - The dl section is the exact HTML to modify with real data bindings
  - GenerationResponse fields show exactly what data is available and which are nullable
  - formatCostWithCredits can be reused for consistent cost display across the app

  **Acceptance Criteria**:

  ```bash
  # Agent runs:
  cd apps/web && pnpm test -- --run --reporter=verbose 2>&1 | grep -E "details|provider|duration|cost"
  # Assert: All detail section display tests pass
  
  # Agent runs:
  cd apps/web && pnpm build 2>&1 | tail -1
  # Assert: Exit code 0
  ```

  **Commit**: YES
  - Message: `feat(web): populate generation details section with real data`
  - Files: `apps/web/src/app/(app)/generations/[id]/page.tsx`, test files, optional utility file
  - Pre-commit: `cd apps/web && pnpm test -- --run`

---

- [ ] 9. Handle edge cases (404, invalid UUID, cancelled generation)

  **What to do**:
  - **RED**: Write tests:
    - Test: "renders 'Generation not found' when API returns 404" — mock useGeneration with specific 404 error, assert "not found" message and link back to `/generations`
    - Test: "renders cancelled state distinctly from failed" — mock with `status: "Cancelled"`, assert "Cancelled" text visible (not "Failed")
    - Test: "handles non-UUID id gracefully" — render with `id: "not-a-uuid"`, assert error state (useGeneration will return error from API)
  - **GREEN**:
    - 404 handling: Check if error is 404, show "Generation not found" with a "Back to Generations" link
    - Cancelled: In the details section, show "Cancelled" badge/text. Map to `"failed"` for GenerationStatus component but display "Cancelled" in the details info
    - Invalid UUID: Handled naturally by useGeneration → API returns 404 → handled by 404 logic above
  - Ensure all edge cases show meaningful UI, not blank pages or unhandled errors

  **Must NOT do**:
  - Do NOT add error boundaries (this is page-level error handling, not component-level)
  - Do NOT redirect automatically on 404 — show inline message with link

  **Recommended Agent Profile**:
  - **Category**: `visual-engineering`
    - Reason: Frontend error state handling
  - **Skills**: [`frontend-ui-ux`]
    - `frontend-ui-ux`: Error UX patterns

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Sequential (last in Wave 2)
  - **Blocks**: None (final task)
  - **Blocked By**: Task 8

  **References**:

  **Pattern References**:
  - `apps/web/src/app/(app)/generations/[id]/page.tsx` — The page being modified (by this point, it's a fully wired client component from Tasks 4-8)
  - `apps/web/src/hooks/useGenerations.ts:48-73` — useGeneration hook: `enabled: !!id` guard already exists; errors come from the API call

  **API/Type References**:
  - `apps/web/src/lib/api/types.ts:691` — `"Cancelled"` is a valid GenerationStatus value that needs distinct handling

  **WHY Each Reference Matters**:
  - The hook already guards against empty ID but not invalid UUID format
  - The Cancelled status is distinct from Failed but GenerationStatus component only knows "failed" — need a visual distinction

  **Acceptance Criteria**:

  ```bash
  # Agent runs:
  cd apps/web && pnpm test -- --run --reporter=verbose 2>&1 | grep -E "not found|cancelled|edge"
  # Assert: All edge case tests pass
  
  # Agent runs:
  cd apps/web && pnpm lint 2>&1 | tail -3
  # Assert: 0 errors, 0 warnings
  
  # Agent runs:
  cd apps/web && pnpm build 2>&1 | tail -1
  # Assert: Exit code 0
  
  # FINAL: Run all tests
  cd apps/web && pnpm test -- --run 2>&1
  # Assert: ALL tests pass (including all new and existing tests)
  ```

  **Commit**: YES
  - Message: `feat(web): handle edge cases in generation detail page (404, cancelled, invalid ID)`
  - Files: `apps/web/src/app/(app)/generations/[id]/page.tsx`, test files
  - Pre-commit: `cd apps/web && pnpm lint && pnpm build && pnpm test -- --run`

---

## Commit Strategy

| After Task(s) | Message | Key Files | Verification |
|------------|---------|-------|--------------|
| 1, 2, 3 (Wave 1) | `fix(web): remove console.logs, fix eslint errors, add eslint-disable for auth deps` | AuthProvider, authStore, login/signup, NavigationProgress, Header, voices, connections, forgot-password, GenerationStatus, payment/service | `pnpm lint && pnpm build && pnpm test -- --run` |
| 4 | `feat(web): convert generation detail page to client component with data fetching` | `generations/[id]/page.tsx`, test file | `pnpm test -- --run` |
| 5 | `feat(web): add status mapping utility and wire GenerationStatus to detail page` | `lib/utils/mapGenerationStatus.ts`, page, tests | `pnpm test -- --run` |
| 6 | `feat(web): wire AudioPlayer to generation detail page` | page, tests | `pnpm test -- --run` |
| 7 | `feat(web): wire FeedbackForm to useSubmitFeedback on generation detail page` | page, tests | `pnpm test -- --run` |
| 8 | `feat(web): populate generation details section with real data` | page, tests, optional utility | `pnpm test -- --run` |
| 9 | `feat(web): handle edge cases in generation detail page (404, cancelled, invalid ID)` | page, tests | `pnpm lint && pnpm build && pnpm test -- --run` |

---

## Beads Issue Tracking

### Wave 1 (Cleanup Branch)
- **Branch**: `voiceprocessor-web-k3k` (primary cleanup issue ID)
- **Issues to close after PR merge**:
  - `bd close voiceprocessor-web-k3k --reason="Console.logs removed, PR merged"`
  - `bd close voiceprocessor-web-a1v --reason="ESLint-disable added, PR merged"`
  - `bd close voiceprocessor-web-cum --reason="ESLint errors fixed, PR merged"`

### Wave 2 (Detail Page Branch)
- **Branch**: `voiceprocessor-web-9dj`
- **Issues to close after PR merge**:
  - `bd close voiceprocessor-web-9dj --reason="Generation detail page with audio player, status, feedback implemented. PR merged"`

---

## Success Criteria

### Verification Commands
```bash
# Zero console.logs
grep -rn "console\.log" apps/web/src/ --include="*.ts" --include="*.tsx"
# Expected: 0 results

# Zero ESLint issues
cd apps/web && pnpm lint
# Expected: 0 errors, 0 warnings

# Build succeeds
cd apps/web && pnpm build
# Expected: Exit code 0

# All tests pass
cd apps/web && pnpm test -- --run
# Expected: All pass (existing + new detail page tests)
```

### Final Checklist
- [ ] All "Must Have" present (console.log removal, eslint fixes, detail page, AudioPlayer, FeedbackForm, status mapping, details section, edge cases, TDD tests)
- [ ] All "Must NOT Have" absent (no new components built, no backend changes, no SignalR hub, no chapter wiring, no dark mode fixes, no navigation chrome)
- [ ] All tests pass
- [ ] ESLint clean
- [ ] Build succeeds
- [ ] 4 beads issues closed (k3k, a1v, cum, 9dj)
- [ ] Both PRs merged
