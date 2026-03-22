import * as Sentry from "@sentry/nextjs";

function initSentryClient(): void {
  if (!process.env.NEXT_PUBLIC_SENTRY_DSN) {
    return;
  }

  const apiUrl = process.env.NEXT_PUBLIC_API_URL;
  const tracePropagationTargets = [
    /^https?:\/\/localhost(?::\d+)?/,
    /^\//,
    ...(apiUrl ? [apiUrl] : []),
  ];

  Sentry.init({
    dsn: process.env.NEXT_PUBLIC_SENTRY_DSN,
    sendDefaultPii: true,
    tracesSampleRate: 1.0,
    enableLogs: true,
    tracePropagationTargets,
  });
}

initSentryClient();

export const onRouterTransitionStart = Sentry.captureRouterTransitionStart;
