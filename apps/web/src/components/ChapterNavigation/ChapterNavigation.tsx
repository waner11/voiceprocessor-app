"use client";

import { useMemo } from "react";
import { cn } from "@/lib/utils";

export interface Chapter {
  title: string;
  index: number;
  startTimeMs: number;
  endTimeMs: number;
  estimatedWordCount?: number;
}

interface ChapterNavigationProps {
  chapters: Chapter[];
  currentTimeMs: number;
  onSeek: (startTimeMs: number) => void;
  className?: string;
}

function formatTime(ms: number): string {
  const totalSeconds = Math.floor(ms / 1000);
  const mins = Math.floor(totalSeconds / 60);
  const secs = totalSeconds % 60;
  return `${mins}:${secs.toString().padStart(2, "0")}`;
}

function formatDuration(startMs: number, endMs: number): string {
  return formatTime(endMs - startMs);
}

export function ChapterNavigation({
  chapters,
  currentTimeMs,
  onSeek,
  className,
}: ChapterNavigationProps) {
  const currentChapterIndex = useMemo(() => {
    if (chapters.length === 0) return -1;
    for (let i = chapters.length - 1; i >= 0; i--) {
      if (currentTimeMs >= chapters[i].startTimeMs) {
        return i;
      }
    }
    return 0;
  }, [chapters, currentTimeMs]);

  const currentChapterProgress = useMemo(() => {
    if (currentChapterIndex < 0 || currentChapterIndex >= chapters.length) return 0;
    const chapter = chapters[currentChapterIndex];
    const chapterDuration = chapter.endTimeMs - chapter.startTimeMs;
    if (chapterDuration <= 0) return 0;
    const elapsed = currentTimeMs - chapter.startTimeMs;
    return Math.round(Math.min(100, Math.max(0, (elapsed / chapterDuration) * 100)));
  }, [chapters, currentChapterIndex, currentTimeMs]);

  if (chapters.length === 0) {
    return null;
  }

  return (
    <div className={cn("space-y-1", className)}>
      {chapters.map((chapter, idx) => {
        const isActive = idx === currentChapterIndex;

        return (
          <button
            key={chapter.index}
            onClick={() => onSeek(chapter.startTimeMs)}
            aria-current={isActive ? "true" : undefined}
            className={cn(
              "flex w-full items-center gap-3 rounded-lg px-3 py-2 text-left text-sm transition-colors",
              isActive
                ? "bg-blue-50 dark:bg-blue-900/20 text-blue-900 dark:text-blue-100 font-medium"
                : "hover:bg-gray-50 dark:hover:bg-gray-800 text-gray-700 dark:text-gray-300"
            )}
          >
            <span className="flex-shrink-0 text-xs text-gray-400 dark:text-gray-500 tabular-nums w-10">
              {formatTime(chapter.startTimeMs)}
            </span>
            <span className="flex-1 truncate">{chapter.title}</span>
            <span className="flex-shrink-0 text-xs text-gray-400 dark:text-gray-500 tabular-nums">
              {formatDuration(chapter.startTimeMs, chapter.endTimeMs)}
            </span>
          </button>
        );
      })}

      {currentChapterIndex >= 0 && (
        <div
          role="progressbar"
          aria-valuenow={currentChapterProgress}
          aria-valuemin={0}
          aria-valuemax={100}
          aria-label={`Chapter progress: ${currentChapterProgress}%`}
          className="mt-2 h-1 w-full rounded-full bg-gray-200 dark:bg-gray-700 overflow-hidden"
        >
          <div
            className="h-full rounded-full bg-blue-500 transition-all duration-300"
            style={{ width: `${currentChapterProgress}%` }}
          />
        </div>
      )}
    </div>
  );
}
