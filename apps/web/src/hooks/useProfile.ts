"use client";

import { useMutation } from "@tanstack/react-query";
import { useAuthStore } from "@/stores";
import { useRouter } from "next/navigation";
import { apiRequest, type ApiError } from "@/lib/api/apiRequest";

interface UpdateProfileRequest {
  name: string;
}

interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

interface SetPasswordRequest {
  newPassword: string;
}

interface UpdateProfileResponse {
  id: string;
  email: string;
  name?: string;
  hasPassword: boolean;
  creditsRemaining: number;
}

export type { ApiError };

export function useUpdateProfile() {
  const user = useAuthStore((state) => state.user);
  const setUser = useAuthStore((state) => state.setUser);

  return useMutation({
    mutationFn: async (data: UpdateProfileRequest) => {
      const response = await apiRequest<UpdateProfileResponse>(
        "/api/v1/auth/profile",
        {
          method: "PUT",
          body: JSON.stringify(data),
        }
      );
      return response;
    },
    onSuccess: (data) => {
      if (user) {
        setUser({ ...user, name: data.name, hasPassword: data.hasPassword });
      }
    },
  });
}

export function useChangePassword() {
  return useMutation({
    mutationFn: async (data: ChangePasswordRequest) => {
      await apiRequest<void>("/api/v1/auth/change-password", {
        method: "POST",
        body: JSON.stringify(data),
      });
    },
  });
}

export function useSetPassword() {
  return useMutation({
    mutationFn: async (data: SetPasswordRequest) => {
      await apiRequest<void>("/api/v1/auth/set-password", {
        method: "POST",
        body: JSON.stringify(data),
      });
    },
  });
}

interface DeleteAccountRequest {
  password?: string;
}

export function useDeleteAccount() {
  const logout = useAuthStore((state) => state.logout);
  const router = useRouter();

  return useMutation({
    mutationFn: async (data?: DeleteAccountRequest) => {
      await apiRequest<void>("/api/v1/auth/account", {
        method: "DELETE",
        body: JSON.stringify(data ?? {}),
      });
    },
    onSuccess: () => {
      logout();
      router.push("/");
    },
  });
}
