import { describe, it, expect } from 'vitest';
import { formatNumber } from '../formatNumber';

describe('formatNumber', () => {
  it('formats 1234 with comma separator', () => {
    expect(formatNumber(1234)).toBe('1,234');
  });

  it('formats 0 as "0"', () => {
    expect(formatNumber(0)).toBe('0');
  });

  it('formats negative numbers with comma separator', () => {
    expect(formatNumber(-1234)).toBe('-1,234');
  });

  it('formats large numbers with multiple comma separators', () => {
    expect(formatNumber(1234567)).toBe('1,234,567');
  });

  it('returns "0" for NaN', () => {
    expect(formatNumber(NaN)).toBe('0');
  });
});
