"use client";

import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api";

/**
 * Placeholder hook for user usage data.
 * The API doesn't currently have a /user/usage endpoint.
 * This can be implemented when the backend adds usage tracking.
 */
export function useUsage() {
  return useQuery({
    queryKey: ["usage"],
    queryFn: async () => {
      // TODO: Update when backend adds usage endpoint
      // For now, return mock data for UI development
      return {
        charactersUsed: 0,
        charactersLimit: 10000,
        generationsCount: 0,
        periodStart: new Date().toISOString(),
        periodEnd: new Date(
          Date.now() + 30 * 24 * 60 * 60 * 1000
        ).toISOString(),
      };
    },
    staleTime: 60 * 1000, // 1 minute
  });
}
