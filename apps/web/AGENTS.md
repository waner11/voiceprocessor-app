# Agent Instructions

This project uses **bd** (beads) for issue tracking. Run `bd onboard` to get started.

## Quick Reference

```bash
bd ready              # Find available work
bd show <id>          # View issue details
bd update <id> --status in_progress  # Claim work
bd close <id>         # Complete work
bd sync               # Sync with git
```

## Development Methodology

This project follows **TDD (Test-Driven Development)**. Use `load_skills=["tdd"]` for all implementation tasks.
Write a failing test first, make it pass with minimal code, then refactor. See the `tdd` skill for the full
Red→Green→Refactor workflow, Vitest/Testing Library patterns, and test commands.

## Starting Work on an Issue

**MANDATORY WORKFLOW** - Before working on ANY issue:

```bash
git checkout main              # Switch to main branch
git pull --rebase              # Get latest changes
git checkout -b {issue-id}     # Create branch from issue ID
bd update {issue-id} --status in_progress  # Claim the work
```

**NEVER:**
- Work directly on `main` branch
- Create a branch without pulling latest first
- Skip the branch creation step

## Work Session Workflow

This section covers the complete git workflow from starting work to finishing and creating a pull request.

### Phase 1: Starting Work

Before you begin coding, follow these steps to set up your feature branch:

1. **Check for clean working directory**:
   ```bash
   git status
   ```
   If you have uncommitted changes, commit or stash them before proceeding.

2. **Switch to main and sync**:
   ```bash
   git checkout main
   git pull --rebase origin main
   ```

3. **Create feature branch from issue ID**:
   ```bash
   git checkout -b voiceprocessor-web-xxx
   ```
   Example: `git checkout -b voiceprocessor-web-p9f`
   
   **Edge case**: If the branch already exists locally, delete it first:
   ```bash
   git branch -D voiceprocessor-web-xxx
   git checkout -b voiceprocessor-web-xxx
   ```

4. **Claim the issue**:
   ```bash
   bd update <id> --status in_progress
   ```

### Phase 2: During Work

While working on your feature:

- **Commit frequently** with meaningful messages
- **Follow commit conventions**:
  - Use lowercase, imperative mood, no period
  - **NO AI attribution** in messages
  - Examples:
    - `fix voice selector not updating on provider change`
    - `add cost estimate component`
    - `handle empty text input gracefully`

### Phase 3: Finishing Work ("Landing the Plane")

When your work is complete, you **MUST** follow this sequence. Work is not done until it is pushed and a PR is created.

1. **Quality Check**: Run `npm run build`, `npm run lint`, and `npm test`. Fix any errors.

2. **Cleanup**: Remove unused imports, temp comments, debug code.

3. **Commit final changes**:
   ```bash
   git add -A
   git commit -m "implement feature description"
   ```

4. **Sync main and rebase**:
   ```bash
   git checkout main
   git pull --rebase origin main
   git checkout voiceprocessor-web-xxx
   git rebase main
   ```
   
   **If conflicts occur during rebase**:
   - Open the conflicted files and resolve conflicts manually
   - Stage the resolved files: `git add <file>`
   - Continue the rebase: `git rebase --continue`
   - Repeat until rebase completes
   - If you need to abort: `git rebase --abort`

5. **Push and create PR**:
   ```bash
   git push -u origin voiceprocessor-web-xxx
   gh pr create --base main --fill
   ```
   
   **Edge case**: If PR already exists, check with:
   ```bash
   gh pr list
   ```

6. **Sync beads and close issue**:
   ```bash
   bd sync
   bd close <id> --reason="PR created"
   ```

**CRITICAL RULES:**
- Work is NOT complete until `git push` succeeds
- NEVER stop before pushing - that leaves work stranded locally
- NEVER say "ready to push when you are" - YOU must push
- If push fails, resolve and retry until it succeeds


## Logging Standards

Use structured logging for better debugging and monitoring in production.

### Log Levels

| Level | When to Use |
|-------|-------------|
| `debug` | Detailed diagnostic info (disabled in prod) |
| `info` | Key user actions: page loads, API calls, form submissions |
| `warn` | Recoverable issues: API retry, fallback used, validation warnings |
| `error` | Failures requiring attention: API errors, component crashes, network failures |

