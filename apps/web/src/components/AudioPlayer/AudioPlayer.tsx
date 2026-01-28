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
  className,
}: AudioPlayerProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const wavesurferRef = useRef<WaveSurfer | null>(null);

  const [isPlaying, setIsPlaying] = useState(false);
  const [isReady, setIsReady] = useState(false);
  const [currentTime, setCurrentTime] = useState(0);
  const [duration, setDuration] = useState(0);
  const [playbackRate, setPlaybackRate] = useState(1);

  useEffect(() => {
    if (!containerRef.current) return;

    const wavesurfer = WaveSurfer.create({
      container: containerRef.current,
      waveColor: "#d1d5db",
      progressColor: "#3b82f6",
      cursorColor: "#1d4ed8",
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
      setCurrentTime(wavesurfer.getCurrentTime());
    });

    wavesurfer.on("seeking", () => {
      setCurrentTime(wavesurfer.getCurrentTime());
    });

    wavesurfer.on("play", () => setIsPlaying(true));
    wavesurfer.on("pause", () => setIsPlaying(false));
    wavesurfer.on("finish", () => setIsPlaying(false));

    return () => {
      wavesurfer.destroy();
    };
  }, [audioUrl]);

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
          "rounded-lg bg-gray-50 overflow-hidden",
          !isReady && "animate-pulse"
        )}
      />

      {/* Controls */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <button
            onClick={() => handleSkip(-10)}
            disabled={!isReady}
            className="rounded-full p-2 hover:bg-gray-100 disabled:opacity-50"
            title="Back 10s"
          >
            ⏪
          </button>

          <button
            onClick={togglePlayPause}
            disabled={!isReady}
            className="rounded-full bg-black p-3 text-white hover:bg-gray-800 disabled:opacity-50"
          >
            {isPlaying ? "⏸" : "▶"}
          </button>

          <button
            onClick={() => handleSkip(10)}
            disabled={!isReady}
            className="rounded-full p-2 hover:bg-gray-100 disabled:opacity-50"
            title="Forward 10s"
          >
            ⏩
          </button>

          <span className="ml-2 text-sm text-gray-500 tabular-nums">
            {formatTime(currentTime)} / {formatTime(duration)}
          </span>
        </div>

        <div className="flex items-center gap-4">
          {/* Playback rate */}
          <select
            value={playbackRate}
            onChange={(e) => handleRateChange(Number(e.target.value))}
            className="rounded border px-2 py-1 text-sm"
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
                className="rounded border px-3 py-1 text-sm hover:bg-gray-50"
              >
                MP3
              </button>
              <button
                onClick={() => onDownload("wav")}
                className="rounded border px-3 py-1 text-sm hover:bg-gray-50"
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
                className="flex w-full items-center justify-between rounded p-2 text-left text-sm hover:bg-gray-50"
              >
                <span>{chapter.title}</span>
                <span className="text-gray-500 tabular-nums">
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
