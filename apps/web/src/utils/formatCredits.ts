/**
 * Format a credit amount with locale-specific thousand separators
 * and singular/plural label.
 * @param credits - The number of credits to format
 * @returns Formatted string like "1,234 credits" or "1 credit"
 */
export function formatCredits(credits: number): string {
  const formatted = credits.toLocaleString('en-US');
  return credits === 1 ? `${formatted} credit` : `${formatted} credits`;
}
