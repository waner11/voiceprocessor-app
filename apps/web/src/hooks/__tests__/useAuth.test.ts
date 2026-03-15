import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useForgotPassword, useResetPassword } from '../useAuth';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import React from 'react';

// Mock the auth store
const mockLogin = vi.fn();
const mockLogout = vi.fn();

vi.mock('@/stores', () => ({
  useAuthStore: (selector: (state: Record<string, unknown>) => unknown) => {
    const state = {
      login: mockLogin,
      logout: mockLogout,
    };
    return selector(state);
  },
}));

// Mock next/navigation
vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
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

describe('useForgotPassword', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    global.fetch = vi.fn();
  });

  it('should call POST /api/v1/auth/forgot-password with email', async () => {
    const mockFetch = vi.fn().mockResolvedValueOnce(
      new Response(JSON.stringify({}), { status: 200 })
    );
    global.fetch = mockFetch;

    const { result } = renderHook(() => useForgotPassword(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync({ email: 'test@example.com' });

    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/auth/forgot-password'),
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({ email: 'test@example.com' }),
      })
    );
  });

  it('should return mutation state with isPending, error, isSuccess', async () => {
    const mockFetch = vi.fn().mockResolvedValueOnce(
      new Response(JSON.stringify({}), { status: 200 })
    );
    global.fetch = mockFetch;

    const { result } = renderHook(() => useForgotPassword(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isPending).toBe(false);
    expect(result.current.error).toBeNull();
    expect(result.current.isSuccess).toBe(false);

    result.current.mutate({ email: 'test@example.com' });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });
  });

  it('should handle API errors gracefully', async () => {
    const mockFetch = vi.fn().mockResolvedValueOnce(
      new Response(JSON.stringify({ code: 'INVALID_EMAIL', message: 'Invalid email' }), { status: 400 })
    );
    global.fetch = mockFetch;

    const { result } = renderHook(() => useForgotPassword(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ email: 'invalid' });

    await waitFor(() => {
      expect(result.current.error).toBeDefined();
    });
  });

  it('should not redirect on success (page responsibility)', async () => {
    const mockFetch = vi.fn().mockResolvedValueOnce(
      new Response(JSON.stringify({}), { status: 200 })
    );
    global.fetch = mockFetch;

    const { result } = renderHook(() => useForgotPassword(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ email: 'test@example.com' });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });
  })
});

describe('useResetPassword', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    global.fetch = vi.fn();
  });

  it('should call POST /api/v1/auth/reset-password with token and newPassword', async () => {
    const mockFetch = vi.fn().mockResolvedValueOnce(
      new Response(JSON.stringify({}), { status: 200 })
    );
    global.fetch = mockFetch;

    const { result } = renderHook(() => useResetPassword(), {
      wrapper: createWrapper(),
    });

    await result.current.mutateAsync({
      token: 'reset-token-123',
      newPassword: 'NewPassword123!',
    });

    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/auth/reset-password'),
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({
          token: 'reset-token-123',
          newPassword: 'NewPassword123!',
        }),
      })
    );
  });

  it('should return mutation state with isPending, error, isSuccess', async () => {
    const mockFetch = vi.fn().mockResolvedValueOnce(
      new Response(JSON.stringify({}), { status: 200 })
    );
    global.fetch = mockFetch;

    const { result } = renderHook(() => useResetPassword(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isPending).toBe(false);
    expect(result.current.error).toBeNull();
    expect(result.current.isSuccess).toBe(false);

    result.current.mutate({
      token: 'reset-token-123',
      newPassword: 'NewPassword123!',
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });
  });

  it('should handle invalid token errors', async () => {
    const mockFetch = vi.fn().mockResolvedValueOnce(
      new Response(
        JSON.stringify({ code: 'INVALID_TOKEN', message: 'Invalid or expired token' }),
        { status: 400 }
      )
    );
    global.fetch = mockFetch;

    const { result } = renderHook(() => useResetPassword(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      token: 'invalid-token',
      newPassword: 'NewPassword123!',
    });

    await waitFor(() => {
      expect(result.current.error).toBeDefined();
    });
  });

  it('should not redirect on success (page responsibility)', async () => {
    const mockFetch = vi.fn().mockResolvedValueOnce(
      new Response(JSON.stringify({}), { status: 200 })
    );
    global.fetch = mockFetch;

    const { result } = renderHook(() => useResetPassword(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      token: 'reset-token-123',
      newPassword: 'NewPassword123!',
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });
  })
});
