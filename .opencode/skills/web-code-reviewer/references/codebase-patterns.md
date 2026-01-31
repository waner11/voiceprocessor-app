# VoiceProcessor Web - Codebase Patterns

Enforced conventions specific to this project. Deviations require justification.

## Project Structure

```
apps/web/src/
├── app/                    # Next.js App Router
│   ├── layout.tsx          # Root layout (fonts, providers, Toaster)
│   ├── providers.tsx       # QueryClient + AuthProvider composition
│   ├── globals.css         # Tailwind v4 theme (CSS variables, dark mode)
│   ├── (auth)/             # Public auth routes (login, signup, register)
│   └── (app)/              # Protected routes (dashboard, generate, etc.)
├── components/             # Shared reusable UI components
│   ├── ComponentName/
│   │   ├── ComponentName.tsx
│   │   └── index.ts        # Barrel export
│   ├── layout/             # Layout components (Header, AppLayout, MobileNav)
│   └── index.ts            # Root barrel export
├── hooks/                  # Custom React hooks (data fetching, real-time)
│   └── index.ts
├── lib/                    # Utilities & infrastructure
│   ├── api/
│   │   ├── client.ts       # openapi-fetch client with auth middleware
│   │   └── types.ts        # Auto-generated from OpenAPI (DO NOT edit)
│   ├── signalr/
│   │   └── connection.ts   # SignalR hub connection management
│   └── utils.ts            # cn() helper
├── stores/                 # Zustand stores
│   ├── authStore.ts        # Auth state + persistence
│   └── uiStore.ts          # UI preferences + persistence
└── types/                  # Shared TypeScript types
```

## File Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Component files | PascalCase | `AudioPlayer.tsx`, `VoiceSelector.tsx` |
| Component dirs | PascalCase | `AudioPlayer/`, `CostEstimate/` |
| Hook files | camelCase with `use` prefix | `useGenerations.ts`, `useAuth.ts` |
| Utility files | camelCase | `mapGenerationStatus.ts`, `utils.ts` |
| Store files | camelCase with `Store` suffix | `authStore.ts`, `uiStore.ts` |
| Test files | Same name + `.test` | `formatCredits.test.ts` |
| Test dirs | `__tests__/` co-located | `generate/__tests__/formatCredits.test.ts` |
| Page files | `page.tsx` (Next.js convention) | `app/(app)/generate/page.tsx` |
| Layout files | `layout.tsx` | `app/(app)/layout.tsx` |
| Barrel exports | `index.ts` | Every component dir + hooks/ + stores/ |

## Component Patterns

### Props Interface

```typescript
// Named interface, not inline
interface AudioPlayerProps {
  audioUrl: string;
  chapters?: Chapter[];
  onDownload?: (format: "mp3" | "wav") => void;
  className?: string;  // Always accept className for composition
}

export function AudioPlayer({
  audioUrl,
  chapters = [],
  onDownload,
  className,
}: AudioPlayerProps) {
  // ...
}
```

Rules:
- Named `interface` (not `type`) for props
- Suffix with `Props`
- Default values via destructuring, not default props
- Always accept `className?` on visual components
- Export function directly (not `FC<>`)

### Barrel Exports

```typescript
// components/AudioPlayer/index.ts
export { AudioPlayer } from "./AudioPlayer";

// components/index.ts (root barrel)
export { TextInput } from "./TextInput";
export { AudioPlayer } from "./AudioPlayer";
export { VoiceSelector, type Voice } from "./VoiceSelector";
export { CostEstimate } from "./CostEstimate";
export { GenerationStatus } from "./GenerationStatus";
export { FeedbackForm, type FeedbackData } from "./FeedbackForm";
export { Header, MobileNav, AppLayout } from "./layout";
```

Import pattern:
```typescript
import { AudioPlayer, GenerationStatus } from "@/components";
```

## State Management

### Zustand (Client State)

