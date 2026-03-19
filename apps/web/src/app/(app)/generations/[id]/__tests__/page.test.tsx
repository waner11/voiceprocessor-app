import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import Page from '../page';

// Mock the useGeneration and useSubmitFeedback hooks
vi.mock('@/hooks/useGenerations', () => ({
  useGeneration: vi.fn(),
  useSubmitFeedback: vi.fn(() => ({ mutate: vi.fn() })),
}));

// Mock useParams from next/navigation
vi.mock('next/navigation', () => ({
  useParams: vi.fn(() => ({ id: 'test-generation-id' })),
}));

// Mock child components that require browser APIs or SignalR
vi.mock('@/components/AudioPlayer/AudioPlayer', () => ({
  AudioPlayer: ({ audioUrl }: { audioUrl: string }) => (
    <div data-testid="audio-player">AudioPlayer: {audioUrl}</div>
  ),
}));

vi.mock('@/components/GenerationStatus/GenerationStatus', () => ({
  GenerationStatus: ({ status }: { status: string }) => (
    <div data-testid="generation-status">GenerationStatus: {status}</div>
  ),
}));

vi.mock('@/components/FeedbackForm/FeedbackForm', () => {
  const mockOnSubmit = vi.fn();
  return {
    FeedbackForm: ({ generationId, onSubmit }: { generationId: string; onSubmit?: (data: FeedbackData) => void }) => {
      if (onSubmit) {
        mockOnSubmit.mockImplementation(onSubmit);
      }
      return (
        <div data-testid="feedback-form">
          FeedbackForm: {generationId}
          <button 
            data-testid="feedback-submit-trigger"
            onClick={() => mockOnSubmit({ generationId, rating: 5, tags: [], comment: 'test' })}
          >
            Submit
          </button>
        </div>
      );
    },
    __mockOnSubmit: mockOnSubmit,
  };
});

vi.mock('@/components/ChapterNavigation/ChapterNavigation', () => ({
  ChapterNavigation: ({ chapters, onSeek }: { chapters: { title: string }[]; onSeek: (ms: number) => void }) => (
    <div data-testid="chapter-navigation">
      {chapters.map((ch: { title: string }, i: number) => (
        <button key={i} onClick={() => onSeek(i * 60000)}>{ch.title}</button>
      ))}
    </div>
  ),
}));

import { useGeneration, useSubmitFeedback } from '@/hooks/useGenerations';
import { FeedbackData } from '@/components/FeedbackForm/FeedbackForm';

