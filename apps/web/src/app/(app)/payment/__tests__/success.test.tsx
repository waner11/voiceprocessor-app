import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
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

const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  });

describe("PaymentSuccessPage", () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    localStorage.clear();
    vi.clearAllMocks();
    queryClient = createTestQueryClient();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it("renders success message and triggers confetti", () => {
    render(
      <QueryClientProvider client={queryClient}>
        <PaymentSuccessPage />
      </QueryClientProvider>
    );

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

    render(
      <QueryClientProvider client={queryClient}>
        <PaymentSuccessPage />
      </QueryClientProvider>
    );

    await waitFor(() => {
      expect(screen.getByText(/Starter Pack/)).toBeInTheDocument();
      expect(screen.getByText(/1,000 credits added/)).toBeInTheDocument();
    });
  });

  it("shows fallback message when localStorage is empty", () => {
    render(
      <QueryClientProvider client={queryClient}>
        <PaymentSuccessPage />
      </QueryClientProvider>
    );

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

    render(
      <QueryClientProvider client={queryClient}>
        <PaymentSuccessPage />
      </QueryClientProvider>
    );

    await waitFor(() => {
      expect(localStorage.getItem("voiceprocessor_checkout_pack")).toBeNull();
    });
  });

  it("displays current credit balance from auth store", () => {
    render(
      <QueryClientProvider client={queryClient}>
        <PaymentSuccessPage />
      </QueryClientProvider>
    );

    expect(screen.getByText("1,500")).toBeInTheDocument();
    expect(screen.getByText("credits")).toBeInTheDocument();
  });

  it("renders dashboard button with correct href", () => {
    render(
      <QueryClientProvider client={queryClient}>
        <PaymentSuccessPage />
      </QueryClientProvider>
    );

    const dashboardButton = screen.getByRole("link", {
      name: /Go to Dashboard/i,
    });
    expect(dashboardButton).toHaveAttribute("href", "/dashboard");
  });

  it("renders billing button with correct href", () => {
    render(
      <QueryClientProvider client={queryClient}>
        <PaymentSuccessPage />
      </QueryClientProvider>
    );

    const billingButton = screen.getByRole("link", { name: /View Billing/i });
    expect(billingButton).toHaveAttribute("href", "/settings/billing");
  });

  it("renders success icon", () => {
    const { container } = render(
      <QueryClientProvider client={queryClient}>
        <PaymentSuccessPage />
      </QueryClientProvider>
    );

    const svgs = container.querySelectorAll("svg");
    expect(svgs.length).toBeGreaterThan(0);
  });
});
