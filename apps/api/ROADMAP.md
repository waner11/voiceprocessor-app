# VoiceProcessor Roadmap

Last updated: January 22, 2026

## Progress Overview

```mermaid
---
config:
  theme: base
  themeVariables:
    primaryColor: "#6366f1"
    primaryTextColor: "#ffffff"
    primaryBorderColor: "#4f46e5"
    secondaryColor: "#e0e7ff"
    tertiaryColor: "#f1f5f9"
---
flowchart LR
    subgraph MVP["🚀 MVP (Paid Beta)"]
        direction TB
        M1[/"✅ Core Foundation"\]
        M2[/"✅ Auth System"\]
        M3[/"✅ TTS Workflow"\]
        M4[/"⏳ Payments"\]
        M5[/"⏳ Deploy"\]
    end

    subgraph BETA["🔧 Beta Polish"]
        direction TB
        B1[/"📋 Monitoring"\]
        B2[/"📋 Real-time UX"\]
        B3[/"📋 Analytics"\]
        B4[/"📋 More Providers"\]
    end

    subgraph PROD["🎯 v1.0 Production"]
        direction TB
        P1[/"📋 Full Testing"\]
        P2[/"📋 Performance"\]
        P3[/"📋 Scale Ready"\]
        P4[/"📋 Public Launch"\]
    end

    MVP ==>|"67% Complete"| BETA ==>|"0%"| PROD

    style MVP fill:#dcfce7,stroke:#16a34a,stroke-width:3px
    style BETA fill:#e0e7ff,stroke:#6366f1,stroke-width:2px
    style PROD fill:#f1f5f9,stroke:#94a3b8,stroke-width:2px
```

**Current Phase: MVP (Paid Beta)** — 18/27 tasks complete (67%)

| Milestone | API Progress | Frontend Progress | Target |
|-----------|--------------|-------------------|--------|
| **MVP (Paid Beta)** | 8/13 (62%) | 10/14 (71%) | Feb 2026 |
| Beta Polish | 0/6 (0%) | 0/6 (0%) | Mar 2026 |
| v1.0 Production | 0/6 (0%) | 0/5 (0%) | Apr 2026 |

---

## Overview

VoiceProcessor is a multi-provider TTS SaaS platform. This roadmap tracks progress across both repositories:
- **voiceprocessor-api** - Backend API (.NET 10)
- **voiceprocessor-web** - Frontend (Next.js 16 + React 19)

---

## Status Legend

| Symbol | Meaning |
|--------|---------|
| Done | Completed |
| In Progress | In Progress |
| Planned | Planned |
| Blocked | Blocked |

---

## Milestone 1: MVP (Paid Beta)

**Goal:** End-to-end TTS generation with payment processing  
**Target:** Ready for early adopters

### Backend (API)

| Task | Status | Issue ID | Notes |
|------|--------|----------|-------|
| JWT Authentication | Done | - | JwtEngine, token generation |
| API Key Authentication | Done | - | X-API-Key header support |
| OAuth (Google/GitHub) | Done | - | GoogleOAuthEngine, GitHubOAuthEngine |
| Voice Management | Done | - | VoiceAccessor, auto-seeding |
| TTS Generation Workflow | Done | - | ChunkingEngine, RoutingEngine, PricingEngine |
| ElevenLabs Provider | Done | - | ElevenLabsAccessor |
| OpenAI TTS Provider | Done | - | OpenAiTtsAccessor |
| Background Processing | Done | - | Hangfire + GenerationProcessor |
| Stripe Integration | Planned | `9n7` | Payments, subscriptions, webhooks |
| Credits Deduction | Planned | `5b9` | Deduct on generation completion |
| FeedbackAccessor | Planned | `1mm` | Store generation ratings |
| API Rate Limiting | Planned | `z3f` | Prevent abuse |
| Railway Deployment | Planned | `nrc` | Production hosting |

### Frontend (Web)

| Task | Status | Issue ID | Notes |
|------|--------|----------|-------|
| Home/Landing Page | Done | - | Features, pricing preview, CTA |
| Login Page | Done | - | Email/password + OAuth buttons |
| Signup Page | Done | - | Registration with validation |
| Dashboard | Done | - | Stats cards, recent generations |
| TTS Generator | Done | - | Text input, voice selector, cost estimate |
| Generations List | Done | - | Filters, pagination |
| Voice Catalog | Done | - | Provider/language/gender filters |
| Settings UI | Done | - | Profile, API Keys, Billing, Connections |
| Generation Detail Page | Planned | `9dj` | Audio player, metadata |
| Profile API Integration | Planned | `ymc` | Connect form to API |
| Voice Preview Playback | Planned | `9t1` | Play voice samples |
| Pricing Page | Planned | - | Dedicated pricing page |
| Stripe Checkout | Planned | - | Payment flow integration |
| Cloudflare Deployment | Planned | `xze` | Production hosting |

