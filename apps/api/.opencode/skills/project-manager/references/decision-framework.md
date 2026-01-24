# PM Decision Framework

## Prioritization Strategy

### 1. Revenue First (P0)
Features that enable billing and monetization are critical. We cannot survive as a free service.
- Stripe integration
- Credits system
- Payment processing
- Subscription management

### 2. User Trust (P1)
Reliability and transparency build confidence in the platform.
- Pricing calculators
- Real-time status updates
- Error handling and recovery
- Data security

### 3. Expansion (P2)
New providers and features that offer distinct advantages.
- Additional TTS providers (Google, Amazon, etc.)
- Cost/quality/speed optimization
- Only if they provide clear ROI

### 4. Polish (P3)
UI/UX improvements and refinements.
- Design tweaks
- Animation and transitions
- Only if they don't block core user journey

---

## Decision Making Framework

When evaluating a new feature request or idea, assess against these criteria:

### 1. Roadmap Alignment
- Is it in `ROADMAP.md`?
- Does it help us hit the "Public Beta" (Mar 2026) or "Production" (Apr 2026) milestone?
- Does it support the current MVP phase (Paid Beta)?

### 2. Business Value (ROI)
- Will users pay for this?
- Does it lower our costs or increase margins?
- What's the revenue impact?

### 3. User Pain
- Does it solve a critical problem?
- Example: "I can't afford ElevenLabs for a whole book" → Long-form support solves this
- Is it blocking conversions or retention?

### 4. Resource Allocation
- Is this worth delaying the launch for?
- How many dev days does it require?
- Can we ship MVP without it?

---

## Communication Style

- **Business-First**: Focus on "Conversion," "Retention," "Revenue," "User Experience"
- **Non-Technical**: Avoid jargon (Dependency Injection, Middleware, Controllers)
- **Decisive**: Make clear calls on scope ("Cut this for MVP" or "Prioritize immediately")
- **Use Beads**: Reference issues by ID (e.g., `voiceprocessor-api-9n7`) to track work status

---

## Key Metrics to Track

- **Conversion Rate**: Free → Pro signup rate
- **Retention**: Monthly active users, churn rate
- **Revenue**: MRR (Monthly Recurring Revenue), ARPU (Average Revenue Per User)
- **User Satisfaction**: NPS, feature requests, support tickets
- **Time to Market**: Days until Public Beta, Production launch
