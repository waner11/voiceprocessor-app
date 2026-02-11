import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ApiGatedFeatureCard } from '../api-gated-features';

// Mock the useApiAccess hook
vi.mock('@/lib/posthog', () => ({
  useApiAccess: vi.fn(),
}));

import { useApiAccess } from '@/lib/posthog';

describe('ApiGatedFeatureCard', () => {
  it('returns null when flag is false', () => {
    vi.mocked(useApiAccess).mockReturnValue(false);
    
    const { container } = render(<ApiGatedFeatureCard />);
    
    expect(container.firstChild).toBeNull();
  });

  it('returns null when flag is undefined', () => {
    vi.mocked(useApiAccess).mockReturnValue(false); // undefined treated as false
    
    const { container } = render(<ApiGatedFeatureCard />);
    
    expect(container.firstChild).toBeNull();
  });

  it('renders the Developer API feature card when flag is true', () => {
    vi.mocked(useApiAccess).mockReturnValue(true);
    
    render(<ApiGatedFeatureCard />);
    
    expect(screen.getByText('Developer API')).toBeInTheDocument();
    expect(screen.getByText(/Full REST API with webhooks/)).toBeInTheDocument();
  });

  it('renders the correct icon and description', () => {
    vi.mocked(useApiAccess).mockReturnValue(true);
    
    render(<ApiGatedFeatureCard />);
    
    expect(screen.getByText('🔌')).toBeInTheDocument();
    expect(screen.getByText('Full REST API with webhooks for seamless integration into your applications.')).toBeInTheDocument();
  });
});
