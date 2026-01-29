"use client";

import { useState } from "react";
import { useVoices } from "@/hooks";
import type { components } from "@/lib/api/types";

type Provider = components["schemas"]["Provider"];

const providers: { value: Provider | ""; label: string }[] = [
  { value: "", label: "All Providers" },
  { value: "ElevenLabs", label: "ElevenLabs" },
  { value: "OpenAI", label: "OpenAI" },
  { value: "GoogleCloud", label: "Google Cloud" },
  { value: "AmazonPolly", label: "Amazon Polly" },
  { value: "FishAudio", label: "Fish Audio" },
  { value: "Cartesia", label: "Cartesia" },
  { value: "Deepgram", label: "Deepgram" },
];

const languages = [
  { value: "", label: "All Languages" },
  { value: "en", label: "English" },
  { value: "es", label: "Spanish" },
  { value: "fr", label: "French" },
  { value: "de", label: "German" },
  { value: "it", label: "Italian" },
  { value: "pt", label: "Portuguese" },
  { value: "ja", label: "Japanese" },
  { value: "ko", label: "Korean" },
  { value: "zh", label: "Chinese" },
];

const genders = [
  { value: "", label: "All Genders" },
  { value: "male", label: "Male" },
  { value: "female", label: "Female" },
  { value: "neutral", label: "Neutral" },
];

export default function VoicesPage() {
  const [search, setSearch] = useState("");
  const [provider, setProvider] = useState<Provider | "">("");
  const [language, setLanguage] = useState("");
  const [gender, setGender] = useState("");
  const [page, setPage] = useState(1);

  const { data, isLoading, error } = useVoices({
    page,
    pageSize: 12,
    provider: provider || undefined,
    language: language || undefined,
    gender: gender || undefined,
  });

  const filteredVoices = data?.items?.filter((voice) =>
    search
      ? voice.name?.toLowerCase().includes(search.toLowerCase()) ||
        voice.description?.toLowerCase().includes(search.toLowerCase())
      : true
  );

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="mb-8 text-3xl font-bold text-gray-900 dark:text-white">Voice Catalog</h1>

      {/* Filters */}
      <div className="mb-6 flex flex-wrap gap-4">
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search voices..."
          className="flex-1 min-w-[200px] rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500"
        />
        <select
          value={language}
          onChange={(e) => {
            setLanguage(e.target.value);
            setPage(1);
          }}
          className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-gray-900 dark:text-white"
        >
          {languages.map((lang) => (
            <option key={lang.value} value={lang.value}>
              {lang.label}
            </option>
          ))}
        </select>
        <select
          value={provider}
          onChange={(e) => {
            setProvider(e.target.value as Provider | "");
            setPage(1);
          }}
          className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-gray-900 dark:text-white"
        >
          {providers.map((p) => (
            <option key={p.value} value={p.value}>
              {p.label}
            </option>
          ))}
        </select>
        <select
          value={gender}
          onChange={(e) => {
            setGender(e.target.value);
            setPage(1);
          }}
          className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-gray-900 dark:text-white"
        >
          {genders.map((g) => (
            <option key={g.value} value={g.value}>
              {g.label}
            </option>
          ))}
        </select>
      </div>

      {/* Loading State */}
      {isLoading && (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {[...Array(6)].map((_, i) => (
            <div
              key={i}
              className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 animate-pulse"
            >
              <div className="flex items-center gap-4 mb-4">
                <div className="h-12 w-12 rounded-full bg-gray-200 dark:bg-gray-700" />
                <div className="flex-1">
                  <div className="h-4 w-24 bg-gray-200 dark:bg-gray-700 rounded mb-2" />
                  <div className="h-3 w-16 bg-gray-200 dark:bg-gray-700 rounded" />
                </div>
              </div>
              <div className="h-3 w-full bg-gray-200 dark:bg-gray-700 rounded mb-2" />
              <div className="h-3 w-2/3 bg-gray-200 dark:bg-gray-700 rounded" />
            </div>
          ))}
        </div>
      )}

      {/* Error State */}
      {error && (
        <div className="rounded-lg border border-red-200 dark:border-red-800 bg-red-50 dark:bg-red-950/30 p-6 text-center">
          <p className="text-red-600 dark:text-red-400">Failed to load voices. Please try again.</p>
        </div>
      )}

      {/* Voice Grid */}
      {!isLoading && !error && (
        <>
          {filteredVoices && filteredVoices.length > 0 ? (
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              {filteredVoices.map((voice) => (
                <div
                  key={voice.id}
                  className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 hover:shadow-md transition-shadow"
                >
                  <div className="flex items-start gap-4 mb-4">
                    <div className="flex h-12 w-12 items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-purple-600 text-white text-lg font-medium">
                      {voice.name?.[0]?.toUpperCase() || "V"}
                    </div>
                    <div className="flex-1 min-w-0">
                      <h3 className="font-semibold text-gray-900 dark:text-white truncate">
                        {voice.name}
                      </h3>
                      <div className="flex items-center gap-2 mt-1">
                        <span className="rounded-full bg-blue-100 dark:bg-blue-900 px-2 py-0.5 text-xs font-medium text-blue-700 dark:text-blue-300">
                          {voice.provider}
                        </span>
                        {voice.gender && (
                          <span className="text-xs text-gray-500 dark:text-gray-400 capitalize">
                            {voice.gender}
                          </span>
                        )}
                      </div>
                    </div>
                    {voice.previewUrl && (
                      <button
                        className="rounded-full p-2 text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800"
                        title="Preview voice"
                      >
                        ▶
                      </button>
                    )}
                  </div>

                  {voice.description && (
                    <p className="text-sm text-gray-600 dark:text-gray-400 line-clamp-2 mb-3">
                      {voice.description}
                    </p>
                  )}

                  <div className="flex items-center justify-between text-sm">
                    <div className="text-gray-500 dark:text-gray-400">
                      {voice.language && <span>{voice.language}</span>}
                      {voice.accent && <span> · {voice.accent}</span>}
                    </div>
                    <div className="text-gray-700 dark:text-gray-300 font-medium">
                      ${voice.costPerThousandChars?.toFixed(4)}/1k
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-12 text-center">
              <p className="text-gray-500 dark:text-gray-400">No voices found matching your filters.</p>
            </div>
          )}

          {/* Pagination */}
          {data && data.totalPages && data.totalPages > 1 && (
            <div className="mt-8 flex items-center justify-center gap-2">
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

          {/* Total Count */}
          {data?.totalCount !== undefined && (
            <p className="mt-4 text-center text-sm text-gray-500 dark:text-gray-400">
              {data.totalCount} voice{data.totalCount !== 1 ? "s" : ""} available
            </p>
          )}
        </>
      )}
    </div>
  );
}
