// Payment Pack Types
export interface CreditPack {
  id: string;
  name: string;
  credits: number;
  price: number;
  description: string;
}

export const CREDIT_PACKS: CreditPack[] = [
  {
    id: "pack_short_story",
    name: "Short Story",
    credits: 15000,
    price: 4.99,
    description: "Perfect for short stories or testing.",
  },
  {
    id: "pack_novella",
    name: "Novella",
    credits: 50000,
    price: 19.99,
    description: "Great for novellas or long chapters.",
  },
  {
    id: "pack_audiobook",
    name: "Audiobook",
    credits: 120000,
    price: 39.99,
    description: "Best value for full-length books.",
  },
];

export interface CheckoutSessionResponse {
  checkoutUrl: string;
}

export interface PaymentVerificationResponse {
  success: boolean;
  creditsAdded: number;
  newBalance: number;
}

/**
 * Mock Payment Service
 * simulating interaction with Stripe backend
 */
export const paymentApi = {
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

    // In a real app, this would call POST /api/v1/payments/checkout
    // and return the Stripe URL provided by the backend.
    // For now, we simulate a success redirect to our own billing page.
    const mockSessionId = `sess_${Math.random().toString(36).substring(7)}`;
    const successUrl = `${window.location.origin}/settings/billing?success=true&session_id=${mockSessionId}&pack_id=${packId}`;
    
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
