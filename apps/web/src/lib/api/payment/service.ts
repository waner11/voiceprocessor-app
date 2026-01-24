import { CREDIT_PACKS } from "./constants";
import { CheckoutSessionResponse, PaymentVerificationResponse } from "./types";

/**
 * Mock Payment Service
 * simulating interaction with Stripe backend
 */
export const paymentService = {
  /**
   * Creates a checkout session for a specific pack
   */
  createCheckoutSession: async (
    packId: string
  ): Promise<CheckoutSessionResponse> => {
    // Simulate network delay
    await new Promise((resolve) => setTimeout(resolve, 1000));

    const pack = CREDIT_PACKS.find((p) => p.id === packId);
    if (!pack) {
      throw new Error("Invalid pack selected");
    }

    // Handle SSR safety for window usage
    const origin =
      typeof window !== "undefined"
        ? window.location.origin
        : process.env.NEXT_PUBLIC_APP_URL || "http://localhost:3000";

    // In a real app, this would call POST /api/v1/payments/checkout
    // and return the Stripe URL provided by the backend.
    // For now, we simulate a success redirect to our own billing page.
    const mockSessionId = `sess_${Math.random().toString(36).substring(7)}`;
    const successUrl = `${origin}/settings/billing?success=true&session_id=${mockSessionId}&pack_id=${packId}`;

    return {
      checkoutUrl: successUrl,
    };
  },

  /**
   * Verifies a payment session and returns the result
   */
  verifyPayment: async (
    sessionId: string,
    packId: string
  ): Promise<PaymentVerificationResponse> => {
    // Simulate network delay
    await new Promise((resolve) => setTimeout(resolve, 800));

    const pack = CREDIT_PACKS.find((p) => p.id === packId);

    // In a real app, this would call GET /api/v1/payments/verify/{sessionId}
    // The backend would verify with Stripe and update the user's balance.

    return {
      success: true,
      creditsAdded: pack?.credits || 0,
      newBalance: 0, // This would normally come from the backend response
    };
  },
};
