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
  accessToken: string;
  refreshToken: string;
  user: {
    id: string;
    email: string;
    name: string;
    creditsRemaining?: number;
  };
  isNewUser?: boolean;
}

interface ApiError {
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
      login(data.user, data.accessToken, data.user.creditsRemaining);
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
      login(data.user, data.accessToken, data.user.creditsRemaining);
      router.push("/dashboard");
    },
  });
}

export function useLogout() {
  const logout = useAuthStore((state) => state.logout);
  const token = useAuthStore((state) => state.token);
  const router = useRouter();

  return useMutation({
    mutationFn: async () => {
      if (token) {
        await apiRequest("/api/v1/Auth/logout", {
          method: "POST",
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }).catch(() => {
          // Ignore logout API errors - still logout locally
        });
      }
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
    mutationFn: async (refreshToken: string) => {
      const response = await apiRequest<AuthResponse>(
        "/api/v1/Auth/refresh",
        {
          method: "POST",
          body: JSON.stringify({ refreshToken }),
        }
      );
      return response;
    },
    onSuccess: (data) => {
      login(data.user, data.accessToken);
    },
    onError: () => {
      logout();
    },
  });
}

export function useCurrentUser() {
  const token = useAuthStore((state) => state.token);
  const login = useAuthStore((state) => state.login);

  return useMutation({
    mutationFn: async () => {
      if (!token) throw new Error("No token");

      const response = await apiRequest<AuthResponse["user"]>(
        "/api/v1/Auth/me",
        {
          method: "GET",
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );
      return response;
    },
    onSuccess: (user) => {
      if (token) {
        login(user, token, user.creditsRemaining);
      }
    },
  });
}
