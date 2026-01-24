"use client";

import { useMutation } from "@tanstack/react-query";
import { paymentService } from "@/lib/api";
import { useAuthStore } from "@/stores";

export function usePayment() {
  const setCredits = useAuthStore((state) => state.setCredits);
  const creditsRemaining = useAuthStore((state) => state.creditsRemaining);

  const checkoutMutation = useMutation({
    mutationFn: (packId: string) => paymentService.createCheckoutSession(packId),
    onSuccess: (data) => {
      // Redirect to the checkout URL (Stripe or Mock)
      window.location.href = data.checkoutUrl;
    },
  });

  const verifyMutation = useMutation({
    mutationFn: ({
      sessionId,
      packId,
    }: {
      sessionId: string;
      packId: string;
    }) => paymentService.verifyPayment(sessionId, packId),
    onSuccess: (data) => {
      // Update local store with new credits
      // Note: In a real app, we would invalidate the "user" query here.
      // Since we use Zustand for user state, we update it manually.
      if (data.success && data.creditsAdded > 0) {
        setCredits(creditsRemaining + data.creditsAdded);
      }
    },
  });

  return {
    startCheckout: (packId: string) => checkoutMutation.mutate(packId),
    verifyTransaction: (sessionId: string, packId: string) =>
      verifyMutation.mutateAsync({ sessionId, packId }),
    isProcessing: checkoutMutation.isPending || verifyMutation.isPending,
    error: checkoutMutation.error || verifyMutation.error,
  };
}
