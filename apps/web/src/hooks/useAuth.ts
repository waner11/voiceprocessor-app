"use client";

import { useMutation } from "@tanstack/react-query";
import { useAuthStore } from "@/stores";
import { useRouter } from "next/navigation";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

interface LoginRequest {
  email: string;
  password: string;
}

interface RegisterRequest {
  email: string;
  password: string;
  name: string;
}

interface AuthResponse {
  user: {
    id: string;
    email: string;
    name: string;
    creditsRemaining?: number;
  };
  isNewUser?: boolean;
}

export interface ApiError {
   code: string;
   message: string;
   detail?: string;
   validationErrors?: { field: string; message: string }[];
 }

async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const response = await fetch(`${API_URL}${endpoint}`, {
    ...options,
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
      ...options.headers,
    },
  });

  if (!response.ok) {
    const error: ApiError = await response.json().catch(() => ({
      code: "UNKNOWN_ERROR",
      message: "An unexpected error occurred",
    }));
    throw error;
  }

  // Handle 204 No Content
  if (response.status === 204) {
    return {} as T;
  }

  return response.json();
}

export function useLogin() {
  const login = useAuthStore((state) => state.login);
  const router = useRouter();

  return useMutation({
    mutationFn: async (credentials: LoginRequest) => {
      const response = await apiRequest<AuthResponse>(
        "/api/v1/Auth/login",
        {
          method: "POST",
          body: JSON.stringify(credentials),
        }
      );
      return response;
    },
    onSuccess: (data) => {
      login(data.user, data.user.creditsRemaining);
      router.push("/dashboard");
    },
  });
}

export function useRegister() {
  const login = useAuthStore((state) => state.login);
  const router = useRouter();

  return useMutation({
    mutationFn: async (data: RegisterRequest) => {
      const response = await apiRequest<AuthResponse>(
        "/api/v1/Auth/register",
        {
          method: "POST",
          body: JSON.stringify(data),
        }
      );
      return response;
    },
    onSuccess: (data) => {
      login(data.user, data.user.creditsRemaining);
      router.push("/dashboard");
    },
  });
}

export function useLogout() {
  const logout = useAuthStore((state) => state.logout);
  const router = useRouter();

  return useMutation({
    mutationFn: async () => {
      // Call logout API - server clears cookie
      await apiRequest("/api/v1/Auth/logout", {
        method: "POST",
      }).catch(() => {
        // Ignore logout API errors - still logout locally
      });
    },
    onSettled: () => {
      logout();
      router.push("/login");
    },
  });
}

export function useRefreshToken() {
  const login = useAuthStore((state) => state.login);
  const logout = useAuthStore((state) => state.logout);

  return useMutation({
    mutationFn: async () => {
      // No refresh token in body - backend reads from cookie
      const response = await apiRequest<AuthResponse>(
        "/api/v1/Auth/refresh",
        {
          method: "POST",
          body: JSON.stringify({}),
        }
      );
      return response;
    },
    onSuccess: (data) => {
      login(data.user, data.user.creditsRemaining);
    },
    onError: () => {
      logout();
    },
  });
}

export function useCurrentUser() {
  const login = useAuthStore((state) => state.login);

  return useMutation({
    mutationFn: async () => {
      // No token needed - cookie sent automatically
      const response = await apiRequest<AuthResponse["user"]>(
        "/api/v1/Auth/me",
        {
          method: "GET",
        }
      );
      return response;
    },
    onSuccess: (user) => {
      login(user, user.creditsRemaining);
    },
  });
}
