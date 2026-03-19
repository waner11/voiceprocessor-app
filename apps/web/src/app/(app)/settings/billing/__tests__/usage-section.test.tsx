import { describe, it, expect, beforeEach, vi } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import BillingSettingsPage from "../page";

const mockRefetch = vi.fn();
let mockUseUsage = {
  data: undefined as
    | {
        creditsUsedThisMonth: number;
        creditsRemaining: number;
        generationsCount: number;
        totalAudioMinutes: number;
      }
    | undefined,
  isLoading: false,
  error: null as Error | null,
  refetch: mockRefetch,
};

vi.mock("@/hooks/useUsage", () => ({
  useUsage: () => mockUseUsage,
}));

vi.mock("@/hooks/usePayment", () => ({
  usePayment: () => ({
    startCheckout: vi.fn(),
    isProcessing: false,
    error: null,
  }),
}));

vi.mock("@/lib/api/payment/service", () => ({
  paymentService: {
    fetchCreditPacks: vi.fn().mockResolvedValue([]),
    fetchPaymentHistory: vi.fn().mockResolvedValue([]),
  },
}));

vi.mock("@/stores/authStore", () => ({
  useAuthStore: vi.fn((selector) => {
    const store = { creditsRemaining: 5000 };
    return selector(store);
  }),
}));

const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  });

function renderBillingPage() {
  const queryClient = createTestQueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      <BillingSettingsPage />
    </QueryClientProvider>
  );
}

describe("Billing Page — Usage This Month Section", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseUsage = {
      data: undefined,
      isLoading: false,
      error: null,
      refetch: mockRefetch,
    };
  });

  it("renders real credits used value from useUsage data", () => {
    mockUseUsage.data = {
      creditsUsedThisMonth: 7250,
      creditsRemaining: 2750,
      generationsCount: 14,
      totalAudioMinutes: 88,
    };

    renderBillingPage();

    expect(screen.getByText("7,250")).toBeInTheDocument();
    expect(screen.getByText("14")).toBeInTheDocument();
    expect(screen.getByText("88")).toBeInTheDocument();
    expect(screen.queryByText("45,000")).not.toBeInTheDocument();
  });

  it("does not render a character progress bar", () => {
    mockUseUsage.data = {
      creditsUsedThisMonth: 100,
      creditsRemaining: 900,
      generationsCount: 2,
      totalAudioMinutes: 5,
    };

    const { container } = renderBillingPage();

    expect(container.querySelector('[role="progressbar"]')).toBeNull();
    expect(screen.queryByText("Characters")).not.toBeInTheDocument();
    expect(screen.queryByText(/characters remaining/i)).not.toBeInTheDocument();
  });

  it("shows loading skeleton while usage data is fetching", () => {
    mockUseUsage.isLoading = true;
    mockUseUsage.data = undefined;

    const { container } = renderBillingPage();

    const usageHeading = screen.getByText("Usage This Month");
    const usageSection = usageHeading.closest("section");
    expect(usageSection).not.toBeNull();

    const skeletons = usageSection!.querySelectorAll(".animate-pulse");
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it("shows error message with retry button on failure", () => {
    mockUseUsage.error = new Error("Network error");
    mockUseUsage.data = undefined;

    renderBillingPage();

    expect(
      screen.getByText("Failed to load usage data")
    ).toBeInTheDocument();

    const retryButton = screen.getByRole("button", { name: /try again/i });
    expect(retryButton).toBeInTheDocument();

    fireEvent.click(retryButton);
    expect(mockRefetch).toHaveBeenCalledTimes(1);
  });

  it("renders zero values for a new user with no usage", () => {
    mockUseUsage.data = {
      creditsUsedThisMonth: 0,
      creditsRemaining: 0,
      generationsCount: 0,
      totalAudioMinutes: 0,
    };

    renderBillingPage();

    const zeros = screen.getAllByText("0");
    expect(zeros.length).toBeGreaterThanOrEqual(3);
  });
});
