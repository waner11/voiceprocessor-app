import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';

const mockLogout = vi.fn();
const mockGetState = vi.fn(() => ({ logout: mockLogout }));

vi.mock('@/stores', () => ({
  useAuthStore: {
    getState: mockGetState,
  },
}));

describe('401 Refresh Interceptor', () => {
  let originalLocation: Location;
  let originalFetch: typeof global.fetch;

  beforeEach(() => {
    vi.clearAllMocks();
    mockLogout.mockClear();

    originalLocation = window.location;
    delete (window as any).location;
    window.location = { href: '' } as Location;

    originalFetch = global.fetch;
  });

  afterEach(() => {
    window.location = originalLocation;
    global.fetch = originalFetch;
    vi.resetModules();
  });

  it('should trigger refresh on 401, retry original request, and succeed', async () => {
    const mockFetch = vi.fn();
    
    mockFetch
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ user: { id: '1', email: 'test@example.com' } }), { status: 200 }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ data: 'success' }), { status: 200 }));

    global.fetch = mockFetch;

    const { api } = await import('../client');

    const response = await api.GET('/api/v1/some-endpoint', {});

    expect(mockFetch).toHaveBeenCalledTimes(3);
    
    const call1 = mockFetch.mock.calls[0][0];
    const call2 = mockFetch.mock.calls[1][0];
    const call3 = mockFetch.mock.calls[2][0];
    
    expect(call1).toBeInstanceOf(Request);
    expect(call1.url).toContain('/api/v1/some-endpoint');
    
    expect(call2).toContain('/api/v1/Auth/refresh');
    expect(mockFetch.mock.calls[1][1]).toMatchObject({ method: 'POST' });
    
    expect(call3).toContain('/api/v1/some-endpoint');
    
    expect(response.data).toEqual({ data: 'success' });
    expect(mockLogout).not.toHaveBeenCalled();
  });

  it('should call logout and redirect to /login when refresh fails', async () => {
    const mockFetch = vi.fn();
    
    mockFetch
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(new Response(null, { status: 401 }));

    global.fetch = mockFetch;

    const { api } = await import('../client');

    await api.GET('/api/v1/some-endpoint', {});

    expect(mockFetch).toHaveBeenCalledTimes(2);
    
    const call1 = mockFetch.mock.calls[0][0];
    const call2 = mockFetch.mock.calls[1][0];
    
    expect(call1).toBeInstanceOf(Request);
    expect(call1.url).toContain('/api/v1/some-endpoint');
    
    expect(call2).toContain('/api/v1/Auth/refresh');
    expect(mockFetch.mock.calls[1][1]).toMatchObject({ method: 'POST' });
    
    expect(mockLogout).toHaveBeenCalledTimes(1);
    expect(window.location.href).toBe('/login');
  });

  it('should handle 3 concurrent 401s with only 1 refresh call', async () => {
    const mockFetch = vi.fn();
    
    mockFetch
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ user: { id: '1', email: 'test@example.com' } }), { status: 200 }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ data: 'success1' }), { status: 200 }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ data: 'success2' }), { status: 200 }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ data: 'success3' }), { status: 200 }));

    global.fetch = mockFetch;

    const { api } = await import('../client');

    const [response1, response2, response3] = await Promise.all([
      api.GET('/api/v1/endpoint1', {}),
      api.GET('/api/v1/endpoint2', {}),
      api.GET('/api/v1/endpoint3', {}),
    ]);

    expect(mockFetch).toHaveBeenCalledTimes(7);
    
    const refreshCalls = mockFetch.mock.calls.filter(call => 
      typeof call[0] === 'string' && call[0].includes('/api/v1/Auth/refresh')
    );
    expect(refreshCalls).toHaveLength(1);
    
    expect(response1.data).toEqual({ data: 'success1' });
    expect(response2.data).toEqual({ data: 'success2' });
    expect(response3.data).toEqual({ data: 'success3' });
    expect(mockLogout).not.toHaveBeenCalled();
  });

  it('should logout immediately if already-retried request gets 401 again', async () => {
    const mockFetch = vi.fn();
    
    mockFetch
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ user: { id: '1', email: 'test@example.com' } }), { status: 200 }))
      .mockResolvedValueOnce(new Response(null, { status: 401 }));

    global.fetch = mockFetch;

    const { api } = await import('../client');

    await api.GET('/api/v1/some-endpoint', {});

    expect(mockFetch).toHaveBeenCalledTimes(3);
    
    const call1 = mockFetch.mock.calls[0][0];
    const call2 = mockFetch.mock.calls[1][0];
    const call3 = mockFetch.mock.calls[2][0];
    
    expect(call1).toBeInstanceOf(Request);
    expect(call1.url).toContain('/api/v1/some-endpoint');
    
    expect(call2).toContain('/api/v1/Auth/refresh');
    
    expect(call3).toContain('/api/v1/some-endpoint');
    
    expect(mockLogout).toHaveBeenCalledTimes(1);
    expect(window.location.href).toBe('/login');
  });

  it('should pass through non-401 errors unchanged', async () => {
    const mockFetch = vi.fn();
    
    mockFetch
      .mockResolvedValueOnce(new Response(null, { status: 500 }))
      .mockResolvedValueOnce(new Response(null, { status: 403 }))
      .mockRejectedValueOnce(new Error('Network error'));

    global.fetch = mockFetch;

    const { api } = await import('../client');

    const response500 = await api.GET('/api/v1/endpoint1', {});
    expect(response500.response.status).toBe(500);
    expect(mockLogout).not.toHaveBeenCalled();

    const response403 = await api.GET('/api/v1/endpoint2', {});
    expect(response403.response.status).toBe(403);
    expect(mockLogout).not.toHaveBeenCalled();

    await expect(api.GET('/api/v1/endpoint3', {})).rejects.toThrow('Network error');
    expect(mockLogout).not.toHaveBeenCalled();

    const refreshCalls = mockFetch.mock.calls.filter(call => 
      typeof call[0] === 'string' && call[0].includes('/api/v1/Auth/refresh')
    );
    expect(refreshCalls).toHaveLength(0);
  });

  it('should retry POST request with original body after 401 refresh', async () => {
    const testBody = { username: 'test', password: 'secret' };
    const mockFetch = vi.fn();
    
    mockFetch
      .mockResolvedValueOnce(new Response(JSON.stringify({ error: 'Unauthorized' }), { status: 401 }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ user: { id: '1', email: 'test@example.com' } }), { status: 200 }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ success: true }), { status: 200 }));

    global.fetch = mockFetch;

    const { api } = await import('../client');

    const response = await api.POST('/api/v1/auth/login', { body: testBody });

    expect(response.response.status).toBe(200);
    expect(response.data).toEqual({ success: true });

    expect(mockFetch).toHaveBeenCalledTimes(3);
    
    const call1 = mockFetch.mock.calls[0][0];
    expect(call1).toBeInstanceOf(Request);
    expect(call1.url).toContain('/api/v1/auth/login');
    
    const call2 = mockFetch.mock.calls[1][0];
    expect(call2).toContain('/api/v1/Auth/refresh');
    
    const retryCall = mockFetch.mock.calls[2];
    const retryInit = retryCall[1] as RequestInit;
    expect(retryInit.body).toBe(JSON.stringify(testBody));
    expect(mockLogout).not.toHaveBeenCalled();
  });
});
