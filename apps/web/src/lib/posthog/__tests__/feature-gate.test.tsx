import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import { FeatureGate } from "../feature-gate";

// Mock posthog-js/react
vi.mock("posthog-js/react", () => ({
  useFeatureFlagEnabled: vi.fn(),
}));

import { useFeatureFlagEnabled } from "posthog-js/react";

describe("FeatureGate", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("hides children when flag is false", () => {
    vi.mocked(useFeatureFlagEnabled).mockReturnValue(false);

    render(
      <FeatureGate flag="test-flag">
        <div>Protected Content</div>
      </FeatureGate>
    );

    expect(screen.queryByText("Protected Content")).not.toBeInTheDocument();
  });

  it("hides children when flag is undefined (loading)", () => {
    vi.mocked(useFeatureFlagEnabled).mockReturnValue(undefined);

    render(
      <FeatureGate flag="test-flag">
        <div>Protected Content</div>
      </FeatureGate>
    );

    expect(screen.queryByText("Protected Content")).not.toBeInTheDocument();
  });

  it("shows children when flag is true", () => {
    vi.mocked(useFeatureFlagEnabled).mockReturnValue(true);

    render(
      <FeatureGate flag="test-flag">
        <div>Protected Content</div>
      </FeatureGate>
    );

    expect(screen.getByText("Protected Content")).toBeInTheDocument();
  });

  it("renders fallback when flag is false and fallback provided", () => {
    vi.mocked(useFeatureFlagEnabled).mockReturnValue(false);

    render(
      <FeatureGate flag="test-flag" fallback={<div>Fallback Content</div>}>
        <div>Protected Content</div>
      </FeatureGate>
    );

    expect(screen.queryByText("Protected Content")).not.toBeInTheDocument();
    expect(screen.getByText("Fallback Content")).toBeInTheDocument();
  });

  it("renders fallback when flag is undefined and fallback provided", () => {
    vi.mocked(useFeatureFlagEnabled).mockReturnValue(undefined);

    render(
      <FeatureGate flag="test-flag" fallback={<div>Fallback Content</div>}>
        <div>Protected Content</div>
      </FeatureGate>
    );

    expect(screen.queryByText("Protected Content")).not.toBeInTheDocument();
    expect(screen.getByText("Fallback Content")).toBeInTheDocument();
  });

  it("passes the correct flag name to useFeatureFlagEnabled", () => {
    vi.mocked(useFeatureFlagEnabled).mockReturnValue(true);

    render(
      <FeatureGate flag="my-custom-flag">
        <div>Content</div>
      </FeatureGate>
    );

    expect(useFeatureFlagEnabled).toHaveBeenCalledWith("my-custom-flag");
  });
});
