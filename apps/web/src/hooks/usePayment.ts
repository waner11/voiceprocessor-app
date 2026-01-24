import { useState } from "react";
import { paymentApi } from "@/lib/api/payment";

export function usePayment() {
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const startCheckout = async (packId: string) => {
    setIsProcessing(true);
    setError(null);

    try {
      const response = await paymentApi.createCheckoutSession(packId);
      // In a real app, we would redirect to Stripe here.
      // For the mock, we redirect to the returned URL (which points back to our billing page).
      window.location.href = response.checkoutUrl;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to start checkout");
      setIsProcessing(false);
    }
  };

  const verifyTransaction = async (sessionId: string, packId: string) => {
    setIsProcessing(true);
    setError(null);
    try {
      const result = await paymentApi.verifyPayment(sessionId, packId);
      return result;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Verification failed");
      throw err;
    } finally {
      setIsProcessing(false);
    }
  };

  return {
    startCheckout,
    verifyTransaction,
    isProcessing,
    error,
  };
}
