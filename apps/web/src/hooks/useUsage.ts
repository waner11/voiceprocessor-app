"use client";

import { useQuery } from "@tanstack/react-query";
import { apiRequest } from "@/lib/api/apiRequest";

export interface UsageData {
  creditsUsedThisMonth: number;
  creditsRemaining: number;
  generationsCount: number;
  totalAudioMinutes: number;
}

export function useUsage() {
  const { data, isLoading, error, refetch } = useQuery<UsageData>({
    queryKey: ["usage"],
    queryFn: async () => {
      return apiRequest<UsageData>("/api/v1/usage");
    },
  });

  return {
    data,
    isLoading,
    error,
    refetch,
  };
}
