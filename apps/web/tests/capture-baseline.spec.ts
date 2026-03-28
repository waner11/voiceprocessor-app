import { test, expect } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';

const EVIDENCE_DIR = '.sisyphus/evidence/baseline';
const BASE_URL = 'http://localhost:3000';

const pages = [
  { url: '/login', filename: 'baseline-login.png' },
  { url: '/dashboard', filename: 'baseline-dashboard.png' },
  { url: '/generate', filename: 'baseline-generate.png' },
  { url: '/settings/profile', filename: 'baseline-settings.png' }
];

test.describe('Baseline Dark Mode Capture', () => {
  const computedStyles: Record<string, unknown> = {};

  for (const pageConfig of pages) {
    test(`capture ${pageConfig.url}`, async ({ page }) => {
      const fullUrl = `${BASE_URL}${pageConfig.url}`;
      console.log(`Capturing: ${fullUrl}`);
      
      await page.goto(fullUrl, { waitUntil: 'networkidle', timeout: 10000 });
      await page.waitForTimeout(500);
      
      // Capture screenshot
      const screenshotPath = path.join(EVIDENCE_DIR, pageConfig.filename);
      await page.screenshot({ path: screenshotPath, fullPage: true });
      console.log(`✓ Screenshot saved: ${screenshotPath}`);
      
      // Extract computed styles
      const styles = await page.evaluate(() => {
        const bodyStyles = window.getComputedStyle(document.body);
        const roundedElement = document.querySelector('.rounded-lg');
        const shadowElement = document.querySelector('[style*="box-shadow"], .shadow, .shadow-sm, .shadow-md, .shadow-lg');
        
        return {
          body: {
            backgroundColor: bodyStyles.backgroundColor,
            color: bodyStyles.color
          },
          roundedLg: roundedElement ? {
            borderRadius: window.getComputedStyle(roundedElement).borderRadius,
            selector: roundedElement.className
          } : null,
          boxShadow: shadowElement ? {
            boxShadow: window.getComputedStyle(shadowElement).boxShadow,
            selector: shadowElement.className
          } : null
        };
      });
      
      computedStyles[pageConfig.url] = styles;
      console.log(`✓ Styles extracted for ${pageConfig.url}`);
    });
  }

  test.afterAll(async () => {
    const stylesPath = path.join(EVIDENCE_DIR, 'computed-styles.json');
    fs.writeFileSync(stylesPath, JSON.stringify(computedStyles, null, 2));
    console.log(`✓ Computed styles saved: ${stylesPath}`);
  });
});
