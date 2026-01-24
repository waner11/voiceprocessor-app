# Phase 1 Monetization: Credit Packs & Turbo Strategy (Backend)

**Date:** January 2026
**Status:** Approved for Implementation

## 1. Overview
This document outlines the "Phase 1" monetization strategy for the VoiceProcessor API. The system is moving to a **"Credit Packs" (Per-Book)** model, replacing the initially planned recurring subscription model for the MVP. This simplifies billing logic while aligning with the market demand for outcome-based pricing.

## 2. Pricing Model: "Credit Packs"

The backend must support purchasing discrete "packs" of credits.
**1 Credit ≈ 1 Character.**

| Pack ID | Name | Credits | Price (USD) |
| :--- | :--- | :--- | :--- |
| `pack_short_story` | **Short Story** | 15,000 | $4.99 |
| `pack_novella` | **Novella** | 50,000 | $19.99 |
| `pack_audiobook` | **Audiobook** | 120,000 | $39.99 |

## 3. "Turbo Profit" Routing Logic

To ensure profitability, the `RoutingEngine` must optimize for margin while respecting user preferences.

### 3.1 Routing Strategy Logic
The `RoutingEngine` should prioritize providers based on the `RoutingPreference` enum:

*   **Balanced (Default):**
    *   Prioritize: **OpenAI TTS-1-HD** OR **ElevenLabs Turbo v2.5**.
    *   Target Cost: < $0.15 / 1k chars.
*   **Cost:**
    *   Prioritize: **Google Neural2** or **Amazon Polly Neural**.
    *   Target Cost: < $0.02 / 1k chars.
*   **Quality:**
    *   Prioritize: **ElevenLabs Multilingual v2** or **OpenAI TTS-1-HD**.
    *   *Note:* No cost cap, but high credit usage.

### 3.2 Premium Provider Handling
When a user selects a "Premium" provider/model (Cost > $0.20/1k chars):
*   The system *must* accurately deduct credits based on the *actual* cost or apply a multiplier if we decide to abstract "Credits" from "Characters" in the future.
*   For Phase 1: **1 Character = 1 Credit** regardless of provider, so the *Business Logic* relies on the frontend pricing tiers ($0.33-$0.40 revenue/1k) covering the worst-case costs ($0.30/1k).

## 4. Implementation Plan (Backend)

### 4.1 Payment Endpoints (`PaymentController`)
*   `POST /api/v1/payments/checkout`
    *   **Input:** `{ packId: string, successUrl: string, cancelUrl: string }`
    *   **Logic:**
        1.  Validate `packId`.
        2.  Create Stripe Checkout Session (metadata: `userId`, `packId`, `credits`).
        3.  Return `{ checkoutUrl: string }`.

*   `POST /webhooks/stripe`
    *   **Logic:**
        1.  Verify Stripe Signature.
        2.  On `checkout.session.completed`:
            *   Extract `userId` and `credits` from metadata.
            *   Call `UserAccessor.AddCredits(userId, credits)`.
            *   Log transaction in `PaymentHistory` table.

### 4.2 Database Schema Updates
*   **Users Table:** Ensure `CreditsRemaining` column exists and supports atomic increments.
*   **PaymentHistory Table:**
    *   `Id` (PK)
    *   `UserId` (FK)
    *   `StripeSessionId`
    *   `Amount` (Decimal)
    *   `Currency`
    *   `CreditsAdded` (Int)
    *   `PackId`
    *   `CreatedAt`

### 4.3 Estimator Logic Update
*   Ensure `POST /api/v1/generations/estimate` returns accurate cost *and* provider details so the frontend can display "Premium" warnings if necessary.

## 5. Security & Idempotency
*   **Webhooks:** Must be idempotent. Processing the same Stripe event twice must *not* double-credit the user.
*   **Concurrency:** Credit deduction during generation must use database transactions or atomic updates to prevent race conditions (double spending).
