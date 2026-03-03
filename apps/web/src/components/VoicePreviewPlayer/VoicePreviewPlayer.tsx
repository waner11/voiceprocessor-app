"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { cn } from "@/lib/utils";

type PlayerState = "idle" | "loading" | "playing" | "paused" | "error";

interface VoicePreviewPlayerProps {
  previewUrl?: string | null;
  onPlay?: () => void;
  stopRef?: (stop: () => void) => void;
  className?: string;
}

export function VoicePreviewPlayer({
  previewUrl,
  onPlay,
  stopRef,
  className,
}: VoicePreviewPlayerProps) {
  const [state, setState] = useState<PlayerState>("idle");
  const audioRef = useRef<HTMLAudioElement | null>(null);

  const stop = useCallback(() => {
    if (audioRef.current) {
      audioRef.current.pause();
      audioRef.current.currentTime = 0;
    }
    setState("idle");
  }, []);

  // Expose stop function to parent for single-active-preview coordination
  useEffect(() => {
    stopRef?.(stop);
  }, [stop, stopRef]);

  // Cleanup audio on unmount
  useEffect(() => {
    return () => {
      if (audioRef.current) {
        audioRef.current.pause();
        audioRef.current = null;
      }
    };
  }, []);

  const handlePlayPause = useCallback(async () => {
    if (!previewUrl) return;

    if (state === "playing") {
      audioRef.current?.pause();
      setState("paused");
      return;
    }

    if (state === "paused" && audioRef.current) {
      try {
        await audioRef.current.play();
        setState("playing");
      } catch {
        setState("error");
      }
      return;
    }

    // Start fresh playback
    onPlay?.();

    // Create new audio element
    const audio = new Audio(previewUrl);
    audioRef.current = audio;

    setState("loading");

    audio.addEventListener("playing", () => {
      setState("playing");
    });

    audio.addEventListener("pause", () => {
      // Only set paused if we're not already in idle/error state
      setState((prev) => (prev === "playing" ? "paused" : prev));
    });

    audio.addEventListener("ended", () => {
      setState("idle");
    });

    audio.addEventListener("error", () => {
      setState("error");
    });

    try {
      await audio.play();
    } catch {
      setState("error");
    }
  }, [previewUrl, state, onPlay]);

  // No preview available
  if (!previewUrl) {
    return (
      <button
        disabled
        className={cn(
          "rounded-full p-2 text-gray-300 dark:text-gray-600 cursor-not-allowed",
          className
        )}
        aria-label="No preview available"
        title="No preview available"
      >
        <PlayIcon />
      </button>
    );
  }

  // Error state
  if (state === "error") {
    return (
      <button
        disabled
        className={cn(
          "rounded-full p-2 text-red-400 dark:text-red-500 cursor-not-allowed",
          className
        )}
        aria-label="Preview unavailable"
        title="Preview unavailable"
      >
        <ErrorIcon />
      </button>
    );
  }

  // Loading state
  if (state === "loading") {
    return (
      <button
        disabled
        className={cn("rounded-full p-2 text-blue-500 animate-pulse", className)}
        aria-label="Loading preview"
        title="Loading preview"
      >
        <LoadingIcon />
      </button>
    );
  }

  // Playing state
  if (state === "playing") {
    return (
      <button
        onClick={handlePlayPause}
        className={cn(
          "rounded-full p-2 text-blue-600 dark:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-900/30",
          className
        )}
        aria-label="Pause preview"
        title="Pause preview"
      >
        <PauseIcon />
      </button>
    );
  }

  // Idle / Paused state
  return (
    <button
      onClick={handlePlayPause}
      className={cn(
        "rounded-full p-2 text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800 hover:text-blue-600 dark:hover:text-blue-400",
        className
      )}
      aria-label="Preview voice"
      title="Preview voice"
    >
      <PlayIcon />
    </button>
  );
}

// Inline SVG icons (small, no external dependency)
function PlayIcon() {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 0 20 20"
      fill="currentColor"
      className="h-4 w-4"
    >
      <path d="M6.3 2.84A1.5 1.5 0 004 4.11v11.78a1.5 1.5 0 002.3 1.27l9.344-5.891a1.5 1.5 0 000-2.538L6.3 2.841z" />
    </svg>
  );
}

function PauseIcon() {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 0 20 20"
      fill="currentColor"
      className="h-4 w-4"
    >
      <path d="M5.75 3a.75.75 0 00-.75.75v12.5c0 .414.336.75.75.75h1.5a.75.75 0 00.75-.75V3.75A.75.75 0 007.25 3h-1.5zM12.75 3a.75.75 0 00-.75.75v12.5c0 .414.336.75.75.75h1.5a.75.75 0 00.75-.75V3.75a.75.75 0 00-.75-.75h-1.5z" />
    </svg>
  );
}

function ErrorIcon() {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 0 20 20"
      fill="currentColor"
      className="h-4 w-4"
    >
      <path
        fillRule="evenodd"
        d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-5a.75.75 0 01.75.75v4.5a.75.75 0 01-1.5 0v-4.5A.75.75 0 0110 5zm0 10a1 1 0 100-2 1 1 0 000 2z"
        clipRule="evenodd"
      />
    </svg>
  );
}

function LoadingIcon() {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 0 20 20"
      fill="currentColor"
      className="h-4 w-4"
    >
      <path
        fillRule="evenodd"
        d="M15.312 11.424a5.5 5.5 0 01-9.201 2.466l-.312-.311h2.433a.75.75 0 000-1.5H4.598a.75.75 0 00-.75.75v3.634a.75.75 0 001.5 0v-2.033l.312.311a7 7 0 0011.712-3.138.75.75 0 00-1.06-.179zm-5.625-7.848a.75.75 0 00-.179-1.06 7 7 0 00-5.656 11.652l.312.311H2.433a.75.75 0 000 1.5h3.634a.75.75 0 00.75-.75V11.6a.75.75 0 00-1.5 0v2.033l-.312-.311A5.5 5.5 0 0114.206 11.1a.75.75 0 001.06.179 7 7 0 00-5.58-7.703z"
        clipRule="evenodd"
      />
    </svg>
  );
}
