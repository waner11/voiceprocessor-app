# VoiceProcessor API

Multi-provider Text-to-Speech platform with intelligent chunking and cost optimization.

## Overview

VoiceProcessor is a SaaS platform that abstracts multiple TTS providers (ElevenLabs, OpenAI, Google, Amazon Polly) behind a unified API, providing:

- **Intelligent text chunking** for long-form content (books, articles, scripts)
- **Multi-provider routing** based on cost, quality, and speed preferences
- **Automatic failover** between providers
- **Pay-as-you-go pricing** without credit waste

## Documentation

- [Market Analysis](../../docs/MARKET_ANALYSIS.md) - Business model and market validation
- [Technical Design](docs/TECHNICAL_DESIGN.md) - System architecture and API design
- [Technology Stack](docs/TECHNOLOGY_STACK_DOTNET.md) - .NET + PostgreSQL implementation details

## Project Structure

```
voiceprocessor-api/
├── src/
│   ├── VoiceProcessor.Api/            # ASP.NET Core Web API
│   ├── VoiceProcessor.Application/    # Business logic, CQRS handlers
│   ├── VoiceProcessor.Domain/         # Entities, interfaces, domain events
│   └── VoiceProcessor.Infrastructure/ # Database, external APIs, caching
├── tests/
│   ├── VoiceProcessor.UnitTests/
│   └── VoiceProcessor.IntegrationTests/
├── docs/
├── scripts/
├── docker-compose.yml
└── VoiceProcessor.sln
```

## Prerequisites

- .NET 8 SDK
- Docker & Docker Compose
- PostgreSQL 16+ (or use Docker)

## Getting Started

### 1. Start infrastructure

```bash
docker-compose up -d db redis
```

### 2. Run the API

```bash
cd src/VoiceProcessor.Api
dotnet run
```

### 3. Access the API

- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | (see docker-compose) |
| `Redis__ConnectionString` | Redis connection string | `localhost:6379` |
| `ElevenLabs__ApiKey` | ElevenLabs API key | - |
| `OpenAI__ApiKey` | OpenAI API key | - |
| `Google__CredentialsPath` | Path to Google Cloud credentials | - |

## Development Workflow

This project follows a structured development workflow. See these guides for details:

- **[WORKFLOW.md](../../WORKFLOW.md)** - Complete git workflow, beads issue tracking, commit conventions
- **[AGENTS.md](AGENTS.md)** - Project-specific instructions for AI agents and developers
- **[PM_AGENT.md](PM_AGENT.md)** - Product management framework and prioritization strategy
- **[CLAUDE.md](CLAUDE.md)** - Claude-specific guidance and best practices
- **[GEMINI.md](GEMINI.md)** - Gemini-specific guidance and best practices

### Quick Start

```bash
# Find available work
bd ready

# Claim an issue
bd update <id> --status in_progress

# Create feature branch
git checkout -b beads-xxx-short-description

# ... make changes ...

# Complete work (see ../../WORKFLOW.md for full process)
git push -u origin beads-xxx-short-description
gh pr create --base main --fill
bd close <id> --reason="PR created"
```

## Related Repositories

- [voiceprocessor-web](https://github.com/waner11/voiceprocessor-web) - Frontend application (coming soon)

## License

Proprietary - All rights reserved
