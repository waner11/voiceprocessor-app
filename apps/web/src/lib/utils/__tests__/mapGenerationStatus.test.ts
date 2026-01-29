import { describe, it, expect } from 'vitest';
import { mapGenerationStatus } from '../mapGenerationStatus';

describe('mapGenerationStatus', () => {
  it('maps Pending to queued', () => {
    expect(mapGenerationStatus('Pending')).toBe('queued');
  });

  it('maps Analyzing to processing', () => {
    expect(mapGenerationStatus('Analyzing')).toBe('processing');
  });

  it('maps Chunking to processing', () => {
    expect(mapGenerationStatus('Chunking')).toBe('processing');
  });

  it('maps Processing to processing', () => {
    expect(mapGenerationStatus('Processing')).toBe('processing');
  });

  it('maps Merging to processing', () => {
    expect(mapGenerationStatus('Merging')).toBe('processing');
  });

  it('maps Completed to completed', () => {
    expect(mapGenerationStatus('Completed')).toBe('completed');
  });

  it('maps Failed to failed', () => {
    expect(mapGenerationStatus('Failed')).toBe('failed');
  });

  it('maps Cancelled to failed', () => {
    expect(mapGenerationStatus('Cancelled')).toBe('failed');
  });
});
