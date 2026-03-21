"use client";

import { useState, useCallback, useRef } from "react";
import { cn } from "@/lib/utils";
import { useUIStore } from "@/stores";

export interface Voice {
  id: string;
  name: string;
  provider: "elevenlabs" | "openai" | "google" | "polly";
  language: string;
  style: string;
  gender: "male" | "female" | "neutral";
  sampleUrl?: string;
  quality: "standard" | "premium" | "ultra";
}

interface VoiceSelectorProps {
  voices: Voice[];
  selectedVoiceId: string | null;
  onSelect: (voiceId: string) => void;
  isLoading?: boolean;
  className?: string;
}

const providerColors: Record<Voice["provider"], string> = {
  elevenlabs: "bg-indigo-subtle text-indigo",
  openai: "bg-success-subtle text-state-success-text",
  google: "bg-indigo-subtle text-indigo",
  polly: "bg-warning-subtle text-state-warning-text",
};

const qualityLabels: Record<Voice["quality"], string> = {
  standard: "Standard",
  premium: "Premium",
  ultra: "Ultra HD",
};

export function VoiceSelector({
  voices,
  selectedVoiceId,
  onSelect,
  isLoading = false,
  className,
}: VoiceSelectorProps) {
  const [filter, setFilter] = useState({
    language: "",
    provider: "",
    style: "",
    search: "",
    showFavoritesOnly: false,
  });
  const [playingId, setPlayingId] = useState<string | null>(null);
  const audioRef = useRef<HTMLAudioElement | null>(null);

  const favoriteVoices = useUIStore((state) => state.favoriteVoices);
  const toggleFavorite = useUIStore((state) => state.toggleFavorite);

  const filteredVoices = voices.filter((voice) => {
    if (filter.showFavoritesOnly && !favoriteVoices.includes(voice.id))
      return false;
    if (filter.language && voice.language !== filter.language) return false;
    if (filter.provider && voice.provider !== filter.provider) return false;
    if (filter.style && voice.style !== filter.style) return false;
    if (
      filter.search &&
      !voice.name.toLowerCase().includes(filter.search.toLowerCase())
    )
      return false;
    return true;
  });

  const languages = [...new Set(voices.map((v) => v.language))].sort();
  const providers = [...new Set(voices.map((v) => v.provider))].sort();
  const styles = [...new Set(voices.map((v) => v.style))].sort();

  const handlePlaySample = useCallback(
    (voice: Voice) => {
      if (!voice.sampleUrl) return;

      if (audioRef.current) {
        audioRef.current.pause();
      }

      if (playingId === voice.id) {
        setPlayingId(null);
        return;
      }

      const audio = new Audio(voice.sampleUrl);
      audioRef.current = audio;
      setPlayingId(voice.id);

      audio.play();
      audio.onended = () => setPlayingId(null);
    },
    [playingId]
  );

  const handleToggleFavorite = useCallback(
    (e: React.MouseEvent, voiceId: string) => {
      e.stopPropagation();
      toggleFavorite(voiceId);
    },
    [toggleFavorite]
  );

  if (isLoading) {
    return (
      <div className={cn("space-y-4", className)}>
        <div className="animate-pulse space-y-4">
          <div className="h-10 bg-bg-sunken rounded" />
          <div className="grid gap-4 md:grid-cols-2">
            {[1, 2, 3, 4].map((i) => (
              <div key={i} className="h-24 bg-bg-sunken rounded-lg" />
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className={cn("space-y-4", className)}>
      {/* Filters */}
      <div className="flex flex-wrap gap-2">
        <input
          type="text"
          placeholder="Search voices..."
          value={filter.search}
          onChange={(e) => setFilter((f) => ({ ...f, search: e.target.value }))}
          className="flex-1 min-w-[150px] rounded-lg border border-border-subtle bg-bg-elevated px-3 py-2 text-sm text-text-primary placeholder-text-muted"
        />
        <select
          value={filter.language}
          onChange={(e) =>
            setFilter((f) => ({ ...f, language: e.target.value }))
          }
          className="rounded-lg border border-border-subtle bg-bg-elevated px-3 py-2 text-sm text-text-primary"
        >
          <option value="">All Languages</option>
          {languages.map((lang) => (
            <option key={lang} value={lang}>
              {lang}
            </option>
          ))}
        </select>
        <select
          value={filter.provider}
          onChange={(e) =>
            setFilter((f) => ({ ...f, provider: e.target.value }))
          }
          className="rounded-lg border border-border-subtle bg-bg-elevated px-3 py-2 text-sm text-text-primary"
        >
          <option value="">All Providers</option>
          {providers.map((provider) => (
            <option key={provider} value={provider}>
              {provider}
            </option>
          ))}
        </select>
        <select
          value={filter.style}
          onChange={(e) => setFilter((f) => ({ ...f, style: e.target.value }))}
          className="rounded-lg border border-border-subtle bg-bg-elevated px-3 py-2 text-sm text-text-primary"
        >
          <option value="">All Styles</option>
          {styles.map((style) => (
            <option key={style} value={style}>
              {style}
            </option>
          ))}
        </select>
        <button
          type="button"
          onClick={() =>
            setFilter((f) => ({ ...f, showFavoritesOnly: !f.showFavoritesOnly }))
          }
          className={cn(
            "flex items-center gap-1 rounded-lg border px-3 py-2 text-sm transition-colors",
            filter.showFavoritesOnly
              ? "bg-error-subtle border-state-error-border text-state-error-text"
              : "hover:bg-bg-sunken"
          )}
        >
          <svg
            className={cn(
              "h-4 w-4",
              filter.showFavoritesOnly ? "fill-error" : "fill-none"
            )}
            viewBox="0 0 24 24"
            stroke="currentColor"
            strokeWidth={2}
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"
            />
          </svg>
          Favorites
          {favoriteVoices.length > 0 && (
            <span className="rounded-full bg-bg-sunken px-1.5 text-xs">
              {favoriteVoices.length}
            </span>
          )}
        </button>
      </div>

      {/* Voice cards */}
      <div className="grid gap-3 md:grid-cols-2">
        {filteredVoices.length === 0 ? (
          <p className="col-span-2 text-center text-text-muted py-8">
            {filter.showFavoritesOnly
              ? "No favorite voices yet. Click the heart icon to add favorites."
              : "No voices match your filters"}
          </p>
        ) : (
          filteredVoices.map((voice) => {
            const isFavorite = favoriteVoices.includes(voice.id);
            return (
              <button
                key={voice.id}
                onClick={() => onSelect(voice.id)}
                className={cn(
                  "flex items-start gap-3 rounded-lg border p-4 text-left transition hover:shadow-soft-2",
                  selectedVoiceId === voice.id
                    ? "border-indigo bg-indigo-subtle"
                    : "hover:border-border-subtle hover:bg-bg-sunken"
                )}
              >
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <span className="font-medium truncate">{voice.name}</span>
                    <span
                      className={cn(
                        "rounded-full px-2 py-0.5 text-xs",
                        providerColors[voice.provider]
                      )}
                    >
                      {voice.provider}
                    </span>
                  </div>
                  <div className="mt-1 flex items-center gap-2 text-sm text-text-muted">
                    <span>{voice.language}</span>
                    <span>·</span>
                    <span>{voice.style}</span>
                    <span>·</span>
                    <span>{qualityLabels[voice.quality]}</span>
                  </div>
                </div>

                <div className="flex items-center gap-1">
                  {/* Favorite button */}
                  <button
                    type="button"
                    onClick={(e) => handleToggleFavorite(e, voice.id)}
                    className={cn(
                      "rounded-full p-2 transition-colors",
                      isFavorite
                        ? "text-error hover:bg-error-subtle"
                        : "text-text-muted hover:bg-bg-sunken hover:text-text-secondary"
                    )}
                    title={isFavorite ? "Remove from favorites" : "Add to favorites"}
                  >
                    <svg
                      className={cn("h-5 w-5", isFavorite && "fill-current")}
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                      strokeWidth={2}
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"
                      />
                    </svg>
                  </button>

                  {/* Play sample button */}
                  {voice.sampleUrl && (
                    <button
                      type="button"
                      onClick={(e) => {
                        e.stopPropagation();
                        handlePlaySample(voice);
                      }}
                      className={cn(
                        "rounded-full p-2 transition-colors",
                        playingId === voice.id
                          ? "bg-indigo text-white"
                          : "bg-bg-sunken hover:bg-bg-surface"
                      )}
                      aria-label={playingId === voice.id ? "Stop preview" : "Play voice preview"}
                    >
                      {playingId === voice.id ? "⏹" : "▶"}
                    </button>
                  )}
                </div>
              </button>
            );
          })
        )}
      </div>
    </div>
  );
}
