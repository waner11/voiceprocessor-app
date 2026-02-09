"use client";

import { ReactNode } from "react";
import { useRouter } from "next/navigation";
import { useApiAccess } from "@/lib/posthog/use-api-access";

interface ApiDocsGateProps {
  children: ReactNode;
}

export function ApiDocsGate({ children }: ApiDocsGateProps) {
  const router = useRouter();
  const hasAccess = useApiAccess();

  if (!hasAccess) {
    router.replace("/");
    return null;
  }

  return <>{children}</>;
}
