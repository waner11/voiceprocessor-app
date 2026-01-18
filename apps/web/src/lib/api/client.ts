import createClient, { type Middleware } from "openapi-fetch";
import type { paths } from "./types";
import { getAuthToken, useAuthStore } from "@/stores";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

// Auth middleware - adds Bearer token to requests
const authMiddleware: Middleware = {
  async onRequest({ request }) {
    const token = getAuthToken();
    if (token) {
      request.headers.set("Authorization", `Bearer ${token}`);
    }
    return request;
  },
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
});

// Register middleware
api.use(authMiddleware);

export type { paths };
