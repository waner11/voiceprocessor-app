import { describe, it, expect, vi, beforeEach } from 'vitest';
import { paymentService } from '../service';

// Mock the API client
vi.mock('@/lib/api/client', () => ({
  api: {
    POST: vi.fn(),
  },
}));

import { api } from '@/lib/api/client';

describe('paymentService.createCheckoutSession', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    (api.POST as unknown as ReturnType<typeof vi.fn>).mockResolvedValue({
      data: { checkoutUrl: 'https://stripe.com/checkout/test' },
      error: null,
    });
  });

  it('sends correct body shape with priceId and URLs', async () => {
    await paymentService.createCheckoutSession('price_short_story');

    expect(api.POST).toHaveBeenCalledWith('/api/v1/payments/checkout', {
      body: expect.objectContaining({
        priceId: 'price_short_story',
        successUrl: expect.stringContaining('/payment/success'),
        cancelUrl: expect.stringContaining('/settings/billing'),
      }),
    });
  });

  it('throws error for empty price ID', async () => {
    await expect(
      paymentService.createCheckoutSession('')
    ).rejects.toThrow('Invalid price ID');
  });

  it('passes priceId directly from API packs', async () => {
    await paymentService.createCheckoutSession('price_from_stripe_api');

    expect(api.POST).toHaveBeenCalledWith('/api/v1/payments/checkout', {
      body: expect.objectContaining({
        priceId: 'price_from_stripe_api',
        successUrl: expect.stringContaining('/payment/success'),
        cancelUrl: expect.stringContaining('/settings/billing'),
      }),
    });
  });
});
