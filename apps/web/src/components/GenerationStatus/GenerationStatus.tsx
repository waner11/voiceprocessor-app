"use client";

import { cn } from "@/lib/utils";
import type { GenerationStatus as Status } from "@/lib/signalr";

interface GenerationStatusProps {
  generationId: string;
  status: Status;
  progress?: number;
  currentStep?: string;
  estimatedTimeRemaining?: number;
  error?: string;
  className?: string;
}

const statusColors: Record<Status, string> = {
  queued: "bg-yellow-100 text-yellow-800",
  processing: "bg-blue-100 text-blue-800",
  completed: "bg-green-100 text-green-800",
  failed: "bg-red-100 text-red-800",
  cancelled: "bg-gray-100 text-gray-800",
};

const statusLabels: Record<Status, string> = {
  queued: "Queued",
  processing: "Processing",
  completed: "Completed",
  failed: "Failed",
  cancelled: "Cancelled",
};

function formatTimeRemaining(seconds: number): string {
  if (seconds < 60) {
    return `${Math.round(seconds)}s`;
  }
  const mins = Math.floor(seconds / 60);
  const secs = Math.round(seconds % 60);
  return secs > 0 ? `${mins}m ${secs}s` : `${mins}m`;
}

export function GenerationStatus({
  generationId,
  status,
  progress = 0,
  currentStep,
  estimatedTimeRemaining,
  error,
  className,
}: GenerationStatusProps) {
  const isActive = status === "queued" || status === "processing";

  return (
    <div className={cn("rounded-lg border p-6 space-y-4", className)}>
      <div className="flex items-center justify-between">
        <h3 className="font-semibold">Status</h3>
        <span
          className={cn(
            "rounded-full px-3 py-1 text-sm font-medium",
            statusColors[status]
          )}
        >
          {statusLabels[status]}
        </span>
      </div>

      {/* Progress bar */}
      {isActive && (
        <div className="space-y-2">
          <div className="h-2 w-full overflow-hidden rounded-full bg-gray-200">
            <div
              className={cn(
                "h-full rounded-full transition-all duration-300",
                status === "queued" ? "bg-yellow-500" : "bg-blue-500"
              )}
              style={{ width: `${Math.max(progress, 2)}%` }}
            />
          </div>
          <div className="flex items-center justify-between text-sm text-gray-500">
            <span>{Math.round(progress)}%</span>
            {estimatedTimeRemaining !== undefined && (
              <span>~{formatTimeRemaining(estimatedTimeRemaining)} remaining</span>
            )}
          </div>
        </div>
      )}

      {/* Current step */}
      {currentStep && isActive && (
        <div className="flex items-center gap-2 text-sm">
          <span className="inline-block h-2 w-2 animate-pulse rounded-full bg-blue-500" />
          <span className="text-gray-600">{currentStep}</span>
        </div>
      )}

      {/* Completion message */}
      {status === "completed" && (
        <div className="rounded-lg bg-green-50 p-4">
          <p className="text-sm text-green-800">
            Generation completed successfully! Your audio is ready to play.
          </p>
        </div>
      )}

      {/* Error message */}
      {status === "failed" && error && (
        <div className="rounded-lg bg-red-50 p-4">
          <p className="text-sm font-medium text-red-800">Generation failed</p>
          <p className="mt-1 text-sm text-red-600">{error}</p>
        </div>
      )}

      {/* Generation ID (for debugging/reference) */}
      <div className="border-t pt-4">
        <p className="text-xs text-gray-400">
          ID: <code className="font-mono">{generationId}</code>
        </p>
      </div>
    </div>
  );
}
