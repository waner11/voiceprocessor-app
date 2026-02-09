import { render, screen } from "@testing-library/react";
import { usePathname } from "next/navigation";
import { describe, it, expect, vi, beforeEach } from "vitest";
import SettingsLayout from "../layout";
import { useApiAccess } from "@/lib/posthog/use-api-access";

// Mock next/navigation
vi.mock("next/navigation", () => ({
  usePathname: vi.fn(),
}));

// Mock the useApiAccess hook
vi.mock("@/lib/posthog/use-api-access", () => ({
  useApiAccess: vi.fn(),
}));

describe("SettingsLayout", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(usePathname).mockReturnValue("/settings/profile");
  });

  it("renders all nav items when useApiAccess returns true", () => {
    vi.mocked(useApiAccess).mockReturnValue(true);

    render(
      <SettingsLayout>
        <div>Test Content</div>
      </SettingsLayout>
    );

    expect(screen.getByText("Profile")).toBeInTheDocument();
    expect(screen.getByText("API Keys")).toBeInTheDocument();
    expect(screen.getByText("Connections")).toBeInTheDocument();
    expect(screen.getByText("Billing")).toBeInTheDocument();
  });

  it("hides API Keys nav item when useApiAccess returns false", () => {
    vi.mocked(useApiAccess).mockReturnValue(false);

    render(
      <SettingsLayout>
        <div>Test Content</div>
      </SettingsLayout>
    );

    expect(screen.getByText("Profile")).toBeInTheDocument();
    expect(screen.queryByText("API Keys")).not.toBeInTheDocument();
    expect(screen.getByText("Connections")).toBeInTheDocument();
    expect(screen.getByText("Billing")).toBeInTheDocument();
  });

  it("renders children content", () => {
    vi.mocked(useApiAccess).mockReturnValue(true);

    render(
      <SettingsLayout>
        <div>Test Content</div>
      </SettingsLayout>
    );

    expect(screen.getByText("Test Content")).toBeInTheDocument();
  });

  it("renders Settings heading", () => {
    vi.mocked(useApiAccess).mockReturnValue(true);

    render(
      <SettingsLayout>
        <div>Test Content</div>
      </SettingsLayout>
    );

    expect(screen.getByText("Settings")).toBeInTheDocument();
  });

  it("applies active state styling to current pathname", () => {
    vi.mocked(useApiAccess).mockReturnValue(true);
    vi.mocked(usePathname).mockReturnValue("/settings/api-keys");

    render(
      <SettingsLayout>
        <div>Test Content</div>
      </SettingsLayout>
    );

    const apiKeysLink = screen.getByText("API Keys").closest("a");
    expect(apiKeysLink).toHaveClass("bg-blue-50");
  });
});
