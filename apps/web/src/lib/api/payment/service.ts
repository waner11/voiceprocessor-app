import { api } from "@/lib/api/client";
import { CREDIT_PACKS } from "./constants";
import {
  CheckoutSessionResponse,
  PaymentVerificationResponse,
  CreditPack,
  Payment,
} from "./types";

function normalizePacksData(packs: unknown[]): CreditPack[] {
  const isValidObject = (pack: unknown): pack is Record<string, unknown> =>
    pack !== null && typeof pack === 'object';

  const hasCheckoutIdentifier = (pack: CreditPack): boolean =>
    pack.priceId !== '';

  return packs
    .filter(isValidObject)
    .map((pack, index) => ({
      id: String(pack.id || pack.priceId || `pack-${index}`),
      name: String(pack.name || 'Credit Pack'),
      credits: Number(pack.credits) || 0,
      price: Number(pack.price) || 0,
      priceId: String(pack.priceId || pack.id || ''),
      description: String(pack.description || ''),
    }))
    .filter(hasCheckoutIdentifier);
}

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

    const normalizedPacks = normalizePacksData(data.packs || []);
    return normalizedPacks.length > 0 ? normalizedPacks : CREDIT_PACKS;
  },

  fetchPaymentHistory: async (): Promise<Payment[]> => {
    const { data, error } = await api.GET("/api/v1/payments/history");

    if (error) {
      console.warn("Failed to fetch payment history", error);
      return [];
    }

    return data?.payments || [];
  },

  verifyPayment: async (
    sessionId: string,
    packId: string
  ): Promise<PaymentVerificationResponse> => {
    throw new Error("Payment verification is handled by Stripe webhooks. This function should not be called directly.");
  },
};
