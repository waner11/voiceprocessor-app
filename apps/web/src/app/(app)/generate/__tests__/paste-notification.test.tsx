import { render, screen, fireEvent, waitFor, act } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import GeneratePage from "../page";

// Mock the hooks
vi.mock("@/hooks", () => ({
  useVoices: vi.fn(() => ({
    data: {
      items: [
        {
          id: "voice-1",
          name: "Test Voice",
          provider: "ElevenLabs",
          language: "English",
          gender: "Male",
          previewUrl: null,
        },
      ],
    },
    isLoading: false,
  })),
  useEstimateCost: vi.fn(() => ({
    mutate: vi.fn(),
    data: null,
    isPending: false,
  })),
  useCreateGeneration: vi.fn(() => ({
    mutate: vi.fn(),
    isPending: false,
  })),
}));

vi.mock("next/navigation", () => ({
  useRouter: vi.fn(() => ({
    push: vi.fn(),
  })),
}));

describe("Paste Notification Timer Cleanup", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.runOnlyPendingTimers();
    vi.useRealTimers();
  });

  it("clears timer when component unmounts (no orphaned timers)", async () => {
    const { unmount } = render(<GeneratePage />);
    const textarea = screen.getByPlaceholderText(/paste your text/i) as HTMLTextAreaElement;

    // Set initial text to allow paste
    fireEvent.change(textarea, { target: { value: "a".repeat(499_990) } });

    // Simulate paste that triggers truncation and timer
    const pasteData = "b".repeat(20);
    const clipboardData = {
      getData: () => pasteData,
    };
    fireEvent.paste(textarea, { clipboardData });

    // Notification should appear
    expect(screen.getByText(/text was truncated/i)).toBeInTheDocument();

    // Unmount component
    unmount();

    // Advance timers - should not throw or cause warnings
    expect(() => {
      vi.advanceTimersByTime(5000);
    }).not.toThrow();
  });

  it("clears previous timer before setting new one on rapid consecutive pastes", async () => {
    render(<GeneratePage />);
    const textarea = screen.getByPlaceholderText(/paste your text/i) as HTMLTextAreaElement;

    act(() => {
      fireEvent.change(textarea, { target: { value: "a".repeat(499_990) } });
    });

    act(() => {
      const pasteData1 = "b".repeat(5);
      fireEvent.paste(textarea, {
        clipboardData: { getData: () => pasteData1 },
      });
    });

    expect(screen.getByText(/text was truncated/i)).toBeInTheDocument();

    act(() => {
      vi.advanceTimersByTime(2000);
    });

    act(() => {
      fireEvent.paste(textarea, {
        clipboardData: { getData: () => "c".repeat(5) },
      });
    });

    expect(screen.getByText(/text was truncated/i)).toBeInTheDocument();

    act(() => {
      vi.advanceTimersByTime(3000);
    });

    expect(screen.getByText(/text was truncated/i)).toBeInTheDocument();

    act(() => {
      vi.advanceTimersByTime(1000);
    });

    expect(screen.queryByText(/text was truncated/i)).not.toBeInTheDocument();
  });

  it("notification disappears after 5 seconds", async () => {
    render(<GeneratePage />);
    const textarea = screen.getByPlaceholderText(/paste your text/i) as HTMLTextAreaElement;

    act(() => {
      fireEvent.change(textarea, { target: { value: "a".repeat(499_990) } });
    });

    act(() => {
      const pasteData = "b".repeat(20);
      const clipboardData = {
        getData: () => pasteData,
      };
      fireEvent.paste(textarea, { clipboardData });
    });

    expect(screen.getByText(/text was truncated/i)).toBeInTheDocument();

    act(() => {
      vi.advanceTimersByTime(4999);
    });

    expect(screen.getByText(/text was truncated/i)).toBeInTheDocument();

    act(() => {
      vi.advanceTimersByTime(1);
    });

    expect(screen.queryByText(/text was truncated/i)).not.toBeInTheDocument();
  });
});
