"use client";

import { useState, useCallback, useRef } from "react";
import { cn } from "@/lib/utils";

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
  elevenlabs: "bg-purple-100 text-purple-700",
  openai: "bg-green-100 text-green-700",
  google: "bg-blue-100 text-blue-700",
  polly: "bg-orange-100 text-orange-700",
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
  });
  const [playingId, setPlayingId] = useState<string | null>(null);
  const audioRef = useRef<HTMLAudioElement | null>(null);

  const filteredVoices = voices.filter((voice) => {
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

  const handlePlaySample = useCallback((voice: Voice) => {
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
  }, [playingId]);

  if (isLoading) {
    return (
      <div className={cn("space-y-4", className)}>
        <div className="animate-pulse space-y-4">
          <div className="h-10 bg-gray-200 rounded" />
          <div className="grid gap-4 md:grid-cols-2">
            {[1, 2, 3, 4].map((i) => (
              <div key={i} className="h-24 bg-gray-200 rounded-lg" />
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
          className="flex-1 min-w-[150px] rounded-lg border px-3 py-2 text-sm"
        />
        <select
          value={filter.language}
          onChange={(e) =>
            setFilter((f) => ({ ...f, language: e.target.value }))
          }
          className="rounded-lg border px-3 py-2 text-sm"
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
          className="rounded-lg border px-3 py-2 text-sm"
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
          className="rounded-lg border px-3 py-2 text-sm"
        >
          <option value="">All Styles</option>
          {styles.map((style) => (
            <option key={style} value={style}>
              {style}
            </option>
          ))}
        </select>
      </div>

      {/* Voice cards */}
      <div className="grid gap-3 md:grid-cols-2">
        {filteredVoices.length === 0 ? (
          <p className="col-span-2 text-center text-gray-500 py-8">
            No voices match your filters
          </p>
        ) : (
          filteredVoices.map((voice) => (
            <button
              key={voice.id}
              onClick={() => onSelect(voice.id)}
              className={cn(
                "flex items-start gap-3 rounded-lg border p-4 text-left transition-colors",
                selectedVoiceId === voice.id
                  ? "border-blue-500 bg-blue-50"
                  : "hover:border-gray-300 hover:bg-gray-50"
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
                <div className="mt-1 flex items-center gap-2 text-sm text-gray-500">
                  <span>{voice.language}</span>
                  <span>·</span>
                  <span>{voice.style}</span>
                  <span>·</span>
                  <span>{qualityLabels[voice.quality]}</span>
                </div>
              </div>

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
                      ? "bg-blue-500 text-white"
                      : "bg-gray-100 hover:bg-gray-200"
                  )}
                >
                  {playingId === voice.id ? "⏹" : "▶"}
                </button>
              )}
            </button>
          ))
        )}
      </div>
    </div>
  );
}
