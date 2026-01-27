# PR Review: #{PR_NUMBER} - {PR_TITLE}

**Date:** {YYYY-MM-DD}  
**Reviewer:** {Your Name}  
**Author:** {PR Author}  
**Branch:** `{branch-name}`  
**Status:** {Draft | In Review | Approved | Changes Requested}

---

## PR Summary

{Brief description of what this PR does - copy from PR description}

**Related Issues:**
- Closes #{issue_number}
- Related to #{issue_number}

**Files Changed:** {count}  
**Lines Added:** {count}  
**Lines Removed:** {count}

---

## Review Checklist

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

---

## Findings

### 🔴 Critical (Must Fix Before Merge)

{List critical issues that MUST be fixed before merging}

**Example:**
- **File:** `src/VoiceProcessor.Managers/GenerationManager.cs:45`
- **Issue:** SQL injection vulnerability - user input not sanitized
- **Fix:** Use parameterized queries or EF Core LINQ

---

### 🟡 Major (Should Fix Before Merge)

{List major issues that should be addressed}

**Example:**
- **File:** `src/VoiceProcessor.Engines/RoutingEngine.cs:120`
- **Issue:** iDesign violation - Engine calling another Engine directly
- **Fix:** Extract shared logic to a separate Engine or use dependency injection

---

### 🟢 Minor (Nice to Have)

{List minor issues and style improvements}

**Example:**
- **File:** `src/VoiceProcessor.Accessors/ElevenLabsAccessor.cs:78`
- **Issue:** Missing XML comments on public method
- **Fix:** Add `/// <summary>` documentation

---

### 💡 Suggestions (Future Improvement)

{List suggestions for future improvements}

**Example:**
- Consider caching voice list to reduce API calls
- Could extract retry logic into a reusable utility
- Might benefit from using the Strategy pattern for provider selection

---

## Positive Highlights

{Call out well-written code, good patterns, or clever solutions}

**Example:**
- Excellent use of structured logging in `GenerationManager`
- Clean separation of concerns in the chunking logic
- Comprehensive test coverage for edge cases

---

## Architectural Notes

{Document any architectural decisions or patterns introduced}

**Example:**
- Introduced new `IRoutingStrategy` interface to support multiple routing algorithms
- Added `ProviderHealthCheck` background service for monitoring provider availability
- Refactored chunking to use a pipeline pattern for better extensibility

---

## Performance Considerations

{Note any performance implications}

**Example:**
- New caching layer reduces database queries by ~70%
- Async streaming improves memory usage for large text inputs
- Consider adding pagination to voice list endpoint (currently loads all voices)

---

## Security Considerations

{Note any security implications}

**Example:**
- API keys now stored in Azure Key Vault (good!)
- User input is validated before processing (good!)
- Consider adding rate limiting to prevent abuse

---

## Testing Notes

{Comments on test coverage and quality}

**Example:**
- Unit tests cover happy path and error cases
- Missing tests for concurrent generation scenarios
- Integration tests verify end-to-end flow

---

## Documentation Updates Needed

{List documentation that should be updated}

**Example:**
- [ ] Update `AGENTS.md` with new routing strategy pattern
- [ ] Add API endpoint documentation to `docs/API.md`
- [ ] Update `README.md` with new environment variables

---

## Action Items

{Summarize what needs to be done before merge}

### Must Do (Blocking)
- [ ] Fix SQL injection in GenerationManager.cs:45
- [ ] Remove iDesign violation in RoutingEngine.cs:120
- [ ] Add missing unit tests for error cases

### Should Do (Recommended)
- [ ] Add XML comments to public APIs
- [ ] Update AGENTS.md with new patterns
- [ ] Refactor duplicated retry logic

### Nice to Have (Optional)
- [ ] Consider caching optimization
- [ ] Extract magic numbers to constants
- [ ] Add performance benchmarks

---

## Review Decision

**Status:** {Approved | Changes Requested | Needs Discussion}

**Summary:**
{1-2 sentence summary of review outcome}

**Next Steps:**
{What should happen next - author fixes issues, team discussion, etc.}

---

## Reviewer Notes

{Any additional context, questions, or discussion points}

**Example:**
- Discussed routing strategy with team - agreed on current approach
- Suggested future refactoring to use Strategy pattern
- Author plans to address critical issues in next commit
