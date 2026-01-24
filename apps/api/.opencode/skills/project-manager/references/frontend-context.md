# Frontend Context - User Flows & Features

## User Journey Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    LANDING PAGE                             │
│  - Hero: "Convert Text to Professional Audio"               │
│  - Features showcase (6 key features)                       │
│  - Pricing tiers (Free, Pro, Enterprise)                    │
│  - CTA: "Start Free Trial" (10K chars free)                 │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
        ┌────────────────────────┐
        │   SIGNUP / LOGIN       │
        │ - Email or OAuth       │
        │ - Free trial starts    │
        └────────────┬───────────┘
                     │
                     ▼
        ┌────────────────────────┐
        │    DASHBOARD           │
        │ - Credits remaining    │
        │ - Generation history   │
        │ - Audio duration stats │
        │ - Recent generations   │
        └────────────┬───────────┘
                     │
        ┌────────────┴────────────┐
        │                         │
        ▼                         ▼
   ┌─────────────┐         ┌──────────────┐
   │  GENERATE   │         │  SETTINGS    │
   │ - Text input│         │ - Profile    │
   │ - Voice sel │         │ - Billing    │
   │ - Routing   │         │ - OAuth link │
   │ - Cost est  │         │ - Password   │
   └──────┬──────┘         └──────┬───────┘
          │                       │
          ▼                       ▼
   ┌─────────────┐         ┌──────────────┐
   │  RESULTS    │         │  BILLING     │
   │ - Play audio│         │ - Current    │
   │ - Download  │         │   plan       │
   │ - Share     │         │ - Usage      │
   │ - Feedback  │         │ - Payment    │
   └─────────────┘         │ - History    │
                           └──────┬───────┘
                                  │
                                  ▼
                           ┌──────────────┐
                           │  UPGRADE     │
                           │ - Plan select│
                           │ - Checkout   │
                           │ - Confirm    │
                           └──────────────┘
