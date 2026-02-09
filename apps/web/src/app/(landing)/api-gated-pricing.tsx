'use client';

import { useApiAccess } from '@/lib/posthog';

export function ApiGatedPricingFeature() {
  const isApiAccessEnabled = useApiAccess();

  if (!isApiAccessEnabled) {
    return null;
  }

  return (
    <li className="flex items-center gap-2">
      <svg
        className="h-5 w-5 text-green-500"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
      >
        <path
          strokeLinecap="round"
          strokeLinejoin="round"
          strokeWidth={2}
          d="M5 13l4 4L19 7"
        />
      </svg>
      <span className="text-sm text-gray-700 dark:text-gray-300">API access</span>
    </li>
  );
}
