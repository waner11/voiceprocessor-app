'use client';

import Link from 'next/link';
import { useApiAccess } from '@/lib/posthog';

interface ApiGatedLinksProps {
  type: 'nav' | 'hero' | 'footer';
}

export function ApiGatedLinks({ type }: ApiGatedLinksProps) {
  const isApiAccessEnabled = useApiAccess();

  if (!isApiAccessEnabled) {
    return null;
  }

  if (type === 'nav') {
    return (
      <Link href="/api-docs" className="text-sm text-text-secondary hover:text-text-primary">
        API Docs
      </Link>
    );
  }

  if (type === 'hero') {
    return (
      <Link
        href="/api-docs"
        className="rounded-lg border border-border-subtle px-8 py-3 text-lg text-text-primary hover:bg-bg-sunken"
      >
        View API Docs
      </Link>
    );
  }

    if (type === 'footer') {
      return (
        <li>
          <Link href="/api-docs" className="hover:text-text-primary">
            API Documentation
          </Link>
        </li>
      );
    }

  return null;
}
