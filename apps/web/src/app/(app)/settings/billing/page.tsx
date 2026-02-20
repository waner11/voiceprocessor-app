"use client";

import { useEffect, useState } from "react";
import { CreditPackCard } from "@/components/CreditPackCard";
import { paymentService } from "@/lib/api/payment/service";
import { usePayment } from "@/hooks/usePayment";
import { useAuthStore } from "@/stores/authStore";
import { CreditPack, Payment } from "@/lib/api/payment/types";
import { formatNumber } from "@/utils/formatNumber";

export default function BillingSettingsPage() {
  const [packs, setPacks] = useState<CreditPack[]>([]);
  const [isLoadingPacks, setIsLoadingPacks] = useState(true);
  const [packsError, setPacksError] = useState<string | null>(null);
  const [processingPackId, setProcessingPackId] = useState<string | null>(null);
  const [packErrors, setPackErrors] = useState<Record<string, string>>({});
  const [payments, setPayments] = useState<Payment[]>([]);
  const [isLoadingPayments, setIsLoadingPayments] = useState(true);
  const [paymentsError, setPaymentsError] = useState<string | null>(null);
  
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

  async function fetchPaymentHistory() {
    try {
      setIsLoadingPayments(true);
      setPaymentsError(null);
      const history = await paymentService.fetchPaymentHistory();
      setPayments(history);
    } catch (err) {
      console.error("Error fetching payment history:", err);
      setPaymentsError("Failed to load payment history");
    } finally {
      setIsLoadingPayments(false);
    }
  }

  useEffect(() => {
    fetchPaymentHistory();
  }, []);

    const handleBuyPack = (priceId: string) => {
      setPackErrors((prev) => ({ ...prev, [priceId]: "" }));
      setProcessingPackId(priceId);
      
      try {
        const pack = packs.find((p) => p.priceId === priceId);
       if (pack) {
         try {
           const packInfo = {
             packId: pack.id,
             name: pack.name,
             credits: pack.credits,
             price: pack.price,
           };
           localStorage.setItem("voiceprocessor_checkout_pack", JSON.stringify(packInfo));
         } catch {
         }
       }
       
        startCheckout(priceId);
      } catch (err) {
        setProcessingPackId(null);
        setPackErrors((prev) => ({
          ...prev,
          [priceId]: err instanceof Error ? err.message : "Checkout failed",
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
             {formatNumber(creditsRemaining)}
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
                 isLoading={isProcessing && processingPackId === pack.priceId}
                 error={packErrors[pack.priceId]}
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
                 {formatNumber(usage.charactersUsed)} / {formatNumber(usage.charactersLimit)}
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
               {formatNumber(usage.charactersLimit - usage.charactersUsed)} characters remaining
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
        
        {isLoadingPayments ? (
          <div className="space-y-3">
            {[1, 2, 3].map((i) => (
              <div key={i} className="h-12 bg-gray-100 dark:bg-gray-800 rounded animate-pulse" />
            ))}
          </div>
        ) : paymentsError ? (
          <div className="text-center py-8">
            <p className="text-red-600 dark:text-red-400 mb-4">{paymentsError}</p>
            <button
              onClick={() => fetchPaymentHistory()}
              className="rounded-lg bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 transition-colors"
            >
              Try Again
            </button>
          </div>
        ) : payments.length === 0 ? (
          <div className="text-center py-8">
            <p className="text-gray-500 dark:text-gray-400">No payment history yet</p>
            <p className="text-sm text-gray-400 dark:text-gray-500 mt-1">
              Your purchases will appear here
            </p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-gray-200 dark:border-gray-700">
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-500 dark:text-gray-400">Date</th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-500 dark:text-gray-400">Pack</th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-500 dark:text-gray-400">Credits</th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-500 dark:text-gray-400">Amount</th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-gray-500 dark:text-gray-400">Status</th>
                </tr>
              </thead>
              <tbody>
                {payments.map((payment) => (
                  <tr key={payment.id} className="border-b border-gray-100 dark:border-gray-800">
                    <td className="py-3 px-4 text-sm text-gray-900 dark:text-white">
                      {new Date(payment.createdAt).toLocaleDateString('en-US', {
                        year: 'numeric',
                        month: 'short',
                        day: 'numeric'
                      })}
                    </td>
                    <td className="py-3 px-4 text-sm text-gray-900 dark:text-white">
                      {payment.packName}
                    </td>
                     <td className="py-3 px-4 text-sm text-gray-900 dark:text-white">
                       +{formatNumber(payment.creditsAdded)}
                     </td>
                    <td className="py-3 px-4 text-sm text-gray-900 dark:text-white">
                      {new Intl.NumberFormat('en-US', {
                        style: 'currency',
                        currency: payment.currency.toUpperCase()
                      }).format(payment.amount)}
                    </td>
                    <td className="py-3 px-4">
                      <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
                        payment.status === 'completed' 
                          ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                          : payment.status === 'pending'
                          ? 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200'
                          : payment.status === 'refunded'
                          ? 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-200'
                          : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
                      }`}>
                        {payment.status.charAt(0).toUpperCase() + payment.status.slice(1)}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  );
}
