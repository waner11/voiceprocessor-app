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
    // GDPR: captures user IP, cookies, request headers for error context.
    // If EU users: disclose in privacy policy or switch to explicit Sentry.setUser().
    sendDefaultPii: true,
    tracesSampleRate: process.env.NODE_ENV === "production" ? 0.2 : 1.0,
    enableLogs: true,
    tracePropagationTargets,
  });
}

initSentryClient();

export const onRouterTransitionStart = Sentry.captureRouterTransitionStart;
