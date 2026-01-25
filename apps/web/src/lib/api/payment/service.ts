import { api } from "@/lib/api/client";
import { CREDIT_PACKS } from "./constants";
import {
  CheckoutSessionResponse,
  PaymentVerificationResponse,
  CheckoutRequest,
  CreditPack,
} from "./types";

export const paymentService = {
  createCheckoutSession: async (
    packId: string
  ): Promise<CheckoutSessionResponse> => {
    const pack = CREDIT_PACKS.find((p) => p.id === packId);
    if (!pack) {
      throw new Error("Invalid pack selected");
    }

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const { data, error } = await (api.POST as any)(
      "/api/v1/payments/checkout",
      {
        body: {
          priceId: pack.priceId,
        } as CheckoutRequest,
      }
    );

    if (error) {
      throw error;
    }

    return data as CheckoutSessionResponse;
  },

   fetchCreditPacks: async (): Promise<CreditPack[]> => {
     // eslint-disable-next-line @typescript-eslint/no-explicit-any
     const { data, error } = await (api.GET as any)("/api/v1/payments/packs");

    if (error) {
      console.warn("Failed to fetch credit packs from API, using fallback", error);
      return CREDIT_PACKS;
    }

    const response = data as { packs: CreditPack[] };
    return response.packs || CREDIT_PACKS;
  },

   verifyPayment: async (
     sessionId: string,
     packId: string
   ): Promise<PaymentVerificationResponse> => {
     throw new Error("Payment verification is handled by Stripe webhooks. This function should not be called directly.");
   },
};
