import { describe, it, expect, vi } from "vitest";

vi.mock("@sentry/nextjs", () => ({
  init: vi.fn(),
  captureRouterTransitionStart: vi.fn(),
}));

describe("instrumentation-client", () => {
  it("exports onRouterTransitionStart as a function", async () => {
    const instrumentationModule = await import("../instrumentation-client.ts");

    expect(instrumentationModule.onRouterTransitionStart).toBeDefined();
    expect(typeof instrumentationModule.onRouterTransitionStart).toBe("function");
  });
});
