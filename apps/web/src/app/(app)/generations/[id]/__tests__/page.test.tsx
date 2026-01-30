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

vi.mock('@/components/FeedbackForm/FeedbackForm', () => ({
  FeedbackForm: ({ generationId }: { generationId: string }) => (
    <div data-testid="feedback-form">FeedbackForm: {generationId}</div>
  ),
}));

import { useGeneration } from '@/hooks/useGenerations';

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
});
