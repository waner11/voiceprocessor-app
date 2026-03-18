"use client";

import { useMutation } from "@tanstack/react-query";
import { useAuthStore } from "@/stores";
import { useRouter } from "next/navigation";
import { apiRequest, type ApiError } from "@/lib/api/apiRequest";

interface LoginRequest {
  email: string;
  password: string;
}

interface RegisterRequest {
  email: string;
  password: string;
  name: string;
}

interface ForgotPasswordRequest {
  email: string;
}

interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

interface AuthResponse {
  user: {
    id: string;
    email: string;
    name: string;
    creditsRemaining?: number;
    hasPassword: boolean;
  };
  isNewUser?: boolean;
}

export type { ApiError };

export function useLogin() {
  const login = useAuthStore((state) => state.login);
  const router = useRouter();

   return useMutation({
     mutationFn: async (credentials: LoginRequest) => {
       const response = await apiRequest<AuthResponse>(
         "/api/v1/auth/login",
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
         "/api/v1/auth/register",
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
       await apiRequest("/api/v1/auth/logout", {
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
         "/api/v1/auth/refresh",
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
         "/api/v1/auth/me",
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

export function useForgotPassword() {
  return useMutation({
    mutationFn: async (data: ForgotPasswordRequest) => {
      const response = await apiRequest<{ message: string }>(
        "/api/v1/auth/forgot-password",
        {
          method: "POST",
          body: JSON.stringify(data),
        }
      );
      return response;
    },
  });
}

export function useResetPassword() {
  return useMutation({
    mutationFn: async (data: ResetPasswordRequest) => {
      const response = await apiRequest<{ message: string }>(
        "/api/v1/auth/reset-password",
        {
          method: "POST",
          body: JSON.stringify(data),
        }
      );
      return response;
    },
  });
}
