import { describe, it, expect, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { GenerationStatus } from '../GenerationStatus';

describe('GenerationStatus', () => {
  const defaultProps = {
    generationId: 'test-generation-id-12345',
    status: 'processing' as const,
    progress: 50,
    currentStep: 'Generating audio...',
  };

  beforeEach(() => {
    // Clear any previous renders
  });

  describe('rendering', () => {
    it('does not display the generation ID', () => {
      render(<GenerationStatus {...defaultProps} />);
      
      // Assert that the ID text is NOT in the document
      expect(screen.queryByText(/^ID:/)).not.toBeInTheDocument();
      
      // Assert that the generation ID value is NOT in the document
      expect(screen.queryByText(defaultProps.generationId)).not.toBeInTheDocument();
    });

    it('does not render a code element with the generation ID', () => {
      render(<GenerationStatus {...defaultProps} />);
      
      const codeElements = screen.queryAllByRole('code', { hidden: true });
      const hasGenerationIdCode = codeElements.some(
        (el) => el.textContent === defaultProps.generationId
      );
      
      expect(hasGenerationIdCode).toBe(false);
    });

    it('still renders status badge and progress bar', () => {
      render(<GenerationStatus {...defaultProps} />);
      
      // Assert that other content is still rendered
      expect(screen.getByText('Status')).toBeInTheDocument();
      expect(screen.getByText('Processing')).toBeInTheDocument();
      expect(screen.getByText(/50%/)).toBeInTheDocument();
      expect(screen.getByText('Generating audio...')).toBeInTheDocument();
    });

    it('still accepts generationId prop (interface unchanged)', () => {
      // This test verifies the prop is still accepted by the component
      // If the prop was removed, this would fail at TypeScript compile time
      const { rerender } = render(<GenerationStatus {...defaultProps} />);
      
      // Should not throw when re-rendering with different ID
      rerender(
        <GenerationStatus
          {...defaultProps}
          generationId="different-id-67890"
        />
      );
      
      expect(screen.getByText('Processing')).toBeInTheDocument();
    });
  });

  describe('different statuses', () => {
    it('does not display ID when status is completed', () => {
      render(
        <GenerationStatus
          {...defaultProps}
          status="completed"
          progress={100}
        />
      );
      
      expect(screen.queryByText(/^ID:/)).not.toBeInTheDocument();
      expect(screen.getByText('Completed')).toBeInTheDocument();
    });

    it('does not display ID when status is failed', () => {
      render(
        <GenerationStatus
          {...defaultProps}
          status="failed"
          error="Something went wrong"
        />
      );
      
      expect(screen.queryByText(/^ID:/)).not.toBeInTheDocument();
      expect(screen.getByText('Failed')).toBeInTheDocument();
    });
  });
});
