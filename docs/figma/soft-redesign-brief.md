# VoiceProcessor Full Redesign Brief (Figma Generation Pack)

## Objective
Create a full redesign for VoiceProcessor (multi-provider TTS SaaS) that feels premium, calm, and audio-native, with production-ready UX for both marketing and authenticated app flows.

This brief is optimized for write-capable Figma MCP clients (Claude/Cursor Figma plugin workflows).

## Product Scope
- Marketing: landing + pricing + API docs entry
- Auth: login/register/forgot/reset
- App: dashboard, generate, generations list/detail, voices, settings, payments
- System: tokenized design foundations + reusable component library

## Figma File Structure
Create one Figma file named `VoiceProcessor - Redesign v1` with pages:
- `00 Foundations`
- `01 Marketing`
- `02 Auth`
- `03 App Shell`
- `04 Core Flows`
- `05 Settings & Billing`
- `06 Components`
- `07 Interaction Notes`

## Visual Direction
- Tone: editorial, soft contrast, studio-like, trustworthy
- Style: modern SaaS with subtle depth, not glossy/dribbblized
- Shapes: rounded surfaces, smooth corners, clear hierarchy
- Motion feel: intentional and sparse, 300-500ms ease, no excessive bounce
- Avoid: emoji icons, neon glows, noisy gradients, generic template cards

## Foundations (00)
### Typography
- Heading font: DM Sans
- Body/UI font: Inter
- Mono: Geist Mono

Create text styles:
- Display 56/64 700
- H1 40/48 700
- H2 32/40 700
- H3 24/32 600
- H4 20/28 600
- Body L 16/26 400
- Body M 15/24 400
- Body S 13/20 400
- Label 12/16 500
- Mono 13/18 400

### Color System
- Neutrals: `#F2F2F2 #E5E5E5 #BDBDBD #767676 #525252 #3D3D3D #292929`
- Accents:
  - Primary Indigo `#5D79DF`
  - Teal `#37C8B5`
  - Sky `#4EC7E0`
  - Success `#2DD28D`
  - Warning `#E3B256`
  - Error `#D84E4E`

Create semantic aliases:
- `bg/base`, `bg/surface`, `bg/elevated`
- `text/primary`, `text/secondary`, `text/muted`
- `border/subtle`, `border/strong`
- `state/success`, `state/warning`, `state/error`, `state/info`

### Spacing + Radius + Shadows
- Spacing scale: 4, 8, 12, 16, 20, 24, 32, 40, 48, 64
- Radius tokens: 4, 8, 12, 20, 28, 999
- Shadows:
  - `shadow/soft-1` small blur, low alpha
  - `shadow/soft-2` medium blur
  - `shadow/focus` 2px outline using primary tint

### Grids
- Desktop frame: 1440 width, 12-column grid, 80 margin, 24 gutter
- Tablet frame: 1024 width, 8 columns
- Mobile frame: 390 width, 4 columns

## Screen Coverage

### 01 Marketing
Build desktop (1440) and mobile (390) variants:
- Hero section (headline, subcopy, primary CTA, secondary CTA)
- Trust strip (providers/logos)
- Interactive demo module (text -> voice -> playback)
- Features mosaic (3-6 cards)
- Pricing section (tiers + CTA)
- API quick start section
- Final CTA + footer

### 02 Auth
- Login
- Register
- Forgot password
- Reset password
- Shared auth shell variants (desktop/mobile)

### 03 App Shell
- Header: default, scrolled, compact
- Desktop navigation
- Mobile bottom nav
- Empty state layout
- Loading skeleton layout

### 04 Core Flows
- Dashboard
  - usage cards
  - recent generations
  - quick actions
- Generate page (desktop + mobile)
  - large text input
  - voice selector with filters
  - cost estimate panel
  - generation controls
  - audio preview state
- Generations list
  - table/list rows in status variants
- Generation detail top
  - player summary, metadata, status, feedback CTA
- Voices catalog
  - voice cards + filters + preview control

### 05 Settings & Billing
- Settings overview
- Profile settings
- API keys management
- Connections/integrations
- Billing + credit packs
- Payment success/cancel states

### 06 Components
Create reusable components with variants and states:
- Buttons (primary, secondary, ghost, destructive)
- Input, textarea, select, segmented control
- Tabs
- Status badges
- Chips/tags
- Cards (default, interactive, selected)
- Voice card
- Cost estimate card
- Audio player bar + waveform region
- Modal, dropdown, toast
- Pagination + search bar

For every interactive component include:
- default
- hover
- focus
- disabled
- error (where applicable)

## Interaction Notes (07)
Define motion behaviors:
- page transition fade/slide (subtle)
- card hover elevation
- button press feedback
- progress/loading indicator patterns
- generation status updates states (pending/processing/completed/failed)

## Prompt Block For Write-Capable Figma MCP
Use this exact block in your write-capable MCP client:

```text
Create a full redesign file for "VoiceProcessor" (multi-provider TTS SaaS) with production-ready UI.

Create Figma file: "VoiceProcessor - Redesign v1"
Create pages:
1) 00 Foundations
2) 01 Marketing
3) 02 Auth
4) 03 App Shell
5) 04 Core Flows
6) 05 Settings & Billing
7) 06 Components
8) 07 Interaction Notes

Use style direction:
- premium, calm, soft contrast, audio-native
- rounded surfaces, clear hierarchy, modern spacing rhythm
- avoid emoji icons, harsh neon outlines, rigid cloned cards

Foundations:
- Typography: DM Sans (headings), Inter (body), Geist Mono (mono)
- Colors:
  Neutrals: #F2F2F2 #E5E5E5 #BDBDBD #767676 #525252 #3D3D3D #292929
  Accents: #5D79DF #37C8B5 #4EC7E0 #2DD28D #E3B256 #D84E4E
- Radius tokens: 4 8 12 20 28 999
- Define semantic aliases for background/text/border/status colors
- Add desktop/tablet/mobile grids

Build screens:
- Marketing: hero, trust strip, demo, features, pricing, API section, footer (desktop + mobile)
- Auth: login/register/forgot/reset
- App shell: header + nav states (desktop/mobile)
- Core flows: dashboard, generate (desktop/mobile), generations list, generation detail top, voices catalog
- Settings/Billing: profile, API keys, connections, billing, payment success/cancel

Build component library with variants/states:
- buttons, inputs, textarea, select, tabs, chips, badges, cards, voice card, cost estimate card, audio player bar, modal, dropdown, pagination
- include default/hover/focus/disabled/error states as applicable

All frames must use auto-layout, reusable styles, and reusable components.
Name layers clearly for developer handoff.
```

## Acceptance Checklist
- All pages exist and are named exactly as specified
- Foundations include tokens as styles (not loose swatches)
- Desktop + mobile coverage for marketing and generate flow
- Core components have full interactive states
- No placeholder lorem ipsum in final key screens
- Visual hierarchy is consistent across pages

## Handoff Step
After generation, share the Figma file URL or key. I will then run a structured QA pass and provide a precise refinement list by page and component.
