# PR Review Tracking

This directory tracks Pull Request reviews and findings to maintain code quality and share knowledge across the team.

## Purpose

- **Document review findings** - Capture issues, suggestions, and learnings from PR reviews
- **Track patterns** - Identify recurring issues to improve coding standards
- **Knowledge sharing** - Help team members learn from review feedback
- **Historical reference** - Maintain a record of architectural decisions made during reviews

## Structure

Each PR review gets its own file named after the PR number:

```
PR-Reviews/
├── README.md           # This file
├── TEMPLATE.md         # Template for new reviews
├── PR-123.md           # Review for PR #123
├── PR-124.md           # Review for PR #124
└── ...
```

## Workflow

### 1. Create Review Document

When reviewing a PR, copy the template:

```bash
cp PR-Reviews/TEMPLATE.md PR-Reviews/PR-{number}.md
```

### 2. Fill in Review Details

Document:
- **PR Information** - Title, author, branch, description
- **Review Checklist** - Standard checks (tests, docs, architecture)
- **Findings** - Issues categorized by severity
- **Suggestions** - Improvements and best practices
- **Learnings** - Patterns to adopt or avoid

### 3. Commit Review

```bash
git add PR-Reviews/PR-{number}.md
git commit -m "docs: add PR #{number} review findings"
```

### 4. Share with Author

- Link to the review document in PR comments
- Reference specific line numbers from the review
- Use findings to guide discussion

## Review Checklist

Every PR review should verify:

### Code Quality
- [ ] Follows iDesign architecture (correct layer, no violations)
- [ ] Naming conventions followed (PascalCase, _camelCase, etc.)
- [ ] No code duplication
- [ ] Error handling is appropriate
- [ ] Logging is structured and meaningful

### Testing
- [ ] Unit tests cover new/changed code
- [ ] Tests follow Arrange-Act-Assert pattern
- [ ] Edge cases are tested
- [ ] Tests are deterministic (no flaky tests)

### Documentation
- [ ] Public APIs have XML comments
- [ ] Complex logic has inline comments
- [ ] README updated if needed
- [ ] AGENTS.md updated if workflow changes

### Architecture
- [ ] Follows iDesign calling rules (Client → Manager → Engine → Accessor)
- [ ] No sideways calls (Manager→Manager, Engine→Engine)
- [ ] No upward calls (Accessor→Engine, Engine→Manager)
- [ ] Dependencies injected via constructor
- [ ] Interfaces used for abstractions

### Performance
- [ ] No N+1 queries
- [ ] Async/await used correctly
- [ ] CancellationToken passed through
- [ ] No blocking calls in async methods

### Security
- [ ] No secrets in code
- [ ] Input validation present
- [ ] SQL injection prevented (parameterized queries)
- [ ] Authorization checks in place

## Finding Severity Levels

### 🔴 Critical (Must Fix Before Merge)
- Security vulnerabilities
- Data loss risks
- Architecture violations that break the system
- Breaking changes without migration path

### 🟡 Major (Should Fix Before Merge)
- Performance issues
- iDesign violations (wrong layer, sideways calls)
- Missing error handling
- Incomplete test coverage

### 🟢 Minor (Nice to Have)
- Code style inconsistencies
- Missing comments
- Refactoring opportunities
- Optimization suggestions

### 💡 Suggestion (Future Improvement)
- Alternative approaches
- Patterns to consider
- Future enhancements
- Learning resources

## Common Patterns to Check

### iDesign Violations

**❌ Manager calling Manager directly:**
```csharp
public class GenerationManager
{
    private readonly VoiceManager _voiceManager; // WRONG
    
    public async Task CreateAsync(...)
    {
        await _voiceManager.GetVoiceAsync(...); // Sideways call
    }
}
```

**✅ Manager calling Engine:**
```csharp
public class GenerationManager
{
    private readonly IVoiceEngine _voiceEngine; // CORRECT
    
    public async Task CreateAsync(...)
    {
        var voice = await _voiceEngine.ValidateVoiceAsync(...);
    }
}
```

### Logging Anti-Patterns

**❌ String interpolation:**
```csharp
_logger.LogInformation($"Processing {chunkId}"); // WRONG
```

**✅ Structured logging:**
```csharp
_logger.LogInformation("Processing chunk {ChunkId}", chunkId); // CORRECT
```

### Async Anti-Patterns

**❌ Blocking in async:**
```csharp
public async Task ProcessAsync()
{
    var result = _accessor.GetDataAsync().Result; // WRONG - blocks
}
```

**✅ Await all the way:**
```csharp
public async Task ProcessAsync(CancellationToken ct)
{
    var result = await _accessor.GetDataAsync(ct); // CORRECT
}
```

## Review Examples

See existing PR review files for examples:
- `PR-123.md` - Example of architecture review
- `PR-124.md` - Example of performance review
- `PR-125.md` - Example of security review

## Tips for Reviewers

1. **Be specific** - Reference exact file paths and line numbers
2. **Explain why** - Don't just say "change this", explain the reasoning
3. **Suggest alternatives** - Provide concrete examples of better approaches
4. **Acknowledge good work** - Call out well-written code and good patterns
5. **Link to docs** - Reference AGENTS.md, IDESIGN_ARCHITECTURE.md, etc.
6. **Prioritize** - Use severity levels to help authors focus on critical issues first

## Tips for Authors

1. **Self-review first** - Use the checklist before requesting review
2. **Provide context** - Explain why you made certain decisions
3. **Ask questions** - If unsure about a pattern, ask in the PR description
4. **Learn from feedback** - Review findings are learning opportunities
5. **Update standards** - If a pattern is unclear, propose updating AGENTS.md

## Metrics to Track

Periodically review PR findings to identify:
- **Most common issues** - Update coding standards or add linting rules
- **Recurring patterns** - Create reusable components or utilities
- **Knowledge gaps** - Schedule team training or documentation updates
- **Review time** - Optimize review process if taking too long

## Related Documentation

- `AGENTS.md` - Project coding standards and workflow
- `docs/IDESIGN_ARCHITECTURE.md` - Architecture patterns and rules
- `WORKFLOW.md` - Git workflow and PR process
