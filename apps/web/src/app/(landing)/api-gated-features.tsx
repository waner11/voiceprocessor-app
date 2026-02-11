'use client';

import { useApiAccess } from '@/lib/posthog';

export function ApiGatedFeatureCard() {
  const isApiAccessEnabled = useApiAccess();

  if (!isApiAccessEnabled) {
    return null;
  }

  return (
    <div className="rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 transition-shadow hover:shadow-md">
      <div className="mb-4 text-4xl">🔌</div>
      <h3 className="mb-2 text-lg font-semibold text-gray-900 dark:text-white">Developer API</h3>
      <p className="text-gray-600 dark:text-gray-400">Full REST API with webhooks for seamless integration into your applications.</p>
    </div>
  );
}
