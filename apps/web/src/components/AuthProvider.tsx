"use client";

import { useEffect } from "react";
import { useRouter, usePathname } from "next/navigation";
import { useAuthStore } from "@/stores";

// Public routes that don't require authentication
const publicRoutes = ["/", "/login", "/signup", "/register", "/forgot-password"];

interface AuthProviderProps {
  children: React.ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const router = useRouter();
  const pathname = usePathname();
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const isLoading = useAuthStore((state) => state.isLoading);

  useEffect(() => {
    // Wait for hydration to complete
    if (isLoading) return;

    const isPublicRoute = publicRoutes.includes(pathname);

    // Redirect to login if not authenticated and on a protected route
    if (!isAuthenticated && !isPublicRoute) {
      router.push("/login");
    }
  }, [isLoading, isAuthenticated, pathname, router]);

  return <>{children}</>;
}