```typescript
// stores/authStore.ts
export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      creditsRemaining: 0,
      isAuthenticated: false,
      isLoading: true,
      login: (user, creditsRemaining) =>
        set({ user, creditsRemaining, isAuthenticated: true }),
      logout: () =>
        set({ user: null, creditsRemaining: 0, isAuthenticated: false }),
      setLoading: (isLoading) => set({ isLoading }),
    }),
    {
      name: "voiceprocessor-auth",
      partialize: (state) => ({
        user: state.user,
        creditsRemaining: state.creditsRemaining,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);
```

Rules:
- `persist` middleware for data that survives refresh
- `partialize` to exclude ephemeral state from storage
- Actions defined inside the store
- Prefix with `use`, suffix with `Store`
- Storage name prefixed with `voiceprocessor-`

### React Query (Server State)

```typescript
// hooks/useGenerations.ts
export function useGenerations(options: UseGenerationsOptions = {}) {
  const { page = 1, pageSize = 20, status } = options;
  return useQuery({
    queryKey: ["generations", { page, pageSize, status }],
    queryFn: async () => {
      const { data, error } = await api.GET("/api/v1/Generations", {
        params: { query: { page, pageSize, status } },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useCreateGeneration() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (params: CreateGenerationParams) => {
      const { data, error } = await api.POST("/api/v1/Generations", {
        body: { text: params.text, voiceId: params.voiceId },
      });
      if (error) throw error;
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["generations"] });
    },
  });
}
```

Rules:
- One hook file per domain: `useGenerations.ts`, `useVoices.ts`, `useAuth.ts`
- Query keys: `["entity", { ...params }]`
- Always handle `error` from `api.GET/POST` — throw to surface to React Query
- Invalidate related queries in `onSuccess`
- `staleTime` for static data (voices: 5min)
- Conditional `refetchInterval` for in-progress items

### Smart Polling Pattern

```typescript
refetchInterval: (query) => {
  const data = query.state.data;
  const inProgressStatuses = ["Pending", "Analyzing", "Chunking", "Processing", "Merging"];
  return data?.status && inProgressStatuses.includes(data.status) ? 2000 : false;
},
```

## API Client

### Type-Safe Client (openapi-fetch)

```typescript
// lib/api/client.ts
import createClient, { type Middleware } from "openapi-fetch";
import type { paths } from "./types";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

export const api = createClient<paths>({
  baseUrl: API_URL,
  headers: { "Content-Type": "application/json" },
  credentials: "include",
});

// Auth middleware — auto-logout on 401
const authMiddleware: Middleware = {
  async onResponse({ response }) {
    if (response.status === 401 && !isLoggingOut) {
      isLoggingOut = true;
      const { logout } = useAuthStore.getState();
      logout();
      if (typeof window !== "undefined") {
        window.location.href = "/login";
      }
      setTimeout(() => { isLoggingOut = false; }, 1000);
    }
    return response;
  },
};
api.use(authMiddleware);
```

Rules:
- **Never edit `lib/api/types.ts`** — it's auto-generated. Run `pnpm generate:api`.
- All API calls go through `api.GET()` / `api.POST()` / etc.
- Auth hooks use raw `fetch` for login/register (before client is authenticated)
- Always `credentials: "include"` for cookie-based auth

## SignalR (Real-Time)

```typescript
// hooks/useGenerationHub.ts
export function useGenerationHub() {
  const queryClient = useQueryClient();

  const handleStatusUpdate = useCallback(
    (event: StatusUpdateEvent) => {
      queryClient.invalidateQueries({
        queryKey: ["generation", event.generationId],
      });
    },
    [queryClient]
  );

  useEffect(() => {
    startConnection();
    onEvent("StatusUpdate", handleStatusUpdate);
    onEvent("Progress", handleProgress);
    return () => {
      offEvent("StatusUpdate", handleStatusUpdate);
      offEvent("Progress", handleProgress);
      stopConnection();
    };
  }, [handleStatusUpdate, handleProgress]);
}
```

Rules:
- SignalR events invalidate React Query caches (single source of truth)
- Always clean up listeners in `useEffect` return
- Use `useCallback` for event handlers passed to SignalR

## Routing

### Route Groups
- `(auth)/` — Public routes: login, signup, register, forgot-password
- `(app)/` — Protected routes: requires authentication via `AuthProvider`

