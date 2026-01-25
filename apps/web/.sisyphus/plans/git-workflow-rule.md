# Add Git Workflow Rule to AGENTS.md

## Context

### Original Request
Add a mandatory workflow rule requiring agents to: switch to main, pull latest, and create a new branch named after the issue ID before starting any work.

---

## Work Objectives

### Core Objective
Add "Starting Work on an Issue" section to AGENTS.md to enforce consistent git branching workflow.

### Concrete Deliverables
- Updated `AGENTS.md` with new section

### Definition of Done
- [ ] New section exists between "Quick Reference" and "Landing the Plane"
- [ ] Git commands are correct and copy-pasteable

---

## TODOs

- [ ] 1. Add "Starting Work on an Issue" section to AGENTS.md

  **What to do**:
  Insert the following after line 13 (after the Quick Reference code block closing), before "## Landing the Plane":

  ```markdown
  ## Starting Work on an Issue

  **MANDATORY WORKFLOW** - Before working on ANY issue:

  ```bash
  git checkout main              # Switch to main branch
  git pull --rebase              # Get latest changes
  git checkout -b {issue-id}     # Create branch from issue ID
  bd update {issue-id} --status in_progress  # Claim the work
  ```

  **NEVER:**
  - Work directly on `main` branch
  - Create a branch without pulling latest first
  - Skip the branch creation step
  ```

  **References**:
  - `AGENTS.md:1-14` - Current Quick Reference section (insert after this)
  - `AGENTS.md:15-41` - Landing the Plane section (insert before this)

  **Acceptance Criteria**:
  - [ ] Section appears between Quick Reference and Landing the Plane
  - [ ] Code block renders correctly in markdown

  **Commit**: YES
  - Message: `docs(agents): add mandatory git workflow for starting issue work`
  - Files: `AGENTS.md`

---

## Success Criteria

### Final Checklist
- [ ] AGENTS.md contains "Starting Work on an Issue" section
- [ ] Commands are correct: checkout main → pull --rebase → checkout -b {issue-id} → bd update
