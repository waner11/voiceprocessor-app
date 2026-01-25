# VoiceProcessor: Frontend Design Document

**Date:** January 2026
**Repository:** voiceprocessor-web

---

## Table of Contents

1. [Technology Stack](#1-technology-stack)
2. [User Experience Flows](#2-user-experience-flows)
3. [UI Components](#3-ui-components)
4. [State Management](#4-state-management)
5. [API Integration](#5-api-integration)
6. [Real-time Updates](#6-real-time-updates)

---

## 1. Technology Stack

### 1.1 Option A: Next.js (Recommended)

Best ecosystem, industry standard for React applications.

```
NEXT.JS STACK
│
├── Framework: Next.js 14+ (App Router)
├── Language: TypeScript
├── Styling: Tailwind CSS + shadcn/ui
├── State: TanStack Query (server state) + Zustand (client state)
├── Forms: React Hook Form + Zod
├── Audio: wavesurfer.js (waveform visualization)
└── Real-time: SignalR client (@microsoft/signalr)
```

### 1.2 Option B: Blazor (Stay in C#)

If you prefer staying in C# for the frontend:

```
BLAZOR STACK
│
├── Framework: Blazor WebAssembly (runs in browser)
│             OR Blazor Server (runs on server, SignalR connection)
├── Language: C#
├── UI: MudBlazor or Radzen (component libraries)
├── State: Fluxor (Redux-like) or built-in cascading parameters
└── Audio: JSInterop with Howler.js
```

**Blazor Pros:**
- No JavaScript/TypeScript to learn
- Share code between frontend/backend
- Strong typing throughout

**Blazor Cons:**
- Smaller ecosystem than React
- Larger download size (WebAssembly)
- Less community resources/tutorials

### 1.3 Recommendation

```
IF you want the best long-term ecosystem → Next.js
IF you want to ship fastest with your skills → Blazor WebAssembly
IF you have limited bandwidth → Blazor Server
```

For a commercial product, **Next.js** is recommended despite the learning curve — the ecosystem for UI components, animations, and third-party integrations is much richer.

---

## 2. User Experience Flows

### 2.1 New User Flow (Audiobook Creator)

```
ONBOARDING FLOW
│
├── 1. SIGN UP
│   └── Email + password OR Google/GitHub OAuth
│
├── 2. FREE CREDITS
│   └── Grant 10,000 characters free trial
│
├── 3. FIRST GENERATION
│   ├── Paste/upload text
│   ├── Auto-detect language
│   ├── Recommend voice
│   ├── Show cost estimate
│   └── Generate preview (first 500 chars free)
│
├── 4. LISTEN TO PREVIEW
│   ├── Like it? → Full generation
│   └── Don't like? → Try different voice (free preview)
│
├── 5. DOWNLOAD
│   └── MP3/WAV with chapter markers
│
└── 6. FEEDBACK
    └── Rate generation (improves recommendations)
```

### 2.2 Power User Flow (Developer API)

```
API INTEGRATION FLOW
│
├── 1. API KEY
│   └── Generate in dashboard
│
├── 2. ESTIMATE
│   └── POST /v1/generate/estimate
│
├── 3. GENERATE
│   └── POST /v1/generate (async)
│
├── 4. POLL OR WEBHOOK
│   ├── Poll: GET /v1/generate/{job_id}
│   └── Webhook: Receive completion event
│
└── 5. RETRIEVE
    └── Download from audio_url
```

### 2.3 UI Mockup Concept

```
┌─────────────────────────────────────────────────────────────────────┐
│  VOICEPROCESSOR                           Credits: 455,000 │ ⚙️ │   │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  📝 Paste your text or upload a file                        │   │
│  │  ─────────────────────────────────────────────────────────  │   │
│  │  Wilfredo "Bazooka" Gómez Probablemente el mejor boxeador   │   │
│  │  que jamás haya salido de Puerto Rico, Gómez arrasó en la   │   │
│  │  división de 122 libras...                                  │   │
│  │                                                             │   │
│  │                                                             │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  Detected: Spanish │ 45,000 chars │ Fiction/Narrative              │
│                                                                     │
│  ┌──────────────────────────┐  ┌──────────────────────────┐        │
│  │ 🎙️ Voice                 │  │ ⚡ Routing                │        │
│  │ ────────────────────────│  │ ────────────────────────│        │
│  │ ○ Carlos - Narrator     │  │ ● Balanced (Recommended) │        │
│  │ ● Diego - Dramatic  ▶️   │  │ ○ Best Quality           │        │
│  │ ○ Sofia - Warm          │  │ ○ Lowest Cost            │        │
│  └──────────────────────────┘  └──────────────────────────┘        │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │ 💰 COST ESTIMATE                                            │   │
│  │                                                             │   │
│  │   Provider: OpenAI TTS                                     │   │
│  │   Cost: $0.68                                              │   │
│  │   Duration: ~45 minutes                                    │   │
│  │   Generation time: ~5 minutes                              │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  ┌─────────────────────┐  ┌─────────────────────────────────────┐  │
│  │ ▶️ Preview (Free)    │  │ 🎧 Generate Full Audio              │  │
│  └─────────────────────┘  └─────────────────────────────────────┘  │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 3. UI Components

### 3.1 Key Pages

```
PAGES
│
├── / (Landing)
│   ├── Hero section with demo
│   ├── Pricing tiers
│   └── Feature highlights
│
├── /dashboard
│   ├── Usage summary
│   ├── Recent generations
│   └── Quick generate form
│
├── /generate
│   ├── Text input (paste or upload)
│   ├── Voice selector with previews
│   ├── Routing options
│   ├── Cost estimate
│   └── Generate button
│
├── /generations
│   ├── List of all generations
│   ├── Status indicators
│   └── Download/play buttons
│
├── /generations/[id]
│   ├── Audio player with waveform
│   ├── Chapter navigation
│   ├── Feedback form
│   └── Re-generate options
│
├── /voices
│   ├── Voice catalog with samples
│   ├── Filter by language/style
│   └── Favorites
│
├── /settings
│   ├── Account settings
│   ├── API keys management
│   ├── Billing/subscription
│   └── Webhooks configuration
│
└── /api-docs
    └── Interactive API documentation
```

### 3.2 Core Components

```
COMPONENTS
│
├── TextInput/
│   ├── TextArea with character count
│   ├── File upload (txt, docx, pdf)
│   ├── Language auto-detection badge
│   └── Content type indicator
│
├── VoiceSelector/
│   ├── Voice cards with sample button
│   ├── Filter sidebar
│   ├── Provider badges
│   └── Quality indicators
│
├── CostEstimate/
│   ├── Provider options comparison
│   ├── Quality vs cost visualization
│   └── Credits remaining
│
├── AudioPlayer/
│   ├── Waveform visualization (wavesurfer.js)
│   ├── Playback controls
│   ├── Speed control
│   ├── Chapter markers
│   └── Download button
│
├── GenerationStatus/
│   ├── Progress bar
│   ├── Current step indicator
│   ├── Estimated time remaining
│   └── Real-time updates (SignalR)
│
└── FeedbackForm/
    ├── Star rating
    ├── Quick tags (too fast, pronunciation)
    └── Comment field
```

---

## 4. State Management

### 4.1 Server State (TanStack Query)

```typescript
// hooks/useGenerations.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';

export function useGenerations() {
  return useQuery({
    queryKey: ['generations'],
    queryFn: () => api.getGenerations(),
  });
}

export function useGeneration(id: string) {
  return useQuery({
    queryKey: ['generations', id],
    queryFn: () => api.getGeneration(id),
    refetchInterval: (data) =>
      data?.status === 'processing' ? 2000 : false,
  });
}

export function useCreateGeneration() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: api.createGeneration,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['generations'] });
    },
  });
}
```

### 4.2 Client State (Zustand)

```typescript
// stores/uiStore.ts
import { create } from 'zustand';

