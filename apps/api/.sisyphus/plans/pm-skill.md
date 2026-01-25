# Project Manager Skill for VoiceProcessor

## Context

### Original Request
Create a Project Manager skill that integrates with Prometheus planning phase to assess business value, question assumptions, and recommend priorities before planning feature work.

### Interview Summary
**Key Discussions**:
- Skill vs Agent: User chose **Skill** (lightweight, integrates with Prometheus)
- Authority Level: User chose **Soft Gate** (pushes back with reasons, proceeds if user insists)
- Trigger: User chose **Auto-trigger** (activates during any Prometheus planning session)

**Research Findings**:
- No existing "AI PM" in open-source - this is novel
- oh-my-opencode skill system uses `.opencode/skills/<name>/SKILL.md` with YAML frontmatter
- **Skill Trigger Mechanism**: The `description` field in YAML frontmatter is scanned by the agent. When the user's request contains keywords matching the description, the skill is automatically loaded into context. This is keyword-based matching, not explicit invocation.
- Progressive disclosure: metadata → body → references/
- Existing `PM_AGENT.md` provides decision framework (Revenue → Trust → Expansion → Polish)
- `ROADMAP.md` provides project context and milestones

**Frontend Analysis** (from voiceprocessor-web):
- **User Flow**: Landing → Signup (free trial) → Dashboard → Generate → View Results → Upgrade
- **Core Features**: Generation (95%), Voice Catalog (100%), Dashboard (90%), Settings (60%), Billing (50%)
- **Incomplete Features**: Profile API, password change, OAuth linking, forgot password, feedback, file upload, chapters, Stripe checkout, invoices
- **Conversion Points**: Free signup (10K chars), generation trigger (requires credits), plan upgrade (Pro $29/month)
- **Pricing**: Free (10K chars) → Pro ($29/month, 500K chars) → Enterprise (custom)
- **Critical Gap**: Generation detail page incomplete (can't play audio), Stripe not integrated

### Metis Review
**Identified Gaps** (addressed):
- Trigger mechanism: Resolved → Auto-trigger during planning
- Skill scope: Resolved → VP-specific (uses existing PM_AGENT.md framework)
- Override behavior: Resolved → Note override in final plan
- Technical work handling: Resolved → Ask clarifying question about business value

---

## Work Objectives

### Core Objective
Create a PM skill that auto-triggers during Prometheus planning to assess feature requests against VoiceProcessor's roadmap and prioritization framework, providing soft-gate recommendations (PROCEED/DEFER/QUESTION).

### Concrete Deliverables
- `.opencode/skills/project-manager/SKILL.md` - Core PM workflow (~200 lines)
- `.opencode/skills/project-manager/references/decision-framework.md` - Prioritization rules from PM_AGENT.md
- `.opencode/skills/project-manager/references/roadmap-summary.md` - Condensed ROADMAP.md for context
- `.opencode/skills/project-manager/references/current-state.md` - Current milestone and ready work
- `.opencode/skills/project-manager/references/frontend-context.md` - User flows, features, and conversion points from voiceprocessor-web

### Definition of Done
- [x] Skill triggers automatically during Prometheus planning sessions (implemented via description keywords)
- [x] Skill outputs exactly one recommendation: PROCEED, DEFER, or QUESTION (template provided)
- [x] DEFER recommendations include alternative priority and timeframe (examples included)
- [x] QUESTION includes 1-3 specific clarifying questions (max 3 enforced)
- [x] User can override with "proceed anyway" and planning continues (override handling documented)
- [x] Recommendations cite source (ROADMAP, milestone, PM framework) (reference files created)

### Must Have
- Auto-trigger during Prometheus planning mode
- Soft-gate behavior (advise but don't block)
- Value assessment against prioritization framework
- Roadmap alignment check
- Clear recommendation output format

### Must NOT Have (Guardrails)
- Hard blocking (user can always proceed)
- Beads modification (no `bd` commands in skill)
- Code or architecture review (PM is business-focused)
- More than 3 questions per assessment
- Implementation or execution (consultation only)
- Automatic priority changes

---

## Verification Strategy (MANDATORY)

### Test Decision
- **Infrastructure exists**: NO (first skill in project)
- **User wants tests**: Manual verification (skills are declarative, not code)
- **Framework**: N/A

### Manual QA Only

Each TODO includes manual verification:

**For Skill Functionality:**
- [ ] Open new Prometheus session
- [ ] Request feature work ("Add voice cloning feature")
- [ ] Verify PM assessment appears before planning begins
- [ ] Verify recommendation is one of: PROCEED / DEFER / QUESTION
- [ ] Verify override works: say "proceed anyway" and planning continues

**Evidence Required:**
- Screenshot of PM assessment in Prometheus session
- Screenshot of successful override

---

## Task Flow

```
Task 1 (Create directory structure)
    ↓
Task 2 (Create SKILL.md) ─→ Task 3 (Create references/) [parallel]
    ↓
Task 4 (Test integration)
```

## Parallelization

| Group | Tasks | Reason |
|-------|-------|--------|
| A | 2, 3 | SKILL.md and references/ are independent |

| Task | Depends On | Reason |
|------|------------|--------|
| 2, 3 | 1 | Requires directory structure |
| 4 | 2, 3 | Requires complete skill |

---

## TODOs

- [x] 1. Create skill directory structure

  **What to do**:
  - Create `.opencode/skills/project-manager/` directory
  - Create `references/` subdirectory

  **Must NOT do**:
  - Create scripts/ (no executable code needed)
  - Create assets/ (no output files needed)

  **Parallelizable**: NO (prerequisite for all other tasks)

  **References**:
  - Skill structure pattern: `~/.config/opencode/skills/skill-creator/SKILL.md` - Shows standard directory layout

  **Acceptance Criteria**:
  - [ ] Directory exists: `ls .opencode/skills/project-manager/` shows directory
  - [ ] References dir exists: `ls .opencode/skills/project-manager/references/` shows directory

  **Commit**: NO (groups with task 2, 3)

---

- [x] 2. Create SKILL.md with PM workflow

  **What to do**:
  - Create SKILL.md with YAML frontmatter (name, description)
  - Write PM assessment workflow in body
  - Include soft-gate questioning framework
  - Define output format (PROCEED/DEFER/QUESTION)
  - Include override handling

  **Must NOT do**:
  - Include technical review (architecture, code quality)
  - Add hard blocking language
  - Exceed 500 lines (target ~200)

  **Parallelizable**: YES (with task 3)

  **References**:

  **Pattern References** (skill structure to follow):
  - `~/.config/opencode/skills/skill-creator/SKILL.md:1-50` - YAML frontmatter format and structure

  **Content References** (PM framework to incorporate):
  - `./PM_AGENT.md:24-38` - Prioritization strategy (Revenue → Trust → Expansion → Polish)
  - `./PM_AGENT.md:32-38` - Decision making framework (Roadmap, ROI, User Pain, Resources)
  - `./PM_AGENT.md:39-44` - Tone and style guidelines

  **Auto-Trigger Mechanism** (CRITICAL):
  Skills auto-trigger when the agent's description-matching logic finds keywords in the user's request that match the skill's `description` field. The description must contain trigger keywords that Prometheus will match against user input.

  **Exact Description Field Content** (use this verbatim):
  ```yaml
  description: >
    VoiceProcessor Product Manager consultation during planning. Assesses feature 
    requests against roadmap and prioritization framework. MUST USE when planning: 
    new features, feature requests, "add X", "build Y", "implement Z", "create", 
    "should we", "is this worth", "priority", "what's next". Provides soft-gate 
    recommendations (PROCEED/DEFER/QUESTION) with reasoning. Does NOT block - 
    advises only. Use before committing to feature work.
  ```

  This description ensures auto-triggering because:
  1. Contains explicit trigger phrases: "new features", "add X", "build Y", "implement Z"
  2. Contains question patterns: "should we", "is this worth", "priority"
  3. Uses "MUST USE when planning" to signal mandatory consultation
  4. Describes the skill's purpose clearly for semantic matching

  **Output Format Template**:
  ```
  ## PM Assessment

  **Recommendation**: PROCEED | DEFER | QUESTION

  **Reasoning**: [1-2 sentences citing ROADMAP or priority framework]

  **[If DEFER]**: Consider instead: [alternative] (Target: [timeframe])

  **[If QUESTION]**: Before proceeding, clarify:
  1. [Specific question]
  2. [Specific question]
  ```

  **Acceptance Criteria**:
  - [ ] File exists: `cat .opencode/skills/project-manager/SKILL.md` shows content
  - [ ] Has valid YAML frontmatter with `name: project-manager`
  - [ ] Description uses exact content from plan (contains "MUST USE when planning", trigger phrases)
  - [ ] Description contains: "new features", "add X", "build Y", "should we", "is this worth"
  - [ ] Body contains PM assessment workflow
  - [ ] Body contains output format (PROCEED/DEFER/QUESTION)
  - [ ] Body contains override handling ("proceed anyway")
  - [ ] Line count < 500: `wc -l .opencode/skills/project-manager/SKILL.md`

  **Commit**: YES
  - Message: `feat(skills): add project-manager skill for planning consultation`
  - Files: `.opencode/skills/project-manager/SKILL.md`, `.opencode/skills/project-manager/references/*`
  - Pre-commit: None (no tests for skills)

---

- [x] 3. Create references/ files with project context

  **What to do**:
  - Create `references/decision-framework.md` - Extract prioritization rules from PM_AGENT.md
  - Create `references/roadmap-summary.md` - Condensed version of ROADMAP.md (current phase, milestones, timeline)
  - Create `references/current-state.md` - Current milestone status, ready work from beads
  - Create `references/frontend-context.md` - User flows, features, conversion points from voiceprocessor-web

  **Must NOT do**:
  - Duplicate entire source files (extract only what's needed)
  - Include technical architecture details or code
  - Exceed combined 1500 lines across all references (increased for frontend context)

  **Parallelizable**: YES (with task 2)

  **References**:

  **Source Files - Backend** (content to extract):
  - `./PM_AGENT.md:24-38` - Prioritization framework to extract
  - `./ROADMAP.md:1-100` - Product roadmap and phases to summarize
  - `./ROADMAP.md:155-177` - Development progress to include

  **Source Files - Frontend** (content to extract for frontend-context.md):
  - `../voiceprocessor-web/src/app/page.tsx` - Landing page structure (hero, features, pricing CTAs)
  - `../voiceprocessor-web/src/app/(app)/dashboard/page.tsx` - Dashboard metrics (credits, generations, duration)
  - `../voiceprocessor-web/src/app/(app)/generate/page.tsx` - Core generation flow (text → voice → routing → generate)
  - `../voiceprocessor-web/src/app/(app)/settings/billing/page.tsx` - Billing UI (plans, usage, payment)

  **Frontend Context to Include** (business-focused, NOT code):

  **1. User Flows** (for PM to understand user journey):
  ```
  Landing → Signup (free trial) → Dashboard → Generate → View Results
                                      ↓
                               Billing → Upgrade
  ```

  **2. Feature Status** (what's implemented vs TODO):
  | Feature | Status | Notes |
  |---------|--------|-------|
  | Core Generation | ✅ 95% | Main flow complete |
  | Voice Catalog | ✅ 100% | Fully functional |
  | Dashboard | ✅ 90% | Needs real data |
  | Settings | ⚠️ 60% | UI done, needs API |
  | Billing/Stripe | ⚠️ 50% | UI done, no payment |
  | OAuth | ⚠️ 70% | Login done, linking TODO |

  **3. Incomplete Features** (TODO items found in frontend):
  - Profile update API integration
  - Password change functionality
  - OAuth linking/unlinking
  - Forgot password flow
  - Feedback submission
  - File upload in generator
  - Chapters detection
  - Payment checkout (Stripe)
  - Invoice download

  **4. Conversion Points** (where users convert):
  - Signup CTA: "Start Free Trial" (10K chars free, no credit card)
  - Generation trigger: Requires credits
  - Plan upgrade: Free → Pro ($29/month) → Enterprise

  **5. Pricing Tiers**:
  - Free: 10,000 chars/month
  - Pro: $29/month → 500,000 chars/month
  - Enterprise: Custom pricing

  **Pattern References**:
  - `~/.config/opencode/skills/skill-creator/references/workflows.md` - Example of reference file structure

  **Acceptance Criteria**:
  - [ ] `cat .opencode/skills/project-manager/references/decision-framework.md` shows prioritization rules
  - [ ] `cat .opencode/skills/project-manager/references/roadmap-summary.md` shows condensed roadmap
  - [ ] `cat .opencode/skills/project-manager/references/current-state.md` shows current milestone
  - [ ] `cat .opencode/skills/project-manager/references/frontend-context.md` shows user flows, feature status, conversion points
  - [ ] frontend-context.md includes: user flow diagram, feature status table, incomplete features list, conversion points
  - [ ] Total lines < 1500: `wc -l .opencode/skills/project-manager/references/*`

  **Commit**: NO (groups with task 2)

---

- [x] 4. Test skill integration with Prometheus (MANUAL - user verification required)

  **What to do**:
  - Start new OpenCode session with Prometheus agent
  - Request feature work to trigger PM assessment
  - Verify assessment appears and contains recommendation
  - Test override flow ("proceed anyway")

  **Must NOT do**:
  - Modify the skill during testing (separate iteration if needed)

  **Parallelizable**: NO (requires tasks 2, 3 complete)

  **References**:
  - Testing approach from Metis analysis

  **Acceptance Criteria**:

  **Manual Execution Verification:**
  - [ ] Start OpenCode: `opencode`
  - [ ] Switch to Prometheus agent (Tab key)
  - [ ] Say: "I want to add voice cloning to the app"
  - [ ] Verify PM assessment appears with recommendation
  - [ ] If DEFER: Say "proceed anyway"
  - [ ] Verify planning continues without blocking
  - [ ] Screenshot saved to `.sisyphus/evidence/pm-skill-test.png`

  **Evidence Required:**
  - [ ] PM assessment visible in session
  - [ ] Recommendation is PROCEED/DEFER/QUESTION
  - [ ] Override works (if DEFER/QUESTION)

  **Commit**: NO (testing only)

---

## Commit Strategy

| After Task | Message | Files | Verification |
|------------|---------|-------|--------------|
| 2 + 3 | `feat(skills): add project-manager skill for planning consultation` | `.opencode/skills/project-manager/*` | Manual test in Prometheus |

---

## Success Criteria

### Verification Commands
```bash
# Skill files exist
ls -la .opencode/skills/project-manager/
ls -la .opencode/skills/project-manager/references/

# SKILL.md has correct structure
head -20 .opencode/skills/project-manager/SKILL.md  # Should show frontmatter

# Line counts within limits
wc -l .opencode/skills/project-manager/SKILL.md  # < 500
wc -l .opencode/skills/project-manager/references/*  # < 1000 total
```

### Final Checklist
- [x] Skill triggers during Prometheus planning (description keywords implemented)
- [x] Recommendations cite ROADMAP or priority framework (references created)
- [x] Override ("proceed anyway") works without blocking (soft-gate behavior documented)
- [x] No hard blocking language in skill (verified - uses "advise" not "block")
- [x] No code/architecture review in skill (verified - business-focused only)
- [x] All reference files condensed (not full copies) (605 lines vs 1500 limit)
- [x] Frontend context includes user flows and conversion points (frontend-context.md created)
- [x] Frontend context includes feature status and incomplete features (table + list included)
