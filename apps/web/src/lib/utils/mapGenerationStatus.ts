import type { components } from "@/lib/api/types";
import type { GenerationStatus as SignalRStatus } from "@/lib/signalr/connection";

type ApiStatus = components["schemas"]["GenerationStatus"];

const STATUS_MAP: Record<ApiStatus, SignalRStatus> = {
  Pending: "queued",
  Analyzing: "processing",
  Chunking: "processing",
  Processing: "processing",
  Merging: "processing",
  Completed: "completed",
  Failed: "failed",
  Cancelled: "failed",
};

export function mapGenerationStatus(apiStatus: ApiStatus): SignalRStatus {
  return STATUS_MAP[apiStatus] ?? "queued";
}
