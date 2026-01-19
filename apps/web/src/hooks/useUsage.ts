"use client";

import { useAuthStore } from "@/stores";

/**
 * Hook to get user's credit/usage information from the auth store.
 * Credits are populated from the login/register response.
 */
export function useUsage() {
  const creditsRemaining = useAuthStore((state) => state.creditsRemaining);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

  return {
    data: isAuthenticated
      ? {
          charactersUsed: 0, // Would need a separate API call to get actual usage
          charactersLimit: creditsRemaining,
          charactersRemaining: creditsRemaining,
        }
      : null,
    isLoading: false,
  };
}
