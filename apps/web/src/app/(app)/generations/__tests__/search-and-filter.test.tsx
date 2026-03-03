import { render, screen, fireEvent, act, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import GenerationsPage from "../page";

// Mock useGenerations hook
const mockUseGenerations = vi.fn();

vi.mock("@/hooks", () => ({
  useGenerations: (...args: unknown[]) => mockUseGenerations(...args),
}));

vi.mock("@/hooks/useGenerationHub", () => ({
  useGenerationHub: vi.fn(),
}));

const mockGenerations = [
  {
    id: "gen-1",
    status: "Completed" as const,
    characterCount: 5000,
    provider: "ElevenLabs",
    createdAt: "2026-01-15T10:00:00Z",
    audioDurationMs: 30000,
    progress: 100,
    creditsUsed: 50,
  },
  {
    id: "gen-2",
    status: "Processing" as const,
    characterCount: 3000,
    provider: "OpenAI",
    createdAt: "2026-01-16T10:00:00Z",
    audioDurationMs: null,
    progress: 45,
    creditsUsed: null,
  },
];

const defaultHookReturn = {
  data: {
    items: mockGenerations,
    page: 1,
    pageSize: 10,
    totalCount: 2,
    totalPages: 1,
    hasNextPage: false,
    hasPreviousPage: false,
  },
  isLoading: false,
  error: null,
};

describe("Generations Page - Search and Provider Filter", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.useFakeTimers();
    mockUseGenerations.mockReturnValue(defaultHookReturn);
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  // --- Search Input Tests ---

  it("renders a search input with placeholder for text search", () => {
    render(<GenerationsPage />);
    const searchInput = screen.getByPlaceholderText("Search generations...");
    expect(searchInput).toBeInTheDocument();
    expect(searchInput).toHaveAttribute("type", "text");
  });

  it("typing in search input triggers debounced server-side search after 300ms", () => {
    render(<GenerationsPage />);
    const searchInput = screen.getByPlaceholderText("Search generations...");

    fireEvent.change(searchInput, { target: { value: "hello world" } });

    // Before debounce fires, hook should NOT have been called with search param
    expect(mockUseGenerations).not.toHaveBeenCalledWith(
      expect.objectContaining({ search: "hello world" })
    );

    // Advance timers past debounce
    act(() => {
      vi.advanceTimersByTime(300);
    });

    // After debounce, hook should be called with search param
    expect(mockUseGenerations).toHaveBeenCalledWith(
      expect.objectContaining({ search: "hello world" })
    );
  });

  it("shows clear search button (X) when search has text", () => {
    render(<GenerationsPage />);
    const searchInput = screen.getByPlaceholderText("Search generations...");

    // No clear button initially
    expect(screen.queryByLabelText("Clear search")).not.toBeInTheDocument();

    fireEvent.change(searchInput, { target: { value: "test" } });

    // Clear button should appear
    expect(screen.getByLabelText("Clear search")).toBeInTheDocument();
  });

  it("clicking clear search button resets search input and triggers API call", () => {
    render(<GenerationsPage />);
    const searchInput = screen.getByPlaceholderText(
      "Search generations..."
    ) as HTMLInputElement;

    fireEvent.change(searchInput, { target: { value: "test" } });
    act(() => {
      vi.advanceTimersByTime(300);
    });

    const clearButton = screen.getByLabelText("Clear search");
    fireEvent.click(clearButton);

    // Input should be cleared
    expect(searchInput.value).toBe("");

    // After clearing, advance debounce
    act(() => {
      vi.advanceTimersByTime(300);
    });

    // Hook should be called without search (or with empty search)
    const lastCall =
      mockUseGenerations.mock.calls[mockUseGenerations.mock.calls.length - 1];
    expect(
      lastCall[0].search === undefined || lastCall[0].search === ""
    ).toBe(true);
  });

  // --- Provider Filter Tests ---

  it("renders a provider filter dropdown with All + provider options", () => {
    render(<GenerationsPage />);
    const providerSelect = screen.getByDisplayValue("All Providers");
    expect(providerSelect).toBeInTheDocument();

    // Check all provider options exist
    expect(screen.getByText("All Providers")).toBeInTheDocument();
    expect(screen.getByText("ElevenLabs")).toBeInTheDocument();
    expect(screen.getByText("OpenAI")).toBeInTheDocument();
    expect(screen.getByText("Google Cloud")).toBeInTheDocument();
    expect(screen.getByText("Amazon Polly")).toBeInTheDocument();
  });

  it("selecting a provider triggers server-side filter via hook", () => {
    render(<GenerationsPage />);
    const providerSelect = screen.getByDisplayValue("All Providers");

    fireEvent.change(providerSelect, { target: { value: "ElevenLabs" } });

    // Hook should be called with provider param
    expect(mockUseGenerations).toHaveBeenCalledWith(
      expect.objectContaining({ provider: "ElevenLabs" })
    );
  });

  it("selecting 'All Providers' clears the provider filter", () => {
    render(<GenerationsPage />);
    const providerSelect = screen.getByDisplayValue("All Providers");

    // Select a provider first
    fireEvent.change(providerSelect, { target: { value: "OpenAI" } });
    // Then select All
    fireEvent.change(providerSelect, { target: { value: "" } });

    const lastCall =
      mockUseGenerations.mock.calls[mockUseGenerations.mock.calls.length - 1];
    expect(
      lastCall[0].provider === undefined || lastCall[0].provider === ""
    ).toBe(true);
  });

  // --- Empty State Tests ---

  it("shows 'No results' empty state when search returns 0 items", () => {
    mockUseGenerations.mockReturnValue({
      ...defaultHookReturn,
      data: {
        ...defaultHookReturn.data,
        items: [],
        totalCount: 0,
      },
    });

    render(<GenerationsPage />);

    // Set a search term so hasActiveFilters is true
    const searchInput = screen.getByPlaceholderText("Search generations...");
    fireEvent.change(searchInput, { target: { value: "nonexistent" } });
    act(() => {
      vi.advanceTimersByTime(300);
    });

    expect(
      screen.getByText(/no generations found/i)
    ).toBeInTheDocument();
  });

  // --- Combined Filter Tests ---

  it("search and provider filter work together", () => {
    render(<GenerationsPage />);
    const searchInput = screen.getByPlaceholderText("Search generations...");
    const providerSelect = screen.getByDisplayValue("All Providers");

    // Set provider
    fireEvent.change(providerSelect, { target: { value: "ElevenLabs" } });

    // Type search
    fireEvent.change(searchInput, { target: { value: "chapter" } });
    act(() => {
      vi.advanceTimersByTime(300);
    });

    // Hook should be called with both params
    expect(mockUseGenerations).toHaveBeenCalledWith(
      expect.objectContaining({
        search: "chapter",
        provider: "ElevenLabs",
      })
    );
  });

  it("resets page to 1 when search changes", () => {
    render(<GenerationsPage />);
    const searchInput = screen.getByPlaceholderText("Search generations...");

    fireEvent.change(searchInput, { target: { value: "test" } });
    act(() => {
      vi.advanceTimersByTime(300);
    });

    // Page should be reset to 1
    expect(mockUseGenerations).toHaveBeenCalledWith(
      expect.objectContaining({ page: 1 })
    );
  });

  it("resets page to 1 when provider changes", () => {
    render(<GenerationsPage />);
    const providerSelect = screen.getByDisplayValue("All Providers");

    fireEvent.change(providerSelect, { target: { value: "OpenAI" } });

    expect(mockUseGenerations).toHaveBeenCalledWith(
      expect.objectContaining({ page: 1 })
    );
  });
});
