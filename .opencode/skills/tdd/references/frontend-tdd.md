# Frontend TDD Reference (React / Next.js)

## Test Stack

| Library | Version | Purpose |
|---------|---------|---------|
| Vitest | 2.1.0 | Test framework (Vite-native) |
| @testing-library/react | 16.1.0 | Component rendering & queries |
| @testing-library/dom | 10.4.0 | DOM queries |
| @testing-library/jest-dom | 6.9.1 | DOM matchers (toBeInTheDocument, etc.) |
| jsdom | 25.0.1 | Browser environment for Node |
| Playwright | 1.49.0 | E2E tests (separate from TDD) |

## Test File Locations

```
apps/web/
├── src/
│   ├── app/(app)/generate/__tests__/
│   │   └── formatCredits.test.ts        # Utility tests co-located
│   ├── app/(app)/generations/[id]/__tests__/
│   │   └── page.test.tsx                # Page component tests
│   ├── lib/utils/__tests__/
│   │   └── mapGenerationStatus.test.ts  # Lib utility tests
│   └── hooks/__tests__/                 # Hook tests (create as needed)
└── tests/
    ├── setup.ts                         # Global test setup
    └── e2e/
        └── app.spec.ts                  # E2E tests (NOT TDD)
```

**Where to create new tests:**
- Utility function → `__tests__/` next to the source file
- Hook → `src/hooks/__tests__/{hookName}.test.ts`
- Component → `src/components/{Name}/__tests__/{Name}.test.tsx`
- Page → `src/app/{route}/__tests__/page.test.tsx`

## Commands

```bash
# Run ALL unit tests
pnpm test

# Run single test file (most useful for TDD)
pnpm test src/lib/utils/__tests__/mapGenerationStatus.test.ts

# Run tests matching a pattern
pnpm test --testNamePattern="displays loading"

# Watch mode (re-runs on file change — ideal for TDD)
pnpm test --watch

# Watch specific file
pnpm test --watch src/hooks/__tests__/useGenerations.test.ts

# With coverage
pnpm test --coverage
```

**TDD loop command** (run in a separate terminal):
```bash
pnpm test --watch <your-test-file>
```

## Test Setup

The global setup (`tests/setup.ts`) runs before every test:

```typescript
import "@testing-library/dom";
import { cleanup } from "@testing-library/react";
import { afterEach } from "vitest";
import "@testing-library/jest-dom";

afterEach(() => {
  cleanup();
});
```

This provides:
- `@testing-library/jest-dom` matchers (`toBeInTheDocument`, etc.)
- Automatic DOM cleanup after each test

## Test Structure

### Utility Function Tests

```typescript
import { describe, it, expect } from 'vitest';
import { mapGenerationStatus } from '../mapGenerationStatus';

describe('mapGenerationStatus', () => {
  it('maps Pending to queued', () => {
    expect(mapGenerationStatus('Pending')).toBe('queued');
  });

  it('maps Completed to completed', () => {
    expect(mapGenerationStatus('Completed')).toBe('completed');
  });

  it('returns unknown for unrecognized status', () => {
    expect(mapGenerationStatus('Gibberish')).toBe('unknown');
  });
});
```

### Component Tests

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CostEstimate } from '../CostEstimate';

describe('CostEstimate', () => {
  it('displays estimated cost and credits', () => {
    render(
      <CostEstimate
        characterCount={500}
        estimatedCost={0.05}
        creditsRequired={50}
      />
    );

    expect(screen.getByText(/50 credits/)).toBeInTheDocument();
    expect(screen.getByText(/\$0\.05/)).toBeInTheDocument();
  });

  it('shows loading skeleton when isLoading is true', () => {
    render(<CostEstimate isLoading />);

    expect(screen.queryByText(/credits/)).not.toBeInTheDocument();
    expect(screen.getByRole('status')).toBeInTheDocument();
  });
});
```

### Component Tests with Mocked Hooks

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import Page from '../page';

// Mock external dependencies
vi.mock('@/hooks/useGenerations', () => ({
  useGeneration: vi.fn(),
  useSubmitFeedback: vi.fn(() => ({ mutate: vi.fn() })),
}));

vi.mock('next/navigation', () => ({
  useParams: vi.fn(() => ({ id: 'test-id' })),
  useRouter: vi.fn(() => ({ push: vi.fn() })),
}));

// Import AFTER mock declaration
import { useGeneration } from '@/hooks/useGenerations';

describe('Generation Detail Page', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders loading state', () => {
    (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
      data: null, error: null, isLoading: true,
    });

    render(<Page />);
    expect(screen.getByText(/loading/i)).toBeInTheDocument();
  });

  it('renders generation details when loaded', () => {
    (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
      data: { id: 'test-id', status: 'Completed', provider: 'ElevenLabs' },
      error: null, isLoading: false,
    });

    render(<Page />);
    expect(screen.getByText('ElevenLabs')).toBeInTheDocument();
  });

  it('renders error state when fetch fails', () => {
    (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
      data: null,
      error: { code: 'NOT_FOUND', message: 'Generation not found' },
      isLoading: false,
    });

    render(<Page />);
    expect(screen.getByText(/not found/i)).toBeInTheDocument();
  });
});
```

