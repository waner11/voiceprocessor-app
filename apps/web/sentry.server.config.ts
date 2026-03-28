import * as Sentry from "@sentry/nextjs";

function initSentryServer(): void {
  if (!process.env.SENTRY_DSN) {
    return;
  }

  Sentry.init({
    dsn: process.env.SENTRY_DSN,
    tracesSampleRate: process.env.NODE_ENV === "production" ? 0.2 : 1.0,
    enableLogs: true,
    // GDPR: captures user IP, cookies, request headers for error context.
    // If EU users: disclose in privacy policy or switch to explicit Sentry.setUser().
    sendDefaultPii: true,
  });
}

initSentryServer();
