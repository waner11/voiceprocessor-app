"use client";

import { useEffect, useState } from "react";
import { CreditPackCard } from "@/components/CreditPackCard";
import { paymentService } from "@/lib/api/payment/service";
import { usePayment } from "@/hooks/usePayment";
import { useAuthStore } from "@/stores/authStore";
import { CreditPack } from "@/lib/api/payment/types";

export default function BillingSettingsPage() {
  const [packs, setPacks] = useState<CreditPack[]>([]);
  const [isLoadingPacks, setIsLoadingPacks] = useState(true);
  const [packsError, setPacksError] = useState<string | null>(null);
  const [processingPackId, setProcessingPackId] = useState<string | null>(null);
  const [packErrors, setPackErrors] = useState<Record<string, string>>({});
  
  const creditsRemaining = useAuthStore((state) => state.creditsRemaining);
  const { startCheckout, isProcessing, error } = usePayment();

  const usage = {
    charactersUsed: 45000,
    charactersLimit: 100000,
    generationsCount: 23,
    totalAudioMinutes: 156,
  };

  const usagePercentage = (usage.charactersUsed / usage.charactersLimit) * 100;

  useEffect(() => {
    async function fetchPacks() {
      try {
        setIsLoadingPacks(true);
        const fetchedPacks = await paymentService.fetchCreditPacks();
        setPacks(fetchedPacks);
        setPacksError(null);
      } catch (error) {
        setPacksError("Failed to load credit packs");
        console.error("Error fetching packs:", error);
      } finally {
        setIsLoadingPacks(false);
      }
    }

    fetchPacks();
  }, []);

  const handleBuyPack = (packId: string) => {
    setPackErrors((prev) => ({ ...prev, [packId]: "" }));
    setProcessingPackId(packId);
    
    try {
      startCheckout(packId);
    } catch (err) {
      setProcessingPackId(null);
      setPackErrors((prev) => ({
        ...prev,
        [packId]: err instanceof Error ? err.message : "Checkout failed",
      }));
    }
  };
  
  useEffect(() => {
    if (error && processingPackId) {
      setPackErrors((prev) => ({
        ...prev,
        [processingPackId]: error instanceof Error ? error.message : "Checkout failed",
      }));
      setProcessingPackId(null);
    }
  }, [error, processingPackId]);

  return (
    <div className="space-y-6">
      <section className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-4">
          Credit Balance
        </h2>
        <div className="flex items-baseline gap-2">
          <span className="text-4xl font-bold text-gray-900 dark:text-white">
            {creditsRemaining.toLocaleString()}
          </span>
          <span className="text-gray-500 dark:text-gray-400">credits</span>
        </div>
      </section>

      <section className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-6">
          Buy Credits
        </h2>

        {isLoadingPacks ? (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {[1, 2, 3].map((i) => (
              <div
                key={i}
                className="rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 p-6 h-64 animate-pulse"
              />
            ))}
          </div>
        ) : packsError ? (
          <div className="text-center py-8">
            <p className="text-red-600 dark:text-red-400 mb-4">{packsError}</p>
            <button
              onClick={() => {
                setPacksError(null);
                setIsLoadingPacks(true);
                paymentService.fetchCreditPacks()
                  .then((fetchedPacks) => {
                    setPacks(fetchedPacks);
                    setPacksError(null);
                  })
                  .catch((error) => {
                    setPacksError("Failed to load credit packs");
                    console.error("Error fetching packs:", error);
                  })
                  .finally(() => {
                    setIsLoadingPacks(false);
                  });
              }}
              className="rounded-lg bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 transition-colors"
            >
              Try Again
            </button>
          </div>
        ) : null}

        {!isLoadingPacks && packs.length > 0 && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {packs.map((pack) => (
              <CreditPackCard
                key={pack.id}
                pack={pack}
                onBuy={handleBuyPack}
                isLoading={isProcessing && processingPackId === pack.id}
                error={packErrors[pack.id]}
              />
            ))}
          </div>
        )}
      </section>

      <section className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-6">Usage This Month</h2>

        <div className="space-y-6">
          <div>
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Characters</span>
              <span className="text-sm text-gray-500 dark:text-gray-400">
                {usage.charactersUsed.toLocaleString()} / {usage.charactersLimit.toLocaleString()}
              </span>
            </div>
            <div className="h-3 rounded-full bg-gray-200 dark:bg-gray-700 overflow-hidden">
              <div
                className={`h-full rounded-full transition-all ${
                  usagePercentage > 90
                    ? "bg-red-500"
                    : usagePercentage > 75
                    ? "bg-yellow-500"
                    : "bg-blue-500"
                }`}
                style={{ width: `${Math.min(usagePercentage, 100)}%` }}
              />
            </div>
            <p className="mt-1 text-xs text-gray-500 dark:text-gray-400">
              {(usage.charactersLimit - usage.charactersUsed).toLocaleString()} characters remaining
            </p>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="rounded-lg bg-gray-50 dark:bg-gray-800 p-4">
              <p className="text-2xl font-bold text-gray-900 dark:text-white">{usage.generationsCount}</p>
              <p className="text-sm text-gray-500 dark:text-gray-400">Generations</p>
            </div>
            <div className="rounded-lg bg-gray-50 dark:bg-gray-800 p-4">
              <p className="text-2xl font-bold text-gray-900 dark:text-white">{usage.totalAudioMinutes}</p>
              <p className="text-sm text-gray-500 dark:text-gray-400">Audio Minutes</p>
            </div>
          </div>
        </div>
      </section>

      <section className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-6">Payment Method</h2>

        <div className="flex items-center justify-between p-4 rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800">
          <div className="flex items-center gap-4">
            <div className="h-10 w-16 rounded bg-gradient-to-r from-blue-600 to-blue-800 flex items-center justify-center text-white text-xs font-bold">
              VISA
            </div>
            <div>
              <p className="font-medium text-gray-900 dark:text-white">•••• •••• •••• 4242</p>
              <p className="text-sm text-gray-500 dark:text-gray-400">Expires 12/2027</p>
            </div>
          </div>
          <button className="text-sm text-blue-600 dark:text-blue-400 hover:underline">
            Update
          </button>
        </div>
      </section>

      <section className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-6">Billing History</h2>
        <div className="text-center py-8">
          <p className="text-gray-500 dark:text-gray-400">
            Payment history coming soon
          </p>
        </div>
      </section>
    </div>
  );
}
