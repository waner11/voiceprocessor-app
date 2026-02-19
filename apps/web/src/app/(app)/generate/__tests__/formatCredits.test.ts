import { describe, it, expect } from 'vitest';
import { formatCredits } from '@/utils/formatCredits';

describe('formatCredits', () => {
  it('returns singular "credit" for exactly 1', () => {
    expect(formatCredits(1)).toBe('1 credit');
  });

  it('returns plural "credits" for values other than 1', () => {
    expect(formatCredits(2)).toBe('2 credits');
  });

  it('returns "0 credits" for zero', () => {
    expect(formatCredits(0)).toBe('0 credits');
  });

  it('formats numbers with comma separators', () => {
    expect(formatCredits(1234)).toBe('1,234 credits');
  });

  it('formats large numbers with comma separators', () => {
    expect(formatCredits(1234567)).toBe('1,234,567 credits');
  });
});
