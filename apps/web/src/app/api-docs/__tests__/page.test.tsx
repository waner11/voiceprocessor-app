import { render, screen } from "@testing-library/react";
import { useRouter } from "next/navigation";
import { describe, it, expect, vi, beforeEach } from "vitest";
import ApiDocsPage from "../page";
import { useApiAccess } from "@/lib/posthog/use-api-access";

vi.mock("next/navigation", () => ({
  useRouter: vi.fn(),
}));

vi.mock("@/lib/posthog/use-api-access", () => ({
  useApiAccess: vi.fn(),
}));

describe("ApiDocsPage", () => {
  const mockReplace = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useRouter).mockReturnValue({
      replace: mockReplace,
    } as any);
  });

  it("redirects to / when useApiAccess returns false", () => {
    vi.mocked(useApiAccess).mockReturnValue(false);

    const { container } = render(<ApiDocsPage />);

    expect(mockReplace).toHaveBeenCalledWith("/");
    expect(container.firstChild).toBeNull();
  });

  it("renders page content when useApiAccess returns true", () => {
    vi.mocked(useApiAccess).mockReturnValue(true);

    render(<ApiDocsPage />);

    expect(mockReplace).not.toHaveBeenCalled();
    expect(screen.getByText("API Documentation")).toBeInTheDocument();
  });

  it("returns null when access is denied", () => {
    vi.mocked(useApiAccess).mockReturnValue(false);

    const { container } = render(<ApiDocsPage />);

    expect(container.firstChild).toBeNull();
  });

  it("renders the authentication section when access is granted", () => {
    vi.mocked(useApiAccess).mockReturnValue(true);

    render(<ApiDocsPage />);

    expect(screen.getByText("Authenticate API requests using Bearer tokens in the Authorization header.")).toBeInTheDocument();
  });
});
