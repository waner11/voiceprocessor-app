"use client";

import { CreditPack } from "@/lib/api/payment/types";
import { formatNumber } from "@/utils/formatNumber";

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
    <div className="rounded-lg border border-border-subtle bg-bg-elevated p-6 space-y-4 transition-shadow hover:shadow-soft-2">
      <h3 className="text-xl font-semibold text-text-primary">
        {pack.name}
      </h3>

       <div>
         <p className="text-3xl font-bold text-text-primary">
           {formatNumber(pack.credits)}
         </p>
         <p className="text-sm text-text-muted">credits</p>
       </div>

      <div>
        <p className="text-2xl font-bold text-text-primary">
          ${pack.price.toFixed(2)}
        </p>
      </div>

      <p className="text-sm text-text-secondary">
        {pack.description}
      </p>

      {error && (
        <p className="text-sm text-error">{error}</p>
      )}

      <button
        onClick={() => onBuy(pack.priceId)}
        disabled={isLoading}
        className="w-full rounded-lg bg-indigo px-4 py-2 text-white hover:bg-indigo-dark active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed transition"
      >
        {isLoading ? "Processing..." : "Buy Now"}
      </button>
    </div>
  );
}
