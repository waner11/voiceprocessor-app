"use client";

import { useFeatureFlagEnabled } from "posthog-js/react";
import { ReactNode } from "react";

interface FeatureGateProps {
  flag: string;
  children: ReactNode;
  fallback?: ReactNode;
}

export function FeatureGate({ flag, children, fallback }: FeatureGateProps) {
  const isEnabled = useFeatureFlagEnabled(flag);

  if (isEnabled) {
    return <>{children}</>;
  }

  return <>{fallback ?? null}</>;
}
