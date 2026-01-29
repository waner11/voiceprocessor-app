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
      window.location.href = data.checkoutUrl;
    },
    onError: (error) => {
      console.error("Checkout failed:", error);
    },
  });

   const verifyMutation = useMutation({
     mutationFn: () => paymentService.verifyPayment(),
     onSuccess: (data) => {
       // Update local store with new credits
       // Note: In a real app, we would invalidate the "user" query here.
       // Since we use Zustand for user state, we update it manually.
       // TODO: Replace manual calculation with data.newBalance from backend response when API is real.
       if (data.success && data.creditsAdded > 0) {
         setCredits(creditsRemaining + data.creditsAdded);
       }
     },
   });

   return {
     startCheckout: (packId: string) => checkoutMutation.mutate(packId),
     verifyTransaction: () =>
       verifyMutation.mutateAsync(),
     isProcessing: checkoutMutation.isPending || verifyMutation.isPending,
     error: checkoutMutation.error || verifyMutation.error,
   };
}
