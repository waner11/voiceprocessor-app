"use client";

import { useEffect } from "react";
import { useAuthStore } from "@/stores";

/**
 * Decodes a JWT token and returns the payload
 */
function decodeToken(token: string): { exp?: number } | null {
  try {
    const parts = token.split(".");
    if (parts.length !== 3) return null;

    const payload = parts[1];
    const decoded = atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
    return JSON.parse(decoded);
  } catch {
    return null;
  }
}

/**
 * Checks if a JWT token is expired
 */
function isTokenExpired(token: string): boolean {
  const payload = decodeToken(token);
  if (!payload?.exp) return true;

  // exp is in seconds, Date.now() is in milliseconds
  // Add 30 second buffer to account for clock skew
  return payload.exp * 1000 < Date.now() + 30000;
}

interface AuthProviderProps {
  children: React.ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const token = useAuthStore((state) => state.token);
  const logout = useAuthStore((state) => state.logout);
  const isLoading = useAuthStore((state) => state.isLoading);

  useEffect(() => {
    // Wait for hydration to complete
    if (isLoading) return;

    // Check if token exists and is expired
    if (token && isTokenExpired(token)) {
      console.log("Token expired, logging out...");
      logout();
    }
  }, [token, logout, isLoading]);

  return <>{children}</>;
}
