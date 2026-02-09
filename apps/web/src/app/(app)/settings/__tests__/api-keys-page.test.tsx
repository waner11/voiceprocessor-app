import { render, screen } from "@testing-library/react";
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
    (useRouter as any).mockReturnValue({
      replace: mockReplace,
    });
  });

  it("redirects to /settings/profile when useApiAccess returns false", () => {
    (useApiAccess as any).mockReturnValue(false);

    const { container } = render(<ApiKeysSettingsPage />);

    expect(mockReplace).toHaveBeenCalledWith("/settings/profile");
    expect(container.firstChild).toBeNull();
  });

  it("renders page content when useApiAccess returns true", () => {
    (useApiAccess as any).mockReturnValue(true);

    render(<ApiKeysSettingsPage />);

    expect(mockReplace).not.toHaveBeenCalled();
    expect(screen.getByText("API Keys")).toBeInTheDocument();
  });

  it("returns null when access is denied", () => {
    (useApiAccess as any).mockReturnValue(false);

    const { container } = render(<ApiKeysSettingsPage />);

    expect(container.firstChild).toBeNull();
  });

  it("renders the create API key section when access is granted", () => {
    (useApiAccess as any).mockReturnValue(true);

    render(<ApiKeysSettingsPage />);

    expect(screen.getByText("API Keys")).toBeInTheDocument();
  });
});
