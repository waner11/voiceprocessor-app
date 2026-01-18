"use client";

import { cn } from "@/lib/utils";
import type { RoutingStrategy } from "@/stores";

interface ProviderEstimate {
  provider: string;
  cost: number;
  duration: number;
  generationTime: number;
  quality: "standard" | "premium" | "ultra";
}

interface CostEstimateProps {
  estimates: ProviderEstimate[];
  selectedProvider?: string;
  routingStrategy: RoutingStrategy;
  creditsRemaining?: number;
  characterCount: number;
  isLoading?: boolean;
  className?: string;
}

const qualityColors = {
  standard: "bg-gray-100 text-gray-700",
  premium: "bg-blue-100 text-blue-700",
  ultra: "bg-purple-100 text-purple-700",
};

function formatDuration(minutes: number): string {
  if (minutes < 60) {
    return `${Math.round(minutes)} min`;
  }
  const hours = Math.floor(minutes / 60);
  const mins = Math.round(minutes % 60);
  return mins > 0 ? `${hours}h ${mins}m` : `${hours}h`;
}

function formatCost(cost: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(cost);
}

export function CostEstimate({
  estimates,
  selectedProvider,
  routingStrategy,
  creditsRemaining,
  characterCount,
  isLoading = false,
  className,
}: CostEstimateProps) {
  if (isLoading) {
    return (
      <div className={cn("rounded-lg border p-6", className)}>
        <div className="animate-pulse space-y-4">
          <div className="h-6 w-32 bg-gray-200 rounded" />
          <div className="space-y-2">
            <div className="h-4 bg-gray-200 rounded" />
            <div className="h-4 bg-gray-200 rounded w-3/4" />
          </div>
        </div>
      </div>
    );
  }

  if (characterCount === 0) {
    return (
      <div className={cn("rounded-lg border p-6", className)}>
        <h3 className="font-semibold mb-2">Cost Estimate</h3>
        <p className="text-sm text-gray-500">
          Enter text to see cost estimate
        </p>
      </div>
    );
  }

  const recommendedEstimate =
    estimates.find((e) => e.provider === selectedProvider) || estimates[0];

  const strategyLabels: Record<RoutingStrategy, string> = {
    cost: "Lowest Cost",
    quality: "Best Quality",
    speed: "Fastest",
    balanced: "Balanced",
  };

  return (
    <div className={cn("rounded-lg border p-6 space-y-4", className)}>
      <div className="flex items-center justify-between">
        <h3 className="font-semibold">Cost Estimate</h3>
        <span className="text-sm text-gray-500">
          {strategyLabels[routingStrategy]}
        </span>
      </div>

      {recommendedEstimate && (
        <div className="rounded-lg bg-gray-50 p-4 space-y-2">
          <div className="flex items-center justify-between">
            <span className="text-sm text-gray-500">Provider</span>
            <span className="font-medium">{recommendedEstimate.provider}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-gray-500">Cost</span>
            <span className="text-xl font-bold">
              {formatCost(recommendedEstimate.cost)}
            </span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-gray-500">Audio Duration</span>
            <span>{formatDuration(recommendedEstimate.duration)}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-gray-500">Generation Time</span>
            <span>~{formatDuration(recommendedEstimate.generationTime)}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-gray-500">Quality</span>
            <span
              className={cn(
                "rounded-full px-2 py-0.5 text-xs",
                qualityColors[recommendedEstimate.quality]
              )}
            >
              {recommendedEstimate.quality}
            </span>
          </div>
        </div>
      )}

      {estimates.length > 1 && (
        <details className="text-sm">
          <summary className="cursor-pointer text-blue-600 hover:underline">
            Compare all providers
          </summary>
          <div className="mt-2 space-y-2">
            {estimates.map((estimate) => (
              <div
                key={estimate.provider}
                className={cn(
                  "flex items-center justify-between rounded p-2",
                  estimate.provider === selectedProvider && "bg-blue-50"
                )}
              >
                <span>{estimate.provider}</span>
                <div className="flex items-center gap-4 text-gray-500">
                  <span>{formatCost(estimate.cost)}</span>
                  <span>{formatDuration(estimate.duration)}</span>
                </div>
              </div>
            ))}
          </div>
        </details>
      )}

      {creditsRemaining !== undefined && (
        <div className="border-t pt-4">
          <div className="flex items-center justify-between text-sm">
            <span className="text-gray-500">Credits Remaining</span>
            <span className="font-medium">
              {creditsRemaining.toLocaleString()} chars
            </span>
          </div>
          {recommendedEstimate && characterCount > creditsRemaining && (
            <p className="mt-2 text-sm text-red-500">
              Insufficient credits. Please upgrade your plan.
            </p>
          )}
        </div>
      )}
    </div>
  );
}
