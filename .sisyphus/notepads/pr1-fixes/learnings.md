## PR #1 Fixes Complete

### Tasks Executed
1. **Task 1**: Extracted `formatCostWithCredits` helper function
   - Location: Inside GeneratePage component, line 91-96
   - Uses explicit `'en-US'` locale for consistency

2. **Task 2**: Updated both display locations
   - Main cost display (line 256): Now uses helper
   - Per-provider display (line 291): Now uses helper
   - Removed duplicate inline ternaries

3. **Task 3**: Updated test file
   - Changed `toLocaleString()` to `toLocaleString('en-US')`
   - Ensures tests pass in any CI environment

### Key Learnings
- `toLocaleString()` without locale uses system default
  - US systems: "1,234"
  - German systems: "1.234"
  - CI servers may use different locales
- Explicit locale parameter ensures consistency
- DRY principle: Extract once, use everywhere

### Verification
- ✅ Build passes: `pnpm build`
- ✅ Tests pass: 4/4 formatCredits tests
- ✅ Locale-independent: Uses explicit 'en-US'
- ✅ No duplication: Single helper function

### Commits
1. `b966659`: fix(web): use explicit locale for credits formatting
2. `2907814`: fix(web): use explicit locale in credits formatting tests

