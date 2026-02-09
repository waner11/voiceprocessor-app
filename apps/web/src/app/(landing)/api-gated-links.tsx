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
      <Link href="/api-docs" className="text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white">
        API Docs
      </Link>
    );
  }

  if (type === 'hero') {
    return (
      <Link
        href="/api-docs"
        className="rounded-lg border border-gray-300 dark:border-gray-700 px-8 py-3 text-lg text-gray-900 dark:text-white hover:bg-gray-50 dark:hover:bg-gray-800"
      >
        View API Docs
      </Link>
    );
  }

  if (type === 'footer') {
    return (
      <Link href="/api-docs" className="hover:text-gray-900 dark:hover:text-white">
        API Documentation
      </Link>
    );
  }

  return null;
}
