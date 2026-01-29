import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import Page from '../page';

// Mock the useGeneration hook
vi.mock('@/hooks/useGenerations', () => ({
  useGeneration: vi.fn(),
}));

// Mock useParams from next/navigation
vi.mock('next/navigation', () => ({
  useParams: vi.fn(() => ({ id: 'test-generation-id' })),
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
      error: { message: 'Failed to fetch generation' },
      isLoading: false,
    });

    render(<Page />);
    
    // Should show error message
    expect(screen.getByText(/error/i)).toBeInTheDocument();
    expect(screen.getByText(/failed to fetch/i)).toBeInTheDocument();
  });
});
