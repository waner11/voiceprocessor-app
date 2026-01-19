"use client";

import { useEffect } from "react";
import { useRouter, usePathname } from "next/navigation";
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

// Public routes that don't require authentication
const publicRoutes = ["/", "/login", "/signup", "/register", "/forgot-password"];

interface AuthProviderProps {
  children: React.ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const router = useRouter();
  const pathname = usePathname();
  const token = useAuthStore((state) => state.token);
  const logout = useAuthStore((state) => state.logout);
  const isLoading = useAuthStore((state) => state.isLoading);
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

  useEffect(() => {
    // Wait for hydration to complete
    if (isLoading) return;

    const isPublicRoute = publicRoutes.includes(pathname);

    // Check if token exists and is expired
    if (token && isTokenExpired(token)) {
      console.log("Token expired, logging out...");
      logout();

      // Redirect to login if on a protected route
      if (!isPublicRoute) {
        router.push("/login");
      }
      return;
    }

    // Redirect to login if not authenticated and on a protected route
    if (!isAuthenticated && !isPublicRoute) {
      router.push("/login");
    }
  }, [token, logout, isLoading, isAuthenticated, pathname, router]);

  // Periodically check token expiry (every minute)
  useEffect(() => {
    if (!token || isLoading) return;

    const interval = setInterval(() => {
      if (isTokenExpired(token)) {
        console.log("Token expired during session, logging out...");
        logout();

        const isPublicRoute = publicRoutes.includes(pathname);
        if (!isPublicRoute) {
          router.push("/login");
        }
      }
    }, 60000); // Check every minute

    return () => clearInterval(interval);
  }, [token, logout, isLoading, pathname, router]);

  return <>{children}</>;
}
