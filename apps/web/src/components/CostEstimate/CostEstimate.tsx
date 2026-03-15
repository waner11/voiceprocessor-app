"use client";

import type { components } from "@/lib/api/types";
import { formatCredits } from "@/utils/formatCredits";
import { formatNumber } from "@/utils/formatNumber";

type CostEstimateResponse = components["schemas"]["CostEstimateResponse"];

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
    <div className="rounded-xl bg-gradient-to-br from-gray-800 to-gray-900 dark:from-gray-800 dark:to-gray-950 p-6 text-white shadow-sm ring-1 ring-gray-700">
      <h2 className="mb-4 text-lg font-semibold">Estimated Credits</h2>
      {characterCount > 0 ? (
        <div className="space-y-4">
          <div className="flex items-baseline justify-between">
            <span className="text-gray-400">Credits</span>
            <span className="text-3xl font-bold">
              {isEstimating ? (
                <span className="text-xl">...</span>
              ) : costEstimate ? (
                formatCredits(costEstimate.creditsRequired)
              ) : (
                <span className="text-gray-400">—</span>
              )}
            </span>
          </div>
          <div className="space-y-2 text-sm">
            <div className="flex justify-between">
               <span className="text-gray-400">Characters</span>
               <span>{formatNumber(costEstimate?.characterCount || characterCount)}</span>
             </div>
            <div className="flex justify-between">
              <span className="text-gray-400">Est. Duration</span>
              <span>~{Math.ceil(wordCount / 150)} min</span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-400">Provider</span>
              <span>
                {selectedProvider || costEstimate?.recommendedProvider || "Auto-select"}
              </span>
            </div>
          </div>
          {costEstimate?.providerEstimates && costEstimate.providerEstimates.length > 1 && (
            <details className="mt-4 text-sm">
              <summary className="cursor-pointer text-blue-400 hover:underline">
                Compare all providers
              </summary>
              <div className="mt-2 space-y-2">
                {costEstimate.providerEstimates.map((estimate) => (
                  <div
                    key={estimate.provider}
                    className="flex items-center justify-between rounded p-2 bg-gray-700/50"
                  >
                    <span>{estimate.provider}</span>
                    <span>
                      {formatCredits(estimate.creditsRequired)}
                    </span>
                  </div>
                ))}
              </div>
            </details>
          )}
        </div>
      ) : (
        <div className="py-4 text-center text-gray-400">
          <p>Enter text to see cost estimate</p>
        </div>
      )}
    </div>
  );
}