```

---

## Feature Status

| Feature | Status | Completion | Notes |
|---------|--------|------------|-------|
| **Core Generation** | ✅ Complete | 95% | Main flow works, needs real API data |
| **Voice Catalog** | ✅ Complete | 100% | 100+ voices, all providers integrated |
| **Dashboard** | ✅ Complete | 90% | UI done, needs real usage data from API |
| **Settings** | ⚠️ Partial | 60% | UI complete, API integration needed |
| **Billing/Stripe** | ⚠️ Partial | 50% | UI done, payment processing not implemented |
| **OAuth** | ⚠️ Partial | 70% | Login works, linking/unlinking TODO |
| **Cost Estimation** | ✅ Complete | 95% | Real-time estimates working |
| **Real-time Progress** | ❌ Not Started | 0% | Requires SignalR backend |

---

## Incomplete Features (Blocking MVP)

### High Priority (P0 - Blocks Launch)
- [ ] **Stripe Checkout Integration** - Payment processing not implemented
  - Requires: Stripe API key, webhook handling, test mode validation
  - Impact: Cannot charge users, cannot launch
  - Effort: 3-5 days

### Medium Priority (P1 - Needed for Beta)
- [ ] **Profile Update API Integration** - Settings page UI done, API calls missing
  - Requires: Backend endpoints for name, email update
  - Impact: Users can't update their profile
  - Effort: 1-2 days

- [ ] **Password Change Functionality** - Settings page UI done, API calls missing
  - Requires: Backend password change endpoint
  - Impact: Users can't change password
  - Effort: 1 day

- [ ] **OAuth Linking/Unlinking** - Login works, account linking missing
  - Requires: Backend endpoints for OAuth provider management
  - Impact: Users can't link multiple OAuth providers
  - Effort: 2-3 days

- [ ] **Forgot Password Flow** - Not implemented
  - Requires: Email verification, password reset token
  - Impact: Users locked out can't recover account
  - Effort: 2-3 days

- [ ] **Real-time Generation Progress** - Polling only, no live updates
  - Requires: SignalR hub on backend
  - Impact: Users don't see live progress
  - Effort: 2-3 days

### Low Priority (P2 - Nice to Have)
- [ ] **File Upload in Generator** - Text input only, no file support
  - Requires: File upload handler, text extraction
  - Effort: 2-3 days

- [ ] **Chapters Detection** - No automatic chapter detection
  - Requires: Text analysis, chapter boundary detection
  - Effort: 3-5 days

- [ ] **Feedback Submission** - UI exists, API not integrated
  - Requires: Backend feedback endpoint
  - Effort: 1 day

- [ ] **Invoice Download** - Billing history shows invoices, can't download
  - Requires: PDF generation, Stripe invoice retrieval
  - Effort: 2-3 days

---

## Conversion Points

### Primary CTA: "Start Free Trial"
- **Location:** Landing page hero, navigation bar, pricing section
- **Offer:** 10,000 free characters/month
- **No credit card required**
- **Expected conversion:** 5-10% of landing page visitors

### Generation Trigger
- **Location:** Dashboard, Generate page
- **Requirement:** User must have credits remaining
- **Free tier:** 10,000 characters/month
- **Upgrade prompt:** When credits running low

### Plan Upgrade
- **Free → Pro:** $29/month (500,000 chars/month)
- **Free → Enterprise:** Custom pricing (unlimited)
- **Trigger:** User runs out of free credits
- **Expected conversion:** 10-20% of free users

---

## Pricing Tiers

### Free Plan
- **Price:** $0/month
- **Characters:** 10,000/month
- **Features:**
  - All voice providers
  - Standard quality
  - Email support
- **Target:** Hobbyists, students, evaluation

### Pro Plan
- **Price:** $29/month
- **Characters:** 500,000/month
- **Features:**
  - All voice providers
  - Premium quality voices
  - Priority generation
  - API access
  - Priority support
- **Target:** Content creators, podcasters, small businesses
- **Status:** Most popular (highlighted on landing)

### Enterprise Plan
- **Price:** Custom (contact sales)
- **Characters:** Unlimited
- **Features:**
  - Custom voice cloning
  - SLA guarantee
  - Dedicated support
  - Volume discounts
  - Custom integrations
- **Target:** Teams, agencies, high-volume users

---

## Key Pages & Components

### Landing Page (`/`)
- Hero section with CTA
- Feature showcase (6 features)
- Live demo with voice selection
- Pricing comparison table
- Final CTA section
- Footer with links

### Dashboard (`/dashboard`)
- **Stats Cards:**
  - Credits remaining
  - Total generations
  - Total audio duration (hours)
- **Recent Generations Table:**
  - Character count
  - Provider used
  - Creation date
  - Duration
  - Status badge
- **Create Button:** Links to `/generate`

### Generate Page (`/generate`)
- **Text Input Section:**
  - Large textarea
  - Upload file button
  - Paste button
  - Character/word count
  - Language detection
- **Voice Selection:**
  - Grid of 6 voices
  - Voice name, provider, language, gender
  - Play preview button
  - Browse all voices link
- **Routing Strategy Sidebar:**
  - Balanced (recommended)
  - Best Quality
  - Lowest Cost
  - Fastest
- **Cost Estimate Card:**
  - Real-time cost calculation
  - Character count
  - Estimated duration
  - Recommended provider
- **Action Buttons:**
  - Generate Audio (primary)
  - Preview First 500 Characters (secondary)
- **Quota Display:**
  - Remaining characters this month

### Settings - Billing (`/settings/billing`)
- **Current Plan Section:**
  - Plan name and price
  - Renewal date
  - Change plan button
  - Cancel subscription button
- **Usage This Month:**
  - Character usage progress bar
  - Generations count
  - Audio minutes total
- **Payment Method:**
  - Card display (masked)
  - Expiration date
  - Update button
- **Billing History Table:**
  - Date, description, amount
  - Invoice download links

---

## User Experience Highlights

### Strengths
- ✅ Clean, modern design with dark mode support
- ✅ Real-time cost estimation
- ✅ Voice preview functionality
- ✅ Clear pricing and feature comparison
- ✅ Responsive design (mobile-friendly)
- ✅ Intuitive generation workflow

### Gaps (Blocking MVP)
- ❌ No payment processing (Stripe not integrated)
- ❌ No real-time progress updates (polling only)
- ❌ Profile management incomplete
- ❌ OAuth linking not implemented
- ❌ No file upload support

### Next Steps
1. Integrate Stripe checkout
2. Add SignalR for real-time progress
3. Complete profile management API integration
4. Implement OAuth linking/unlinking
5. Add file upload support
