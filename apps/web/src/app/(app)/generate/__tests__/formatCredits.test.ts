import { describe, it, expect } from 'vitest';

/**
 * Format credits and cost display
 * Shows "X,XXX credits ($Y.YYYY)" when credits > 0
 * Shows "$Y.YYYY" when credits is 0, undefined, or null
 */
const formatCostDisplay = (credits: number | undefined, cost: number) => {
  if (credits && credits > 0) {
    return `${credits.toLocaleString()} credits ($${cost.toFixed(4)})`;
  }
  return `$${cost.toFixed(4)}`;
};

describe('formatCostDisplay', () => {
  it('formats credits and cost together for normal case', () => {
    expect(formatCostDisplay(1234, 0.037)).toBe('1,234 credits ($0.0370)');
  });

  it('shows cost only when credits is zero', () => {
    expect(formatCostDisplay(0, 0.037)).toBe('$0.0370');
  });

  it('shows cost only when credits is undefined', () => {
    expect(formatCostDisplay(undefined, 0.037)).toBe('$0.0370');
  });

  it('formats large credit numbers with commas', () => {
    expect(formatCostDisplay(1234567, 37.037)).toBe('1,234,567 credits ($37.0370)');
  });
});
