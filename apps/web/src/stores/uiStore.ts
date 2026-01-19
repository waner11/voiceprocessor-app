import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";
import { useState, useEffect } from "react";

export type RoutingStrategy = "cost" | "quality" | "speed" | "balanced";
export type Theme = "light" | "dark" | "system";

interface UIState {
  selectedVoice: string | null;
  routingStrategy: RoutingStrategy;
  isGenerating: boolean;
  favoriteVoices: string[];
  theme: Theme;

  setSelectedVoice: (voiceId: string | null) => void;
  setRoutingStrategy: (strategy: RoutingStrategy) => void;
  setIsGenerating: (isGenerating: boolean) => void;
  toggleFavorite: (voiceId: string) => void;
  isFavorite: (voiceId: string) => boolean;
  setTheme: (theme: Theme) => void;
  reset: () => void;
}

const initialState = {
  selectedVoice: null,
  routingStrategy: "balanced" as RoutingStrategy,
  isGenerating: false,
  favoriteVoices: [] as string[],
  theme: "dark" as Theme,
};

export const useUIStore = create<UIState>()(
  persist(
    (set, get) => ({
      ...initialState,

      setSelectedVoice: (voiceId) => set({ selectedVoice: voiceId }),
      setRoutingStrategy: (strategy) => set({ routingStrategy: strategy }),
      setIsGenerating: (isGenerating) => set({ isGenerating }),

      toggleFavorite: (voiceId) =>
        set((state) => ({
          favoriteVoices: state.favoriteVoices.includes(voiceId)
            ? state.favoriteVoices.filter((id) => id !== voiceId)
            : [...state.favoriteVoices, voiceId],
        })),

      isFavorite: (voiceId) => get().favoriteVoices.includes(voiceId),

      setTheme: (theme) => set({ theme }),

      reset: () => set(initialState),
    }),
    {
      name: "voiceprocessor-ui",
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({
        selectedVoice: state.selectedVoice,
        routingStrategy: state.routingStrategy,
        favoriteVoices: state.favoriteVoices,
        theme: state.theme,
      }),
    }
  )
);

// Helper to check if store is hydrated
export const useHydrated = () => {
  const [hydrated, setHydrated] = useState(false);

  useEffect(() => {
    const unsubFinishHydration = useUIStore.persist.onFinishHydration(() => {
      setHydrated(true);
    });

    // Check if already hydrated
    if (useUIStore.persist.hasHydrated()) {
      setHydrated(true);
    }

    return () => {
      unsubFinishHydration();
    };
  }, []);

  return hydrated;
};
