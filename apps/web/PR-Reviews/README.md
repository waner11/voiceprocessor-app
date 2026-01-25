# PR Reviews

This folder contains code reviews for pull requests in the VoiceProcessor Web project.

## Naming Convention

Files are named using the beads issue ID:

```
{issue-id}.md
```

**Example:** `voiceprocessor-web-d3c.md`

This allows easy cross-referencing between:
- The PR review document
- The beads issue (`bd show {issue-id}`)
- The git branch (branches follow the same naming convention)

## Finding a Review

To find a review for a specific issue:

```bash
# Check the issue details
bd show voiceprocessor-web-xyz

# Read the corresponding review
cat PR-Reviews/voiceprocessor-web-xyz.md
```

## Review Template

Each review should include:

1. **Summary** - Brief description of changes and overall assessment
2. **Issues** - Critical, medium, and minor issues found
3. **What's Good** - Positive aspects of the implementation
4. **Verdict** - Approved / Changes Requested / Needs Discussion
5. **Checklist** - Action items for the author (if applicable)
