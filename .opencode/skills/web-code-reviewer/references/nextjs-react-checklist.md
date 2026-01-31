# Next.js + React 19 + TypeScript Review Checklist

## 1. Server vs Client Components

### Correct Boundary Placement

**Server Components (default — no directive needed):**
- Data fetching from APIs/databases
- Accessing backend resources directly
- Keeping secrets server-side (API keys, tokens)
- SEO-critical content rendering
- Components with no interactivity

**Client Components (`'use client'` required):**
- Uses `useState`, `useReducer`, `useEffect`
- Has event handlers (`onClick`, `onChange`)
- Uses browser APIs (`localStorage`, `window`)
- Uses custom hooks that depend on state/effects

### Red Flags

| Smell | Severity | Fix |
|-------|----------|-----|
| `'use client'` on a component with no interactivity | MAJOR | Remove directive, keep as Server Component |
| `'use client'` on entire page when only one part is interactive | MAJOR | Extract interactive part to separate Client Component |
| Server Component imported directly into Client Component | BLOCKING | Use composition — pass as `children` prop |
| Fetching data in Client Component when server fetch is possible | MAJOR | Move fetch to Server Component, pass data as props |
| Secrets accessible in Client Component | BLOCKING | Move to server-side only |

### Composition Pattern

```tsx
// CORRECT: Server Component passes data, Client handles interaction
// app/page.tsx (Server Component)
export default async function Page() {
  const data = await fetchData()
  return (
    <InteractiveWrapper>       {/* Client Component */}
      <DataDisplay data={data} /> {/* Server Component as children */}
    </InteractiveWrapper>
  )
}
```

## 2. Data Fetching

### Server-Side Fetching

- Fetch in Server Components with `async/await` directly
- Parallelize independent requests with `Promise.all()`
- Set appropriate caching: `force-cache`, `no-store`, `revalidate: N`
- Use `revalidatePath()` or `revalidateTag()` after mutations

### Client-Side Fetching (React Query)

- Use React Query hooks for client-side data
- Set `staleTime` for data that doesn't change often (voices: 5min)
- Use conditional `refetchInterval` (only poll in-progress items)
- Invalidate queries after mutations with `queryClient.invalidateQueries()`
- Handle loading, error, and empty states explicitly

### Red Flags

| Smell | Severity | Fix |
|-------|----------|-----|
| Sequential `await` for independent data | MAJOR | Use `Promise.all()` |
| `useEffect` for data fetching in Server Component | BLOCKING | Use `async/await` directly |
| No loading state for async data | MAJOR | Add loading skeleton/spinner |
| No error handling for fetches | MAJOR | Add error state + user feedback |
| Polling always-on regardless of status | MINOR | Conditional `refetchInterval` |
| Missing query invalidation after mutation | MAJOR | Add `onSuccess` invalidation |

## 3. React 19 Patterns

### Server Actions
- Mark with `'use server'` directive
- Validate permissions server-side
- Use `revalidatePath()` after mutations
- Handle errors and return them to client

### useActionState (forms)
- Replaces manual `useState` + `onSubmit` for server actions
- Provides `isPending` for loading state
- Returns error state from action

### useOptimistic
- Use for instant UI feedback before server confirms
- Automatically reverts on error
- Don't use for critical operations (payments)

### use() Hook
- Unwrap promises in render
- Combine with Suspense for loading states
- Pair with Error Boundaries for error handling

### ref as Prop
- Pass `ref` directly — `forwardRef` no longer required in many cases

## 4. TypeScript Strictness

### Required Config
```json
{
  "strict": true,
  "noUncheckedIndexedAccess": true  // Recommended
}
```

### Type Patterns

| Pattern | Correct | Wrong |
|---------|---------|-------|
| Unknown data | `unknown` + type guard | `any` |
| Component props | `interface Props { ... }` | Inline object types |
| Children | `children: ReactNode` | `children: any` |
| Events | `React.ChangeEvent<HTMLInputElement>` | `any` or generic `Event` |
| API responses | Auto-generated from OpenAPI | Manual type definitions |
| Component state | Discriminated unions | Boolean flags |
| Constants | `as const` | Plain object |

### Red Flags

| Smell | Severity | Fix |
|-------|----------|-----|
| `any` type | BLOCKING | Use `unknown` with type guards or proper types |
| `@ts-ignore` / `@ts-expect-error` | BLOCKING | Fix the underlying type issue |
| Type assertion (`as`) without justification | MAJOR | Use type guards or proper typing |
| Boolean soup state (`isLoading && isError`) | MAJOR | Use discriminated union |
| Missing return type on exported functions | MINOR | Add explicit return type |
| String interpolation in log templates | MINOR | Use structured logging |

