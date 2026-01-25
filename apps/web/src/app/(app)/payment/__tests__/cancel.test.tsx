import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import PaymentCancelPage from "../cancel/page";

vi.mock("next/link", () => ({
  default: ({ href, children, ...props }: { href: string; children: React.ReactNode; [key: string]: unknown }) => (
    <a href={href} {...props}>
      {children}
    </a>
  ),
}));

describe("PaymentCancelPage", () => {
  beforeEach(() => {
    localStorage.clear();
    vi.clearAllMocks();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it("renders cancel message", () => {
    render(<PaymentCancelPage />);

    expect(screen.getByText("Payment Cancelled")).toBeInTheDocument();
  });

  it("shows friendly not charged message", () => {
    render(<PaymentCancelPage />);

    expect(
      screen.getByText(
        "No worries! Your card was not charged. You can try again anytime."
      )
    ).toBeInTheDocument();
  });

  it("renders return to billing button with correct href", () => {
    render(<PaymentCancelPage />);

    const billingButton = screen.getByRole("link", {
      name: /Return to Billing/i,
    });
    expect(billingButton).toHaveAttribute("href", "/settings/billing");
  });

  it("renders contact support link", () => {
    render(<PaymentCancelPage />);

    const supportLink = screen.getByRole("link", { name: /Contact Support/i });
    expect(supportLink).toHaveAttribute("href", "mailto:support@voiceprocessor.com");
  });

  it("clears localStorage on mount", async () => {
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

    render(<PaymentCancelPage />);

    await waitFor(() => {
      expect(localStorage.getItem("voiceprocessor_checkout_pack")).toBeNull();
    });
  });

  it("renders cancel icon", () => {
    const { container } = render(<PaymentCancelPage />);

    const svgs = container.querySelectorAll("svg");
    expect(svgs.length).toBeGreaterThan(0);
  });
});
