import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook } from "@testing-library/react";
import { useApiAccess } from "../use-api-access";

vi.mock("posthog-js/react", () => ({
  useFeatureFlagEnabled: vi.fn(),
}));

import { useFeatureFlagEnabled } from "posthog-js/react";

describe("useApiAccess", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("returns false when flag is false", () => {
    vi.mocked(useFeatureFlagEnabled).mockReturnValue(false);

    const { result } = renderHook(() => useApiAccess());

    expect(result.current).toBe(false);
  });

  it("returns false when flag is undefined (loading)", () => {
    vi.mocked(useFeatureFlagEnabled).mockReturnValue(undefined);

    const { result } = renderHook(() => useApiAccess());

    expect(result.current).toBe(false);
  });

  it("returns true when flag is true", () => {
    vi.mocked(useFeatureFlagEnabled).mockReturnValue(true);

    const { result } = renderHook(() => useApiAccess());

    expect(result.current).toBe(true);
  });

  it("calls useFeatureFlagEnabled with 'enable-api-access' flag", () => {
    vi.mocked(useFeatureFlagEnabled).mockReturnValue(false);

    renderHook(() => useApiAccess());

    expect(useFeatureFlagEnabled).toHaveBeenCalledWith("enable-api-access");
  });
});
