"use client";

import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api";

export function useVoices() {
  return useQuery({
    queryKey: ["voices"],
    queryFn: async () => {
      const { data, error } = await api.GET("/api/v1/voices");
      if (error) throw error;
      return data;
    },
    staleTime: 5 * 60 * 1000, // 5 minutes - voices don't change often
  });
}

export function useVoice(id: string) {
  return useQuery({
    queryKey: ["voices", id],
    queryFn: async () => {
      const { data, error } = await api.GET("/api/v1/voices/{id}", {
        params: { path: { id } },
      });
      if (error) throw error;
      return data;
    },
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
  });
}
