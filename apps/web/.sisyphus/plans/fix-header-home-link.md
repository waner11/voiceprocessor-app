# Fix Header Logo Link for Authenticated Users

## Context

### Original Request
When logged in, clicking "VoiceProcessor" header logo navigates to the public landing page, making it appear like user got logged out. Expected behavior: should navigate to Dashboard.

### Root Cause
- `Header.tsx` line 65 has `href="/"` which goes to the public landing page
- The Header component is only used in authenticated routes (`(app)` layout)
- The landing page has its own header without authenticated UI
- MobileNav already correctly links "Home" to `/dashboard`

---

## Work Objectives

### Core Objective
Make the Header logo link to `/dashboard` instead of `/` for authenticated users.

### Concrete Deliverables
- Updated `src/components/layout/Header.tsx` with correct logo link

### Definition of Done
- [x] Clicking "VoiceProcessor" logo redirects to `/dashboard`
- [x] User stays in the authenticated app experience

### Must NOT Have (Guardrails)
- Do NOT modify the landing page header (that should still link to `/`)
- Do NOT change MobileNav (already correct)

---

## TODOs

- [x] 1. Change Header logo link to /dashboard

  **What to do**:
  - Change line 65 from `href="/"` to `href="/dashboard"`

  **File**: `src/components/layout/Header.tsx`

  **Exact change**:
  ```diff
  - <Link href="/" className="text-xl font-bold text-gray-900 dark:text-white">
  + <Link href="/dashboard" className="text-xl font-bold text-gray-900 dark:text-white">
  ```

  **Parallelizable**: NO (single file change)

  **Acceptance Criteria**:
  - [x] While logged in, click "VoiceProcessor" logo
  - [x] Should navigate to `/dashboard` (not `/`)
  - [x] Should stay in authenticated app (profile dropdown visible)

  **Commit**: YES
  - Message: `fix(header): link logo to dashboard for authenticated users`
  - Files: `src/components/layout/Header.tsx`

---

## Success Criteria

### Verification Steps
1. Log in to the app
2. Navigate to any page (e.g., `/generate`)
3. Click the "VoiceProcessor" logo in header
4. Verify you land on `/dashboard` with authenticated UI intact

### Final Checklist
- [x] Logo links to `/dashboard`
- [x] User stays authenticated
- [x] MobileNav "Home" still works correctly
