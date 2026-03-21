"use client";

import { useEffect } from "react";
import Link from "next/link";

export default function PaymentCancelPage() {
  useEffect(() => {
    try {
      localStorage.removeItem("voiceprocessor_checkout_pack");
    } catch {
      // Silent failure - localStorage not available
    }
  }, []);

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="max-w-2xl mx-auto">
        {/* Cancel Card */}
        <div className="rounded-lg border border-border-subtle bg-bg-elevated p-8 text-center">
          {/* Cancel Icon */}
          <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-warning-subtle">
            <svg
              className="h-12 w-12 text-state-warning-text"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </div>

          {/* Headline */}
          <h1 className="mb-4 text-3xl font-bold text-text-primary font-heading">
            Payment Cancelled
          </h1>

          {/* Message */}
          <p className="mb-8 text-lg text-text-secondary">
            No worries! Your card was not charged. You can try again anytime.
          </p>

          {/* Action Buttons */}
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link
              href="/settings/billing"
              className="rounded-lg bg-indigo px-6 py-3 text-white font-medium hover:bg-indigo-dark transition-colors"
            >
              Return to Billing
            </Link>
            <a
              href="mailto:support@voiceprocessor.com"
              className="rounded-lg border border-border-subtle px-6 py-3 text-text-secondary font-medium hover:bg-bg-sunken transition-colors"
            >
              Contact Support
            </a>
          </div>
        </div>
      </div>
    </div>
  );
}