### Discriminated Unions for State

```typescript
// BAD: Boolean flags
type State = { isLoading: boolean; isError: boolean; data?: T; error?: Error }

// GOOD: Discriminated union
type State =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; data: T }
  | { status: 'error'; error: Error }
```

## 5. Component Architecture

### Single Responsibility
- One component = one job
- Extract logic to custom hooks
- Keep components under 200 lines
- Split container (data) from presentational (UI)

### Composition
- Build complex UIs from simple components
- Use `children` prop for flexibility
- Avoid deep prop drilling (max 2-3 levels)
- Use Zustand/Context for deeply shared state

### Custom Hooks
- `use*` prefix mandatory
- Single responsibility per hook
- Correct dependency arrays (never ignore ESLint warnings)
- Return stable references (`useCallback` for functions)
- Include cleanup in `useEffect` returns

### Red Flags

| Smell | Severity | Fix |
|-------|----------|-----|
| Component > 250 lines | MAJOR | Extract sub-components or hooks |
| Hook does 3+ unrelated things | MAJOR | Split into focused hooks |
| Missing `useEffect` cleanup | BLOCKING | Add cleanup for timers/listeners/subscriptions |
| Empty dependency array with state reference | BLOCKING | Stale closure — use functional update |
| Missing `useCallback` for memoized child props | MINOR | Wrap in `useCallback` |
| Conditional hook call | BLOCKING | Hooks must be called unconditionally |

## 6. Performance

### Bundle Size
- Dynamic import heavy components: `const Chart = dynamic(() => import('./Chart'))`
- Import specific functions, not entire libraries
- Keep `'use client'` boundary as narrow as possible
- Analyze with `@next/bundle-analyzer` if suspicious

### Rendering
- `useCallback` for functions passed to `React.memo` children
- `useMemo` for expensive derived calculations
- Don't over-optimize — React 19 compiler handles many cases
- Stable keys for lists (IDs, not array index for dynamic lists)

### Data Fetching
- Conditional `refetchInterval` — only poll in-progress items
- `staleTime` for rarely-changing data (voices, providers)
- Parallel queries for independent data
- Abort controllers for cancelled requests

### Layout & CLS
- Reserve space for images (width/height or aspect-ratio)
- Use `next/image` for automatic optimization
- Skeleton loaders instead of spinners (prevents layout shift)
- Don't inject content above the fold after load

### Red Flags

| Smell | Severity | Fix |
|-------|----------|-----|
| `import _ from 'lodash'` (full import) | MAJOR | Import specific: `import debounce from 'lodash-es/debounce'` |
| Always-on polling regardless of state | MINOR | Conditional `refetchInterval` |
| Array index as key on dynamic list | MAJOR | Use stable unique ID |
| `new Audio()` without cleanup | MAJOR | Store ref, cleanup on unmount |
| Inline objects/arrays in JSX props | MINOR | Extract to variable or `useMemo` |
| Missing `Suspense` for async components | MINOR | Wrap with fallback |

## 7. Accessibility (a11y)

### Semantic HTML
- Use `<button>` for actions, `<a>` for navigation
- Use `<nav>`, `<main>`, `<article>`, `<aside>`, `<section>`
- Headings in order: `<h1>` → `<h2>` → `<h3>` (no skipping)
- Use `<label htmlFor>` for form inputs

### ARIA
- `aria-label` on icon-only buttons
- `role="dialog"` + `aria-modal="true"` on modals
- `aria-live="polite"` for dynamic status updates
- Don't overuse ARIA — semantic HTML first

### Keyboard Navigation
- All interactive elements focusable via Tab
- Visible focus indicators (never `outline: none` without replacement)
- Escape closes modals/popovers
- Focus trapped in modals, returned on close

### Red Flags

| Smell | Severity | Fix |
|-------|----------|-----|
| `<div onClick>` instead of `<button>` | MAJOR | Use semantic `<button>` |
| Icon button without `aria-label` | MAJOR | Add descriptive label |
| Form input without `<label>` | MAJOR | Add associated label |
| `outline: none` / `outline-none` without `focus-visible` | MAJOR | Add visible focus style |
| Color-only state indicator | MINOR | Add icon or text alongside color |
| Missing `alt` on images | MAJOR | Add descriptive alt text |
| No loading announcement for screen readers | MINOR | Add `aria-live` region |

## 8. Error Handling

