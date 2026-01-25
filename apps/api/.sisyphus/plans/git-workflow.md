# Formalize Complete Git Workflow in AGENTS.md

## Context

### Original Request
Add a complete git workflow to AGENTS.md that includes:
1. Switch to main if not on main
2. Pull from origin to stay up to date
3. Create new branch with issue ID
4. When work is done, rebase and create PR to main automatically

### Interview Summary
**Key Discussions**:
- Current workflow is split between CLAUDE.md (branching) and AGENTS.md (finishing)
- User chose to consolidate into AGENTS.md only as single source of truth
- Added enhancement: rebase before PR to keep history clean and catch conflicts locally
- User confirmed `gh` CLI is available for PR creation

**Research Findings**:
- AGENTS.md Section 4 currently only covers "Landing the Plane" (finish workflow)
- CLAUDE.md lines 100-123 has branching but misses "ensure on main" and "pull first"
- Branch naming convention exists: `beads-xxx-short-description`
- Commit style defined: lowercase, imperative mood, no period, no AI attribution

### Metis Review
**Identified Gaps** (addressed):
- Missing: Check for uncommitted changes before starting
- Missing: Detailed conflict resolution steps during rebase
- Missing: Edge case handling (branch already exists, PR already exists)
- Missing: Integration with beads status update (`bd update --status in_progress`)

---

## Work Objectives

### Core Objective
Restructure AGENTS.md Section 4 into a complete three-phase workflow (Starting → During → Finishing) and simplify CLAUDE.md to reference it.

### Concrete Deliverables
- AGENTS.md Section 4 with "Starting Work", "During Work", "Finishing Work" subsections
- CLAUDE.md lines 100-123 simplified to reference AGENTS.md
- All commands with examples

### Definition of Done
- [x] AGENTS.md Section 4 has all three workflow phases
- [x] `bd update --status in_progress` integrated after branch creation
- [x] Rebase step with conflict resolution guidance included
- [x] `gh pr create` command documented
- [x] CLAUDE.md branching section references AGENTS.md
- [x] No existing content from other sections modified

### Must Have
- Clean working directory check with warning
- Switch to main + pull from origin
- Branch creation with beads ID format
- Beads status integration (`bd update`, `bd close`)
- Rebase onto main before PR
- Conflict resolution steps
- PR creation via `gh pr create`

