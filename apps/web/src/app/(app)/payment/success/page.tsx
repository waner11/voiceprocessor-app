"use client";

import { useEffect, useState, useLayoutEffect } from "react";
import Link from "next/link";
import confetti from "canvas-confetti";
import toast from "react-hot-toast";
import { useAuthStore } from "@/stores/authStore";
import { useCurrentUser } from "@/hooks/useAuth";

interface PackInfo {
  packId: string;
  name: string;
  credits: number;
  price: number;
}

export default function PaymentSuccessPage() {
  const [packInfo, setPackInfo] = useState<PackInfo | null>(null);
  const [hasRefreshed, setHasRefreshed] = useState(false);
  const creditsRemaining = useAuthStore((state) => state.creditsRemaining);
  const { mutate: refreshUser } = useCurrentUser();

  // Read pack info from localStorage before first render
  useLayoutEffect(() => {
    try {
      const stored = localStorage.getItem("voiceprocessor_checkout_pack");
      if (stored) {
        const parsed = JSON.parse(stored) as PackInfo;
        // eslint-disable-next-line react-hooks/set-state-in-effect
        setPackInfo(parsed);
        // Clear localStorage after reading
        localStorage.removeItem("voiceprocessor_checkout_pack");
      }
    } catch {
      // Silent failure - localStorage not available or invalid JSON
    }
  }, []);

  // Refresh credits from API on mount
  useEffect(() => {
    if (!hasRefreshed) {
      refreshUser(undefined, {
        onSuccess: () => {
          setHasRefreshed(true);
        },
        onError: (error) => {
          console.error("Failed to refresh credits:", error);
        },
      });
    }
  }, [hasRefreshed, refreshUser]);

  // Show toast when pack info is available and credits refreshed
  useEffect(() => {
    if (packInfo && hasRefreshed) {
      toast.success(
        `Added ${packInfo.credits.toLocaleString()} credits to your account!`,
        { id: "credits-added" } // Prevent duplicate toasts
      );
    }
  }, [packInfo, hasRefreshed]);

  // Trigger confetti animation
  useEffect(() => {
    const duration = 3000; // 3 seconds
    const end = Date.now() + duration;

    const frame = () => {
      confetti({
        particleCount: 2,
        angle: 60,
        spread: 55,
        origin: { x: 0 },
        colors: ["#10b981", "#34d399", "#6ee7b7"],
      });
      confetti({
        particleCount: 2,
        angle: 120,
        spread: 55,
        origin: { x: 1 },
        colors: ["#10b981", "#34d399", "#6ee7b7"],
      });

      if (Date.now() < end) {
        requestAnimationFrame(frame);
      }
    };

    frame();
  }, []);

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="max-w-2xl mx-auto">
        {/* Success Card */}
        <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-8 text-center">
          {/* Success Icon */}
          <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-green-100 dark:bg-green-900">
            <svg
              className="h-12 w-12 text-green-600 dark:text-green-400"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M5 13l4 4L19 7"
              />
            </svg>
          </div>

          {/* Headline */}
          <h1 className="mb-4 text-3xl font-bold text-gray-900 dark:text-white">
            Payment Successful!
          </h1>

          {/* Pack Details or Fallback */}
          {packInfo ? (
            <div className="mb-6">
              <p className="text-lg text-gray-700 dark:text-gray-300 mb-2">
                You purchased the <span className="font-semibold">{packInfo.name}</span> pack
              </p>
              <p className="text-2xl font-bold text-green-600 dark:text-green-400">
                {packInfo.credits.toLocaleString()} credits added
              </p>
            </div>
          ) : (
            <p className="mb-6 text-lg text-gray-700 dark:text-gray-300">
              Your credits have been added to your account!
            </p>
          )}

          {/* Current Balance */}
          <div className="mb-8 rounded-lg bg-gray-50 dark:bg-gray-800 p-4">
            <p className="text-sm text-gray-500 dark:text-gray-400 mb-1">
              Current Balance
            </p>
            <p className="text-3xl font-bold text-gray-900 dark:text-white">
              {creditsRemaining.toLocaleString()}
            </p>
            <p className="text-sm text-gray-500 dark:text-gray-400">credits</p>
          </div>

          {/* Action Buttons */}
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link
              href="/dashboard"
              className="rounded-lg bg-blue-600 px-6 py-3 text-white font-medium hover:bg-blue-700 transition-colors"
            >
              Go to Dashboard
            </Link>
            <Link
              href="/settings/billing"
              className="rounded-lg border border-gray-200 dark:border-gray-700 px-6 py-3 text-gray-700 dark:text-gray-300 font-medium hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
            >
              View Billing
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
