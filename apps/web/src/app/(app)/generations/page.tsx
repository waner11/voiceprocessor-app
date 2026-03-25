"use client";

import { useState, useEffect, useCallback } from "react";
import Link from "next/link";
import { Headphones, Clock, XCircle, FileText } from "lucide-react";
import { useGenerations } from "@/hooks";
import { useGenerationHub } from "@/hooks/useGenerationHub";
import type { components } from "@/lib/api/types";
import { formatNumber } from "@/utils/formatNumber";
import { formatCredits } from "@/utils/formatCredits";

type GenerationStatus = components["schemas"]["GenerationStatus"];

const statusOptions: { value: GenerationStatus | ""; label: string }[] = [
  { value: "", label: "All Status" },
  { value: "Completed", label: "Completed" },
  { value: "Processing", label: "Processing" },
  { value: "Pending", label: "Pending" },
  { value: "Failed", label: "Failed" },
  { value: "Cancelled", label: "Cancelled" },
];

const providerOptions: { value: string; label: string }[] = [
  { value: "", label: "All Providers" },
  { value: "ElevenLabs", label: "ElevenLabs" },
  { value: "OpenAI", label: "OpenAI" },
  { value: "GoogleCloud", label: "Google Cloud" },
  { value: "AmazonPolly", label: "Amazon Polly" },
];

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

export default function GenerationsPage() {
  const [searchInput, setSearchInput] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [status, setStatus] = useState<GenerationStatus | "">("");
  const [provider, setProvider] = useState("");
  const [page, setPage] = useState(1);

  // Debounce search input by 300ms
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(searchInput);
      setPage(1);
    }, 300);
    return () => clearTimeout(timer);
  }, [searchInput]);

  const { data, isLoading, error } = useGenerations({
    page,
    pageSize: 10,
    status: status || undefined,
    search: debouncedSearch || undefined,
    provider: provider || undefined,
  });
  useGenerationHub();

  const generations = data?.items || [];

  const handleClearSearch = useCallback(() => {
    setSearchInput("");
    setDebouncedSearch("");
    setPage(1);
  }, []);

  const handleProviderChange = useCallback((value: string) => {
    setProvider(value);
    setPage(1);
  }, []);

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

  const hasActiveFilters = debouncedSearch || status || provider;

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-8 flex items-center justify-between">
        <h1 className="text-3xl font-bold text-text-primary">Generations</h1>
        <Link
          href="/generate"
          className="rounded-lg bg-indigo px-4 py-2 text-sm text-text-inverse hover:bg-indigo-dark"
        >
          New Generation
        </Link>
      </div>

      <div className="rounded-lg border border-border-subtle bg-bg-elevated">
        {/* Filters */}
        <div className="border-b border-border-subtle p-4">
          <div className="flex gap-4">
            <div className="relative flex-1">
              <input
                type="text"
                value={searchInput}
                onChange={(e) => setSearchInput(e.target.value)}
                placeholder="Search generations..."
                className="w-full rounded-lg border border-border-subtle bg-bg-elevated px-4 py-2 pr-10 text-text-primary placeholder-text-muted"
              />
              {searchInput && (
                <button
                  type="button"
                  aria-label="Clear search"
                  onClick={handleClearSearch}
                  className="absolute right-2 top-1/2 -translate-y-1/2 rounded p-1 text-text-muted hover:text-text-primary"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd" />
                  </svg>
                </button>
              )}
            </div>
            <select
              value={provider}
              onChange={(e) => handleProviderChange(e.target.value)}
              className="rounded-lg border border-border-subtle bg-bg-elevated px-4 py-2 text-text-primary"
            >
              {providerOptions.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
            <select
              value={status}
              onChange={(e) => {
                setStatus(e.target.value as GenerationStatus | "");
                setPage(1);
              }}
              className="rounded-lg border border-border-subtle bg-bg-elevated px-4 py-2 text-text-primary"
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
                <div key={i} className="animate-pulse flex items-center gap-4 p-4 border border-border-subtle rounded-lg">
                  <div className="h-12 w-12 bg-bg-sunken rounded" />
                  <div className="flex-1">
                    <div className="h-4 w-48 bg-bg-sunken rounded mb-2" />
                    <div className="h-3 w-32 bg-bg-sunken rounded" />
                  </div>
                  <div className="h-6 w-20 bg-bg-sunken rounded" />
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Error State */}
        {error && (
          <div className="p-8 text-center">
            <p className="text-error">Failed to load generations. Please try again.</p>
          </div>
        )}

        {/* Generations List */}
        {!isLoading && !error && (
          <>
            {generations.length > 0 ? (
              <div className="divide-y divide-border-subtle">
                {generations.map((generation) => (
                  <Link
                    key={generation.id}
                    href={`/generations/${generation.id}`}
                    className="flex items-center gap-4 p-4 hover:bg-bg-sunken transition-colors"
                  >
                    {/* Status Icon */}
                    <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-bg-sunken">
                      {generation.status === "Completed" ? <Headphones className="w-5 h-5 text-success" /> :
                       generation.status === "Processing" ? <Clock className="w-5 h-5 text-indigo" /> :
                       generation.status === "Failed" ? <XCircle className="w-5 h-5 text-error" /> :
                       <FileText className="w-5 h-5 text-text-muted" />}
                    </div>

                    {/* Info */}
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

                    {/* Progress (for in-progress generations) */}
                    {generation.status !== "Completed" && generation.status !== "Failed" && generation.status !== "Cancelled" && (
                      <div className="text-right">
                        <p className="text-sm font-medium text-text-primary">
                          {generation.progress}%
                        </p>
                        <div className="mt-1 h-1.5 w-24 rounded-full bg-bg-sunken">
                          <div
                            className="h-full rounded-full bg-indigo"
                            style={{ width: `${generation.progress}%` }}
                          />
                        </div>
                      </div>
                    )}

                    {/* Status Badge */}
                    <span className={`rounded-full px-3 py-1 text-xs font-medium ${statusColors[generation.status]}`}>
                      {generation.status}
                    </span>

                    {/* Credits */}
                    {generation.creditsUsed != null && (
                      <span className="text-sm text-text-secondary font-medium">
                        {formatCredits(generation.creditsUsed)}
                      </span>
                    )}
                  </Link>
                ))}
              </div>
            ) : (
              <div className="p-8 text-center text-text-muted">
                {hasActiveFilters
                  ? "No generations found matching your filters. Try adjusting your search or filters."
                  : "No generations yet. Create your first audiobook to get started."}
              </div>
            )}

            {/* Pagination */}
            {data && data.totalPages && data.totalPages > 1 && (
              <div className="border-t border-border-subtle p-4 flex items-center justify-center gap-2">
                <button
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={!data.hasPreviousPage}
                  className="rounded-lg border border-border-subtle px-4 py-2 text-sm text-text-secondary hover:bg-bg-sunken disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Previous
                </button>
                <span className="text-sm text-text-secondary">
                  Page {data.page} of {data.totalPages}
                </span>
                <button
                  onClick={() => setPage((p) => p + 1)}
                  disabled={!data.hasNextPage}
                  className="rounded-lg border border-border-subtle px-4 py-2 text-sm text-text-secondary hover:bg-bg-sunken disabled:opacity-50 disabled:cursor-not-allowed"
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
