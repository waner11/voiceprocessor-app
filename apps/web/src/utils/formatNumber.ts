/**
 * Format a number with locale-specific thousand separators
 * @param value - The number to format
 * @returns Formatted string with 'en-US' locale, or "0" for NaN
 */
export function formatNumber(value: number): string {
  if (Number.isNaN(value)) {
    return '0';
  }
  return value.toLocaleString('en-US');
}