### Must NOT Have (Guardrails)
- Do NOT create shell scripts or aliases
- Do NOT add CI/CD workflows or git hooks
- Do NOT modify AGENTS.md Sections 1-3, 5-6
- Do NOT remove any existing content (expand, don't replace)
- Do NOT add web UI fallback for PR creation

---

## Verification Strategy (MANDATORY)

### Test Decision
- **Infrastructure exists**: N/A (documentation)
- **User wants tests**: N/A
- **Framework**: N/A

### Manual QA Verification
Review the documentation for completeness and correctness.

---

## Task Flow

```
Task 1 (AGENTS.md Section 4) → Task 2 (CLAUDE.md simplify) → Task 3 (Verify)
```

## Parallelization

| Task | Depends On | Reason |
|------|------------|--------|
| 1 | None | Primary content creation |
| 2 | 1 | Must reference completed Section 4 |
| 3 | 2 | Must verify both files |

---

## TODOs

- [x] 1. Restructure AGENTS.md Section 4 into complete git workflow

  **What to do**:
  Rewrite Section 4 "Work Session Workflow" to include three phases:

  **4.1 Starting Work**
  ```bash
  # 1. Check for clean working directory
  git status  # Warn if uncommitted changes exist
  
  # 2. Switch to main and sync
  git checkout main
  git pull --rebase origin main
  
  # 3. Create feature branch from issue ID
  git checkout -b beads-xxx-short-description
  # Example: git checkout -b beads-p71-add-credits-field
  
  # 4. Claim the issue
  bd update <id> --status in_progress
  ```

  **4.2 During Work**
  - Commit frequently with meaningful messages
  - Follow commit conventions (lowercase, imperative, no period)
  - Reference existing guidance

  **4.3 Finishing Work** (expand existing "Landing the Plane")
  ```bash
  # 1. Quality check
  dotnet build && dotnet test
  
  # 2. Cleanup
  # Remove unused usings, temp comments
  
  # 3. Commit final changes
  git add -A
  git commit -m "implement feature description"
  
  # 4. Sync main and rebase
  git checkout main
  git pull --rebase origin main
  git checkout beads-xxx-short-description
  git rebase main
  # If conflicts: resolve, git add, git rebase --continue
  
  # 5. Push and create PR
  git push -u origin beads-xxx-short-description
  gh pr create --base main --fill
  
  # 6. Sync beads and close
  bd sync
  bd close <id> --reason="PR created"
  ```

  Include edge case notes:
  - If branch already exists: `git branch -D beads-xxx` to delete old
  - If PR already exists: check with `gh pr list`
  - If rebase has conflicts: step-by-step resolution guide

  **Must NOT do**:
  - Do not modify sections 1-3, 5-6
  - Do not remove existing commit message guidance
  - Do not add scripts or automation

  **Parallelizable**: NO (first task)

  **References**:

  **Pattern References**:
  - `AGENTS.md:119-139` - Current "Work Session Workflow" section to expand
  - `CLAUDE.md:100-123` - Existing branching workflow pattern to incorporate
  - `CLAUDE.md:78-98` - Commit message conventions to preserve

  **Documentation References**:
  - `AGENTS.md:8-16` - Beads commands reference (bd ready, bd update, bd close)

  **Acceptance Criteria**:

  **Structure Verification**:
  - [x] Section 4 has "4.1 Starting Work" subsection
  - [x] Section 4 has "4.2 During Work" subsection
  - [x] Section 4 has "4.3 Finishing Work" subsection
  - [x] Each subsection has numbered steps with commands
  - [x] Edge cases documented (branch exists, conflicts)

  **Content Verification**:
  - [x] `git status` check at start
  - [x] `git checkout main && git pull --rebase` present
  - [x] `git checkout -b beads-xxx-...` present
  - [x] `bd update --status in_progress` present
  - [x] `git rebase main` before push
  - [x] Conflict resolution steps included
  - [x] `gh pr create --base main --fill` present
  - [x] `bd close` at end

  **Commit**: YES
  - Message: `docs: add complete git workflow to AGENTS.md`
  - Files: `AGENTS.md`

---

- [x] 2. Simplify CLAUDE.md branching section to reference AGENTS.md

  **What to do**:
  Replace CLAUDE.md lines 100-123 (Branching Workflow section) with a brief reference:

  ```markdown
  ### Branching Workflow

  See **AGENTS.md Section 4** for the complete git workflow including:
  - Starting work (sync main, create branch, claim issue)
  - During work (commit conventions)
  - Finishing work (rebase, PR creation, close issue)
  ```

  Keep Git Guidelines section (lines 78-99) unchanged - those commit message conventions are still valid and useful here.

  **Must NOT do**:
  - Do not modify Git Guidelines section (lines 78-99)
  - Do not remove useful context, just the duplicated workflow

  **Parallelizable**: NO (depends on Task 1)

  **References**:

  **Pattern References**:
  - `CLAUDE.md:100-123` - Current branching workflow to simplify

  **Acceptance Criteria**:

  **Content Verification**:
  - [x] Lines 78-99 (Git Guidelines) unchanged
  - [x] Lines 100-123 replaced with reference to AGENTS.md Section 4
  - [x] Reference mentions the three phases (Starting, During, Finishing)

  **Commit**: YES
  - Message: `docs: simplify CLAUDE.md to reference AGENTS.md workflow`
  - Files: `CLAUDE.md`

---

- [x] 3. Verify documentation completeness

  **What to do**:
  - Read through AGENTS.md Section 4 end-to-end
  - Verify all commands are correct and executable
  - Check cross-references are accurate

  **Must NOT do**:
  - Do not modify any code

  **Parallelizable**: NO (depends on Task 2)

  **References**:

  **Documentation References**:
  - `AGENTS.md` - Full file to review
  - `CLAUDE.md` - Verify reference is accurate

  **Acceptance Criteria**:

  **Manual Verification**:
  - [x] All bash commands have correct syntax
  - [x] Branch naming example matches convention: `beads-xxx-short-description`
  - [x] CLAUDE.md reference points to correct section
  - [x] No broken cross-references
  - [x] Flow makes logical sense: start → work → finish

  **Commit**: NO (verification only)

---

## Commit Strategy

| After Task | Message | Files | Verification |
|------------|---------|-------|--------------|
| 1 | `docs: add complete git workflow to AGENTS.md` | AGENTS.md | Read through section |
| 2 | `docs: simplify CLAUDE.md to reference AGENTS.md workflow` | CLAUDE.md | Verify reference |

---

## Success Criteria

### Verification Commands
```bash
# View updated AGENTS.md Section 4
grep -A 100 "Work Session Workflow" AGENTS.md | head -120

# View simplified CLAUDE.md
grep -A 10 "Branching Workflow" CLAUDE.md
```

### Final Checklist
- [x] AGENTS.md Section 4 has three phases (Starting, During, Finishing)
- [x] Git workflow integrates with beads (`bd update`, `bd close`)
- [x] Rebase step documented with conflict resolution
- [x] PR creation via `gh pr create` documented
- [x] CLAUDE.md references AGENTS.md (no duplication)
- [x] No existing sections 1-3, 5-6 modified

---

## Scope Boundaries

### IN Scope
- AGENTS.md Section 4 restructure
- CLAUDE.md lines 100-123 simplification

### OUT of Scope
- Shell scripts or aliases
- CI/CD workflows (.github/workflows)
- Git hooks
- Branch protection rules
- Web UI fallback for PR creation
