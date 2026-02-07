"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "@/lib/api";
import type { components } from "@/lib/api/types";
import { getConnectionState } from "@/lib/signalr";
import { HubConnectionState } from "@microsoft/signalr";

type GenerationStatus = components["schemas"]["GenerationStatus"];
type RoutingPreference = components["schemas"]["RoutingPreference"];

interface CreateGenerationParams {
  text: string;
  voiceId: string;
  routingPreference?: RoutingPreference;
  audioFormat?: string;
  callbackUrl?: string;
}

interface EstimateCostParams {
  text: string;
  voiceId?: string;
  provider?: components["schemas"]["Provider"];
  routingPreference?: RoutingPreference;
}

interface UseGenerationsOptions {
  page?: number;
  pageSize?: number;
  status?: GenerationStatus;
}

export function useGenerations(options: UseGenerationsOptions = {}) {
  const { page, pageSize, status } = options;

  return useQuery({
    queryKey: ["generations", { page, pageSize, status }],
    queryFn: async () => {
      const { data, error } = await api.GET("/api/v1/Generations", {
        params: {
          query: { page, pageSize, status },
        },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useGeneration(id: string) {
  return useQuery({
    queryKey: ["generation", id],
    queryFn: async () => {
      const { data, error } = await api.GET("/api/v1/Generations/{id}", {
        params: { path: { id } },
      });
      if (error) throw error;
      return data;
    },
    enabled: !!id,
    refetchInterval: (query) => {
      const data = query.state.data;
      const inProgressStatuses: GenerationStatus[] = [
        "Pending",
        "Analyzing",
        "Chunking",
        "Processing",
        "Merging",
      ];
      if (!data?.status || !inProgressStatuses.includes(data.status)) {
        return false;
      }
      const isConnected = getConnectionState() === HubConnectionState.Connected;
      return isConnected ? 10000 : 2000;
    },
  });
}

export function useCreateGeneration() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (params: CreateGenerationParams) => {
      const { data, error } = await api.POST("/api/v1/Generations", {
        body: {
          text: params.text,
          voiceId: params.voiceId,
          routingPreference: params.routingPreference,
          audioFormat: params.audioFormat,
          callbackUrl: params.callbackUrl,
        },
      });
      if (error) throw error;
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["generations"] });
    },
  });
}

export function useCancelGeneration() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      const { error } = await api.DELETE("/api/v1/Generations/{id}", {
        params: { path: { id } },
      });
      if (error) throw error;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["generations"] });
    },
  });
}

export function useEstimateCost() {
  return useMutation({
    mutationFn: async (params: EstimateCostParams) => {
      const { data, error } = await api.POST("/api/v1/Generations/estimate", {
        body: {
          text: params.text,
          voiceId: params.voiceId,
          provider: params.provider,
          routingPreference: params.routingPreference,
        },
      });
      if (error) throw error;
      return data;
    },
  });
}

export function useSubmitFeedback() {
  return useMutation({
    mutationFn: async ({
      id,
      rating,
      comment,
    }: {
      id: string;
      rating?: number;
      comment?: string;
    }) => {
      const { error } = await api.POST("/api/v1/Generations/{id}/feedback", {
        params: { path: { id } },
        body: { rating, comment },
      });
      if (error) throw error;
    },
  });
}
