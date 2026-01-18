"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "@/lib/api";

export function useGenerations() {
  return useQuery({
    queryKey: ["generations"],
    queryFn: async () => {
      const { data, error } = await api.GET("/api/v1/generations");
      if (error) throw error;
      return data;
    },
  });
}

export function useGeneration(id: string) {
  return useQuery({
    queryKey: ["generation", id],
    queryFn: async () => {
      const { data, error } = await api.GET("/api/v1/generations/{id}", {
        params: { path: { id } },
      });
      if (error) throw error;
      return data;
    },
    enabled: !!id,
    refetchInterval: (query) => {
      const data = query.state.data as { status?: string } | undefined;
      return data?.status === "processing" ? 2000 : false;
    },
  });
}

interface CreateGenerationParams {
  text: string;
  voiceId: string;
  routingStrategy?: "cost" | "quality" | "speed" | "balanced";
}

export function useCreateGeneration() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (params: CreateGenerationParams) => {
      const { data, error } = await api.POST("/api/v1/generations", {
        body: params,
      });
      if (error) throw error;
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["generations"] });
    },
  });
}

export function useGenerationEstimate() {
  return useMutation({
    mutationFn: async (params: CreateGenerationParams) => {
      const { data, error } = await api.POST("/api/v1/generations/estimate", {
        body: params,
      });
      if (error) throw error;
      return data;
    },
  });
}
