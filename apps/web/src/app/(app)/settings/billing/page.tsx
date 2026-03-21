"use client";

import { useEffect, useState } from "react";
import { CreditPackCard } from "@/components/CreditPackCard";
import { paymentService } from "@/lib/api/payment/service";
import { usePayment } from "@/hooks/usePayment";
import { useUsage } from "@/hooks/useUsage";
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
  const { data: usage, isLoading: isLoadingUsage, error: usageError, refetch: refetchUsage } = useUsage();

  useEffect(() => {
    async function fetchPacks() {
      try {
        setIsLoadingPacks(true);
        const fetchedPacks = await paymentService.fetchCreditPacks();
        setPacks(fetchedPacks);
        setPacksError(null);
      } catch (error) {
        setPacksError("Failed to load credit packs");
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
      <section className="rounded-lg border border-border-subtle bg-bg-elevated p-6">
        <h2 className="text-xl font-semibold text-text-primary mb-4">
          Credit Balance
        </h2>
         <div className="flex items-baseline gap-2">
           <span className="text-4xl font-bold text-text-primary">
             {formatNumber(creditsRemaining)}
           </span>
           <span className="text-text-muted">credits</span>
         </div>
      </section>

      <section className="rounded-lg border border-border-subtle bg-bg-elevated p-6">
        <h2 className="text-xl font-semibold text-text-primary mb-6">
          Buy Credits
        </h2>

        {isLoadingPacks ? (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {[1, 2, 3].map((i) => (
              <div
                key={i}
                className="rounded-lg border border-border-subtle bg-bg-sunken p-6 h-64 animate-pulse"
              />
            ))}
          </div>
        ) : packsError ? (
          <div className="text-center py-8">
            <p className="text-state-error-text mb-4">{packsError}</p>
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
                  })
                  .finally(() => {
                    setIsLoadingPacks(false);
                  });
              }}
              className="rounded-lg bg-indigo px-4 py-2 text-white hover:bg-indigo-dark transition-colors"
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

      <section className="rounded-lg border border-border-subtle bg-bg-elevated p-6">
        <h2 className="text-xl font-semibold text-text-primary mb-6">Usage This Month</h2>

        {isLoadingUsage ? (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {[1, 2, 3].map((i) => (
              <div
                key={i}
                className="rounded-lg bg-bg-sunken p-4 h-20 animate-pulse"
              />
            ))}
          </div>
        ) : usageError ? (
          <div className="text-center py-8">
            <p className="text-state-error-text mb-4">Failed to load usage data</p>
            <button
              onClick={() => refetchUsage()}
              className="rounded-lg bg-indigo px-4 py-2 text-white hover:bg-indigo-dark transition-colors"
            >
              Try Again
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="rounded-lg bg-bg-sunken p-4">
              <p className="text-2xl font-bold text-text-primary">{formatNumber(usage?.creditsUsedThisMonth ?? 0)}</p>
              <p className="text-sm text-text-muted">Credits Used This Month</p>
            </div>
            <div className="rounded-lg bg-bg-sunken p-4">
              <p className="text-2xl font-bold text-text-primary">{formatNumber(usage?.generationsCount ?? 0)}</p>
              <p className="text-sm text-text-muted">Generations</p>
            </div>
            <div className="rounded-lg bg-bg-sunken p-4">
              <p className="text-2xl font-bold text-text-primary">{formatNumber(usage?.totalAudioMinutes ?? 0)}</p>
              <p className="text-sm text-text-muted">Audio Minutes</p>
            </div>
          </div>
        )}
      </section>

      <section className="rounded-lg border border-border-subtle bg-bg-elevated p-6">
        <h2 className="text-xl font-semibold text-text-primary mb-6">Payment Method</h2>

        <div className="flex items-center justify-between p-4 rounded-lg border border-border-subtle bg-bg-sunken">
          <div className="flex items-center gap-4">
            <div className="h-10 w-16 rounded bg-gradient-to-r from-indigo to-indigo-dark flex items-center justify-center text-white text-xs font-bold">
              VISA
            </div>
            <div>
              <p className="font-medium text-text-primary">•••• •••• •••• 4242</p>
              <p className="text-sm text-text-muted">Expires 12/2027</p>
            </div>
          </div>
          <button className="text-sm text-text-link hover:underline">
            Update
          </button>
        </div>
      </section>

      <section className="rounded-lg border border-border-subtle bg-bg-elevated p-6">
        <h2 className="text-xl font-semibold text-text-primary mb-6">Billing History</h2>
        
        {isLoadingPayments ? (
          <div className="space-y-3">
            {[1, 2, 3].map((i) => (
              <div key={i} className="h-12 bg-bg-sunken rounded animate-pulse" />
            ))}
          </div>
        ) : paymentsError ? (
          <div className="text-center py-8">
            <p className="text-state-error-text mb-4">{paymentsError}</p>
            <button
              onClick={() => fetchPaymentHistory()}
              className="rounded-lg bg-indigo px-4 py-2 text-white hover:bg-indigo-dark transition-colors"
            >
              Try Again
            </button>
          </div>
        ) : payments.length === 0 ? (
          <div className="text-center py-8">
            <p className="text-text-muted">No payment history yet</p>
            <p className="text-sm text-text-muted mt-1">
              Your purchases will appear here
            </p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-border-subtle">
                  <th className="text-left py-3 px-4 text-sm font-medium text-text-muted">Date</th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-text-muted">Pack</th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-text-muted">Credits</th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-text-muted">Amount</th>
                  <th className="text-left py-3 px-4 text-sm font-medium text-text-muted">Status</th>
                </tr>
              </thead>
              <tbody>
                {payments.map((payment) => (
                  <tr key={payment.id} className="border-b border-border-subtle">
                    <td className="py-3 px-4 text-sm text-text-primary">
                      {new Date(payment.createdAt).toLocaleDateString('en-US', {
                        year: 'numeric',
                        month: 'short',
                        day: 'numeric'
                      })}
                    </td>
                    <td className="py-3 px-4 text-sm text-text-primary">
                      {payment.packName}
                    </td>
                     <td className="py-3 px-4 text-sm text-text-primary">
                       +{formatNumber(payment.creditsAdded)}
                     </td>
                    <td className="py-3 px-4 text-sm text-text-primary">
                      {new Intl.NumberFormat('en-US', {
                        style: 'currency',
                        currency: payment.currency.toUpperCase()
                      }).format(payment.amount)}
                    </td>
                    <td className="py-3 px-4">
                      <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
                        payment.status === 'completed' 
                          ? 'bg-success-subtle text-state-success-text'
                          : payment.status === 'pending'
                          ? 'bg-warning-subtle text-state-warning-text'
                          : payment.status === 'refunded'
                          ? 'bg-bg-sunken text-text-muted'
                          : 'bg-error-subtle text-state-error-text'
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
