import * as Sentry from "@sentry/nextjs";

function initSentryServer(): void {
  if (!process.env.SENTRY_DSN) {
    return;
  }

  Sentry.init({
    dsn: process.env.SENTRY_DSN,
    tracesSampleRate: 1.0,
    enableLogs: true,
    sendDefaultPii: true,
  });
}

initSentryServer();
