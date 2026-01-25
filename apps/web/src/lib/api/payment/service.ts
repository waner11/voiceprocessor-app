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

    const { data, error } = await api.POST("/api/v1/payments/checkout", {
      body: {
        priceId: pack.priceId,
      },
    });

    if (error) {
      throw error;
    }

    return data;
  },

    fetchCreditPacks: async (): Promise<CreditPack[]> => {
      const { data, error } = await api.GET("/api/v1/payments/packs");

      if (error) {
        console.warn("Failed to fetch credit packs from API, using fallback", error);
        return CREDIT_PACKS;
      }

      return data.packs || CREDIT_PACKS;
    },

   verifyPayment: async (
     sessionId: string,
     packId: string
   ): Promise<PaymentVerificationResponse> => {
     throw new Error("Payment verification is handled by Stripe webhooks. This function should not be called directly.");
   },
};
