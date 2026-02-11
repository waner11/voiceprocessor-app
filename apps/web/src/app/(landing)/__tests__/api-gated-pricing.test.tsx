import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ApiGatedPricingFeature } from '../api-gated-pricing';

vi.mock('@/lib/posthog', () => ({
  useApiAccess: vi.fn(),
}));

import { useApiAccess } from '@/lib/posthog';

describe('ApiGatedPricingFeature', () => {
  it('returns null when flag is false', () => {
    vi.mocked(useApiAccess).mockReturnValue(false);
    
    const { container } = render(<ApiGatedPricingFeature />);
    
    expect(container.firstChild).toBeNull();
  });

  it('returns null when flag is undefined', () => {
    vi.mocked(useApiAccess).mockReturnValue(false);
    
    const { container } = render(<ApiGatedPricingFeature />);
    
    expect(container.firstChild).toBeNull();
  });

  it('renders the API access list item when flag is true', () => {
    vi.mocked(useApiAccess).mockReturnValue(true);
    
    render(<ApiGatedPricingFeature />);
    
    expect(screen.getByText('API access')).toBeInTheDocument();
  });

  it('renders with correct styling when flag is true', () => {
    vi.mocked(useApiAccess).mockReturnValue(true);
    
    const { container } = render(<ApiGatedPricingFeature />);
    
    const listItem = container.querySelector('li');
    expect(listItem).toBeInTheDocument();
    expect(listItem).toHaveClass('flex', 'items-center', 'gap-2');
  });
});
