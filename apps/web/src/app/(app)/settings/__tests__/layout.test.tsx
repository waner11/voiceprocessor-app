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
    (usePathname as any).mockReturnValue("/settings/profile");
  });

  it("renders all nav items when useApiAccess returns true", () => {
    (useApiAccess as any).mockReturnValue(true);

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
    (useApiAccess as any).mockReturnValue(false);

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
    (useApiAccess as any).mockReturnValue(true);

    render(
      <SettingsLayout>
        <div>Test Content</div>
      </SettingsLayout>
    );

    expect(screen.getByText("Test Content")).toBeInTheDocument();
  });

  it("renders Settings heading", () => {
    (useApiAccess as any).mockReturnValue(true);

    render(
      <SettingsLayout>
        <div>Test Content</div>
      </SettingsLayout>
    );

    expect(screen.getByText("Settings")).toBeInTheDocument();
  });

  it("applies active state styling to current pathname", () => {
    (useApiAccess as any).mockReturnValue(true);
    (usePathname as any).mockReturnValue("/settings/api-keys");

    const { container } = render(
      <SettingsLayout>
        <div>Test Content</div>
      </SettingsLayout>
    );

    const apiKeysLink = screen.getByText("API Keys").closest("a");
    expect(apiKeysLink).toHaveClass("bg-blue-50");
  });
});
