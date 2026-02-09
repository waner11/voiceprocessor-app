import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ApiGatedLinks } from '../api-gated-links';

vi.mock('@/lib/posthog', () => ({
  useApiAccess: vi.fn(),
}));

import { useApiAccess } from '@/lib/posthog';

describe('ApiGatedLinks', () => {
  describe('nav type', () => {
    it('returns null when flag is false', () => {
      vi.mocked(useApiAccess).mockReturnValue(false);
      
      const { container } = render(<ApiGatedLinks type="nav" />);
      
      expect(container.firstChild).toBeNull();
    });

    it('renders API Docs nav link when flag is true', () => {
      vi.mocked(useApiAccess).mockReturnValue(true);
      
      render(<ApiGatedLinks type="nav" />);
      
      const link = screen.getByRole('link', { name: /API Docs/i });
      expect(link).toBeInTheDocument();
      expect(link).toHaveAttribute('href', '/api-docs');
    });
  });

  describe('hero type', () => {
    it('returns null when flag is false', () => {
      vi.mocked(useApiAccess).mockReturnValue(false);
      
      const { container } = render(<ApiGatedLinks type="hero" />);
      
      expect(container.firstChild).toBeNull();
    });

    it('renders View API Docs hero button when flag is true', () => {
      vi.mocked(useApiAccess).mockReturnValue(true);
      
      render(<ApiGatedLinks type="hero" />);
      
      const link = screen.getByRole('link', { name: /View API Docs/i });
      expect(link).toBeInTheDocument();
      expect(link).toHaveAttribute('href', '/api-docs');
    });
  });

  describe('footer type', () => {
    it('returns null when flag is false', () => {
      vi.mocked(useApiAccess).mockReturnValue(false);
      
      const { container } = render(<ApiGatedLinks type="footer" />);
      
      expect(container.firstChild).toBeNull();
    });

    it('renders API Documentation footer link when flag is true', () => {
      vi.mocked(useApiAccess).mockReturnValue(true);
      
      render(<ApiGatedLinks type="footer" />);
      
      const link = screen.getByRole('link', { name: /API Documentation/i });
      expect(link).toBeInTheDocument();
      expect(link).toHaveAttribute('href', '/api-docs');
    });
  });
});