### Hook Tests

```typescript
import { describe, it, expect, vi } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useGenerations } from '../useGenerations';

// Mock the API client
vi.mock('@/lib/api/client', () => ({
  api: {
    GET: vi.fn(),
  },
}));

import { api } from '@/lib/api/client';

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}

describe('useGenerations', () => {
  it('returns generations on success', async () => {
    (api.GET as ReturnType<typeof vi.fn>).mockResolvedValue({
      data: { items: [{ id: '1', status: 'Completed' }], totalCount: 1 },
      error: undefined,
    });

    const { result } = renderHook(() => useGenerations(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data?.items).toHaveLength(1);
  });

  it('throws when API returns error', async () => {
    (api.GET as ReturnType<typeof vi.fn>).mockResolvedValue({
      data: undefined,
      error: { code: 'UNAUTHORIZED', message: 'Not authenticated' },
    });

    const { result } = renderHook(() => useGenerations(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
  });
});
```

## Mocking Patterns

### Mock a Module

```typescript
// Mock entire module
vi.mock('@/hooks/useAuth', () => ({
  useLogin: vi.fn(() => ({ mutate: vi.fn(), isPending: false })),
  useLogout: vi.fn(() => ({ mutate: vi.fn() })),
}));

// Mock with factory (reset per test)
vi.mock('@/stores/authStore', () => ({
  useAuthStore: vi.fn(),
}));
```

### Mock Return Values

```typescript
// Static return
(useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
  data: mockData, error: null, isLoading: false,
});

// Resolved promise
(api.GET as ReturnType<typeof vi.fn>).mockResolvedValue({ data, error: undefined });

// Rejected promise
(api.POST as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('Network error'));
```

### Mock Next.js

```typescript
// Navigation
vi.mock('next/navigation', () => ({
  useRouter: vi.fn(() => ({ push: vi.fn(), back: vi.fn() })),
  useParams: vi.fn(() => ({ id: 'test-id' })),
  usePathname: vi.fn(() => '/dashboard'),
  useSearchParams: vi.fn(() => new URLSearchParams()),
}));

// Image
vi.mock('next/image', () => ({
  default: (props: any) => <img {...props} />,
}));
```

### Clear Mocks Between Tests

```typescript
beforeEach(() => {
  vi.clearAllMocks();
});
```

## Assertion Patterns

### DOM Assertions (Testing Library)

```typescript
// Element presence
expect(screen.getByText('Submit')).toBeInTheDocument();
expect(screen.queryByText('Error')).not.toBeInTheDocument();

// Async element (waits for it to appear)
expect(await screen.findByText('Loaded')).toBeInTheDocument();

// By role (accessibility-first — preferred)
expect(screen.getByRole('button', { name: /submit/i })).toBeInTheDocument();
expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Dashboard');

// By label (for form inputs)
expect(screen.getByLabelText(/email/i)).toBeInTheDocument();

// Element state
expect(screen.getByRole('button')).toBeDisabled();
expect(screen.getByRole('button')).toBeEnabled();
expect(screen.getByRole('textbox')).toHaveValue('hello');
```

### User Interaction

```typescript
import userEvent from '@testing-library/user-event';

it('submits form on button click', async () => {
  const user = userEvent.setup();
  const onSubmit = vi.fn();

  render(<Form onSubmit={onSubmit} />);

  await user.type(screen.getByLabelText(/text/i), 'Hello world');
  await user.click(screen.getByRole('button', { name: /generate/i }));

  expect(onSubmit).toHaveBeenCalledWith(
    expect.objectContaining({ text: 'Hello world' })
  );
});
```

### Value Assertions (Vitest)

