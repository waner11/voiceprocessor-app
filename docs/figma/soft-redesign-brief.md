# VoiceProcessor Soft Redesign Brief (ElevenLabs-Inspired)

## Goal
Redesign the VoiceProcessor web experience to feel softer, premium, and human-crafted while keeping the product clear and conversion-focused.

## Visual Direction
- Tone: calm, editorial, high-trust, audio-native
- Character: rounded surfaces, restrained neutrals, selective color accents, rich whitespace
- Avoid: sharp boxes, hard blue-only palette, emoji-driven UI, flat card grids

## Foundation Tokens

### Typography
- Heading: DM Sans SemiBold (or Geist SemiBold fallback)
- Body/UI: Inter Regular/Medium
- Mono: Geist Mono

### Radius Scale
- `4px`, `8px`, `12px`, `20px`, `28px`, `999px`

### Color System
- Neutrals: `#F2F2F2` to `#292929`
- Primary Action: `#5D79DF`
- Voice Accent: `#37C8B5`
- Live Accent: `#4EC7E0`
- Success: `#2DD28D`
- Warning: `#E3B256`
- Error: `#D84E4E`

### Motion Rules
- Transition speed: `300-500ms`, ease-in-out
- One hero motion at a time
- Audio-reactive surfaces should feel smooth, never jittery

## Figma File Structure
- Page 1: `00 Foundations`
  - Color styles
  - Type styles
  - Radius/elevation references
- Page 2: `01 Marketing`
  - Desktop and mobile landing
- Page 3: `02 App Shell`
  - Header, nav, profile menu
- Page 4: `03 Core Flows`
  - Generate Audio
  - Dashboard
  - Generations list/detail
- Page 5: `04 Components`
  - Buttons, inputs, select, badges, cards, status chips, progress states

## Screen Blueprints

### Landing (Desktop 1440)
- Soft gradient backdrop with subtle radial bloom behind hero
- Hero copy left-aligned with stronger typographic hierarchy
- Primary CTA as pill button; secondary as ghost button
- Replace rigid 3-column cards with staggered feature mosaic
- Demo area as audio console card with waveform + voice chips
- Pricing cards with clear contrast hierarchy (middle plan elevated by surface, not neon ring)

### Landing (Mobile 390)
- Stack order: hero, trust strip, demo, key features, pricing, CTA
- Keep top nav minimal with clear primary action

### App Header
- Frosted but warm translucent bar
- Active nav uses tinted neutral pill, not hard rectangle
- Credits and profile area grouped inside soft chip container

### Dashboard
- Stats row uses mixed card sizes (not uniform clones)
- Recent generations list includes polished status chips with icon + label
- Remove emoji; use consistent icon family

### Generate Audio
- Two-panel layout desktop, single-column mobile
- Left: text + chapter preview + voice list
- Right: routing, preset, format, estimate, action stack
- Strong visual anchor around generation action card (soft gradient panel)

## Component Specs
- Buttons
  - Primary: filled, 8px radius (12px on hero CTAs), medium weight
  - Secondary: neutral surface, subtle border
- Cards
  - Standard: 20px radius, neutral tint background, 1px soft border
  - Highlight: same structure with accent tint layer
- Inputs/Textareas
  - 12px radius, high legibility, visible focus ring
- Badges/Status
  - Rounded pills with low-saturation bg and darker text tint

## Copy Style
- Remove generic AI phrasing
- Use concise, confident product language
- Keep headings sentence-case, not title-case overload

## Accessibility
- Minimum AA contrast across text and controls
- Focus states visible on all interactive elements
- Tap targets at least 44px on mobile

## Figma MCP Execution Prompt
Use this exact prompt when connected to the Figma remote MCP server (`https://mcp.figma.com/mcp`) with authentication:

```text
Create a complete redesign for "VoiceProcessor" inspired by ElevenLabs visual quality (not copied), focused on a softer, premium, human-designed feel.

Output requirements:
1) Build a new Figma file with pages:
   - 00 Foundations
   - 01 Marketing
   - 02 App Shell
   - 03 Core Flows
   - 04 Components

2) Foundations:
   - Typography styles: DM Sans/Inter/Geist Mono hierarchy
   - Color styles with neutral scale and accents:
     #F2F2F2, #E5E5E5, #BDBDBD, #767676, #525252, #3D3D3D, #292929,
     #5D79DF, #37C8B5, #4EC7E0, #2DD28D, #E3B256, #D84E4E
   - Radius tokens: 4, 8, 12, 20, 28, 999

3) Marketing page:
   - Desktop frame 1440 and mobile frame 390
   - Hero, trust strip, demo module, features mosaic, pricing, final CTA
   - Rounded, soft surfaces and subtle atmospheric background depth

4) App Shell page:
   - Header variants (default, scrolled, mobile)
   - Nav active/inactive states

5) Core Flows page:
   - Dashboard (overview + recent generations)
   - Generate Audio (desktop + mobile)
   - Generations list row states and detail top section

6) Components page:
   - Buttons, cards, input, textarea, select, chips, badges, status labels, section headers
   - Include hover/focus/disabled state variants

7) Visual constraints:
   - No emoji icons
   - No hard neon outlines
   - No rigid equal-height card clones
   - Prioritize calm typography, soft contrast, and modern spacing rhythm

Deliver fully editable auto-layout frames with reusable components and styles.
```

## Acceptance Checklist
- Visual system is clearly softer than current implementation
- Marketing and app shell look cohesive as one product family
- Mobile layouts are complete and intentional
- Components are reusable and state-complete
- Final result feels senior-designed, not template-generated
