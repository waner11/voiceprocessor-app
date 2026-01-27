import { create } from "zustand";
import { persist } from "zustand/middleware";

export interface User {
  id: string;
  email: string;
  name?: string;
  avatarUrl?: string;
}

interface AuthState {
  user: User | null;
  creditsRemaining: number;
  isAuthenticated: boolean;
  isLoading: boolean;

  setUser: (user: User | null) => void;
  setCredits: (credits: number) => void;
  login: (user: User, creditsRemaining?: number) => void;
  logout: () => void;
  setLoading: (isLoading: boolean) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      creditsRemaining: 0,
      isAuthenticated: false,
      isLoading: true,

      setUser: (user) =>
        set({ user, isAuthenticated: !!user }),

      setCredits: (credits) =>
        set({ creditsRemaining: credits }),

      login: (user, creditsRemaining) =>
        set({
          user,
          creditsRemaining: creditsRemaining ?? 0,
          isAuthenticated: true,
          isLoading: false
        }),

      logout: () =>
        set({ user: null, creditsRemaining: 0, isAuthenticated: false }),

      setLoading: (isLoading) =>
        set({ isLoading }),
    }),
    {
      name: "voiceprocessor-auth",
      partialize: (state) => ({
        user: state.user,
        creditsRemaining: state.creditsRemaining,
        isAuthenticated: state.isAuthenticated,
      }),
      onRehydrateStorage: () => (state) => {
        state?.setLoading(false);
      },
    }
  )
);


