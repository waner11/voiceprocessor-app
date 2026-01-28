# VoiceProcessor

Multi-provider Text-to-Speech platform.

## Structure

```
voiceprocessor/
├── apps/
│   ├── api/    # .NET API - TTS orchestration
│   └── web/    # Next.js - User interface
├── .beads/     # Issue tracking
├── .sisyphus/  # Work planning
└── .opencode/  # Shared skills
```

## Quick Start

### API
```bash
cd apps/api
dotnet run --project src/VoiceProcessor.Clients.Api
```

### Web
```bash
cd apps/web
pnpm install
pnpm dev
```

## Documentation

- [API Documentation](apps/api/README.md)
- [Web Documentation](apps/web/README.md)
- [Agent Instructions](AGENTS.md)

## Development Workflow

This project uses:
- **Beads** for issue tracking (`bd ready`, `bd create`, `bd close`)
- **Sisyphus** for work planning
- **Git** for version control

See [AGENTS.md](AGENTS.md) for detailed workflow instructions.

## Monorepo Benefits

- Unified issue tracking across API and Web
- Atomic commits for full-stack features
- Shared types and utilities
- Single source of truth
- AI agents with full system context
