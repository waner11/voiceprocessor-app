"use client";

import { useFeatureFlagEnabled } from "posthog-js/react";

export function useApiAccess(): boolean {
  const isEnabled = useFeatureFlagEnabled("enable-api-access");
  return isEnabled ?? false;
}
