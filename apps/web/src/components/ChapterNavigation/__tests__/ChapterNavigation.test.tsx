import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { ChapterNavigation, type Chapter } from '../ChapterNavigation';

const mockChapters: Chapter[] = [
  {
    title: 'Introduction',
    index: 0,
    startTimeMs: 0,
    endTimeMs: 60000,
    estimatedWordCount: 150,
  },
  {
    title: 'Chapter 1: The Beginning',
    index: 1,
    startTimeMs: 60000,
    endTimeMs: 180000,
    estimatedWordCount: 300,
  },
  {
    title: 'Chapter 2: The Middle',
    index: 2,
    startTimeMs: 180000,
    endTimeMs: 300000,
    estimatedWordCount: 250,
  },
];

describe('ChapterNavigation', () => {
  const defaultProps = {
    chapters: mockChapters,
    currentTimeMs: 0,
    onSeek: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  // --- Rendering ---

  it('renders all chapters in order', () => {
    render(<ChapterNavigation {...defaultProps} />);

    expect(screen.getByText('Introduction')).toBeInTheDocument();
    expect(screen.getByText('Chapter 1: The Beginning')).toBeInTheDocument();
    expect(screen.getByText('Chapter 2: The Middle')).toBeInTheDocument();
  });

  it('renders chapter durations', () => {
    render(<ChapterNavigation {...defaultProps} />);

    // Introduction: 0ms to 60000ms = 1:00 (also appears as Chapter 1 start time)
    const oneMinEntries = screen.getAllByText('1:00');
    expect(oneMinEntries.length).toBeGreaterThanOrEqual(1);
    // Chapter 1: 60000ms to 180000ms = 2:00
    // Chapter 2: 180000ms to 300000ms = 2:00 (two instances)
    const twoMinEntries = screen.getAllByText('2:00');
    expect(twoMinEntries).toHaveLength(2);
  });

  it('renders chapter start times', () => {
    render(<ChapterNavigation {...defaultProps} />);

    // Introduction starts at 0:00
    expect(screen.getByText('0:00')).toBeInTheDocument();
    // Chapter 1 starts at 1:00 (already checked above via duration)
    // Chapter 2 starts at 3:00
    expect(screen.getByText('3:00')).toBeInTheDocument();
  });

  it('renders nothing when chapters array is empty', () => {
    const { container } = render(
      <ChapterNavigation chapters={[]} currentTimeMs={0} onSeek={vi.fn()} />
    );

    expect(container.firstChild).toBeNull();
  });

  // --- Click to seek ---

  it('calls onSeek with chapter startTimeMs when chapter is clicked', () => {
    const onSeek = vi.fn();
    render(<ChapterNavigation {...defaultProps} onSeek={onSeek} />);

    fireEvent.click(screen.getByText('Chapter 1: The Beginning'));

    expect(onSeek).toHaveBeenCalledWith(60000);
    expect(onSeek).toHaveBeenCalledTimes(1);
  });

  it('calls onSeek with 0 when first chapter is clicked', () => {
    const onSeek = vi.fn();
    render(<ChapterNavigation {...defaultProps} onSeek={onSeek} />);

    fireEvent.click(screen.getByText('Introduction'));

    expect(onSeek).toHaveBeenCalledWith(0);
  });

  // --- Current chapter highlighting ---

  it('highlights the currently playing chapter based on currentTimeMs', () => {
    // currentTimeMs = 90000 (1:30) → should be in Chapter 1 (60000-180000)
    render(<ChapterNavigation {...defaultProps} currentTimeMs={90000} />);

    const chapterButtons = screen.getAllByRole('button');
    // Chapter 1 (index 1) should have active styling
    expect(chapterButtons[1]).toHaveAttribute('aria-current', 'true');
    // Others should not
    expect(chapterButtons[0]).not.toHaveAttribute('aria-current', 'true');
    expect(chapterButtons[2]).not.toHaveAttribute('aria-current', 'true');
  });

  it('highlights first chapter when currentTimeMs is 0', () => {
    render(<ChapterNavigation {...defaultProps} currentTimeMs={0} />);

    const chapterButtons = screen.getAllByRole('button');
    expect(chapterButtons[0]).toHaveAttribute('aria-current', 'true');
  });

  it('highlights last chapter when currentTimeMs is near the end', () => {
    render(<ChapterNavigation {...defaultProps} currentTimeMs={250000} />);

    const chapterButtons = screen.getAllByRole('button');
    expect(chapterButtons[2]).toHaveAttribute('aria-current', 'true');
  });

  // --- Chapter progress ---

  it('shows progress indicator for the current chapter', () => {
    // At 120000ms (2:00), we're in Chapter 1 (60000-180000)
    // Progress = (120000 - 60000) / (180000 - 60000) = 50%
    render(<ChapterNavigation {...defaultProps} currentTimeMs={120000} />);

    const progressBar = screen.getByRole('progressbar');
    expect(progressBar).toBeInTheDocument();
    expect(progressBar).toHaveAttribute('aria-valuenow', '50');
  });
});
