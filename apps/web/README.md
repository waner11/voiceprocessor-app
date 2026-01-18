# VoiceProcessor Web

Frontend application for the VoiceProcessor multi-provider Text-to-Speech platform.

## Overview

A modern web application that allows users to:

- Convert long-form text (books, articles, scripts) to natural-sounding audio
- Choose from multiple TTS providers (ElevenLabs, OpenAI, Google, Amazon Polly)
- Preview audio before committing credits
- Download generated audiobooks in various formats
- Track usage and manage subscriptions

## Tech Stack

- **Framework**: Next.js 14+ (App Router)
- **Language**: TypeScript
- **Styling**: Tailwind CSS
- **State Management**: Zustand or React Query
- **API Client**: Auto-generated from OpenAPI spec
- **Testing**: Vitest + Playwright

## Project Structure

```
voiceprocessor-web/
├── src/
│   ├── app/              # Next.js App Router pages
│   ├── components/       # Reusable UI components
│   ├── features/         # Feature-specific components and logic
│   ├── hooks/            # Custom React hooks
│   ├── lib/              # Utilities and API client
│   ├── stores/           # State management
│   └── types/            # TypeScript types
├── public/               # Static assets
├── tests/
│   ├── unit/
│   └── e2e/
├── package.json
└── next.config.js
```

## Prerequisites

- Node.js 20+
- pnpm (recommended) or npm

## Getting Started

### 1. Install dependencies

```bash
pnpm install
```

### 2. Set up environment

```bash
cp .env.example .env.local
# Edit .env.local with your API URL
```

### 3. Run development server

```bash
pnpm dev
```

### 4. Open the app

Navigate to http://localhost:3000

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `NEXT_PUBLIC_API_URL` | Backend API URL | `http://localhost:5000` |
| `NEXT_PUBLIC_APP_ENV` | Environment (development/production) | `development` |

## Scripts

| Command | Description |
|---------|-------------|
| `pnpm dev` | Start development server |
| `pnpm build` | Build for production |
| `pnpm start` | Start production server |
| `pnpm lint` | Run ESLint |
| `pnpm test` | Run unit tests |
| `pnpm test:e2e` | Run end-to-end tests |
| `pnpm generate:api` | Generate API client from OpenAPI spec |

## API Client Generation

The API client is auto-generated from the backend's OpenAPI specification:

```bash
pnpm generate:api
```

This ensures type safety and keeps the frontend in sync with the API.

## Related Repositories

- [voiceprocessor-api](https://github.com/waner11/voiceprocessor-api) - Backend API

## License

Proprietary - All rights reserved
