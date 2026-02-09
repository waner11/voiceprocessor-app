import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import { PostHogProvider } from "../provider";

// Mock posthog-js
vi.mock("posthog-js", () => ({
  default: {
    init: vi.fn(),
  },
}));

// Mock @posthog/react
vi.mock("@posthog/react", () => ({
  PostHogProvider: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="posthog-provider">{children}</div>
  ),
}));

describe("PostHogProvider", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    // Clear environment variables
    delete process.env.NEXT_PUBLIC_POSTHOG_KEY;
    delete process.env.NEXT_PUBLIC_POSTHOG_HOST;
  });

  it("renders children without crashing", () => {
    process.env.NEXT_PUBLIC_POSTHOG_KEY = "test-key";
    process.env.NEXT_PUBLIC_POSTHOG_HOST = "https://us.i.posthog.com";

    render(
      <PostHogProvider>
        <div>Test Child</div>
      </PostHogProvider>
    );

    expect(screen.getByText("Test Child")).toBeInTheDocument();
  });

  it("handles missing POSTHOG_KEY gracefully without crashing", () => {
    // No env vars set - should not crash
    expect(() => {
      render(
        <PostHogProvider>
          <div>Test Child</div>
        </PostHogProvider>
      );
    }).not.toThrow();

    expect(screen.getByText("Test Child")).toBeInTheDocument();
  });

  it("handles missing POSTHOG_HOST gracefully without crashing", () => {
    process.env.NEXT_PUBLIC_POSTHOG_KEY = "test-key";
    // No host set - should not crash

    expect(() => {
      render(
        <PostHogProvider>
          <div>Test Child</div>
        </PostHogProvider>
      );
    }).not.toThrow();

    expect(screen.getByText("Test Child")).toBeInTheDocument();
  });
});
