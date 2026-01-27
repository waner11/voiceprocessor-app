import createClient, { type Middleware } from "openapi-fetch";
import type { paths } from "./types";
import { useAuthStore } from "@/stores";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

// Auth middleware - handles 401 responses (logout on unauthorized)
const authMiddleware: Middleware = {
  async onResponse({ response }) {
    // Handle 401 Unauthorized - logout user
    if (response.status === 401) {
      const { logout } = useAuthStore.getState();
      logout();
      // Optionally redirect to login
      if (typeof window !== "undefined") {
        window.location.href = "/login";
      }
    }
    return response;
  },
};

export const api = createClient<paths>({
  baseUrl: API_URL,
  headers: {
    "Content-Type": "application/json",
  },
  credentials: "include",
});

// Register middleware
api.use(authMiddleware);

export type { paths };
