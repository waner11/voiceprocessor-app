# Figma Generation Runbook

## Why this is needed
In this OpenCode runtime, the configured `figma_token` server exposes read/export operations only. It cannot create pages or frames in Figma.

## Fastest path to generate the design
1. Open your write-capable Figma MCP client (Claude Code plugin or Cursor Figma plugin).
2. Copy the prompt block from `docs/figma/soft-redesign-brief.md` (section: "Prompt Block For Write-Capable Figma MCP").
3. Run it in that client and let it generate the Figma file.
4. Return the created Figma URL or file key.

## After generation (handled by me)
I will perform a structured QA pass against:
- Foundations coverage
- Marketing + Auth + App screens
- Component variants/states
- Auto-layout and naming quality

Then I will provide a precise refinement pass and implementation mapping to `apps/web` components.
