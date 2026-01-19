"use client";

import Link from "next/link";
import { useGenerations, useUsage } from "@/hooks";
import type { components } from "@/lib/api/types";

type GenerationStatus = components["schemas"]["GenerationStatus"];

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
      <h1 className="mb-8 text-3xl font-bold text-gray-900 dark:text-white">Dashboard</h1>

      {/* Stats Cards */}
      <div className="grid gap-6 md:grid-cols-3">
        <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
          <h2 className="mb-2 text-lg font-semibold text-gray-900 dark:text-white">Credits</h2>
          {usageLoading ? (
            <div className="animate-pulse">
              <div className="h-9 w-24 bg-gray-200 dark:bg-gray-700 rounded mb-2" />
              <div className="h-4 w-32 bg-gray-200 dark:bg-gray-700 rounded" />
            </div>
          ) : (
            <>
              <p className="text-3xl font-bold text-gray-900 dark:text-white">
                {(usageData?.charactersRemaining || 0).toLocaleString()}
              </p>
              <p className="text-sm text-gray-500 dark:text-gray-400">characters remaining</p>
            </>
          )}
        </div>

        <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
          <h2 className="mb-2 text-lg font-semibold text-gray-900 dark:text-white">Generations</h2>
          {generationsLoading ? (
            <div className="animate-pulse">
              <div className="h-9 w-16 bg-gray-200 dark:bg-gray-700 rounded mb-2" />
              <div className="h-4 w-28 bg-gray-200 dark:bg-gray-700 rounded" />
            </div>
          ) : (
            <>
              <p className="text-3xl font-bold text-gray-900 dark:text-white">
                {totalGenerations.toLocaleString()}
              </p>
              <p className="text-sm text-gray-500 dark:text-gray-400">total generations</p>
            </>
          )}
        </div>

        <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
          <h2 className="mb-2 text-lg font-semibold text-gray-900 dark:text-white">Audio Duration</h2>
          {generationsLoading ? (
            <div className="animate-pulse">
              <div className="h-9 w-20 bg-gray-200 dark:bg-gray-700 rounded mb-2" />
              <div className="h-4 w-24 bg-gray-200 dark:bg-gray-700 rounded" />
            </div>
          ) : (
            <>
              <p className="text-3xl font-bold text-gray-900 dark:text-white">
                {totalAudioHours}
              </p>
              <p className="text-sm text-gray-500 dark:text-gray-400">total hours</p>
            </>
          )}
        </div>
      </div>

      {/* Recent Generations */}
      <div className="mt-8">
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-xl font-semibold text-gray-900 dark:text-white">Recent Generations</h2>
          <Link href="/generations" className="text-sm text-blue-600 dark:text-blue-400 hover:underline">
            View all
          </Link>
        </div>

        {generationsLoading ? (
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 divide-y divide-gray-200 dark:divide-gray-800">
            {[...Array(3)].map((_, i) => (
              <div key={i} className="animate-pulse flex items-center gap-4 p-4">
                <div className="h-10 w-10 bg-gray-200 dark:bg-gray-700 rounded" />
                <div className="flex-1">
                  <div className="h-4 w-32 bg-gray-200 dark:bg-gray-700 rounded mb-2" />
                  <div className="h-3 w-24 bg-gray-200 dark:bg-gray-700 rounded" />
                </div>
                <div className="h-6 w-20 bg-gray-200 dark:bg-gray-700 rounded" />
              </div>
            ))}
          </div>
        ) : generations.length > 0 ? (
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 divide-y divide-gray-200 dark:divide-gray-800">
            {generations.map((generation) => (
              <Link
                key={generation.id}
                href={`/generations/${generation.id}`}
                className="flex items-center gap-4 p-4 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
              >
                <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-gray-100 dark:bg-gray-800 text-xl">
                  {generation.status === "Completed" ? "🎧" :
                   generation.status === "Processing" ? "⏳" :
                   generation.status === "Failed" ? "❌" : "📝"}
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <p className="font-medium text-gray-900 dark:text-white truncate">
                      {generation.characterCount.toLocaleString()} characters
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
                <span className={`rounded-full px-3 py-1 text-xs font-medium ${statusColors[generation.status]}`}>
                  {generation.status}
                </span>
              </Link>
            ))}
          </div>
        ) : (
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-8 text-center text-gray-500 dark:text-gray-400">
            No generations yet. Start by creating your first audiobook.
          </div>
        )}
      </div>

      {/* Create Button */}
      <div className="mt-8 flex justify-center">
        <Link
          href="/generate"
          className="rounded-lg bg-blue-600 px-6 py-3 text-white hover:bg-blue-700 transition-colors"
        >
          Create New Generation
        </Link>
      </div>
    </div>
  );
}
