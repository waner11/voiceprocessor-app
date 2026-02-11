import { render, screen, waitFor } from "@testing-library/react";
import { useRouter } from "next/navigation";
import { describe, it, expect, vi, beforeEach } from "vitest";
import ApiKeysSettingsPage from "../api-keys/page";
import { useApiAccess } from "@/lib/posthog/use-api-access";

vi.mock("next/navigation", () => ({
  useRouter: vi.fn(),
}));

vi.mock("@/lib/posthog/use-api-access", () => ({
  useApiAccess: vi.fn(),
}));

describe("ApiKeysSettingsPage", () => {
  const mockReplace = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useRouter).mockReturnValue({
      replace: mockReplace,
      push: vi.fn(),
      back: vi.fn(),
      forward: vi.fn(),
      refresh: vi.fn(),
      prefetch: vi.fn(),
    });
  });

  it("redirects to /settings/profile when useApiAccess returns false", async () => {
    vi.mocked(useApiAccess).mockReturnValue(false);

    const { container } = render(<ApiKeysSettingsPage />);

    await waitFor(() => {
      expect(mockReplace).toHaveBeenCalledWith("/settings/profile");
    });
    expect(container.firstChild).toBeNull();
  });

  it("renders page content when useApiAccess returns true", () => {
    vi.mocked(useApiAccess).mockReturnValue(true);

    render(<ApiKeysSettingsPage />);

    expect(mockReplace).not.toHaveBeenCalled();
    expect(screen.getByText("API Keys")).toBeInTheDocument();
  });
});