### What to Log

**Always include:**
- User ID (if authenticated)
- Request ID / Correlation ID
- Component name
- Action being performed
- Relevant data (sanitized - no passwords/tokens)

**Frontend-specific:**
- Page route
- User agent (browser/device)
- Performance metrics (load time, render time)
- API response times

### Structured Logging Format

Use JSON format for logs (not plain strings):

```typescript
// GOOD - Structured logging
logger.info('User submitted generation request', {
  userId: user.id,
  textLength: text.length,
  provider: selectedProvider,
  voiceId: selectedVoice,
  requestId: generateRequestId()
});

// BAD - String interpolation
console.log(`User ${user.id} submitted request with ${text.length} chars`);
```

### Console.log vs Proper Logging

**❌ Don't use console.log in production code:**
```typescript
console.log('API call failed:', error);  // WRONG
console.log(`Processing ${count} items`); // WRONG
```

**✅ Use a logging library (e.g., pino, winston, or custom logger):**
```typescript
logger.error('API call failed', { error, endpoint, userId });  // CORRECT
logger.info('Processing items', { count, batchId });           // CORRECT
```

### Examples

**Page Load:**
```typescript
logger.info('Dashboard page loaded', {
  userId: session.user.id,
  route: '/dashboard',
  loadTime: performance.now(),
  userAgent: navigator.userAgent
});
```

**API Call:**
```typescript
logger.info('Fetching generations', { userId, page, limit });

try {
  const response = await api.getGenerations({ page, limit });
  logger.info('Generations fetched successfully', {
    userId,
    count: response.data.length,
    duration: response.duration
  });
} catch (error) {
  logger.error('Failed to fetch generations', {
    userId,
    error: error.message,
    statusCode: error.response?.status
  });
}
```

**User Action:**
```typescript
logger.info('User started generation', {
  userId: user.id,
  generationId: generation.id,
  provider: generation.provider,
  textLength: generation.text.length,
  estimatedCost: generation.estimatedCost
});
```

### Performance Logging

Track performance metrics for key operations:

```typescript
const startTime = performance.now();

// ... operation ...

const duration = performance.now() - startTime;

logger.info('Generation completed', {
  generationId,
  duration,
  audioSize: audioBlob.size,
  provider
});
```

### Error Logging

Always log errors with full context:

```typescript
try {
  await processGeneration(request);
} catch (error) {
  logger.error('Generation processing failed', {
    generationId: request.id,
    userId: request.userId,
    error: {
      message: error.message,
      stack: error.stack,
      name: error.name
    },
    context: {
      provider: request.provider,
      textLength: request.text.length
    }
  });
  
  // Re-throw or handle
  throw error;
}
```

### Privacy & Security

**Never log sensitive data:**
- ❌ Passwords, API keys, tokens
- ❌ Full credit card numbers
- ❌ Personal identifiable information (PII) without consent
- ❌ Full text content (log length instead)

**Sanitize before logging:**
```typescript
// GOOD - Sanitized
logger.info('User authenticated', {
  userId: user.id,
  email: maskEmail(user.email), // user@example.com → u***@example.com
  role: user.role
});

// BAD - Exposes PII
logger.info('User authenticated', {
  email: user.email,
  password: user.password  // NEVER LOG PASSWORDS
});
```

### Production Logging

In production, logs should be:
- **Aggregated** - Send to a logging service (e.g., Datadog, LogRocket, Sentry)
- **Searchable** - Use structured format for easy querying
- **Monitored** - Set up alerts for error spikes
- **Retained** - Keep logs for compliance and debugging

### Development vs Production

```typescript
const logger = {
  info: (message: string, data?: object) => {
    if (process.env.NODE_ENV === 'development') {
      console.log(`[INFO] ${message}`, data);
    } else {
      // Send to logging service
      loggingService.log('info', message, data);
    }
  },
  
  error: (message: string, data?: object) => {
    if (process.env.NODE_ENV === 'development') {
      console.error(`[ERROR] ${message}`, data);
    } else {
      // Send to logging service + error tracking
      loggingService.log('error', message, data);
      errorTracking.captureException(data?.error);
    }
  }
};
```

---

**Related:** See `../../WORKFLOW.md` for git workflow and commit conventions.
