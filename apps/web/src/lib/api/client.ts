import createClient, { type Middleware } from "openapi-fetch";
import type { paths } from "./types";
import { useAuthStore } from "@/stores";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

let refreshPromise: Promise<boolean> | null = null;

function logoutAndRedirect(): void {
  const { logout } = useAuthStore.getState();
  logout();
  if (typeof window !== "undefined") {
    window.location.href = "/login";
  }
}

async function attemptRefresh(): Promise<boolean> {
  if (refreshPromise) {
    return refreshPromise;
  }

  refreshPromise = (async () => {
    try {
      const response = await fetch(`${API_URL}/api/v1/Auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({}),
      });

      if (response.ok) {
        return true;
      }

      logoutAndRedirect();
      return false;
    } catch {
      logoutAndRedirect();
      return false;
    } finally {
      refreshPromise = null;
    }
  })();

  return refreshPromise;
}

async function retryRequest(request: Request): Promise<Response> {
  const retryResponse = await fetch(request.url, {
    method: request.method,
    headers: request.headers,
    body: request.method !== 'GET' && request.method !== 'HEAD' ? await request.clone().text() : undefined,
    credentials: 'include',
  });

  if (retryResponse.status === 401) {
    logoutAndRedirect();
  }

  return retryResponse;
}

const authMiddleware: Middleware = {
  async onResponse({ request, response }) {
    if (response.status === 401) {
      const refreshSuccess = await attemptRefresh();
      
      if (refreshSuccess) {
        return retryRequest(request);
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
