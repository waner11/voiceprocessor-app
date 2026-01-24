# Phase 1 Monetization: Credit Packs & Turbo Strategy

**Date:** January 2026
**Status:** Approved for Implementation

## 1. Overview
This document outlines the "Phase 1" monetization strategy for VoiceProcessor Web. We are moving away from a recurring subscription model to a **"Credit Packs" (Per-Book)** model. This aligns with market demands for "outcome-based" pricing and simplifies the MVP by avoiding complex subscription logic.

Additionally, we implementing a **"Turbo Profit Strategy"** to ensure healthy margins by defaulting to cost-effective providers and adding friction (visual warnings/surcharges) to expensive ones.

## 2. Pricing Strategy: "Credit Packs"

Users purchase packs of credits (1 Credit ≈ 1 Character).

| Pack Name | Credits (Chars) | Price | Revenue/1k | Margin Analysis |
| :--- | :--- | :--- | :--- | :--- |
| **Short Story** | 15,000 | **$4.99** | ~$0.33 | **Safe.** (OpenAI cost: $0.03) |
| **Novella** | 50,000 | **$19.99** | ~$0.40 | **Safe.** (Even with expensive providers) |
| **Audiobook** | 120,000 | **$39.99** | ~$0.33 | **Safe.** (Protects against $0.30 overage costs) |

*Note: Prices adjusted to ensure profitability even if user selects expensive ElevenLabs models.*

## 3. "Turbo Profit" Routing Strategy

To maintain profitability, we guide users toward high-margin providers (OpenAI, ElevenLabs Turbo v2.5) while still offering premium options.

### 3.1 Defaults
*   **Balanced Strategy (Default):** Prefers **OpenAI TTS-1-HD** or **ElevenLabs Turbo v2.5**.
    *   *Cost:* $0.03 - $0.15 per 1k chars.
    *   *Margin:* > 50%.

### 3.2 Premium Handling
*   **High Cost Providers:** When a user explicitly selects a "Best Quality" model (e.g., ElevenLabs Multilingual v2) that costs >$0.20/1k:
    *   **UI Warning:** Display a badge: **"Premium Quality (High Credit Usage)"** or **"2x Credits"**.
    *   *Rationale:* Educates user on cost and protects against margin loss.

## 4. Implementation Plan (Frontend)

### 4.1 Billing Page (`/settings/billing`)
*   **Remove:** "Current Plan" / Subscription UI.
*   **Add:**
    *   **Credit Balance:** Large display of `user.creditsRemaining`.
    *   **Purchase Cards:** 3 cards for the packs defined above.
    *   **Billing History:** Table showing past purchases.

### 4.2 Mock Payment Integration
*   Since backend is separate, we implement a frontend `PaymentService` mock.
*   **Flow:**
    1.  User clicks "Buy".
    2.  `createCheckoutSession(packId)` called (simulated delay).
    3.  Redirect to `/settings/billing?success=true`.
    4.  Show toast: "Payment Successful! 50,000 Credits Added."

### 4.3 Cost Estimate Component
*   Update to check `provider` and `cost` returned from API.
*   If `cost > threshold`, render `Badge` component with "Premium" warning.
*   Highlight "Best Value" when `RoutingStrategy === 'balanced'`.

## 5. Risks & Mitigation
*   **Risk:** Users exclusively use expensive ElevenLabs models on the cheapest pack.
*   **Mitigation:** The price bump to $19.99/$39.99 ensures we break even or make small profit even in worst-case scenarios ($0.30 cost vs $0.33 revenue).