interface UIState {
  selectedVoice: string | null;
  routingStrategy: 'cost' | 'quality' | 'speed' | 'balanced';
  isGenerating: boolean;

  setSelectedVoice: (voiceId: string) => void;
  setRoutingStrategy: (strategy: UIState['routingStrategy']) => void;
  setIsGenerating: (isGenerating: boolean) => void;
}

export const useUIStore = create<UIState>((set) => ({
  selectedVoice: null,
  routingStrategy: 'balanced',
  isGenerating: false,

  setSelectedVoice: (voiceId) => set({ selectedVoice: voiceId }),
  setRoutingStrategy: (strategy) => set({ routingStrategy: strategy }),
  setIsGenerating: (isGenerating) => set({ isGenerating }),
}));
```

---

## 5. API Integration

### 5.1 Auto-Generated API Client

Generate TypeScript types from the backend OpenAPI spec:

```bash
# package.json script
"generate:api": "openapi-typescript http://localhost:5000/swagger/v1/swagger.json -o src/lib/api/types.ts"
```

### 5.2 API Client

```typescript
// lib/api/client.ts
import createClient from 'openapi-fetch';
import type { paths } from './types';

const client = createClient<paths>({
  baseUrl: process.env.NEXT_PUBLIC_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token interceptor
client.use({
  async onRequest({ request }) {
    const token = getAuthToken();
    if (token) {
      request.headers.set('Authorization', `Bearer ${token}`);
    }
    return request;
  },
});

export const api = {
  // Generations
  getGenerations: () =>
    client.GET('/api/v1/generations'),

  getGeneration: (id: string) =>
    client.GET('/api/v1/generations/{id}', { params: { path: { id } } }),

  createGeneration: (data: GenerationRequest) =>
    client.POST('/api/v1/generations', { body: data }),

  getEstimate: (data: GenerationRequest) =>
    client.POST('/api/v1/generations/estimate', { body: data }),

  // Voices
  getVoices: () =>
    client.GET('/api/v1/voices'),

  // User
  getUsage: () =>
    client.GET('/api/v1/user/usage'),
};
```

---

## 6. Real-time Updates

### 6.1 SignalR Integration

```typescript
// lib/signalr.ts
import * as signalR from '@microsoft/signalr';

class GenerationHub {
  private connection: signalR.HubConnection;

  constructor() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${process.env.NEXT_PUBLIC_API_URL}/hubs/generation`)
      .withAutomaticReconnect()
      .build();
  }

  async connect(token: string) {
    this.connection.accessTokenFactory = () => token;
    await this.connection.start();
  }

  onStatusUpdate(callback: (data: StatusUpdate) => void) {
    this.connection.on('StatusUpdate', callback);
  }

  onProgress(callback: (data: ProgressUpdate) => void) {
    this.connection.on('Progress', callback);
  }

  onCompleted(callback: (data: CompletedUpdate) => void) {
    this.connection.on('Completed', callback);
  }

  onFailed(callback: (data: FailedUpdate) => void) {
    this.connection.on('Failed', callback);
  }
}

export const generationHub = new GenerationHub();
```

### 6.2 React Hook for Real-time

```typescript
// hooks/useGenerationRealtime.ts
import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { generationHub } from '@/lib/signalr';

export function useGenerationRealtime(generationId: string) {
  const queryClient = useQueryClient();

  useEffect(() => {
    generationHub.onStatusUpdate((data) => {
      if (data.id === generationId) {
        queryClient.setQueryData(
          ['generations', generationId],
          (old: any) => ({ ...old, status: data.status })
        );
      }
    });

    generationHub.onProgress((data) => {
      if (data.id === generationId) {
        queryClient.setQueryData(
          ['generations', generationId],
          (old: any) => ({ ...old, progress: data.progress })
        );
      }
    });

    generationHub.onCompleted((data) => {
      if (data.id === generationId) {
        queryClient.invalidateQueries({ queryKey: ['generations', generationId] });
      }
    });
  }, [generationId, queryClient]);
}
```

---

## 7. Hosting

### Recommended: Vercel

```
VERCEL DEPLOYMENT
│
├── Free tier sufficient for MVP
├── Automatic deployments from Git
├── Edge functions for API routes
├── Analytics built-in
└── Easy environment variables management
```

---

*Document maintained by: VoiceProcessor Team*
*Last updated: January 2026*
