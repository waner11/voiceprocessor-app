"use client";

import { CreditPack } from "@/lib/api/payment/types";

interface CreditPackCardProps {
  pack: CreditPack;
  onBuy: (priceId: string) => void;
  isLoading?: boolean;
  error?: string;
}

export function CreditPackCard({
  pack,
  onBuy,
  isLoading = false,
  error,
}: CreditPackCardProps) {
  return (
    <div className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-6 space-y-4">
      <h3 className="text-xl font-semibold text-gray-900 dark:text-white">
        {pack.name}
      </h3>

      <div>
        <p className="text-3xl font-bold text-gray-900 dark:text-white">
          {pack.credits.toLocaleString()}
        </p>
        <p className="text-sm text-gray-500 dark:text-gray-400">credits</p>
      </div>

      <div>
        <p className="text-2xl font-bold text-gray-900 dark:text-white">
          ${pack.price.toFixed(2)}
        </p>
      </div>

      <p className="text-sm text-gray-600 dark:text-gray-400">
        {pack.description}
      </p>

      {error && (
        <p className="text-sm text-red-600 dark:text-red-400">{error}</p>
      )}

      <button
        onClick={() => onBuy(pack.priceId)}
        disabled={isLoading}
        className="w-full rounded-lg bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
      >
        {isLoading ? "Processing..." : "Buy Now"}
      </button>
    </div>
  );
}
