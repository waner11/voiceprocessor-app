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
  token: string | null;
  creditsRemaining: number;
  isAuthenticated: boolean;
  isLoading: boolean;

  setUser: (user: User | null) => void;
  setToken: (token: string | null) => void;
  setCredits: (credits: number) => void;
  login: (user: User, token: string, creditsRemaining?: number) => void;
  logout: () => void;
  setLoading: (isLoading: boolean) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      creditsRemaining: 0,
      isAuthenticated: false,
      isLoading: true,

      setUser: (user) =>
        set({ user, isAuthenticated: !!user }),

      setToken: (token) =>
        set({ token }),

      setCredits: (credits) =>
        set({ creditsRemaining: credits }),

      login: (user, token, creditsRemaining) =>
        set({
          user,
          token,
          creditsRemaining: creditsRemaining ?? 0,
          isAuthenticated: true,
          isLoading: false
        }),

      logout: () =>
        set({ user: null, token: null, creditsRemaining: 0, isAuthenticated: false }),

      setLoading: (isLoading) =>
        set({ isLoading }),
    }),
    {
      name: "voiceprocessor-auth",
      partialize: (state) => ({
        user: state.user,
        token: state.token,
        creditsRemaining: state.creditsRemaining,
        isAuthenticated: state.isAuthenticated,
      }),
      onRehydrateStorage: () => (state) => {
        state?.setLoading(false);
      },
    }
  )
);

// Helper to get token for API calls
export function getAuthToken(): string | null {
  return useAuthStore.getState().token;
}
