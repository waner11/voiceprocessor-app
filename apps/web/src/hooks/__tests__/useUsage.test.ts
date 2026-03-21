import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useUsage } from '../useUsage';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import React from 'react';

// Mock the auth store
vi.mock('@/stores', () => ({
  useAuthStore: (selector: (state: Record<string, unknown>) => unknown) => {
    const state = {
      isAuthenticated: true,
    };
    return selector(state);
  },
}));

// Helper to create a wrapper with QueryClientProvider
const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  const Wrapper = ({ children }: { children: React.ReactNode }) =>
    React.createElement(QueryClientProvider, { client: queryClient }, children);
  Wrapper.displayName = 'TestQueryWrapper';
  return Wrapper;
};

describe('useUsage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    global.fetch = vi.fn();
  });

  it('should call GET /api/v1/usage endpoint', async () => {
    const mockFetch = vi.fn().mockResolvedValueOnce(
      new Response(
        JSON.stringify({
          creditsUsedThisMonth: 100,
          creditsRemaining: 900,
          generationsCount: 5,
          totalAudioMinutes: 45,
        }),
        { status: 200 }
      )
    );
    global.fetch = mockFetch;

    const { result } = renderHook(() => useUsage(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.data).toBeDefined();
    });

    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/usage'),
      expect.objectContaining({
        credentials: 'include',
        headers: expect.objectContaining({
          'Content-Type': 'application/json',
        }),
      })
    );
  });

  it('should return loading state initially', async () => {
    const mockFetch = vi.fn().mockResolvedValueOnce(
      new Response(
        JSON.stringify({
          creditsUsedThisMonth: 100,
          creditsRemaining: 900,
          generationsCount: 5,
          totalAudioMinutes: 45,
        }),
        { status: 200 }
      )
    );
    global.fetch = mockFetch;

    const { result } = renderHook(() => useUsage(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });
  });

  it('should return data on successful fetch', async () => {
    const mockData = {
      creditsUsedThisMonth: 100,
      creditsRemaining: 900,
      generationsCount: 5,
      totalAudioMinutes: 45,
    };

    const mockFetch = vi.fn().mockResolvedValueOnce(
      new Response(JSON.stringify(mockData), { status: 200 })
    );
    global.fetch = mockFetch;

    const { result } = renderHook(() => useUsage(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.data).toEqual(mockData);
    });

    expect(result.current.error).toBeNull();
  });

  it('should return error on failed fetch', async () => {
    const mockFetch = vi.fn().mockResolvedValueOnce(
      new Response(
        JSON.stringify({ code: 'INTERNAL_ERROR', message: 'Server error' }),
        { status: 500 }
      )
    );
    global.fetch = mockFetch;

    const { result } = renderHook(() => useUsage(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.error).toBeDefined();
    });

    expect(result.current.data).toBeUndefined();
  });

  it('should have refetch function available', async () => {
    const mockFetch = vi.fn().mockResolvedValueOnce(
      new Response(
        JSON.stringify({
          creditsUsedThisMonth: 100,
          creditsRemaining: 900,
          generationsCount: 5,
          totalAudioMinutes: 45,
        }),
        { status: 200 }
      )
    );
    global.fetch = mockFetch;

    const { result } = renderHook(() => useUsage(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.data).toBeDefined();
    });

    expect(typeof result.current.refetch).toBe('function');
  });
});
