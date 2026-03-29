"use client";

import { useEffect, useRef, useState, useCallback } from "react";
import WaveSurfer from "wavesurfer.js";
import { cn } from "@/lib/utils";

interface Chapter {
  id: string;
  title: string;
  startTime: number;
}

interface AudioPlayerProps {
  audioUrl: string;
  chapters?: Chapter[];
  onDownload?: (format: "mp3" | "wav") => void;
  onTimeUpdate?: (currentTimeMs: number) => void;
  seekTimeMs?: number | null;
  className?: string;
}

const PLAYBACK_RATES = [0.5, 0.75, 1, 1.25, 1.5, 1.75, 2];

function formatTime(seconds: number): string {
  const mins = Math.floor(seconds / 60);
  const secs = Math.floor(seconds % 60);
  return `${mins}:${secs.toString().padStart(2, "0")}`;
}

export function AudioPlayer({
  audioUrl,
  chapters = [],
  onDownload,
  onTimeUpdate,
  seekTimeMs,
  className,
}: AudioPlayerProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const wavesurferRef = useRef<WaveSurfer | null>(null);

  const [isPlaying, setIsPlaying] = useState(false);
  const [isReady, setIsReady] = useState(false);
  const [currentTime, setCurrentTime] = useState(0);
  const [duration, setDuration] = useState(0);
  const [playbackRate, setPlaybackRate] = useState(1);

  // Use ref to avoid stale closure in WaveSurfer event handlers
  const onTimeUpdateRef = useRef(onTimeUpdate);
  onTimeUpdateRef.current = onTimeUpdate;

  useEffect(() => {
    if (!containerRef.current) return;

    const root = document.documentElement;
    const style = getComputedStyle(root);
    const wavesurfer = WaveSurfer.create({
      container: containerRef.current,
      waveColor: style.getPropertyValue("--border-subtle").trim() || "#E5E5E5",
      progressColor: style.getPropertyValue("--indigo").trim() || "#5D79DF",
      cursorColor: style.getPropertyValue("--indigo-dark").trim() || "#4A63BF",
      barWidth: 2,
      barGap: 1,
      barRadius: 2,
      height: 80,
      normalize: true,
    });

    wavesurferRef.current = wavesurfer;

    wavesurfer.load(audioUrl);

    wavesurfer.on("ready", () => {
      setIsReady(true);
      setDuration(wavesurfer.getDuration());
    });

    wavesurfer.on("audioprocess", () => {
      const time = wavesurfer.getCurrentTime();
      setCurrentTime(time);
      onTimeUpdateRef.current?.(Math.round(time * 1000));
    });

    wavesurfer.on("seeking", () => {
      const time = wavesurfer.getCurrentTime();
      setCurrentTime(time);
      onTimeUpdateRef.current?.(Math.round(time * 1000));
    });

    wavesurfer.on("play", () => setIsPlaying(true));
    wavesurfer.on("pause", () => setIsPlaying(false));
    wavesurfer.on("finish", () => setIsPlaying(false));

    return () => {
      wavesurfer.destroy();
    };
  }, [audioUrl]);

  // External seek via seekTimeMs prop
  useEffect(() => {
    if (seekTimeMs != null && wavesurferRef.current && duration > 0) {
      const seekSeconds = seekTimeMs / 1000;
      wavesurferRef.current.seekTo(seekSeconds / duration);
    }
  }, [seekTimeMs, duration]);

  const togglePlayPause = useCallback(() => {
    wavesurferRef.current?.playPause();
  }, []);

  const handleRateChange = useCallback((rate: number) => {
    setPlaybackRate(rate);
    wavesurferRef.current?.setPlaybackRate(rate);
  }, []);

  const seekToChapter = useCallback((startTime: number) => {
    if (wavesurferRef.current && duration > 0) {
      wavesurferRef.current.seekTo(startTime / duration);
    }
  }, [duration]);

  const handleSkip = useCallback((seconds: number) => {
    if (wavesurferRef.current) {
      const newTime = Math.max(
        0,
        Math.min(duration, wavesurferRef.current.getCurrentTime() + seconds)
      );
      wavesurferRef.current.seekTo(newTime / duration);
    }
  }, [duration]);

  return (
    <div className={cn("rounded-lg border p-6 space-y-4", className)}>
      {/* Waveform */}
      <div
        ref={containerRef}
        className={cn(
          "rounded-lg bg-bg-sunken overflow-hidden",
          !isReady && "animate-pulse"
        )}
      />

      {/* Controls */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <button
            onClick={() => handleSkip(-10)}
            disabled={!isReady}
            className="rounded-full p-2 hover:bg-bg-sunken disabled:opacity-50"
            aria-label="Skip back 10 seconds"
          >
            ⏪
          </button>

          <button
            onClick={togglePlayPause}
            disabled={!isReady}
            className="rounded-full bg-indigo p-3 text-text-inverse hover:bg-indigo-dark disabled:opacity-50"
            aria-label={isPlaying ? "Pause" : "Play"}
          >
            {isPlaying ? "⏸" : "▶"}
          </button>

          <button
            onClick={() => handleSkip(10)}
            disabled={!isReady}
            className="rounded-full p-2 hover:bg-bg-sunken disabled:opacity-50"
            aria-label="Skip forward 10 seconds"
          >
            ⏩
          </button>

          <span className="ml-2 text-sm text-text-muted tabular-nums">
            {formatTime(currentTime)} / {formatTime(duration)}
          </span>
        </div>

        <div className="flex items-center gap-4">
          {/* Playback rate */}
          <select
            value={playbackRate}
            onChange={(e) => handleRateChange(Number(e.target.value))}
            className="rounded border border-border-subtle bg-bg-elevated px-2 py-1 text-sm text-text-primary"
            aria-label="Playback speed"
          >
            {PLAYBACK_RATES.map((rate) => (
              <option key={rate} value={rate}>
                {rate}x
              </option>
            ))}
          </select>

          {/* Download */}
          {onDownload && (
            <div className="flex gap-1">
              <button
                onClick={() => onDownload("mp3")}
                className="rounded border border-border-subtle px-3 py-1 text-sm text-text-secondary hover:bg-bg-sunken"
                aria-label="Download MP3"
              >
                MP3
              </button>
              <button
                onClick={() => onDownload("wav")}
                className="rounded border border-border-subtle px-3 py-1 text-sm text-text-secondary hover:bg-bg-sunken"
                aria-label="Download WAV"
              >
                WAV
              </button>
            </div>
          )}
        </div>
      </div>

      {/* Chapters */}
      {chapters.length > 0 && (
        <div className="border-t pt-4">
          <h4 className="text-sm font-medium mb-2">Chapters</h4>
          <div className="space-y-1">
            {chapters.map((chapter) => (
              <button
                key={chapter.id}
                onClick={() => seekToChapter(chapter.startTime)}
                className="flex w-full items-center justify-between rounded p-2 text-left text-sm hover:bg-bg-sunken"
              >
                <span>{chapter.title}</span>
                <span className="text-text-muted tabular-nums">
                  {formatTime(chapter.startTime)}
                </span>
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
