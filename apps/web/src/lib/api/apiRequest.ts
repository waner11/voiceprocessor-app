export interface ApiError {
  code: string;
  message: string;
  detail?: string;
  validationErrors?: { field: string; message: string }[];
}

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

export async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const response = await fetch(`${API_URL}${endpoint}`, {
    ...options,
    credentials: "include",
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
