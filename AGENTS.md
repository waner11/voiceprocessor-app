# VoiceProcessor Monorepo - Agent Instructions

This monorepo contains both the API and Web applications.

## Project Structure

- `apps/api/` - .NET API (see `apps/api/AGENTS.md`)
- `apps/web/` - Next.js Frontend (see `apps/web/AGENTS.md`)
- `.beads/` - Unified issue tracking
- `.sisyphus/` - Unified work planning
- `.opencode/` - Shared skills and tools

## Issue Prefixes

- `voiceprocessor-api-xxx` - Backend issues
- `voiceprocessor-web-xxx` - Frontend issues

## For App-Specific Instructions

- **API Development**: See `apps/api/AGENTS.md`
- **Web Development**: See `apps/web/AGENTS.md`

## Shared Documentation

These docs apply to the entire monorepo:

- `WORKFLOW.md` - Git workflow, beads issue tracking, commit conventions
- `docs/IDESIGN_ARCHITECTURE.md` - iDesign architecture patterns and layer rules
- `docs/APPLICATION_FLOWS.md` - User journeys and application flows
- `docs/MARKET_ANALYSIS.md` - Market validation and business analysis

## Beads Workflow

Use beads for all task tracking:
- `bd ready` - Find available work
- `bd create "[api] title"` or `bd create "[web] title"` - Create issues with project prefix in title
- `bd close <id>` - Complete work
- `bd sync` - Sync with git

## Working in the Monorepo

### API Work
```bash
cd apps/api
dotnet run --project src/VoiceProcessor.Clients.Api
```

### Web Work
```bash
cd apps/web
pnpm install
pnpm dev
```

### Running Both
Terminal 1: `cd apps/api && dotnet run`
Terminal 2: `cd apps/web && pnpm dev`

## Code Search Preference

This project has GrepAI indexed with semantic embeddings (Ollama/nomic-embed-text).
When searching code, prefer GrepAI MCP tools over Grep:

- **Primary** — `grepai_search`: Semantic/intent-based code discovery. Finds code by meaning
  (e.g., "authentication flow", "error handling middleware"), not just text patterns.
- **Relationships** — `grepai_trace_callers` / `grepai_trace_callees` / `grepai_trace_graph`:
  Understand function call relationships, dependencies, and impact analysis.
- **Fallback only** — `Grep`: Use for exact text pattern matching (variable names, imports, regex).

For exploration tasks, prefer the `deep-explore` agent (`subagent_type="deep-explore"`)
which is configured for grepai-first search with Grep as fallback.

## Development Methodology

This project follows **Test-Driven Development (TDD)**. When implementing features or fixing bugs,
use the `tdd` skill (`load_skills=["tdd"]`) and follow the Red→Green→Refactor cycle:
write a failing test first, write minimal code to pass, then refactor.

## Git Workflow

When working on features that touch both API and Web:
1. Create single branch: `git checkout -b beads-xxx-feature-name`
2. Make changes in both `apps/api/` and `apps/web/`
3. Commit atomic changes: `git commit -m "feat: description"`
4. Create single PR with both changes

This is the power of the monorepo - atomic cross-stack commits!