### Protected Routes

```typescript
// components/AuthProvider.tsx
const publicRoutes = ["/", "/login", "/signup", "/register", "/forgot-password"];

export function AuthProvider({ children }: AuthProviderProps) {
  useEffect(() => {
    if (isLoading) return;
    const isPublicRoute = publicRoutes.includes(pathname);
    if (!isAuthenticated && !isPublicRoute) {
      router.push("/login");
    }
  }, [isLoading, isAuthenticated, pathname, router]);

  return <>{children}</>;
}
```

### Provider Composition

```typescript
// app/providers.tsx
export function Providers({ children }: ProvidersProps) {
  return (
    <QueryClientProvider client={queryClient}>
      <Suspense fallback={null}>
        <NavigationProgress />
      </Suspense>
      <AuthProvider>{children}</AuthProvider>
    </QueryClientProvider>
  );
}
```

Rules:
- Providers wrap the entire app in `layout.tsx`
- `QueryClientProvider` outermost (data layer)
- `AuthProvider` innermost (depends on React Query)
- `Suspense` for async components (NavigationProgress)

## Styling

### cn() Utility

```typescript
// lib/utils.ts
import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

// Usage in components
<div className={cn(
  "rounded-lg p-4 bg-white dark:bg-gray-800",
  isActive && "ring-2 ring-blue-500",
  className
)} />
```

### Tailwind CSS 4 Theme (globals.css)

```css
@import "tailwindcss";

@custom-variant dark (&:where(.dark, .dark *));

:root {
  --background: #ffffff;
  --foreground: #171717;
}

.dark {
  --background: #0a0a0a;
  --foreground: #ededed;
}

@theme inline {
  --color-background: var(--background);
  --color-foreground: var(--foreground);
  --font-sans: var(--font-geist-sans);
  --font-mono: var(--font-geist-mono);
}
```

Rules:
- Use `cn()` for conditional/merged classes
- Dark mode via `.dark` class on `<html>`
- CSS variables for theme tokens
- Geist font family
- Mobile-first responsive (`sm:`, `md:`, `lg:`)

## Logging

From AGENTS.md — structured logging is mandatory:

```typescript
// CORRECT: Structured
logger.info('User submitted generation', {
  userId: user.id,
  textLength: text.length,
  provider: selectedProvider,
});

// WRONG: String interpolation
console.log(`User ${user.id} submitted with ${text.length} chars`);
```

Rules:
- No `console.log` in production code
- Structured JSON format with context
- Sanitize PII (mask emails, never log passwords/tokens)
- Log levels: `debug`, `info`, `warn`, `error`

## Environment

```bash
NEXT_PUBLIC_API_URL=http://localhost:5000    # API base URL
NEXT_PUBLIC_APP_ENV=development              # Environment
```

**Note**: `API_URL` is defined in multiple files. Reference via `process.env.NEXT_PUBLIC_API_URL`.

## Anti-Patterns to Flag

| Anti-Pattern | Severity |
|-------------|----------|
| `any` type usage | BLOCKING |
| `@ts-ignore` / `@ts-expect-error` | BLOCKING |
| `console.log` in production code | MAJOR |
| Missing `'use client'` on interactive component | BLOCKING |
| `'use client'` on non-interactive component | MAJOR |
| Editing `lib/api/types.ts` manually | BLOCKING |
| Server data in Zustand store (duplicate source) | MAJOR |
| Missing `useEffect` cleanup (timers, listeners) | BLOCKING |
| Missing error/loading state for async data | MAJOR |
| `<div onClick>` instead of `<button>` | MAJOR |
| Icon button without `aria-label` | MAJOR |
| Magic pixel values (`w-[347px]`) | MINOR |
| Array index as key on filtered/reorderable list | MAJOR |
| `dangerouslySetInnerHTML` with user input | BLOCKING |
| Secrets in `NEXT_PUBLIC_*` env vars | BLOCKING |
| `window`/`localStorage` access outside `useEffect` | BLOCKING |
| Empty catch block | BLOCKING |
| Inline styles for static values | MINOR |
| Missing dark mode variants on new UI | MINOR |
