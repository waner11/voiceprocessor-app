"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { useState, Suspense, type ReactNode } from "react";
import { AuthProvider } from "@/components/AuthProvider";
import { NavigationProgress } from "@/components/NavigationProgress";
import { PostHogProvider } from "@/lib/posthog";

interface ProvidersProps {
  children: ReactNode;
}

export function Providers({ children }: ProvidersProps) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 60 * 1000, // 1 minute
            refetchOnWindowFocus: false,
          },
        },
      })
  );

  return (
    <QueryClientProvider client={queryClient}>
      <PostHogProvider>
        <Suspense fallback={null}>
          <NavigationProgress />
        </Suspense>
        <AuthProvider>{children}</AuthProvider>
        <ReactQueryDevtools initialIsOpen={false} />
      </PostHogProvider>
    </QueryClientProvider>
  );
}