### Error Boundaries
- `error.tsx` per route segment (Next.js App Router convention)
- User-friendly message + retry button
- Log errors to monitoring service
- Don't show stack traces to users

### Async Errors
- Try/catch in mutation `onError` callbacks
- Toast notifications for user feedback (react-hot-toast)
- Specific error messages, not generic "Something went wrong"
- Retry logic for transient failures (React Query `retry: 3`)

### API Errors
- Structured `ApiError` type: `{ code, message, detail? }`
- 401 middleware for automatic logout
- Per-status-code handling in hooks
- Graceful degradation for non-critical failures

### Red Flags

| Smell | Severity | Fix |
|-------|----------|-----|
| No error state for data fetching | MAJOR | Handle `error` from React Query |
| Empty catch block | BLOCKING | Handle or re-throw with feedback |
| Generic "Error" message to user | MINOR | Specific, actionable message |
| No `error.tsx` for route segment | MINOR | Add error boundary |
| No retry on transient failures | MINOR | Configure React Query retry |

## 9. Security

### XSS Prevention
- React escapes JSX by default — rely on it
- Never use `dangerouslySetInnerHTML` with unsanitized input
- Sanitize with DOMPurify if HTML rendering is required

### Environment Variables
- `NEXT_PUBLIC_*` only for client-safe values (API URL, public keys)
- Never prefix secrets with `NEXT_PUBLIC_`
- Validate env vars at build time

### Input Handling
- Validate all inputs server-side (Zod)
- Don't trust client-side validation alone
- Sanitize before database queries

### Red Flags

| Smell | Severity | Fix |
|-------|----------|-----|
| `dangerouslySetInnerHTML` with user input | BLOCKING | Sanitize or use JSX |
| Secret in `NEXT_PUBLIC_*` variable | BLOCKING | Move to server-side env |
| No server-side validation on form action | MAJOR | Add Zod validation |
| Logging sensitive data (tokens, passwords) | BLOCKING | Sanitize log output |

## 10. Styling (Tailwind CSS 4)

### Conventions
- Use `cn()` utility for conditional/merged classes
- Mobile-first responsive: `text-sm md:text-base lg:text-lg`
- Dark mode: `bg-white dark:bg-gray-800`
- Use design tokens from Tailwind config, not arbitrary values
- Group related classes with `clsx()` for readability

### Red Flags

| Smell | Severity | Fix |
|-------|----------|-----|
| Magic numbers: `w-[347px]` | MINOR | Use Tailwind scale: `w-80` |
| Inline styles for static values | MINOR | Use Tailwind classes |
| Missing dark mode variant | MINOR | Add `dark:` variants |
| Missing responsive breakpoints | MINOR | Add `md:` / `lg:` variants |
| `!important` usage | MAJOR | Fix specificity or use `cn()` |

## 11. Testing

### Unit Tests (Vitest + Testing Library)
- Test behavior, not implementation
- Use `userEvent` over `fireEvent`
- Query by role/label/text (accessibility-first), not test IDs
- Use `findBy` / `waitFor` for async assertions
- MSW for API mocking at network level

### E2E Tests (Playwright)
- Critical user flows (auth, generation, payment)
- Cross-browser (Chromium, Firefox, WebKit)
- Don't duplicate unit test coverage

### Red Flags

| Smell | Severity | Fix |
|-------|----------|-----|
| Testing implementation details (internal state) | MAJOR | Test observable behavior |
| `fireEvent` instead of `userEvent` | MINOR | Use `userEvent.setup()` |
| `getBy` for async content | BLOCKING | Use `findBy` or `waitFor` |
| `getByTestId` as first choice | MINOR | Use semantic queries first |
| No test for changed behavior | MAJOR | Add test covering the change |
| Snapshot tests as primary strategy | MINOR | Prefer behavioral assertions |

## 12. Hydration Safety

### Common Causes of Hydration Mismatch
- `Date.now()` or `Math.random()` in render
- Browser API access during SSR (`window`, `localStorage`)
- Conditional rendering based on client-only state

### Prevention
- Use `useEffect` for client-only code
- Use `suppressHydrationWarning` for intentional mismatches (timestamps)
- Initialize client state as `undefined`, set in `useEffect`

### Red Flags

| Smell | Severity | Fix |
|-------|----------|-----|
| `window` access outside `useEffect` | BLOCKING | Guard with `typeof window !== 'undefined'` or use `useEffect` |
| `Date.now()` in Server Component render | MAJOR | Move to `useEffect` or `suppressHydrationWarning` |
| `localStorage` in initial render | BLOCKING | Read in `useEffect`, use Zustand `onRehydrateStorage` |
