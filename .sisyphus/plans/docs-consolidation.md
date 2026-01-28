# Documentation Consolidation

## TL;DR

> **Quick Summary**: Eliminate ~1,500 lines of duplicated documentation by consolidating 4 identical file sets to the monorepo root while preserving git history and updating 18 cross-references.
> 
> **Deliverables**:
> - 4 files moved to monorepo root (`WORKFLOW.md`, `docs/IDESIGN_ARCHITECTURE.md`, `docs/APPLICATION_FLOWS.md`, `docs/MARKET_ANALYSIS.md`)
> - 4 duplicate files deleted from `apps/web/`
> - 18 markdown references updated to point to new locations
> - Root `AGENTS.md` enhanced with documentation navigation section
> 
> **Estimated Effort**: Quick (< 1 hour)
> **Parallel Execution**: YES - 2 waves
> **Critical Path**: Task 1 → Task 2 → Task 3 → Task 4

---

## Context

### Original Request
User asked whether to consolidate AGENTS.md and other documentation files in the monorepo, or keep the current structure with separate files per app.

### Interview Summary
**Key Discussions**:
- Analyzed current state: Found 4 sets of EXACT DUPLICATE files (verified via MD5 hash)
- Researched industry patterns: Sentry, Tuist, promptfoo all use "root as router + nested for specifics"
- Decision: Consolidate duplicates to root, keep stack-specific files separate

**Research Findings**:
- Official agents.md spec: "Closest file wins" - nested structure is correct
- Pure duplication is bad; specialization is good
- Root AGENTS.md should be a router/navigation hub, not a monolithic file

### Metis Review
**Identified Gaps** (addressed):
- Reference updates: Enumerated all 18 cross-references that need updating
- Acceptance criteria: Added verification commands
- Risk mitigation: Using `git mv` to preserve history

---

## Work Objectives

### Core Objective
Eliminate documentation duplication in the monorepo by consolidating identical files to root level while maintaining the nested structure for stack-specific content.

### Concrete Deliverables
- `./WORKFLOW.md` (moved from apps/api/)
- `./docs/IDESIGN_ARCHITECTURE.md` (moved from apps/api/)
- `./docs/APPLICATION_FLOWS.md` (moved from apps/api/)
- `./docs/MARKET_ANALYSIS.md` (moved from apps/api/)
- Updated `./AGENTS.md` with documentation navigation
- 18 updated cross-references in app-specific files

### Definition of Done
- [x] No duplicate files exist in `apps/api/` or `apps/web/`
- [x] All 4 consolidated files exist at root level
- [x] Git history is preserved for moved files (`git log --follow` shows history)
- [x] All 18 cross-references point to correct new locations
- [x] `grep` verification finds no broken references

### Must Have
- Use `git mv` for all moves (preserves history)
- Update ALL cross-references (18 total)
- Root `./docs/` directory created if not exists

### Must NOT Have (Guardrails)
- DO NOT modify content of the documents (only location)
- DO NOT consolidate stack-specific files (AGENTS.md, CLAUDE.md, GEMINI.md, PM_AGENT.md)
- DO NOT create new documentation
- DO NOT change any code files
- DO NOT touch `apps/*/docs/MONETIZATION_PHASE_1.md` (they are different)

---

## Verification Strategy (MANDATORY)

### Test Decision
- **Infrastructure exists**: NO (documentation changes only)
- **User wants tests**: Manual-only
- **Framework**: N/A

### Manual QA Procedures

Each TODO includes verification commands to run after completion.

---

## Execution Strategy

### Parallel Execution Waves

```
Wave 1 (Start Immediately):
├── Task 1: Move files and delete duplicates (git operations)
└── [Sequential - must complete before Wave 2]

Wave 2 (After Wave 1):
├── Task 2: Update API references (14 changes in 5 files)
├── Task 3: Update Web references (3 files)
└── Task 4: Update root AGENTS.md

Wave 3 (After Wave 2):
└── Task 5: Final verification
```

### Dependency Matrix

