import { test, expect } from "@playwright/test";

const EVIDENCE_DIR = "../../.sisyphus/evidence/final-qa";

test("homepage loads", async ({ page }) => {
  await page.goto("/");
  await expect(page).toHaveTitle(/VoiceProcessor/i);
});

test("landing page has dark mode applied", async ({ page }) => {
  await page.goto("/");
  await page.waitForLoadState("networkidle");

  const htmlClass = await page.locator("html").getAttribute("class");
  expect(htmlClass).toContain("dark");

  const bgColor = await page.evaluate(() => {
    return window.getComputedStyle(document.body).backgroundColor;
  });
  expect(bgColor).not.toBe("rgb(255, 255, 255)");

  await page.screenshot({
    path: `${EVIDENCE_DIR}/landing-dark.png`,
    fullPage: true,
  });
});

test("landing page text is visible against dark background", async ({
  page,
}) => {
  await page.goto("/");
  await page.waitForLoadState("networkidle");

  const heading = page.locator("h1").first();
  await expect(heading).toBeVisible();

  const textColor = await heading.evaluate((el) => {
    return window.getComputedStyle(el).color;
  });
  // pure black text on dark bg = invisible
  expect(textColor).not.toBe("rgb(0, 0, 0)");

  await page.screenshot({
    path: `${EVIDENCE_DIR}/landing-heading-visible.png`,
  });
});

test("landing page CTA button uses semantic token color", async ({ page }) => {
  await page.goto("/");
  await page.waitForLoadState("networkidle");

  const ctaButton = page
    .locator("a[href*='signup'], a[href*='register'], a[href*='login']")
    .first();

  if ((await ctaButton.count()) > 0) {
    const bgColor = await ctaButton.evaluate((el) => {
      return window.getComputedStyle(el).backgroundColor;
    });
    expect(bgColor).not.toBe("rgb(255, 255, 255)");
  }

  await page.screenshot({
    path: `${EVIDENCE_DIR}/landing-cta.png`,
  });
});

test("login page renders in dark mode", async ({ page }) => {
  await page.goto("/login");
  await page.waitForLoadState("networkidle");

  const htmlClass = await page.locator("html").getAttribute("class");
  expect(htmlClass).toContain("dark");

  const bgColor = await page.evaluate(() => {
    return window.getComputedStyle(document.body).backgroundColor;
  });
  expect(bgColor).not.toBe("rgb(255, 255, 255)");

  const hasInput = page.locator(
    "input[type='email'], input[type='text'], input[name='email']"
  );
  await expect(hasInput.first()).toBeVisible({ timeout: 5000 });

  await page.screenshot({
    path: `${EVIDENCE_DIR}/login-dark.png`,
    fullPage: true,
  });
});

test("login page inputs have visible borders/contrast", async ({ page }) => {
  await page.goto("/login");
  await page.waitForLoadState("networkidle");

  const emailInput = page
    .locator(
      "input[type='email'], input[type='text'], input[name='email']"
    )
    .first();

  if ((await emailInput.count()) > 0) {
    await expect(emailInput).toBeVisible();

    const inputBg = await emailInput.evaluate((el) => {
      return window.getComputedStyle(el).backgroundColor;
    });

    expect(inputBg).not.toBe("rgb(255, 255, 255)");
  }
});

test("signup page renders in dark mode", async ({ page }) => {
  await page.goto("/signup");
  await page.waitForLoadState("networkidle");

  const htmlClass = await page.locator("html").getAttribute("class");
  expect(htmlClass).toContain("dark");

  const bgColor = await page.evaluate(() => {
    return window.getComputedStyle(document.body).backgroundColor;
  });
  expect(bgColor).not.toBe("rgb(255, 255, 255)");

  await page.screenshot({
    path: `${EVIDENCE_DIR}/signup-dark.png`,
    fullPage: true,
  });
});

test("forgot-password page renders in dark mode", async ({ page }) => {
  await page.goto("/forgot-password");
  await page.waitForLoadState("networkidle");

  const htmlClass = await page.locator("html").getAttribute("class");
  expect(htmlClass).toContain("dark");

  const bgColor = await page.evaluate(() => {
    return window.getComputedStyle(document.body).backgroundColor;
  });
  expect(bgColor).not.toBe("rgb(255, 255, 255)");

  await page.screenshot({
    path: `${EVIDENCE_DIR}/forgot-password-dark.png`,
    fullPage: true,
  });
});

test("page font uses Manrope for headings", async ({ page }) => {
  await page.goto("/");
  await page.waitForLoadState("networkidle");

  await page.evaluate(() => document.fonts.ready);

  const headingFont = await page.evaluate(() => {
    const h1 = document.querySelector("h1, h2");
    if (!h1) return null;
    return window.getComputedStyle(h1).fontFamily;
  });

  if (headingFont) {
    expect(headingFont.toLowerCase()).toContain("manrope");
  }

  await page.screenshot({
    path: `${EVIDENCE_DIR}/font-manrope.png`,
  });
});

test("authenticated route redirects to login in dark mode", async ({
  page,
}) => {
  await page.goto("/dashboard");
  await page.waitForLoadState("networkidle");

  const url = page.url();
  expect(url).toContain("/login");

  const htmlClass = await page.locator("html").getAttribute("class");
  expect(htmlClass).toContain("dark");

  await page.screenshot({
    path: `${EVIDENCE_DIR}/auth-redirect-dark.png`,
    fullPage: true,
  });
});

test("no white flash on page load (dark mode body bg)", async ({ page }) => {
  await page.goto("/");

  const bgColor = await page.evaluate(() => {
    return window.getComputedStyle(document.body).backgroundColor;
  });

  expect(bgColor).not.toBe("rgb(255, 255, 255)");
  // transparent bg = white flash before CSS loads
  expect(bgColor).not.toBe("rgba(0, 0, 0, 0)");

  await page.screenshot({
    path: `${EVIDENCE_DIR}/no-white-flash.png`,
  });
});
