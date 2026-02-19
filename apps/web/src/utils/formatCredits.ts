/**
 * Format a credit amount with locale-specific thousand separators
 * and singular/plural label.
 * @param credits - The number of credits to format
 * @returns Formatted string like "1,234 credits" or "1 credit"
 */
export function formatCredits(credits: number): string {
  if (!Number.isFinite(credits) || credits < 0) {
    const formatted = (0).toLocaleString('en-US');
    return `${formatted} credits`;
  }
  const rounded = Math.ceil(credits);
  const formatted = rounded.toLocaleString('en-US');
  return rounded === 1 ? `${formatted} credit` : `${formatted} credits`;
}
