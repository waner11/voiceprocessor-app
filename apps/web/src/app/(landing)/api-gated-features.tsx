'use client';

import { useApiAccess } from '@/lib/posthog';

export function ApiGatedFeatureCard() {
  const isApiAccessEnabled = useApiAccess();

  if (!isApiAccessEnabled) {
    return null;
  }

  return (
    <div className="rounded-xl border border-border-subtle bg-bg-elevated p-6 transition-shadow hover:shadow-soft-2">
      <div className="mb-4 text-4xl">🔌</div>
      <h3 className="mb-2 text-lg font-semibold text-text-primary">Developer API</h3>
      <p className="text-text-secondary">Full REST API with webhooks for seamless integration into your applications.</p>
    </div>
  );
}