```typescript
expect(result).toBe('exact value');
expect(result).toEqual({ id: 1, name: 'test' });
expect(result).toContain('partial');
expect(array).toHaveLength(3);
expect(fn).toHaveBeenCalledWith('arg1', 'arg2');
expect(fn).toHaveBeenCalledTimes(1);
```

## Query Priority (IMPORTANT)

Always use the most accessible query. This order matters:

| Priority | Query | When |
|----------|-------|------|
| 1 | `getByRole` | Interactive elements (button, heading, textbox) |
| 2 | `getByLabelText` | Form inputs |
| 3 | `getByPlaceholderText` | Inputs without visible label |
| 4 | `getByText` | Non-interactive text content |
| 5 | `getByDisplayValue` | Current input values |
| 6 | `getByAltText` | Images |
| 7 | `getByTitle` | Title attributes |
| **Last resort** | `getByTestId` | Only when nothing else works |

**For async content:** Replace `getBy` with `findBy` (returns a Promise, waits up to 1s).

## TDD by Component Type

### Utility Functions (Easiest — Start Here)

Pure input → output. No mocking needed.

```typescript
// RED: Write test first
describe('formatCurrency', () => {
  it('formats dollars with 2 decimal places', () => {
    expect(formatCurrency(10.5)).toBe('$10.50');
  });
});

// GREEN: Implement
export function formatCurrency(amount: number): string {
  return `$${amount.toFixed(2)}`;
}
```

### Custom Hooks

Test through `renderHook`. Mock API client.

```typescript
// RED: Write test first
describe('useVoices', () => {
  it('returns voices sorted by name', async () => {
    (api.GET as any).mockResolvedValue({
      data: { items: [{ id: '1', name: 'Bella' }, { id: '2', name: 'Adam' }] },
    });

    const { result } = renderHook(() => useVoices(), { wrapper: createWrapper() });
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data?.items[0].name).toBe('Adam');
  });
});

// GREEN: Implement the hook
```

### Presentational Components

Test what the user sees. Mock data via props.

```typescript
// RED: Write test first
describe('GenerationStatus', () => {
  it('shows progress bar at 75%', () => {
    render(<GenerationStatus status="Processing" progress={75} />);
    const progressBar = screen.getByRole('progressbar');
    expect(progressBar).toHaveAttribute('aria-valuenow', '75');
  });
});

// GREEN: Implement component
```

### Page Components (Container)

Mock hooks, test state combinations (loading, error, success).

```typescript
// RED: Three tests first — loading, success, error
it('shows skeleton while loading', () => { /* ... */ });
it('renders data when loaded', () => { /* ... */ });
it('shows error message on failure', () => { /* ... */ });

// GREEN: Implement page component to satisfy all three
```

## TDD Cycle Example: New Custom Hook

Goal: Add `useEstimateCost` hook.

```
Step 1 (RED):
  Write: useEstimateCost.test.ts → "returns cost estimate for valid text"
  Run: pnpm test --watch useEstimateCost.test.ts
  Result: FAIL (module not found)

Step 2 (GREEN):
  Create: src/hooks/useEstimateCost.ts with minimal useMutation wrapper
  Run: PASS ✓

Step 3 (RED):
  Write: "invalidates generation queries on success"
  Run: FAIL

Step 4 (GREEN):
  Add: onSuccess → queryClient.invalidateQueries
  Run: PASS ✓

Step 5 (RED):
  Write: "returns error when API fails"
  Run: FAIL

Step 6 (GREEN):
  Add: proper error handling (throw error from api response)
  Run: PASS ✓

Step 7 (REFACTOR):
  Extract CreateWrapper helper, improve type safety
  Run ALL: ALL PASS ✓

Step 8: Commit
  git commit -m "add cost estimation hook with query invalidation"
```

## Common Pitfalls

| Pitfall | Fix |
|---------|-----|
| Testing implementation (mock internals) | Test behavior — what user sees/does |
| `getByTestId` as first choice | Use `getByRole`, `getByLabelText`, `getByText` first |
| `fireEvent` instead of `userEvent` | Use `userEvent.setup()` for realistic interactions |
| Forgetting `await` on `findBy` queries | Always `await screen.findByText(...)` |
| Not clearing mocks between tests | `beforeEach(() => vi.clearAllMocks())` |
| Snapshot tests as primary strategy | Test specific behavior, not entire DOM |
| Testing React internals (state, refs) | Test through the rendered output |
