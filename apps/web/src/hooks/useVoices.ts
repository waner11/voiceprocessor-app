"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "@/lib/api";
import type { components } from "@/lib/api/types";

type Provider = components["schemas"]["Provider"];

interface UseVoicesOptions {
  page?: number;
  pageSize?: number;
  provider?: Provider;
  language?: string;
  gender?: string;
}

export function useVoices(options: UseVoicesOptions = {}) {
  const { page, pageSize, provider, language, gender } = options;

  return useQuery({
    queryKey: ["voices", { page, pageSize, provider, language, gender }],
    queryFn: async () => {
      const { data, error } = await api.GET("/api/v1/Voices", {
        params: {
          query: { page, pageSize, provider, language, gender },
        },
      });
      if (error) throw error;
      return data;
    },
    staleTime: 5 * 60 * 1000, // 5 minutes - voices don't change often
  });
}

export function useVoice(id: string) {
  return useQuery({
    queryKey: ["voice", id],
    queryFn: async () => {
      const { data, error } = await api.GET("/api/v1/Voices/{id}", {
        params: { path: { id } },
      });
      if (error) throw error;
      return data;
    },
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
  });
}

export function useVoicesByProvider() {
  return useQuery({
    queryKey: ["voices", "by-provider"],
    queryFn: async () => {
      const { data, error } = await api.GET("/api/v1/Voices/by-provider");
      if (error) throw error;
      return data;
    },
    staleTime: 5 * 60 * 1000,
  });
}

export function useRefreshVoices() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      const { error } = await api.POST("/api/v1/Voices/refresh");
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["voices"] });
    },
  });
}
