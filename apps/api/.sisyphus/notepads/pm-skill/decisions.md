# Decisions - PM Skill

This file tracks architectural and implementation decisions.

---

## [2026-01-24 23:18] Implementation Decisions

### Skill Structure
- **Location**: `.opencode/skills/project-manager/`
- **Format**: SKILL.md with YAML frontmatter + 4 reference files
- **Line counts**: SKILL.md (182), references (605 total) - well under limits

### Auto-Trigger Mechanism
- **Method**: Keyword matching in description field
- **Keywords**: "new features", "add X", "build Y", "should we", "is this worth", "priority", "what's next"
- **Trigger phrase**: "MUST USE when planning" signals mandatory consultation

### Prioritization Framework
- **P0 (Revenue First)**: Stripe, payments, credits - blocks monetization
- **P1 (User Trust)**: Real-time updates, monitoring, error handling
- **P2 (Expansion)**: New providers, features (only if distinct advantage)
- **P3 (Polish)**: UI tweaks, animations (unless blocking core journey)

### Soft-Gate Behavior
- **Recommendation types**: PROCEED / DEFER / QUESTION
- **Override handling**: "proceed anyway" → acknowledge and continue
- **No hard blocking**: User always has final decision

### Reference Files Strategy
- **decision-framework.md**: Prioritization rules from PM_AGENT.md
- **roadmap-summary.md**: Condensed ROADMAP.md (phases, milestones, progress)
- **current-state.md**: Live status from `bd ready` (10 ready tasks, Stripe blocker)
- **frontend-context.md**: User flows, features, conversion points from voiceprocessor-web

### Subagent Delegation Issue
- **Problem**: Task 2 subagent refused due to "SINGLE TASK ONLY" directive
- **Root cause**: Directive conflict - creating one file IS a single task
- **Resolution**: Orchestrator created SKILL.md directly to unblock
- **Lesson**: Directive may need refinement for skill creation tasks
