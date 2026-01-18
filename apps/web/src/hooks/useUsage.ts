"use client";

import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api";

export function useUsage() {
  return useQuery({
    queryKey: ["usage"],
    queryFn: async () => {
      const { data, error } = await api.GET("/api/v1/user/usage");
      if (error) throw error;
      return data;
    },
    staleTime: 60 * 1000, // 1 minute
  });
}