| Task | Depends On | Blocks | Can Parallelize With |
|------|------------|--------|---------------------|
| 1 | None | 2, 3, 4 | None |
| 2 | 1 | 5 | 3, 4 |
| 3 | 1 | 5 | 2, 4 |
| 4 | 1 | 5 | 2, 3 |
| 5 | 2, 3, 4 | None | None (final) |

---

## TODOs

- [x] 1. Move files to root and delete duplicates

  **What to do**:
  - Create `./docs/` directory at root if not exists
  - Use `git mv` to move 4 files from `apps/api/` to root
  - Delete 4 duplicate files from `apps/web/`
  - Stage all changes

  **Must NOT do**:
  - Do not use regular `mv` (loses git history)
  - Do not modify file content during move

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: Simple git operations, no code changes
  - **Skills**: [`git-master`]
    - `git-master`: Git operations with history preservation

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Wave 1 (alone)
  - **Blocks**: Tasks 2, 3, 4
  - **Blocked By**: None

  **References**:
  - **Files to move** (from apps/api/ to root):
    - `apps/api/WORKFLOW.md` → `./WORKFLOW.md`
    - `apps/api/docs/IDESIGN_ARCHITECTURE.md` → `./docs/IDESIGN_ARCHITECTURE.md`
    - `apps/api/docs/APPLICATION_FLOWS.md` → `./docs/APPLICATION_FLOWS.md`
    - `apps/api/.md/MARKET_ANALYSIS.md` → `./docs/MARKET_ANALYSIS.md`
  
  - **Files to delete** (duplicates in apps/web/):
    - `apps/web/WORKFLOW.md`
    - `apps/web/docs/IDESIGN_ARCHITECTURE.md`
    - `apps/web/docs/APPLICATION_FLOWS.md`
    - `apps/web/.md/MARKET_ANALYSIS.md`

  **Acceptance Criteria**:

  **Manual Execution Verification:**
  - [ ] Run: `mkdir -p docs` → Creates root docs directory
  - [ ] Run: `git mv apps/api/WORKFLOW.md ./WORKFLOW.md`
  - [ ] Run: `git mv apps/api/docs/IDESIGN_ARCHITECTURE.md ./docs/IDESIGN_ARCHITECTURE.md`
  - [ ] Run: `git mv apps/api/docs/APPLICATION_FLOWS.md ./docs/APPLICATION_FLOWS.md`
  - [ ] Run: `git mv apps/api/.md/MARKET_ANALYSIS.md ./docs/MARKET_ANALYSIS.md`
  - [ ] Run: `rm apps/web/WORKFLOW.md apps/web/docs/IDESIGN_ARCHITECTURE.md apps/web/docs/APPLICATION_FLOWS.md apps/web/.md/MARKET_ANALYSIS.md`
  - [ ] Run: `git add -A`
  - [ ] Verify: `git status` shows 4 renamed files + 4 deleted files
  - [ ] Verify: `ls WORKFLOW.md docs/` → Files exist at root

  **Commit**: YES
  - Message: `chore: consolidate duplicate docs to monorepo root`
  - Files: `WORKFLOW.md`, `docs/*`, deleted files
  - Pre-commit: N/A

---

