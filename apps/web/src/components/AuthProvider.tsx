"use client";

import { useEffect } from "react";
import { useRouter, usePathname } from "next/navigation";
import { useAuthStore } from "@/stores";

// Public routes that don't require authentication
const publicRoutes = ["/", "/login", "/signup", "/register", "/forgot-password"];

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

interface AuthProviderProps {
  children: React.ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const router = useRouter();
  const pathname = usePathname();
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  const isLoading = useAuthStore((state) => state.isLoading);
  const logout = useAuthStore((state) => state.logout);
  const setLoading = useAuthStore((state) => state.setLoading);
  const login = useAuthStore((state) => state.login);

  // Validate session on mount
  useEffect(() => {
    const validateSession = async () => {
      // If we think we're authenticated (from persisted state), verify with server
      if (isAuthenticated) {
        try {
          const response = await fetch(`${API_URL}/api/v1/Auth/me`, {
            credentials: 'include',
          });
          
           if (response.ok) {
             const user = await response.json();
             login(user, user.creditsRemaining); // Update user data
           } else {
             logout();
           }
         } catch {
           logout();
         }
      }
      setLoading(false);
    };

    validateSession();
   // Session validation runs once on mount — adding deps would cause infinite loops
   // eslint-disable-next-line react-hooks/exhaustive-deps
   }, []);

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
