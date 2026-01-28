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

## Landing the Plane (Session Completion)

**When ending a work session**, you MUST complete ALL steps below. Work is NOT complete until `git push` succeeds.

**MANDATORY WORKFLOW:**

1. **File issues for remaining work** - Create issues for anything that needs follow-up
2. **Run quality gates** (if code changed) - Tests, linters, builds
3. **Update issue status** - Close finished work, update in-progress items
4. **PUSH TO REMOTE** - This is MANDATORY:
   ```bash
   git pull --rebase
   bd sync
   git push
   git status  # MUST show "up to date with origin"
   ```
5. **Clean up** - Clear stashes, prune remote branches
6. **Verify** - All changes committed AND pushed
7. **Hand off** - Provide context for next session

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

**Related:** See `WORKFLOW.md` for git workflow and commit conventions.
