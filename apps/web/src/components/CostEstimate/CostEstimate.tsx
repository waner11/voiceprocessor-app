"use client";

import type { components } from "@/lib/api/types";
import { formatCredits } from "@/utils/formatCredits";
import { formatNumber } from "@/utils/formatNumber";

type CostEstimateResponse = components["schemas"]["CostEstimateResponse"];

function isPremiumProvider(qualityTier: string | null | undefined): boolean {
  if (!qualityTier) return false;
  const tier = qualityTier.toLowerCase();
  return tier.includes("high") || tier.includes("premium");
}

function getLowestCost(estimates: Array<{ creditsRequired: number; isAvailable?: boolean }>): number | null {
  const available = estimates.filter((e) => e.isAvailable !== false);
  if (available.length === 0) return null;
  return Math.min(...available.map((e) => e.creditsRequired));
}

interface ProviderListProps {
  estimates: Array<{
    provider: string;
    creditsRequired: number;
    qualityTier?: string | null;
    isAvailable?: boolean;
  }>;
}

function ProviderList({ estimates }: ProviderListProps) {
  const lowestCost = getLowestCost(estimates);
  const availableCount = estimates.filter((e) => e.isAvailable !== false).length;

  return (
    <>
      {estimates.map((estimate) => {
        const isPremium = isPremiumProvider(estimate.qualityTier);
        const isBestValue = availableCount > 1 && estimate.isAvailable !== false && estimate.creditsRequired === lowestCost;
        return (
          <div
            key={estimate.provider}
            className="flex items-center justify-between rounded p-2 bg-bg-sunken"
          >
            <div className="flex flex-col gap-1">
              <span>{estimate.provider}</span>
              <div className="flex gap-1">
                {isPremium && (
                  <span className="rounded-full bg-warning-subtle text-state-warning-text px-2 py-0.5 text-xs font-medium">
                    Premium Quality (2x Credits)
                  </span>
                )}
                {isBestValue && (
                  <span className="rounded-full bg-success-subtle text-state-success-text px-2 py-0.5 text-xs font-medium">
                    Best Value
                  </span>
                )}
              </div>
            </div>
            <span>
              {formatCredits(estimate.creditsRequired)}
            </span>
          </div>
        );
      })}
    </>
  );
}

interface CostEstimateProps {
  costEstimate: CostEstimateResponse | null | undefined;
  isEstimating: boolean;
  characterCount: number;
  wordCount: number;
  selectedProvider?: string;
}

export function CostEstimate({
  costEstimate,
  isEstimating,
  characterCount,
  wordCount,
  selectedProvider,
}: CostEstimateProps) {
  return (
    <div className="rounded-xl bg-bg-elevated p-6 text-text-primary shadow-sm ring-1 ring-border-subtle">
      <h2 className="mb-4 text-lg font-semibold">Estimated Credits</h2>
      {characterCount > 0 ? (
        <div className="space-y-4">
          <div className="flex items-baseline justify-between">
            <span className="text-text-muted">Credits</span>
            <span className="text-3xl font-bold">
              {isEstimating ? (
                <span className="text-xl">...</span>
              ) : costEstimate ? (
                formatCredits(costEstimate.creditsRequired)
              ) : (
                <span className="text-text-muted">—</span>
              )}
            </span>
          </div>
          <div className="space-y-2 text-sm">
            <div className="flex justify-between">
               <span className="text-text-muted">Characters</span>
               <span>{formatNumber(costEstimate?.characterCount || characterCount)}</span>
             </div>
            <div className="flex justify-between">
              <span className="text-text-muted">Est. Duration</span>
              <span>~{Math.ceil(wordCount / 150)} min</span>
            </div>
            <div className="flex justify-between">
              <span className="text-text-muted">Provider</span>
              <span>
                {selectedProvider || costEstimate?.recommendedProvider || "Auto-select"}
              </span>
            </div>
          </div>
           {costEstimate?.providerEstimates && costEstimate.providerEstimates.length > 1 && (
              <details className="mt-4 text-sm">
                <summary className="cursor-pointer text-text-link hover:underline">
                  Compare all providers
                </summary>
                <div className="mt-2 space-y-2">
                  <ProviderList estimates={costEstimate.providerEstimates} />
                </div>
              </details>
            )}
        </div>
      ) : (
        <div className="py-4 text-center text-text-muted">
          <p>Enter text to see cost estimate</p>
        </div>
      )}
    </div>
  );
}
