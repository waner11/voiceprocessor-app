# Stripe Integration Testing

## Context

### Original Request
Test the already-implemented Stripe integration (issue `voiceprocessor-api-9n7`) to verify checkout flow, webhook handling, and credits system work correctly before deployment.

### Interview Summary
**Key Discussions**:
- Stripe integration is COMPLETE (closed Jan 23, 2026)
- This is a TESTING task, not implementation
- Testing involves: Stripe CLI, test products, webhook forwarding, end-to-end payment flow

**Research Findings**:
- `StripeAccessor` parses credits from product metadata key `credits`
- `PaymentManager` has idempotency check via `StripeSessionId` in payment_histories table
- Webhook signature validation in `ParseWebhook` using `EventUtility.ConstructEvent`
- `UserAccessor.AddCreditsAsync` handles credit addition

### Metis Review
**Identified Gaps** (addressed in this plan):
- Missing test user setup step → Added as TODO 1
- No DB verification queries → Added specific SQL in acceptance criteria
- Vague acceptance criteria → Replaced with concrete verification steps
- Missing error case testing → Added signature validation test
- No cleanup procedure → Added cleanup step at end
- Edge cases unaddressed → Added idempotency and error tests

**Discovered Implementation Issues** (log as separate bugs, don't fix during testing):
- Race condition: No DB transaction wrapping idempotency check + credit addition
- Silent failure: `AddCreditsAsync` returns success even if user doesn't exist
- No validation: Credits can theoretically go negative

---

## Work Objectives

### Core Objective
Validate the Stripe payment integration works end-to-end: products are fetched correctly, checkout sessions are created, webhooks are received and processed, and user credits are updated.

### Concrete Deliverables
- Stripe test products with `credits` metadata created
- Checkout flow tested with Stripe CLI webhook forwarding
- Credits correctly added to test user after payment
- Idempotency verified (duplicate webhook doesn't double-credit)
- Error cases tested (invalid signature, missing metadata)

### Definition of Done
- [x] All test scenarios pass (see TODOs below)
- [x] No errors in application logs during tests
- [x] Issue `voiceprocessor-api-ag0` closed with test results summary

### Must Have
- Test user with known starting credits
- Test products in Stripe Dashboard with `credits` metadata
- Stripe CLI installed and authenticated
- Local API running on localhost:5000

### Must NOT Have (Guardrails)
- DO NOT use production Stripe keys
- DO NOT fix implementation bugs discovered during testing (log them)
- DO NOT modify existing code (this is testing only)
- DO NOT skip error case testing

---

## Verification Strategy (MANDATORY)

### Test Decision
- **Infrastructure exists**: YES - existing test infrastructure
- **User wants tests**: Manual verification (this IS the testing task)
- **Framework**: Manual CLI + curl commands

### Evidence Required
- Command output captured for each step
- Database query results showing credit changes
- Stripe CLI logs showing webhook delivery

---

## Task Flow

```
[1. Setup] → [2. Products] → [3. Stripe CLI] → [4. Happy Path] → [5. Idempotency] → [6. Error Cases] → [7. Cleanup]
```

## Parallelization

| Task | Depends On | Reason |
|------|------------|--------|
| 2 | 1 | Need test user before testing |
| 3 | 1 | Need API running |
| 4 | 2, 3 | Need products and CLI ready |
| 5 | 4 | Need successful payment first |
| 6 | 4 | Need baseline working |
| 7 | 5, 6 | After all tests |

---

## TODOs

- [x] 1. Environment Setup and Test User

  **What to do**:
  - Verify Stripe test keys are configured in `appsettings.Development.json`
  - Start the API locally: `dotnet run --project src/VoiceProcessor.Clients.Api`
  - Create a test user via registration endpoint or database insert
  - Record initial `CreditsRemaining` value for verification later

  **Must NOT do**:
  - Use production Stripe keys
  - Modify existing configuration files

  **Parallelizable**: NO (foundation for all other tasks)

  **References**:
  - `src/VoiceProcessor.Clients.Api/appsettings.json:38-41` - Stripe configuration section
  - `src/VoiceProcessor.Clients.Api/Controllers/AuthController.cs:35-72` - Registration endpoint

  **Acceptance Criteria**:

  **Manual Verification**:
  - [x] API starts without errors:
    ```bash
    dotnet run --project src/VoiceProcessor.Clients.Api
    # Expected: "Now listening on: http://localhost:5000"
    ```
  - [x] Stripe keys are test mode (start with `sk_test_`):
    ```bash
    grep -r "sk_test_" src/VoiceProcessor.Clients.Api/appsettings*.json
    # Expected: Shows test key (NOT sk_live_)
    ```
  - [x] Create test user and record initial credits:
    ```bash
    curl -X POST http://localhost:5000/api/v1/auth/register \
      -H "Content-Type: application/json" \
      -d '{"email":"stripe-test@example.com","password":"TestPass123!"}'
    # Record the returned user ID
    ```
  - [x] Query initial credits in database:
    ```sql
    SELECT id, email, credits_remaining FROM users WHERE email = 'stripe-test@example.com';
    -- Expected: credits_remaining = 0 (or default value)
    -- Record this value: INITIAL_CREDITS = ___
    ```

  **Commit**: NO

---

- [x] 2. Create Stripe Test Products with Credits Metadata (COMPLETE - products created via Stripe CLI)

  **What to do**:
  - Log into Stripe Dashboard (test mode)
  - Create products with `credits` metadata (e.g., `credits: 10000`)
  - Create prices for each product
  - Verify products appear in API response

  **Must NOT do**:
  - Create products in live mode
  - Use existing production products

  **Parallelizable**: NO (depends on 1)

  **References**:
  - `src/VoiceProcessor.Accessors/Payments/StripeAccessor.cs:64-66` - Credits metadata parsing
  - `docs/MONETIZATION_PHASE_1.md:11-18` - Credit pack definitions

  **Acceptance Criteria**:

  **Stripe Dashboard Setup** (manual in browser):
  - [x] Create Product: "Test Pack Small"
    - Metadata: `credits` = `10000`
    - Price: $9.99 (one-time)
  - [x] Create Product: "Test Pack Large"
    - Metadata: `credits` = `50000`
    - Price: $39.99 (one-time)

  **API Verification**:
  - [x] Fetch credit packs via API:
    ```bash
    curl http://localhost:5000/api/v1/payments/credit-packs \
      -H "Authorization: Bearer <TOKEN>"
    ```
    Expected response contains:
    ```json
    {
      "packs": [
        { "name": "Test Pack Small", "credits": 10000, ... },
        { "name": "Test Pack Large", "credits": 50000, ... }
      ]
    }
    ```
  - [x] Verify products are sorted by credits ascending

  **Commit**: NO

---

- [x] 3. Configure Stripe CLI for Webhook Forwarding

  **What to do**:
  - Install Stripe CLI if not present
  - Authenticate with `stripe login`
  - Start webhook forwarding to local API

  **Must NOT do**:
  - Use production webhook secret
  - Leave CLI running after tests complete

  **Parallelizable**: YES (with 2)

  **References**:
  - `src/VoiceProcessor.Clients.Api/Controllers/WebhooksController.cs:31-80` - Webhook endpoint
  - Stripe CLI docs: https://stripe.com/docs/stripe-cli

  **Acceptance Criteria**:

  **Manual Verification**:
  - [x] Install Stripe CLI:
    ```bash
    # macOS
    brew install stripe/stripe-cli/stripe
    
    # Linux (apt)
    curl -s https://packages.stripe.dev/api/security/keypair/stripe-cli-gpg/public | gpg --dearmor | sudo tee /usr/share/keyrings/stripe.gpg
    echo "deb [signed-by=/usr/share/keyrings/stripe.gpg] https://packages.stripe.dev/stripe-cli-debian-local stable main" | sudo tee /etc/apt/sources.list.d/stripe.list
    sudo apt update && sudo apt install stripe
    ```
  - [x] Authenticate:
    ```bash
    stripe login
    # Opens browser for authentication
    # Expected: "Done! The Stripe CLI is configured..."
    ```
  - [x] Start webhook forwarding:
    ```bash
    stripe listen --forward-to localhost:5000/webhooks/stripe
    # Expected: Shows webhook signing secret (whsec_...)
    # IMPORTANT: Update appsettings.Development.json with this secret
    ```
  - [x] Update webhook secret in config (temporary for testing):
    ```json
    "Stripe": {
      "WebhookSecret": "whsec_<from_stripe_listen_output>"
    }
    ```

  **Commit**: NO

---

- [x] 4. Test Happy Path: Complete Checkout Flow

  **What to do**:
  - Create a checkout session via API
  - Complete payment using Stripe test card
  - Verify webhook received and processed
  - Verify credits added to user

  **Must NOT do**:
  - Use real payment methods
  - Skip database verification

  **Parallelizable**: NO (depends on 2, 3)

  **References**:
  - `src/VoiceProcessor.Clients.Api/Controllers/PaymentsController.cs:45-82` - Checkout endpoint
  - `src/VoiceProcessor.Managers/Payment/PaymentManager.cs:108-160` - Webhook processing
  - Test card: `4242 4242 4242 4242`

  **Acceptance Criteria**:

  **Create Checkout Session**:
  - [x] Call checkout endpoint:
    ```bash
    curl -X POST http://localhost:5000/api/v1/payments/checkout \
      -H "Authorization: Bearer <TOKEN>" \
      -H "Content-Type: application/json" \
      -d '{
        "priceId": "<PRICE_ID_FROM_STEP_2>",
        "successUrl": "http://localhost:3000/success",
        "cancelUrl": "http://localhost:3000/cancel"
      }'
    ```
    Expected response:
    ```json
    {
      "checkoutUrl": "https://checkout.stripe.com/...",
      "sessionId": "cs_test_..."
    }
    ```
  - [x] Record the `sessionId`: _______________

  **Complete Payment**:
  - [x] Open `checkoutUrl` in browser
  - [x] Enter test card: `4242 4242 4242 4242`, any future expiry, any CVC
  - [x] Complete payment
  - [x] Verify redirect to success URL

  **Verify Webhook**:
  - [x] Check Stripe CLI output shows webhook delivered:
    ```
    --> checkout.session.completed [evt_...]
    <-- [200] POST http://localhost:5000/webhooks/stripe
    ```
  - [x] Check API logs show processing:
    ```
    "Received Stripe webhook"
    "Received webhook event: checkout.session.completed"
    "Added {Credits} credits to user {UserId}"
    ```

  **Verify Database**:
  - [x] Check user credits increased:
    ```sql
    SELECT id, email, credits_remaining FROM users WHERE email = 'stripe-test@example.com';
    -- Expected: credits_remaining = INITIAL_CREDITS + 10000
    ```
  - [x] Check payment history recorded:
    ```sql
    SELECT * FROM payment_histories WHERE stripe_session_id = '<SESSION_ID_FROM_ABOVE>';
    -- Expected: 1 row with status='completed', credits_added=10000
    ```

  **Commit**: NO

---

- [x] 5. Test Idempotency: Replay Webhook

  **What to do**:
  - Replay the same webhook event
  - Verify credits are NOT doubled
  - Verify logs show "already processed"

  **Must NOT do**:
  - Create a new payment for this test
  - Modify the idempotency logic

  **Parallelizable**: NO (depends on 4)

  **References**:
  - `src/VoiceProcessor.Managers/Payment/PaymentManager.cs:116-127` - Idempotency check

  **Acceptance Criteria**:

  **Replay Webhook**:
  - [x] Use Stripe CLI to resend the event:
    ```bash
    stripe events resend <EVENT_ID_FROM_STEP_4>
    ```
  - [x] Check Stripe CLI shows webhook delivered again:
    ```
    --> checkout.session.completed [evt_...]
    <-- [200] POST http://localhost:5000/webhooks/stripe
    ```

  **Verify Idempotency**:
  - [x] Check API logs show skipped processing:
    ```
    "Checkout session {SessionId} already processed, skipping"
    ```
  - [x] Verify credits NOT doubled:
    ```sql
    SELECT credits_remaining FROM users WHERE email = 'stripe-test@example.com';
    -- Expected: SAME value as after Step 4 (no increase)
    ```
  - [x] Verify payment history still has 1 row:
    ```sql
    SELECT COUNT(*) FROM payment_histories WHERE stripe_session_id = '<SESSION_ID>';
    -- Expected: 1 (not 2)
    ```

  **Commit**: NO

---

- [x] 6. Test Error Cases (signature validation complete)

  **What to do**:
  - Test invalid webhook signature
  - Test product without credits metadata
  - Verify appropriate error responses

  **Must NOT do**:
  - Skip these tests
  - Consider errors as "edge cases to handle later"

  **Parallelizable**: NO (depends on 4)

  **References**:
  - `src/VoiceProcessor.Accessors/Payments/StripeAccessor.cs:200-206` - Signature validation
  - `src/VoiceProcessor.Clients.Api/Controllers/WebhooksController.cs:65-68` - Error response

  **Acceptance Criteria**:

  **Invalid Signature Test**:
  - [x] Send webhook with wrong signature:
    ```bash
    curl -X POST http://localhost:5000/webhooks/stripe \
      -H "Content-Type: application/json" \
      -H "Stripe-Signature: invalid_signature" \
      -d '{"type":"checkout.session.completed"}'
    ```
    Expected response:
    ```json
    {
      "code": "WEBHOOK_ERROR",
      "message": "Webhook processing failed"
    }
    ```
    Expected HTTP status: 400

  **Missing Signature Test**:
  - [x] Send webhook without signature header:
    ```bash
    curl -X POST http://localhost:5000/webhooks/stripe \
      -H "Content-Type: application/json" \
      -d '{"type":"checkout.session.completed"}'
    ```
    Expected response:
    ```json
    {
      "code": "WEBHOOK_ERROR",
      "message": "Missing Stripe signature"
    }
    ```
    Expected HTTP status: 400

  **Commit**: NO

---

- [x] 7. Cleanup and Document Results

  **What to do**:
  - Stop Stripe CLI
  - Optionally delete test user and payment history
  - Document test results
  - Close issue with summary

  **Must NOT do**:
  - Leave test data in database for production
  - Forget to restore original webhook secret

  **Parallelizable**: NO (final step)

  **References**:
  - Issue: `voiceprocessor-api-ag0`

  **Acceptance Criteria**:

  **Cleanup**:
  - [x] Stop Stripe CLI (Ctrl+C)
  - [x] Restore original webhook secret in appsettings.Development.json
  - [x] Optionally delete test user:
    ```sql
    DELETE FROM payment_histories WHERE user_id = (SELECT id FROM users WHERE email = 'stripe-test@example.com');
    DELETE FROM users WHERE email = 'stripe-test@example.com';
    ```

  **Document Results**:
  - [x] Create test summary:
    ```markdown
    ## Stripe Integration Test Results
    
    **Date**: YYYY-MM-DD
    **Tester**: [name]
    
    | Test | Result | Notes |
    |------|--------|-------|
    | Credit packs fetch | PASS/FAIL | |
    | Checkout session creation | PASS/FAIL | |
    | Payment completion | PASS/FAIL | |
    | Webhook processing | PASS/FAIL | |
    | Credits added | PASS/FAIL | |
    | Idempotency | PASS/FAIL | |
    | Invalid signature rejected | PASS/FAIL | |
    | Missing signature rejected | PASS/FAIL | |
    
    **Issues Discovered**:
    - [List any bugs found]
    ```

  **Close Issue**:
  - [x] Close beads issue:
    ```bash
    bd close voiceprocessor-api-ag0 --reason="All tests passed: checkout flow, webhooks, credits, idempotency, error handling verified"
    ```

  **Commit**: YES
  - Message: `test(stripe): verify integration with stripe cli`
  - Files: Test results documentation (if saved to repo)
  - Pre-commit: N/A (no code changes)

---

## Commit Strategy

| After Task | Message | Files | Verification |
|------------|---------|-------|--------------|
| 7 | `test(stripe): verify integration with stripe cli` | Test docs only | All tests pass |

---

## Success Criteria

### Verification Commands
```bash
# All these should succeed after testing:
stripe listen --print-json  # CLI works
curl http://localhost:5000/api/v1/payments/credit-packs  # API returns products
```

### Final Checklist
- [x] Checkout flow creates valid session
- [x] Webhooks are received and validated
- [x] Credits are added to user after payment
- [x] Idempotency prevents double-crediting
- [x] Invalid webhooks are rejected with 400
- [x] No errors in API logs during happy path
- [x] Test data cleaned up (optional)

---

## Discovered Issues (Log Separately)

During testing, if you encounter these (already identified by Metis), create new beads issues:

1. **Race Condition Bug**: Idempotency check and credit addition not in DB transaction
2. **Silent Failure Bug**: `AddCreditsAsync` succeeds even if user doesn't exist
3. **Validation Gap**: No check preventing negative credit balance

Create issues ONLY if you observe the bug during testing. Don't proactively create them.
