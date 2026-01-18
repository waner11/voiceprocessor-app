import { create } from "zustand";
import { persist } from "zustand/middleware";

export type RoutingStrategy = "cost" | "quality" | "speed" | "balanced";

interface UIState {
  selectedVoice: string | null;
  routingStrategy: RoutingStrategy;
  isGenerating: boolean;

  setSelectedVoice: (voiceId: string | null) => void;
  setRoutingStrategy: (strategy: RoutingStrategy) => void;
  setIsGenerating: (isGenerating: boolean) => void;
  reset: () => void;
}

const initialState = {
  selectedVoice: null,
  routingStrategy: "balanced" as RoutingStrategy,
  isGenerating: false,
};

export const useUIStore = create<UIState>()(
  persist(
    (set) => ({
      ...initialState,

      setSelectedVoice: (voiceId) => set({ selectedVoice: voiceId }),
      setRoutingStrategy: (strategy) => set({ routingStrategy: strategy }),
      setIsGenerating: (isGenerating) => set({ isGenerating }),
      reset: () => set(initialState),
    }),
    {
      name: "voiceprocessor-ui",
      partialize: (state) => ({
        selectedVoice: state.selectedVoice,
        routingStrategy: state.routingStrategy,
      }),
    }
  )
);