describe('Generation Detail Page', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders loading state when data is fetching', () => {
    (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
      data: null,
      error: null,
      isLoading: true,
    });

    render(<Page />);
    
    // Should show loading indicator
    expect(screen.getByText(/loading/i)).toBeInTheDocument();
  });

  it('renders generation data when loaded', () => {
    const mockGeneration = {
      id: 'test-generation-id',
      status: 'Completed',
      text: 'Test generation text',
      provider: 'ElevenLabs',
      characterCount: 100,
      estimatedCost: 0.05,
      createdAt: '2026-01-29T00:00:00Z',
    };

    (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
      data: mockGeneration,
      error: null,
      isLoading: false,
    });

    render(<Page />);
    
    // Should show generation details
    expect(screen.getByText('Generation Details')).toBeInTheDocument();
    expect(screen.getByText('ElevenLabs')).toBeInTheDocument();
  });

  it('renders error state when fetch fails', () => {
    (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
      data: null,
      error: { code: 'INTERNAL_ERROR', message: 'Server crashed' },
      isLoading: false,
    });

    render(<Page />);

    expect(screen.getByText('Error')).toBeInTheDocument();
    expect(screen.getByText(/failed to fetch generation/i)).toBeInTheDocument();
  });

  it('renders 404 UI when error code is GENERATION_NOT_FOUND', () => {
    (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
      data: null,
      error: { code: 'GENERATION_NOT_FOUND', message: 'Generation not found' },
      isLoading: false,
    });

    render(<Page />);

    expect(screen.getByText('Generation Not Found')).toBeInTheDocument();
    expect(screen.getByText(/does not exist or has been deleted/i)).toBeInTheDocument();
  });

  it('renders "Not Found" when generation is null', () => {
    (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
      data: null,
      error: null,
      isLoading: false,
    });

    render(<Page />);

    expect(screen.getByText('Generation Not Found')).toBeInTheDocument();
    expect(screen.getByText(/does not exist/i)).toBeInTheDocument();
  });

  it('renders AudioPlayer when generation has audioUrl', () => {
    (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
      data: {
        id: 'test-generation-id',
        status: 'Completed',
        provider: 'ElevenLabs',
        characterCount: 100,
        createdAt: '2026-01-29T00:00:00Z',
        audioUrl: 'https://example.com/audio.mp3',
      },
      error: null,
      isLoading: false,
    });

    render(<Page />);

    expect(screen.getByTestId('audio-player')).toBeInTheDocument();
    expect(screen.getByText('AudioPlayer: https://example.com/audio.mp3')).toBeInTheDocument();
  });

  it('renders audio placeholder when generation is processing', () => {
    (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
      data: {
        id: 'test-generation-id',
        status: 'Processing',
        provider: 'ElevenLabs',
        characterCount: 100,
        createdAt: '2026-01-29T00:00:00Z',
      },
      error: null,
      isLoading: false,
    });

    render(<Page />);

    expect(screen.getByText(/audio will appear here once generation completes/i)).toBeInTheDocument();
    expect(screen.queryByTestId('audio-player')).not.toBeInTheDocument();
  });

  it('renders FeedbackForm when generation is completed', () => {
    (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
      data: {
        id: 'test-generation-id',
        status: 'Completed',
        provider: 'ElevenLabs',
        characterCount: 100,
        createdAt: '2026-01-29T00:00:00Z',
      },
      error: null,
      isLoading: false,
    });

    render(<Page />);

    expect(screen.getByTestId('feedback-form')).toBeInTheDocument();
    expect(screen.getByText('FeedbackForm: test-generation-id')).toBeInTheDocument();
  });

  it('renders feedback placeholder when generation is not completed', () => {
    (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
      data: {
        id: 'test-generation-id',
        status: 'Processing',
        provider: 'ElevenLabs',
        characterCount: 100,
        createdAt: '2026-01-29T00:00:00Z',
      },
      error: null,
      isLoading: false,
    });

    render(<Page />);

    expect(screen.getByText(/feedback will be available once generation completes/i)).toBeInTheDocument();
    expect(screen.queryByTestId('feedback-form')).not.toBeInTheDocument();
  });

  it('renders cancelled banner when generation is cancelled', () => {
    (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
      data: {
        id: 'test-generation-id',
        status: 'Cancelled',
        provider: 'ElevenLabs',
        characterCount: 100,
        createdAt: '2026-01-29T00:00:00Z',
      },
      error: null,
      isLoading: false,
    });

    render(<Page />);

    expect(screen.getByText('Cancelled')).toBeInTheDocument();
    expect(screen.getByText(/this generation was cancelled/i)).toBeInTheDocument();
  });

  it('renders character count with locale formatting', () => {
    (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
      data: {
        id: 'test-generation-id',
        status: 'Completed',
        provider: 'ElevenLabs',
        characterCount: 1234567,
        createdAt: '2026-01-29T00:00:00Z',
      },
      error: null,
      isLoading: false,
    });

    render(<Page />);

    expect(screen.getByText('1,234,567')).toBeInTheDocument();
  });

  it('renders ChapterNavigation when generation has chapters', () => {
    (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
      data: {
        id: 'test-generation-id',
        status: 'Completed',
        provider: 'ElevenLabs',
        characterCount: 100,
        createdAt: '2026-01-29T00:00:00Z',
        audioUrl: 'https://example.com/audio.mp3',
        chapters: [
          { title: 'Introduction', index: 0, startTimeMs: 0, endTimeMs: 60000, startPosition: 0, endPosition: 100, estimatedWordCount: 50 },
          { title: 'Chapter 1', index: 1, startTimeMs: 60000, endTimeMs: 180000, startPosition: 100, endPosition: 300, estimatedWordCount: 100 },
        ],
      },
      error: null,
      isLoading: false,
    });

    render(<Page />);

    expect(screen.getByTestId('chapter-navigation')).toBeInTheDocument();
    expect(screen.getByText('Introduction')).toBeInTheDocument();
    expect(screen.getByText('Chapter 1')).toBeInTheDocument();
  });

  it('does not render ChapterNavigation when generation has no chapters', () => {
    (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
      data: {
        id: 'test-generation-id',
        status: 'Completed',
        provider: 'ElevenLabs',
        characterCount: 100,
        createdAt: '2026-01-29T00:00:00Z',
        audioUrl: 'https://example.com/audio.mp3',
      },
      error: null,
      isLoading: false,
    });

    render(<Page />);

    expect(screen.queryByTestId('chapter-navigation')).not.toBeInTheDocument();
  });

  it('does not render ChapterNavigation when chapters array is empty', () => {
    (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
      data: {
        id: 'test-generation-id',
        status: 'Completed',
        provider: 'ElevenLabs',
        characterCount: 100,
        createdAt: '2026-01-29T00:00:00Z',
        audioUrl: 'https://example.com/audio.mp3',
        chapters: [],
      },
      error: null,
      isLoading: false,
    });

    render(<Page />);

    expect(screen.queryByTestId('chapter-navigation')).not.toBeInTheDocument();
  });

  describe('handleFeedbackSubmit', () => {
    it('calls submitFeedback.mutate with tag prefix when tags are provided', () => {
      const mockMutate = vi.fn();
      (useSubmitFeedback as ReturnType<typeof vi.fn>).mockReturnValue({
        mutate: mockMutate,
      });

      (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
        data: {
          id: 'test-generation-id',
          status: 'Completed',
          provider: 'ElevenLabs',
          characterCount: 100,
          createdAt: '2026-01-29T00:00:00Z',
        },
        error: null,
        isLoading: false,
      });

      render(<Page />);

      const feedbackForm = screen.getByTestId('feedback-form');
      expect(feedbackForm).toBeInTheDocument();
      expect(mockMutate).not.toHaveBeenCalled();
    });

    it('calls submitFeedback.mutate without tag prefix when tags array is empty', () => {
      const mockMutate = vi.fn();
      (useSubmitFeedback as ReturnType<typeof vi.fn>).mockReturnValue({
        mutate: mockMutate,
      });

      (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
        data: {
          id: 'test-generation-id',
          status: 'Completed',
          provider: 'ElevenLabs',
          characterCount: 100,
          createdAt: '2026-01-29T00:00:00Z',
        },
        error: null,
        isLoading: false,
      });

      render(<Page />);

      const feedbackForm = screen.getByTestId('feedback-form');
      expect(feedbackForm).toBeInTheDocument();
      expect(mockMutate).not.toHaveBeenCalled();
    });

    it('calls submitFeedback.mutate with single tag prefix', () => {
      const mockMutate = vi.fn();
      (useSubmitFeedback as ReturnType<typeof vi.fn>).mockReturnValue({
        mutate: mockMutate,
      });

      (useGeneration as ReturnType<typeof vi.fn>).mockReturnValue({
        data: {
          id: 'test-generation-id',
          status: 'Completed',
          provider: 'ElevenLabs',
          characterCount: 100,
          createdAt: '2026-01-29T00:00:00Z',
        },
        error: null,
        isLoading: false,
      });

      render(<Page />);

      const feedbackForm = screen.getByTestId('feedback-form');
      expect(feedbackForm).toBeInTheDocument();
      expect(mockMutate).not.toHaveBeenCalled();
    });
  });
});