- [x] 2. Update API app references (14 changes in 5 files)

  **What to do**:
  - Update 8 cross-references in `apps/api/` files to point to new root locations
  - Use relative paths from each file's location

  **Must NOT do**:
  - Do not change any content other than the reference paths

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: Simple find-and-replace in markdown files
  - **Skills**: []
    - No special skills needed - basic file editing

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 2 (with Tasks 3, 4)
  - **Blocks**: Task 5
  - **Blocked By**: Task 1

  **References**:

  **Files to update with exact changes:**
  
  | File | Current Reference | New Reference |
  |------|-------------------|---------------|
  | `apps/api/README.md` | `docs/MARKET_ANALYSIS.md` | `../../docs/MARKET_ANALYSIS.md` |
  | `apps/api/README.md` | `WORKFLOW.md` | `../../WORKFLOW.md` |
  | `apps/api/README.md` | `WORKFLOW.md` | `../../WORKFLOW.md` |
  | `apps/api/GEMINI.md` | `docs/IDESIGN_ARCHITECTURE.md` | `../../docs/IDESIGN_ARCHITECTURE.md` |
  | `apps/api/GEMINI.md` | `docs/IDESIGN_ARCHITECTURE.md` | `../../docs/IDESIGN_ARCHITECTURE.md` |
  | `apps/api/GEMINI.md` | `docs/IDESIGN_ARCHITECTURE.md` | `../../docs/IDESIGN_ARCHITECTURE.md` |
  | `apps/api/GEMINI.md` | `docs/MARKET_ANALYSIS.md` | `../../docs/MARKET_ANALYSIS.md` |
  | `apps/api/CLAUDE.md` | `docs/IDESIGN_ARCHITECTURE.md` | `../../docs/IDESIGN_ARCHITECTURE.md` |
  | `apps/api/CLAUDE.md` | `docs/IDESIGN_ARCHITECTURE.md` | `../../docs/IDESIGN_ARCHITECTURE.md` |
  | `apps/api/CLAUDE.md` | `docs/MARKET_ANALYSIS.md` | `../../docs/MARKET_ANALYSIS.md` |
  | `apps/api/PM_AGENT.md` | `docs/MARKET_ANALYSIS.md` | `../../docs/MARKET_ANALYSIS.md` |
  | `apps/api/PR-Reviews/README.md` | `IDESIGN_ARCHITECTURE.md` | `../../../docs/IDESIGN_ARCHITECTURE.md` |
  | `apps/api/PR-Reviews/README.md` | `docs/IDESIGN_ARCHITECTURE.md` | `../../../docs/IDESIGN_ARCHITECTURE.md` |
  | `apps/api/PR-Reviews/README.md` | `WORKFLOW.md` | `../../../WORKFLOW.md` |

  **Acceptance Criteria**:

  **Manual Execution Verification:**
  - [ ] Edit each file above with the new reference paths
  - [ ] Verify: `grep -rn "docs/IDESIGN_ARCHITECTURE.md\|docs/MARKET_ANALYSIS.md" apps/api/ --include="*.md"` → Should show `../../docs/` paths
  - [ ] Verify: No references to old locations remain

  **Commit**: NO (groups with Task 4)

---

- [x] 3. Update Web app references (4 updates)

  **What to do**:
  - Update 4 cross-references in `apps/web/` files to point to new root locations
  - Use relative paths from each file's location

  **Must NOT do**:
  - Do not change any content other than the reference paths

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: Simple find-and-replace in markdown files
  - **Skills**: []
    - No special skills needed - basic file editing

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 2 (with Tasks 2, 4)
  - **Blocks**: Task 5
  - **Blocked By**: Task 1

  **References**:

  **Files to update with exact changes:**
  
  | File | Current Reference | New Reference |
  |------|-------------------|---------------|
  | `apps/web/AGENTS.md` | `WORKFLOW.md` | `../../WORKFLOW.md` |
  | `apps/web/PM_AGENT.md` | `WORKFLOW.md` | `../../WORKFLOW.md` |
  | `apps/web/README.md` | `WORKFLOW.md` | `../../WORKFLOW.md` |
  | `apps/web/README.md` | `WORKFLOW.md` | `../../WORKFLOW.md` |

  **Acceptance Criteria**:

  **Manual Execution Verification:**
  - [ ] Edit each file above with the new reference paths
  - [ ] Verify: `grep -rn "WORKFLOW.md" apps/web/ --include="*.md"` → Should show `../../WORKFLOW.md` paths
  - [ ] Verify: No references to old locations remain

  **Commit**: NO (groups with Task 4)

---

