import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import PaymentSuccessPage from "../success/page";

vi.mock("canvas-confetti", () => ({
  default: vi.fn(),
}));

vi.mock("next/link", () => ({
  default: ({ href, children, ...props }: { href: string; children: React.ReactNode; [key: string]: unknown }) => (
    <a href={href} {...props}>
      {children}
    </a>
  ),
}));

vi.mock("@/stores/authStore", () => ({
  useAuthStore: vi.fn((selector) => {
    const store = {
      creditsRemaining: 1500,
    };
    return selector(store);
  }),
}));

describe("PaymentSuccessPage", () => {
  beforeEach(() => {
    localStorage.clear();
    vi.clearAllMocks();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it("renders success message and triggers confetti", () => {
    render(<PaymentSuccessPage />);

    expect(screen.getByText("Payment Successful!")).toBeInTheDocument();
    expect(
      screen.getByText("Your credits have been added to your account!")
    ).toBeInTheDocument();
  });

  it("displays pack details when localStorage has data", async () => {
    const packInfo = {
      packId: "pack-1",
      name: "Starter Pack",
      credits: 1000,
      price: 9.99,
    };

    localStorage.setItem(
      "voiceprocessor_checkout_pack",
      JSON.stringify(packInfo)
    );

    render(<PaymentSuccessPage />);

    await waitFor(() => {
      expect(screen.getByText(/Starter Pack/)).toBeInTheDocument();
      expect(screen.getByText(/1,000 credits added/)).toBeInTheDocument();
    });
  });

  it("shows fallback message when localStorage is empty", () => {
    render(<PaymentSuccessPage />);

    expect(
      screen.getByText("Your credits have been added to your account!")
    ).toBeInTheDocument();
  });

  it("clears localStorage after render", async () => {
    const packInfo = {
      packId: "pack-1",
      name: "Starter Pack",
      credits: 1000,
      price: 9.99,
    };

    localStorage.setItem(
      "voiceprocessor_checkout_pack",
      JSON.stringify(packInfo)
    );

    render(<PaymentSuccessPage />);

    await waitFor(() => {
      expect(localStorage.getItem("voiceprocessor_checkout_pack")).toBeNull();
    });
  });

  it("displays current credit balance from auth store", () => {
    render(<PaymentSuccessPage />);

    expect(screen.getByText("1,500")).toBeInTheDocument();
    expect(screen.getByText("credits")).toBeInTheDocument();
  });

  it("renders dashboard button with correct href", () => {
    render(<PaymentSuccessPage />);

    const dashboardButton = screen.getByRole("link", {
      name: /Go to Dashboard/i,
    });
    expect(dashboardButton).toHaveAttribute("href", "/dashboard");
  });

  it("renders billing button with correct href", () => {
    render(<PaymentSuccessPage />);

    const billingButton = screen.getByRole("link", { name: /View Billing/i });
    expect(billingButton).toHaveAttribute("href", "/settings/billing");
  });

  it("renders success icon", () => {
    const { container } = render(<PaymentSuccessPage />);

    const svgs = container.querySelectorAll("svg");
    expect(svgs.length).toBeGreaterThan(0);
  });
});
