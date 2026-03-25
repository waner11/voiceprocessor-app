"use client";

import Link from "next/link";
import { Headphones, Clock, XCircle, FileText } from "lucide-react";
import { useGenerations, useUsage } from "@/hooks";
import type { components } from "@/lib/api/types";
import { formatNumber } from "@/utils/formatNumber";

type GenerationStatus = components["schemas"]["GenerationStatus"];

const statusColors: Record<GenerationStatus, string> = {
  Pending: "bg-warning-subtle text-state-warning-text",
  Analyzing: "bg-indigo-subtle text-indigo",
  Chunking: "bg-indigo-subtle text-indigo",
  Processing: "bg-indigo-subtle text-indigo",
  Merging: "bg-indigo-subtle text-indigo",
  Completed: "bg-success-subtle text-state-success-text",
  Failed: "bg-error-subtle text-state-error-text",
  Cancelled: "bg-bg-sunken text-text-muted",
};

export default function DashboardPage() {
  const { data: usageData, isLoading: usageLoading } = useUsage();
  const { data: generationsData, isLoading: generationsLoading } = useGenerations({
    page: 1,
    pageSize: 5,
  });

  const generations = generationsData?.items || [];
  const totalGenerations = generationsData?.totalCount || 0;

  // Calculate total audio duration in hours
  const totalAudioMs = generations.reduce(
    (sum, gen) => sum + (gen.audioDurationMs || 0),
    0
  );
  const totalAudioHours = (totalAudioMs / (1000 * 60 * 60)).toFixed(1);

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const formatDuration = (ms?: number | null) => {
    if (!ms) return "--";
    const seconds = Math.floor(ms / 1000);
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes}:${remainingSeconds.toString().padStart(2, "0")}`;
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="mb-8 text-3xl font-bold text-text-primary">Dashboard</h1>

      {/* Stats Cards */}
      <div className="grid gap-6 md:grid-cols-3">
        <div className="rounded-lg border border-border-subtle bg-bg-elevated p-6 transition-shadow hover:shadow-soft-2">
          <h2 className="mb-2 text-lg font-semibold text-text-primary">Credits</h2>
          {usageLoading ? (
            <div className="animate-pulse">
              <div className="h-9 w-24 bg-bg-sunken rounded mb-2" />
              <div className="h-4 w-32 bg-bg-sunken rounded" />
            </div>
           ) : (
             <>
                <p className="text-3xl font-bold text-text-primary">
                  {formatNumber(usageData?.creditsRemaining || 0)}
                </p>
               <p className="text-sm text-text-muted">credits remaining</p>
             </>
           )}
        </div>

        <div className="rounded-lg border border-border-subtle bg-bg-elevated p-6 transition-shadow hover:shadow-soft-2">
          <h2 className="mb-2 text-lg font-semibold text-text-primary">Generations</h2>
          {generationsLoading ? (
            <div className="animate-pulse">
              <div className="h-9 w-16 bg-bg-sunken rounded mb-2" />
              <div className="h-4 w-28 bg-bg-sunken rounded" />
            </div>
          ) : (
            <>
               <p className="text-3xl font-bold text-text-primary">
                 {formatNumber(totalGenerations)}
               </p>
              <p className="text-sm text-text-muted">total generations</p>
            </>
          )}
        </div>

        <div className="rounded-lg border border-border-subtle bg-bg-elevated p-6 transition-shadow hover:shadow-soft-2">
          <h2 className="mb-2 text-lg font-semibold text-text-primary">Audio Duration</h2>
          {generationsLoading ? (
            <div className="animate-pulse">
              <div className="h-9 w-20 bg-bg-sunken rounded mb-2" />
              <div className="h-4 w-24 bg-bg-sunken rounded" />
            </div>
          ) : (
            <>
              <p className="text-3xl font-bold text-text-primary">
                {totalAudioHours}
              </p>
              <p className="text-sm text-text-muted">total hours</p>
            </>
          )}
        </div>
      </div>

      {/* Recent Generations */}
      <div className="mt-8">
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-xl font-semibold text-text-primary">Recent Generations</h2>
          <Link href="/generations" className="text-sm text-text-link hover:underline">
            View all
          </Link>
        </div>

        {generationsLoading ? (
          <div className="rounded-lg border border-border-subtle bg-bg-elevated divide-y divide-border-subtle">
            {[...Array(3)].map((_, i) => (
              <div key={i} className="animate-pulse flex items-center gap-4 p-4">
                <div className="h-10 w-10 bg-bg-sunken rounded" />
                <div className="flex-1">
                  <div className="h-4 w-32 bg-bg-sunken rounded mb-2" />
                  <div className="h-3 w-24 bg-bg-sunken rounded" />
                </div>
                <div className="h-6 w-20 bg-bg-sunken rounded" />
              </div>
            ))}
          </div>
        ) : generations.length > 0 ? (
          <div className="rounded-lg border border-border-subtle bg-bg-elevated divide-y divide-border-subtle">
            {generations.map((generation) => (
              <Link
                key={generation.id}
                href={`/generations/${generation.id}`}
                className="flex items-center gap-4 p-4 hover:bg-bg-sunken hover:shadow-soft-2 transition"
              >
                <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-bg-sunken">
                  {generation.status === "Completed" ? <Headphones className="w-5 h-5 text-success" /> :
                   generation.status === "Processing" ? <Clock className="w-5 h-5 text-indigo" /> :
                   generation.status === "Failed" ? <XCircle className="w-5 h-5 text-error" /> :
                   <FileText className="w-5 h-5 text-text-muted" />}
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <p className="font-medium text-text-primary truncate">
                       {formatNumber(generation.characterCount)} characters
                     </p>
                    {generation.provider && (
                      <span className="text-xs text-text-muted">
                        via {generation.provider}
                      </span>
                    )}
                  </div>
                  <p className="text-sm text-text-muted">
                    {formatDate(generation.createdAt)}
                    {generation.audioDurationMs && ` · ${formatDuration(generation.audioDurationMs)}`}
                  </p>
                </div>
                <span className={`rounded-full px-3 py-1 text-xs font-medium transition-colors duration-300 ${statusColors[generation.status]}`}>
                  {generation.status}
                </span>
              </Link>
            ))}
          </div>
        ) : (
          <div className="rounded-lg border border-border-subtle bg-bg-elevated p-8 text-center text-text-muted">
            No generations yet. Start by creating your first audiobook.
          </div>
        )}
      </div>

      {/* Create Button */}
      <div className="mt-8 flex justify-center">
        <Link
          href="/generate"
          className="rounded-lg bg-indigo px-6 py-3 text-text-inverse hover:bg-indigo-dark active:scale-[0.98] transition"
        >
          Create New Generation
        </Link>
      </div>
    </div>
  );
}
