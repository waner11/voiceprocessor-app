"use client";

import { useState, useCallback, useRef } from "react";
import { useVoices } from "@/hooks";
import type { components } from "@/lib/api/types";
import { VoicePreviewPlayer } from "@/components/VoicePreviewPlayer/VoicePreviewPlayer";

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

  // Track stop functions for single-active-preview behavior
  const stopFnsRef = useRef<Map<string, () => void>>(new Map());

  const handlePlay = useCallback((voiceId: string) => {
    // Stop all other previews before playing this one
    stopFnsRef.current.forEach((stopFn, id) => {
      if (id !== voiceId) stopFn();
    });
  }, []);

  const registerStop = useCallback((voiceId: string, stopFn: () => void) => {
    stopFnsRef.current.set(voiceId, stopFn);
  }, []);

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
      <h1 className="mb-8 text-3xl font-bold text-text-primary">Voice Catalog</h1>

      {/* Filters */}
      <div className="mb-6 flex flex-wrap gap-4 rounded-lg bg-bg-surface border border-border-subtle p-4">
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search voices..."
          className="flex-1 min-w-[200px] rounded-lg border border-border-subtle bg-bg-elevated px-4 py-2 text-text-primary placeholder-text-muted"
        />
        <select
          value={language}
          onChange={(e) => {
            setLanguage(e.target.value);
            setPage(1);
          }}
          className="rounded-lg border border-border-subtle bg-bg-elevated px-4 py-2 text-text-primary"
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
          className="rounded-lg border border-border-subtle bg-bg-elevated px-4 py-2 text-text-primary"
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
          className="rounded-lg border border-border-subtle bg-bg-elevated px-4 py-2 text-text-primary"
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
              className="rounded-lg border border-border-subtle bg-bg-elevated p-6 animate-pulse"
            >
              <div className="flex items-center gap-4 mb-4">
                <div className="h-12 w-12 rounded-full bg-bg-sunken" />
                <div className="flex-1">
                  <div className="h-4 w-24 bg-bg-sunken rounded mb-2" />
                  <div className="h-3 w-16 bg-bg-sunken rounded" />
                </div>
              </div>
              <div className="h-3 w-full bg-bg-sunken rounded mb-2" />
              <div className="h-3 w-2/3 bg-bg-sunken rounded" />
            </div>
          ))}
        </div>
      )}

      {/* Error State */}
      {error && (
        <div className="rounded-lg border border-error bg-error-subtle p-6 text-center">
          <p className="text-state-error-text">Failed to load voices. Please try again.</p>
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
                  className="rounded-lg border border-border-subtle bg-bg-elevated p-6 hover:shadow-soft-2 transition-shadow"
                >
                  <div className="flex items-start gap-4 mb-4">
                    <div className="flex h-12 w-12 items-center justify-center rounded-full bg-indigo text-text-inverse text-lg font-medium">
                      {voice.name?.[0]?.toUpperCase() || "V"}
                    </div>
                    <div className="flex-1 min-w-0">
                      <h3 className="font-semibold text-text-primary truncate">
                        {voice.name}
                      </h3>
                      <div className="flex items-center gap-2 mt-1">
                        <span className="rounded-full bg-indigo-subtle px-2 py-0.5 text-xs font-medium text-indigo">
                          {voice.provider}
                        </span>
                        {voice.gender && (
                          <span className="text-xs text-text-muted capitalize">
                            {voice.gender}
                          </span>
                        )}
                      </div>
                    </div>
                    <VoicePreviewPlayer
                      previewUrl={voice.previewUrl}
                      onPlay={() => handlePlay(voice.id)}
                      stopRef={(stop) => registerStop(voice.id, stop)}
                    />
                  </div>

                  {voice.description && (
                    <p className="text-sm text-text-secondary line-clamp-2 mb-3">
                      {voice.description}
                    </p>
                  )}

                  <div className="flex items-center justify-between text-sm">
                    <div className="text-text-muted">
                      {voice.language && <span>{voice.language}</span>}
                      {voice.accent && <span> · {voice.accent}</span>}
                    </div>
                    <div className="text-text-secondary font-medium">
                      ${voice.costPerThousandChars?.toFixed(4)}/1k
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="rounded-lg border border-border-subtle bg-bg-elevated p-12 text-center">
              <p className="text-text-muted">No voices found matching your filters.</p>
            </div>
          )}

          {/* Pagination */}
          {data && data.totalPages && data.totalPages > 1 && (
            <div className="mt-8 flex items-center justify-center gap-2">
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

          {/* Total Count */}
          {data?.totalCount !== undefined && (
            <p className="mt-4 text-center text-sm text-text-muted">
              {data.totalCount} voice{data.totalCount !== 1 ? "s" : ""} available
            </p>
          )}
        </>
      )}
    </div>
  );
}
