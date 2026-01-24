# Roadmap Summary

## Product Phases

| Phase | Name | What You Get | Status |
|-------|------|--------------|--------|
| **1** | **Core** | Text-to-speech with ElevenLabs & OpenAI, long-form support | **Available** |
| **2** | **Smart** | 6+ providers, smart routing (cost/quality/speed), pay-as-you-go | **In Progress** |
| **3** | **Pro** | Voice cloning, multi-speaker dialogue, pronunciation control | Planned |
| **4** | **Scale** | Enterprise API, mobile SDKs, webhook integrations, white-label | Future |

---

## Release Timeline

| Stage | Date | Access | What's Included |
|-------|------|--------|-----------------|
| **Alpha** | Jan 2026 | Internal only | Core TTS, basic UI, local testing |
| **Closed Beta** | Feb 2026 | Invite-only | Payments, deployed to Railway/Cloudflare |
| **Public Beta** | Mar 2026 | Open signups | Monitoring, real-time progress, feedback |
| **Production** | Apr 2026 | General availability | Full test coverage, SLA, support |

---

## Current Development Progress

**Current Phase: MVP (Paid Beta)** — 18/27 tasks complete (67%)

| Milestone | Target | API | Frontend | Focus |
|-----------|--------|-----|----------|-------|
| **MVP** | Feb 2026 | 8/13 (62%) | 10/14 (71%) | Core TTS + Payments |
| **Beta** | Mar 2026 | 0/6 (0%) | 0/6 (0%) | Monitoring + UX Polish |
| **v1.0** | Apr 2026 | 0/6 (0%) | 0/5 (0%) | Testing + Launch |

---

## Critical Path to Public Beta

### Backend (API) - 8/13 tasks (62%)
**Completed:**
- Core TTS generation engine
- Multi-provider routing
- User authentication (OAuth + email)
- Generation history and status tracking
- Voice catalog integration

**In Progress / Blocked:**
- Stripe payment integration (P0 blocker)
- Credits system (depends on Stripe)
- Rate limiting
- Real-time progress (SignalR)
- Error tracking (Sentry)
- Feature flags (PostHog)

### Frontend - 10/14 tasks (71%)
**Completed:**
- Landing page with pricing
- User signup/login
- Dashboard with generation history
- Generation form with voice selection
- Cost estimation
- Settings/profile pages

**In Progress / Blocked:**
- Stripe checkout integration (P0 blocker)
- Payment method management
- Billing history
- OAuth linking/unlinking
- Profile update API integration
- Password change functionality

---

## Key Dependencies

1. **Stripe Integration** (P0)
   - Blocks: Credits system, payment processing, billing page
   - Status: Ready to implement
   - Impact: Cannot launch without this

2. **Deployment Infrastructure**
   - Railway (backend)
   - Cloudflare Pages (frontend)
   - Status: Ready to configure
   - Impact: Required for Closed Beta (Feb 2026)

3. **Monitoring & Analytics**
   - Sentry (error tracking)
   - PostHog (feature flags, analytics)
   - Status: Ready to integrate
   - Impact: Required for Public Beta (Mar 2026)

---

## Success Metrics for Each Phase

### MVP (Feb 2026)
- ✅ Core TTS working with 2+ providers
- ✅ User authentication functional
- ✅ Payments processing (Stripe)
- ✅ 10 beta users with positive feedback

### Public Beta (Mar 2026)
- ✅ 100+ active users
- ✅ 5+ providers available
- ✅ Real-time progress tracking
- ✅ <1% error rate

### Production (Apr 2026)
- ✅ 1,000+ active users
- ✅ $5K+ MRR
- ✅ 99.9% uptime
- ✅ <24h support response time