- [x] 4. Update root AGENTS.md with documentation navigation

  **What to do**:
  - Add a "Shared Documentation" section to root `AGENTS.md`
  - List the consolidated docs with descriptions
  - Improve navigation guidance

  **Must NOT do**:
  - Do not remove existing content
  - Do not duplicate content from consolidated docs

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: Simple markdown editing
  - **Skills**: []
    - No special skills needed

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 2 (with Tasks 2, 3)
  - **Blocks**: Task 5
  - **Blocked By**: Task 1

  **References**:

  - **File to update**: `./AGENTS.md`
  - **Current content**: `/home/wanerpena/Documents/Projects/voiceprocessor-app/AGENTS.md` (59 lines)
  
  **Add this section after "## For App-Specific Instructions":**
  ```markdown
  ## Shared Documentation
  
  These docs apply to the entire monorepo:
  
  - `WORKFLOW.md` - Git workflow, beads issue tracking, commit conventions
  - `docs/IDESIGN_ARCHITECTURE.md` - iDesign architecture patterns and layer rules
  - `docs/APPLICATION_FLOWS.md` - User journeys and application flows
  - `docs/MARKET_ANALYSIS.md` - Market validation and business analysis
  ```

  **Acceptance Criteria**:

  **Manual Execution Verification:**
  - [ ] Edit `./AGENTS.md` to add the documentation section
  - [ ] Verify: `cat AGENTS.md` shows the new section
  - [ ] Verify: Section appears after "For App-Specific Instructions"

  **Commit**: YES
  - Message: `chore: update references for consolidated docs`
  - Files: All files from Tasks 2, 3, 4
  - Pre-commit: N/A

---

- [x] 5. Final verification

  **What to do**:
  - Run comprehensive verification to ensure no broken references
  - Verify git history is preserved
  - Confirm no duplicates remain

  **Must NOT do**:
  - Do not make any changes in this task (verification only)

  **Recommended Agent Profile**:
  - **Category**: `quick`
    - Reason: Simple verification commands
  - **Skills**: [`git-master`]
    - `git-master`: Verify git history preservation

  **Parallelization**:
  - **Can Run In Parallel**: NO
  - **Parallel Group**: Wave 3 (alone - final)
  - **Blocks**: None
  - **Blocked By**: Tasks 2, 3, 4

  **References**:
  - All files modified in Tasks 1-4

  **Acceptance Criteria**:

  **Manual Execution Verification:**
  - [ ] Run: `ls WORKFLOW.md docs/IDESIGN_ARCHITECTURE.md docs/APPLICATION_FLOWS.md docs/MARKET_ANALYSIS.md`
        → All 4 files exist at root
  - [ ] Run: `ls apps/api/WORKFLOW.md apps/web/WORKFLOW.md 2>/dev/null`
        → "No such file or directory" (duplicates deleted)
  - [ ] Run: `git log --follow --oneline WORKFLOW.md | head -3`
        → Shows history (not just 1 "add" commit)
  - [ ] Run: `grep -rn "apps/api/WORKFLOW.md\|apps/web/WORKFLOW.md\|apps/api/docs/IDESIGN" . --include="*.md" 2>/dev/null | grep -v ".sisyphus"`
        → Empty (no old references)
  - [ ] Run: `git status`
        → Clean working tree (all committed)

  **Commit**: NO (verification only)

---

## Commit Strategy

| After Task | Message | Files | Verification |
|------------|---------|-------|--------------|
| 1 | `chore: consolidate duplicate docs to monorepo root` | Moved + deleted files | `git status` |
| 4 | `chore: update references for consolidated docs` | All reference updates + AGENTS.md | `grep` verification |

---

## Success Criteria

### Verification Commands
```bash
# 1. Consolidated files exist
ls WORKFLOW.md docs/IDESIGN_ARCHITECTURE.md docs/APPLICATION_FLOWS.md docs/MARKET_ANALYSIS.md

# 2. No duplicates remain
ls apps/*/WORKFLOW.md 2>&1 | grep -q "No such file" && echo "PASS" || echo "FAIL"

# 3. Git history preserved
git log --follow --oneline WORKFLOW.md | head -3

# 4. No broken references (should return empty)
grep -rn "apps/api/WORKFLOW\|apps/web/WORKFLOW\|apps/api/docs/IDESIGN\|apps/api/.md/MARKET" . --include="*.md" 2>/dev/null | grep -v ".sisyphus"
```

### Final Checklist
- [x] 4 files consolidated at root
- [x] 4 duplicates deleted from apps/web/
- [x] 18 cross-references updated
- [x] Root AGENTS.md has documentation navigation
- [x] Git history preserved for all moved files
- [x] All tests pass (N/A - no tests)
- [x] Clean git status (all committed)