---

## Milestone 2: Beta Polish

**Goal:** Production stability, monitoring, improved UX  
**Target:** Public beta launch

### Backend (API)

| Task | Status | Issue ID | Notes |
|------|--------|----------|-------|
| SignalR Real-time Progress | Planned | `bss` | WebSocket updates |
| Sentry Error Tracking | Planned | `tkn` | APM + error monitoring |
| PostHog Analytics | Planned | `ai5` | Feature flags, usage analytics |
| User Profile Endpoints | Planned | `8qr` | Update name, email, password |
| Google Cloud TTS Provider | Planned | - | Cost-effective option |
| Amazon Polly Provider | Planned | - | AWS integration |

### Frontend (Web)

| Task | Status | Issue ID | Notes |
|------|--------|----------|-------|
| Real-time Progress | Planned | `04s` | SignalR integration |
| OAuth Connections | Planned | `9hi` | Link/unlink Google/GitHub |
| Forgot Password Flow | Planned | `27x` | Email reset flow |
| Billing Page | Planned | `o7x` | Usage, invoices, plan management |
| Token Auto-Refresh | Planned | - | Proactive refresh before expiry |
| Download Audio | Planned | - | Download generated files |

---

## Milestone 3: v1.0 Production

**Goal:** Feature-complete, tested, scalable  
**Target:** Public launch

### Backend (API)

| Task | Status | Issue ID | Notes |
|------|--------|----------|-------|
| Unit Tests | Planned | `buu` | Engine + Manager coverage |
| Integration Tests | Planned | `buu` | API endpoint tests |
| Fish Audio Provider | Planned | - | Alternative provider |
| Cartesia Provider | Planned | - | High-quality provider |
| Subscription Tiers | Planned | - | Usage limits per tier |
| Webhook Events | Planned | - | Generation completed, etc. |

### Frontend (Web)

| Task | Status | Issue ID | Notes |
|------|--------|----------|-------|
| E2E Tests | Planned | - | Playwright test suite |
| Mobile Responsive Polish | Planned | - | All breakpoints |
| Error Handling UX | Planned | - | Graceful error states |
| Performance Optimization | Planned | - | Bundle size, lazy loading |
| Accessibility Audit | Planned | - | WCAG compliance |

---

## Future (v1.x+)

These features are planned for post-launch:

| Feature | Description |
|---------|-------------|
| Voice Cloning | User uploads voice sample |
| Multi-voice Dialogue | Auto-detect speakers in scripts |
| Translation + TTS | Translate then generate speech |
| Pronunciation Dictionary | User-defined word pronunciations |
| Mobile SDKs | iOS + Android native support |
| ML-based Routing | Learn from user feedback |
| Video Dubbing | Sync audio to video timestamps |

---

## Architecture Reference

```
+-------------------------------------------------------------+
|                    voiceprocessor-web                       |
|          Next.js 16 + React 19 + TanStack Query            |
+-------------------------------------------------------------+
                              |
                              v REST API + SignalR
+-------------------------------------------------------------+
|                    voiceprocessor-api                       |
|                  ASP.NET Core 10 / C# 14                    |
|  +---------+ +---------+ +---------+ +---------------------+
|  | Managers|>| Engines |>|Accessors|>| PostgreSQL + Redis  |
|  +---------+ +---------+ +---------+ | Hangfire, SignalR   |
+--------------------------------------+---------------------+
                              |
                              v Provider APIs
         +--------------+--------------+--------------+
         |  ElevenLabs  |   OpenAI     |  Google TTS  |
         |    (done)    |   (done)     |  (planned)   |
         +--------------+--------------+--------------+
```

---

## Progress Summary

| Milestone | API Progress | Frontend Progress |
|-----------|--------------|-------------------|
| MVP | 8/13 (62%) | 10/14 (71%) |
| Beta | 0/6 (0%) | 0/6 (0%) |
| v1.0 | 0/6 (0%) | 0/5 (0%) |

---

## Contributing

1. Check available work: `bd ready`
2. Claim an issue: `bd update <id> --status in_progress`
3. Complete work: `bd close <id>`
4. Sync changes: `bd sync`
