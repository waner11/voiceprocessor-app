"use client";

import { useMemo } from "react";
import { formatNumber } from "@/utils/formatNumber";

export interface DetectedChapter {
  title: string;
  startPosition: number;
  endPosition: number;
  characterCount: number;
}

// Client-side chapter detection mirroring ChapterDetectionEngine.cs patterns
const CHAPTER_PATTERNS = [
  // "Chapter 1" or "Chapter 1: Title"
  /^Chapter\s+(\d+)(?:\s*[:\-]\s*(.+))?$/gim,
  // "Ch. 1" or "Ch 1: Title"
  /^Ch\.?\s+(\d+)(?:\s*[:\-]\s*(.+))?$/gim,
  // "Part 1" or "Part 1: Title"
  /^Part\s+(\d+)(?:\s*[:\-]\s*(.+))?$/gim,
  // "Section 1" or "Section 1: Title"
  /^Section\s+(\d+)(?:\s*[:\-]\s*(.+))?$/gim,
  // Named sections
  /^(Prologue|Epilogue|Introduction|Foreword|Afterword|Preface)(?:\s*[:\-]\s*(.+))?$/gim,
];

// Divider pattern: ---, ***, ===
const DIVIDER_PATTERN = /^(\*{3,}|-{3,}|={3,})$/gm;

interface ChapterMatch {
  position: number;
  title: string;
  isDivider: boolean;
}

function detectChapters(text: string): DetectedChapter[] {
  if (!text.trim()) return [];

  const matches: ChapterMatch[] = [];

  // Find heading-based chapters
  for (const pattern of CHAPTER_PATTERNS) {
    // Reset lastIndex for each use
    const regex = new RegExp(pattern.source, pattern.flags);
    let match: RegExpExecArray | null;
    while ((match = regex.exec(text)) !== null) {
      matches.push({
        position: match.index,
        title: match[0].trim(),
        isDivider: false,
      });
    }
  }

  // Find divider-based chapters
  const dividerRegex = new RegExp(DIVIDER_PATTERN.source, DIVIDER_PATTERN.flags);
  let dividerMatch: RegExpExecArray | null;
  while ((dividerMatch = dividerRegex.exec(text)) !== null) {
    matches.push({
      position: dividerMatch.index,
      title: "",
      isDivider: true,
    });
  }

  if (matches.length === 0) return [];

  // Sort by position
  matches.sort((a, b) => a.position - b.position);

  // Build chapters from matches
  const chapters: DetectedChapter[] = [];

  // If the first match is not at position 0, there's content before it — that's chapter 1
  const firstMatchPos = matches[0].position;
  if (firstMatchPos > 0 && text.slice(0, firstMatchPos).trim().length > 0) {
    // Content before first marker is a chapter
    const chapterText = text.slice(0, firstMatchPos);
    chapters.push({
      title: matches[0].isDivider ? "Chapter 1" : "Chapter 1",
      startPosition: 0,
      endPosition: firstMatchPos,
      characterCount: chapterText.length,
    });
  }

  for (let i = 0; i < matches.length; i++) {
    const current = matches[i];
    const endPos = i < matches.length - 1 ? matches[i + 1].position : text.length;
    const chapterText = text.slice(current.position, endPos);
    const chapterIndex = chapters.length + 1;

    let title: string;
    if (current.isDivider) {
      title = `Chapter ${chapterIndex}`;
    } else {
      title = current.title;
    }

    chapters.push({
      title,
      startPosition: current.position,
      endPosition: endPos,
      characterCount: chapterText.length,
    });
  }

  return chapters;
}

interface ChapterPreviewProps {
  text: string;
}

export function ChapterPreview({ text }: ChapterPreviewProps) {
  const chapters = useMemo(() => detectChapters(text), [text]);

  if (chapters.length === 0) return null;

  return (
    <div data-testid="chapter-preview" className="mt-4 rounded-lg border border-border-subtle bg-bg-sunken p-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-sm font-semibold text-text-secondary">
          📖 {chapters.length} {chapters.length === 1 ? "chapter" : "chapters"} detected
        </h3>
      </div>
      <ul className="space-y-1.5">
        {chapters.map((chapter, index) => (
          <li
            key={index}
            className="flex items-center justify-between rounded-md bg-bg-elevated px-3 py-2 text-sm ring-1 ring-border-subtle"
          >
            <span className="font-medium text-text-primary truncate mr-3">
              {chapter.title}
            </span>
            <span className="text-xs text-text-muted whitespace-nowrap">
              {formatNumber(chapter.characterCount)} chars
            </span>
          </li>
        ))}
      </ul>
    </div>
  );
}
