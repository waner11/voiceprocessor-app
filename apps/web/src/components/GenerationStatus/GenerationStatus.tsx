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
  queued: "bg-warning-subtle text-state-warning-text",
  processing: "bg-indigo-subtle text-indigo",
  completed: "bg-success-subtle text-state-success-text",
  failed: "bg-error-subtle text-state-error-text",
  cancelled: "bg-bg-sunken text-text-muted",
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
            "rounded-full px-3 py-1 text-sm font-medium transition-colors duration-300",
            statusColors[status]
          )}
        >
          {statusLabels[status]}
        </span>
      </div>

      {/* Progress bar */}
      {isActive && (
        <div className="space-y-2">
          <div className="h-2 w-full overflow-hidden rounded-full bg-bg-sunken">
            <div
              className={cn(
                "h-full rounded-full transition-all duration-300",
                status === "queued" ? "bg-warning" : "bg-indigo"
              )}
              style={{ width: `${Math.max(progress, 2)}%` }}
            />
          </div>
          <div className="flex items-center justify-between text-sm text-text-muted">
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
          <span className="inline-block h-2 w-2 animate-pulse rounded-full bg-indigo" />
          <span className="text-text-secondary">{currentStep}</span>
        </div>
      )}

      {/* Completion message */}
      {status === "completed" && (
        <div className="rounded-lg bg-success-subtle p-4">
          <p className="text-sm text-state-success-text">
            Generation completed successfully! Your audio is ready to play.
          </p>
        </div>
      )}

      {/* Error message */}
      {status === "failed" && error && (
        <div className="rounded-lg bg-error-subtle p-4">
          <p className="text-sm font-medium text-state-error-text">Generation failed</p>
          <p className="mt-1 text-sm text-error">{error}</p>
        </div>
      )}

    </div>
  );
}
