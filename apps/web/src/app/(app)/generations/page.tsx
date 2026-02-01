"use client";

import { useState } from "react";
import Link from "next/link";
import { useGenerations } from "@/hooks";
import type { components } from "@/lib/api/types";
import { formatNumber } from "@/utils/formatNumber";

type GenerationStatus = components["schemas"]["GenerationStatus"];

const statusOptions: { value: GenerationStatus | ""; label: string }[] = [
  { value: "", label: "All Status" },
  { value: "Completed", label: "Completed" },
  { value: "Processing", label: "Processing" },
  { value: "Pending", label: "Pending" },
  { value: "Failed", label: "Failed" },
  { value: "Cancelled", label: "Cancelled" },
];

const statusColors: Record<GenerationStatus, string> = {
  Pending: "bg-yellow-100 dark:bg-yellow-900 text-yellow-700 dark:text-yellow-300",
  Analyzing: "bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300",
  Chunking: "bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300",
  Processing: "bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300",
  Merging: "bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300",
  Completed: "bg-green-100 dark:bg-green-900 text-green-700 dark:text-green-300",
  Failed: "bg-red-100 dark:bg-red-900 text-red-700 dark:text-red-300",
  Cancelled: "bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300",
};

export default function GenerationsPage() {
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState<GenerationStatus | "">("");
  const [page, setPage] = useState(1);

  const { data, isLoading, error } = useGenerations({
    page,
    pageSize: 10,
    status: status || undefined,
  });

  const generations = data?.items || [];

  const filteredGenerations = generations.filter((gen) =>
    search ? gen.id.toLowerCase().includes(search.toLowerCase()) : true
  );

  const formatDuration = (ms?: number | null) => {
    if (!ms) return "--";
    const seconds = Math.floor(ms / 1000);
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes}:${remainingSeconds.toString().padStart(2, "0")}`;
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-8 flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Generations</h1>
        <Link
          href="/generate"
          className="rounded-lg bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700"
        >
          New Generation
        </Link>
      </div>

      <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900">
        {/* Filters */}
        <div className="border-b border-gray-200 dark:border-gray-800 p-4">
          <div className="flex gap-4">
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search by ID..."
              className="flex-1 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500"
            />
            <select
              value={status}
              onChange={(e) => {
                setStatus(e.target.value as GenerationStatus | "");
                setPage(1);
              }}
              className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-gray-900 dark:text-white"
            >
              {statusOptions.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
          </div>
        </div>

        {/* Loading State */}
        {isLoading && (
          <div className="p-8">
            <div className="space-y-4">
              {[...Array(3)].map((_, i) => (
                <div key={i} className="animate-pulse flex items-center gap-4 p-4 border border-gray-200 dark:border-gray-700 rounded-lg">
                  <div className="h-12 w-12 bg-gray-200 dark:bg-gray-700 rounded" />
                  <div className="flex-1">
                    <div className="h-4 w-48 bg-gray-200 dark:bg-gray-700 rounded mb-2" />
                    <div className="h-3 w-32 bg-gray-200 dark:bg-gray-700 rounded" />
                  </div>
                  <div className="h-6 w-20 bg-gray-200 dark:bg-gray-700 rounded" />
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Error State */}
        {error && (
          <div className="p-8 text-center">
            <p className="text-red-600 dark:text-red-400">Failed to load generations. Please try again.</p>
          </div>
        )}

        {/* Generations List */}
        {!isLoading && !error && (
          <>
            {filteredGenerations.length > 0 ? (
              <div className="divide-y divide-gray-200 dark:divide-gray-800">
                {filteredGenerations.map((generation) => (
                  <Link
                    key={generation.id}
                    href={`/generations/${generation.id}`}
                    className="flex items-center gap-4 p-4 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
                  >
                    {/* Status Icon */}
                    <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-gray-100 dark:bg-gray-800 text-2xl">
                      {generation.status === "Completed" ? "🎧" :
                       generation.status === "Processing" ? "⏳" :
                       generation.status === "Failed" ? "❌" : "📝"}
                    </div>

                    {/* Info */}
                    <div className="flex-1 min-w-0">
                       <div className="flex items-center gap-2">
                         <p className="font-medium text-gray-900 dark:text-white truncate">
                           {formatNumber(generation.characterCount)} characters
                         </p>
                        {generation.provider && (
                          <span className="text-xs text-gray-500 dark:text-gray-400">
                            via {generation.provider}
                          </span>
                        )}
                      </div>
                      <p className="text-sm text-gray-500 dark:text-gray-400">
                        {formatDate(generation.createdAt)}
                        {generation.audioDurationMs && ` · ${formatDuration(generation.audioDurationMs)}`}
                      </p>
                    </div>

                    {/* Progress (for in-progress generations) */}
                    {generation.status !== "Completed" && generation.status !== "Failed" && generation.status !== "Cancelled" && (
                      <div className="text-right">
                        <p className="text-sm font-medium text-gray-900 dark:text-white">
                          {generation.progress}%
                        </p>
                        <div className="mt-1 h-1.5 w-24 rounded-full bg-gray-200 dark:bg-gray-700">
                          <div
                            className="h-full rounded-full bg-blue-500"
                            style={{ width: `${generation.progress}%` }}
                          />
                        </div>
                      </div>
                    )}

                    {/* Status Badge */}
                    <span className={`rounded-full px-3 py-1 text-xs font-medium ${statusColors[generation.status]}`}>
                      {generation.status}
                    </span>

                    {/* Cost */}
                    {generation.actualCost !== undefined && generation.actualCost !== null && (
                      <span className="text-sm text-gray-700 dark:text-gray-300 font-medium">
                        ${generation.actualCost.toFixed(4)}
                      </span>
                    )}
                  </Link>
                ))}
              </div>
            ) : (
              <div className="p-8 text-center text-gray-500 dark:text-gray-400">
                No generations yet. Create your first audiobook to get started.
              </div>
            )}

            {/* Pagination */}
            {data && data.totalPages && data.totalPages > 1 && (
              <div className="border-t border-gray-200 dark:border-gray-800 p-4 flex items-center justify-center gap-2">
                <button
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={!data.hasPreviousPage}
                  className="rounded-lg border border-gray-200 dark:border-gray-700 px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Previous
                </button>
                <span className="text-sm text-gray-600 dark:text-gray-400">
                  Page {data.page} of {data.totalPages}
                </span>
                <button
                  onClick={() => setPage((p) => p + 1)}
                  disabled={!data.hasNextPage}
                  className="rounded-lg border border-gray-200 dark:border-gray-700 px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Next
                </button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}
