# VoiceProcessor Unified Workflow Guide

**Version:** 1.0  
**Last Updated:** January 27, 2026  
**Applies To:** All VoiceProcessor repositories (API, Web, Mobile, etc.)

---

## Purpose

This document defines the **standard workflow** for all VoiceProcessor projects. It combines best practices from multiple repositories into a single source of truth.

**Key Principles:**
- 🎯 **Consistency** - Same workflow across all repos
- 🔄 **Git-native** - Issues tracked in git alongside code
- 📋 **Beads-first** - Issue tracking drives all work
- ✅ **Quality gates** - Code must pass checks before merge
- 📝 **Documentation** - Capture learnings for future sessions

---

## Table of Contents

1. [Quick Reference](#quick-reference)
2. [Issue Tracking (Beads)](#issue-tracking-beads)
3. [Starting Work (3-Phase Workflow)](#starting-work-3-phase-workflow)
4. [During Work](#during-work)
5. [Finishing Work ("Landing the Plane")](#finishing-work-landing-the-plane)
6. [Commit Conventions](#commit-conventions)
7. [Quality Gates](#quality-gates)
8. [Sisyphus Work Planning](#sisyphus-work-planning)
9. [Troubleshooting](#troubleshooting)

---

## Quick Reference

### Essential Commands

```bash
# Issue Tracking
bd ready                              # Find available work
bd show <id>                          # View issue details
bd update <id> --status in_progress   # Claim work
bd close <id>                         # Complete work
bd sync                               # Sync with git

# Git Workflow
git status                            # Check working directory
git checkout main                     # Switch to main
git pull --rebase origin main         # Get latest changes
git checkout -b beads-xxx-description # Create feature branch
git push -u origin beads-xxx-desc     # Push and set upstream
gh pr create --base main --fill       # Create pull request

# Quality Checks (adapt to your stack)
npm run build && npm test             # Frontend (Node.js)
dotnet build && dotnet test           # Backend (.NET)
```

---

## Issue Tracking (Beads)

All work items are tracked using **beads** (`bd` CLI). Issues are stored in `.beads/issues.jsonl` and synced with git.

### Finding Work

```bash
# Show all issues ready to work (no blockers)
bd ready

# List all open issues
bd list --status=open

# Show issues you're currently working on
bd list --status=in_progress

# View detailed issue information
bd show <id>
```

### Creating Issues

```bash
# Create a new issue
bd create --title="Fix authentication bug" --type=bug --priority=2

# Create with description
bd create --title="Add voice preview" --type=feature --priority=1 \
  --description="Users should be able to preview voices before selecting"
```

**Priority Levels:**
- `0` (P0) - Critical, blocking production
- `1` (P1) - High priority, blocks users
- `2` (P2) - Medium priority, normal work
- `3` (P3) - Low priority, nice to have
- `4` (P4) - Backlog, future consideration

### Managing Dependencies

```bash
# Issue A depends on Issue B (B blocks A)
bd dep add <issue-a> <issue-b>

# Show what's blocking an issue
bd show <id>

# List all blocked issues
bd blocked
```

### Closing Issues

```bash
# Close issue with default reason
bd close <id>

# Close with specific reason
bd close <id> --reason="PR merged"

# Close multiple issues at once
bd close <id1> <id2> <id3>
```

### Syncing with Git

```bash
# Sync beads changes to remote
bd sync

# Check sync status
bd sync --status
```

**Note:** Git hooks automatically sync beads on commit/merge, but run `bd sync` manually at session end to ensure everything is pushed.

---

## Starting Work (3-Phase Workflow)

### Phase 1: Preparation

**Before touching any code**, complete these steps:

#### 1. Check for Clean Working Directory

```bash
git status
```

**If you have uncommitted changes:**
- Commit them: `git add -A && git commit -m "your message"`
- Or stash them: `git stash`

#### 2. Switch to Main and Sync

```bash
git checkout main
git pull --rebase origin main
```

**Why rebase?** Keeps history linear and avoids merge commits on main.

#### 3. Find or Create Your Issue

```bash
# Option A: Find existing work
bd ready

# Option B: Create new issue
bd create --title="Your task" --type=task --priority=2
```

#### 4. Create Feature Branch

```bash
# Branch naming: beads-{issue-id}-{short-description}
git checkout -b beads-p71-add-credits-field
```

**Branch Naming Convention:**
- Start with `beads-`
- Include issue ID (e.g., `p71`, `abc`, `5uc`)
- Add short hyphen-separated description
- All lowercase, no special characters

**Examples:**
- ✅ `beads-p71-add-credits-field`
- ✅ `beads-9n7-stripe-integration`
- ✅ `beads-d3c-fix-auth-cookie`
- ❌ `feature/add-credits` (missing issue ID)
- ❌ `beads-p71_AddCredits` (underscores, PascalCase)

**Edge Case:** If branch already exists locally, delete it first:
```bash
git branch -D beads-p71-add-credits-field
git checkout -b beads-p71-add-credits-field
```

#### 5. Claim the Issue

```bash
bd update <id> --status in_progress
```

This marks the issue as yours and signals to others that you're working on it.

---

## During Work

### Commit Frequently

**Don't wait until everything is perfect.** Commit working increments:

```bash
git add -A
git commit -m "implement basic routing logic"

# Continue working...

git add -A
git commit -m "add error handling for edge cases"
```

**Benefits of frequent commits:**
- ✅ Easier to revert if you make a mistake
- ✅ Clear history of your thought process
- ✅ Safer against data loss
- ✅ Easier code review (smaller diffs)

### Follow Commit Conventions

**Rules:**
1. **Lowercase** - All commit messages start with lowercase
2. **Imperative mood** - "add feature" not "added feature"
3. **No period** - Don't end with punctuation
4. **No AI attribution** - Never include "Co-Authored-By" or "Generated by"
5. **Concise** - One line is usually enough

**Good Examples:**
```bash
implement routing engine logic
add elevenlabs provider implementation
fix null check in chunking service
update api client for new generation endpoint
handle empty text input gracefully
```

**Bad Examples:**
```bash
Added comprehensive error handling for the voice selector component
Fix: Implemented robust error handling (too verbose)
This commit addresses the issue where... (too long)
add feature (Co-Authored-By: Claude) (NO AI attribution!)
```

### Use Sisyphus for Complex Work

For work that spans multiple sessions or requires planning:

```bash
# Plans are in .sisyphus/plans/
# Create new plan or use existing one
# Notepads track learnings: .sisyphus/notepads/{plan-name}/
```

**Notepad Structure:**
- `learnings.md` - Patterns, conventions, successful approaches
- `decisions.md` - Architectural choices and rationales
- `issues.md` - Problems, blockers, gotchas
- `problems.md` - Unresolved issues, technical debt
- `HANDOFF.md` - What's next (for resuming)

**Always APPEND to notepads, never overwrite.**

---

## Finishing Work ("Landing the Plane")

When your work is complete, follow this **mandatory** sequence. **Work is not done until it's pushed and PR is created.**

### Step 1: Quality Checks

Run all quality gates for your project:

**Frontend (Node.js/TypeScript):**
```bash
npm run build    # Must succeed
npm run lint     # Fix all errors
npm test         # All tests must pass
```

**Backend (.NET):**
```bash
dotnet build     # Must succeed
dotnet test      # All tests must pass
```

**Other stacks:**
- Python: `pytest && mypy .`
- Go: `go build && go test ./...`
- Rust: `cargo build && cargo test`

**Fix all errors before proceeding.**

### Step 2: Cleanup

```bash
# Remove unused imports, temp comments, debug code
# Ensure code is production-ready
```

### Step 3: Commit Final Changes

```bash
git add -A
git commit -m "implement feature description"
```

### Step 4: Sync Main and Rebase

```bash
# Switch back to main
git checkout main

# Get latest changes
git pull --rebase origin main

# Switch back to your branch
git checkout beads-xxx-description

# Rebase onto main
git rebase main
```

**If conflicts occur during rebase:**

1. Git will pause and show conflicted files
2. Open each file and resolve conflicts manually
3. Stage resolved files: `git add <file>`
4. Continue rebase: `git rebase --continue`
5. Repeat until rebase completes

**If you need to abort:**
```bash
git rebase --abort
```

### Step 5: Push and Create PR

```bash
# Push to remote (with upstream tracking)
git push -u origin beads-xxx-description

# Create pull request
gh pr create --base main --fill
```

**Edge Case:** If PR already exists:
```bash
gh pr list  # Check if PR exists
```

### Step 6: Sync Beads and Close Issue

```bash
# Sync beads changes
bd sync

# Close the issue
bd close <id> --reason="PR created"
```

**Alternative:** Close issue after PR is merged (depends on team preference)

---

## Commit Conventions

### Message Format

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

**Types:**
- `feat` - New feature
- `fix` - Bug fix
- `docs` - Documentation only
- `style` - Code style (formatting, semicolons)
- `refactor` - Code restructuring (no behavior change)
- `perf` - Performance improvement
- `test` - Adding/updating tests
- `chore` - Maintenance (dependencies, configs)
- `security` - Security fix

**Scopes (optional but recommended):**
- `api` - API changes
- `auth` - Authentication/authorization
- `ui` - User interface
- `db` - Database
- `engine` - Business logic
- `manager` - Orchestration layer
- `accessor` - Data access layer

**Examples:**
```bash
feat(auth): add httponly cookie support
fix(api): expose CreditsRequired in cost estimate response
docs(readme): update setup instructions
refactor(engine): simplify routing logic
security(auth): prevent JWT theft via localStorage
```

### When to Use Scope

**Use scope when:**
- ✅ Change affects a specific module/area
- ✅ Helps clarify what part of the system changed
- ✅ Useful for filtering commit history

**Skip scope when:**
- ❌ Change is obvious from description
- ❌ Affects entire codebase
- ❌ Scope doesn't add value

---

## Quality Gates

Before merging, code must pass these gates:

### 1. Build Success

**All projects must compile/build:**
```bash
# Must exit with code 0
npm run build     # Frontend
dotnet build      # Backend
go build          # Go
cargo build       # Rust
```

### 2. Test Pass

**All tests must pass:**
```bash
npm test          # Frontend
dotnet test       # Backend
pytest            # Python
go test ./...     # Go
```

### 3. Linting

**Code must pass linter:**
```bash
npm run lint      # Frontend
dotnet format     # Backend (if configured)
```

### 4. Type Checking

**TypeScript/typed languages:**
```bash
tsc --noEmit      # TypeScript
mypy .            # Python
```

### 5. Manual Review

**Before creating PR, review:**
- ✅ All requirements from issue are met
- ✅ No debug code, console.logs, or temp comments
- ✅ Code follows project conventions
- ✅ Tests cover new functionality
- ✅ Documentation updated if needed

---

## Sisyphus Work Planning

For complex work spanning multiple sessions, use Sisyphus:

### When to Use Sisyphus

**Use for:**
- ✅ Work spanning multiple sessions
- ✅ Work with 5+ distinct tasks
- ✅ Work requiring planning/research
- ✅ Work with complex dependencies
- ✅ Work where you want to track learnings

**Don't use for:**
- ❌ Simple bug fixes (1-2 commits)
- ❌ Trivial changes (typos, formatting)
- ❌ Quick updates (< 30 minutes)

### Plan Structure

Plans are stored in `.sisyphus/plans/{plan-name}.md`:

```markdown
# Plan Title

## Context
Why this work exists, user request, background research

## Work Objectives
What we're delivering, core objective

## Concrete Deliverables
Specific files/features being created

## Definition of Done
Checkboxes of completion criteria

## TODOs
- [ ] 1. Task one
- [ ] 2. Task two
```

### Notepad Structure

Notepads are in `.sisyphus/notepads/{plan-name}/`:

- `README.md` - Navigation and quick reference
- `STATUS.md` - Current status summary
- `learnings.md` - Patterns, conventions, discoveries
- `decisions.md` - Architectural choices
- `issues.md` - Problems encountered
- `problems.md` - Unresolved blockers
- `HANDOFF.md` - How to resume work
- `COMPLETION_SUMMARY.md` - Final summary

**Always append to notepads:**
```bash
cat >> .sisyphus/notepads/{plan}/learnings.md << 'EOF'
## [2026-01-27] New Learning

Description of what was learned...
EOF
```

### Boulder Tracking

`.sisyphus/boulder.json` tracks active work:

```json
{
  "active_plan": "/path/to/plan.md",
  "started_at": "2026-01-27T02:10:15.603Z",
  "session_ids": ["ses_xyz"],
  "plan_name": "feature-name",
  "status": "in_progress",
  "completion_percentage": 60,
  "tasks_completed": 6,
  "tasks_total": 10,
  "blocker": "Waiting for API endpoint",
  "next_action": "CONTINUE_WORK"
}
```

---

## Troubleshooting

### "I have uncommitted changes and need to switch branches"

**Option 1: Commit them**
```bash
git add -A
git commit -m "wip: save progress"
```

**Option 2: Stash them**
```bash
git stash
# ... switch branches, do work ...
git stash pop  # Restore changes
```

### "My branch is behind main"

```bash
git checkout main
git pull --rebase origin main
git checkout your-branch
git rebase main
```

### "I have merge conflicts during rebase"

```bash
# Git pauses and shows conflicted files
# Open each file, resolve conflicts manually
git add <resolved-file>
git rebase --continue

# If you made a mistake, abort and start over
git rebase --abort
```

### "I accidentally committed to main"

```bash
# Create a branch from current main
git branch beads-xxx-rescue

# Reset main to origin/main
git checkout main
git reset --hard origin/main

# Switch to rescue branch and continue
git checkout beads-xxx-rescue
```

### "I forgot to create a branch"

```bash
# Create branch from current HEAD
git checkout -b beads-xxx-description

# Now you're on a branch with your commits
```

### "bd sync failed"

```bash
# Check sync status
bd sync --status

# Force flush to JSONL
bd list  # This triggers flush

# Try sync again
bd sync
```

### "Git hooks not working"

```bash
# Check if hooks exist
ls -la .git/hooks/

# Re-run bd init to reinstall hooks
bd init

# Or manually install from .beads/hooks/
```

---

## Best Practices

### Git

- ✅ **Always work on a feature branch** (never commit directly to main)
- ✅ **Pull before pushing** (avoid conflicts)
- ✅ **Rebase, don't merge** (keeps history clean)
- ✅ **Write clear commit messages** (future you will thank you)
- ✅ **Commit frequently** (small, focused commits)

### Beads

- ✅ **Create issues before coding** (tracks work, enables planning)
- ✅ **Update status as you work** (signals progress)
- ✅ **Close issues when done** (keeps backlog clean)
- ✅ **Use dependencies** (tracks blockers)
- ✅ **Sync regularly** (prevents drift)

### Code Quality

- ✅ **Write tests** (prevents regressions)
- ✅ **Follow conventions** (consistency matters)
- ✅ **Document complex logic** (comments explain "why")
- ✅ **Keep functions small** (easier to test and understand)
- ✅ **Handle errors gracefully** (don't let failures cascade)

### Communication

- ✅ **Document decisions** (in notepads or commits)
- ✅ **Ask questions early** (don't guess)
- ✅ **Share context** (link to related issues/PRs)
- ✅ **Update team** (on blockers or changes)

---

## Checklist: Before Creating PR

Use this checklist before every PR:

```
Quality Gates:
- [ ] Code builds successfully (npm run build / dotnet build)
- [ ] All tests pass (npm test / dotnet test)
- [ ] Linter passes (npm run lint / dotnet format)
- [ ] Type checker passes (tsc --noEmit)

Code Quality:
- [ ] No debug code, console.logs, or temp comments
- [ ] All requirements from issue are met
- [ ] Code follows project conventions
- [ ] Tests cover new functionality
- [ ] Documentation updated if needed

Git:
- [ ] Branch rebased on latest main
- [ ] Commit messages follow conventions
- [ ] No merge commits (rebased, not merged)
- [ ] Branch pushed to remote

Beads:
- [ ] Issue status updated
- [ ] Dependencies tracked
- [ ] Learnings documented (if complex work)
```

---

## Related Documentation

- `AGENTS.md` - Project-specific agent instructions
- `PM_AGENT.md` - Product management decision framework
- `.beads/README.md` - Beads issue tracking guide
- `.sisyphus/plans/` - Active work plans
- `ROADMAP.md` - Product roadmap and milestones

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-01-27 | Initial unified workflow combining best practices from API and Web repos |

---

**Questions or improvements?** Open an issue with `bd create` or update this document directly.
