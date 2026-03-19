import { render, screen, fireEvent, act } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import GeneratePage from "../page";

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

  it("clears timer when component unmounts (no orphaned timers)", () => {
    const { unmount } = render(<GeneratePage />);
    const textarea = screen.getByPlaceholderText(/paste your text/i) as HTMLTextAreaElement;

    act(() => {
      fireEvent.change(textarea, { target: { value: "a".repeat(499_990) } });
    });

    act(() => {
      fireEvent.paste(textarea, {
        clipboardData: { getData: () => "b".repeat(20) },
      });
    });

    expect(screen.getByText(/text was truncated/i)).toBeInTheDocument();

    unmount();

    expect(() => {
      vi.advanceTimersByTime(5000);
    }).not.toThrow();
  });

  it("only one timer is active at a time (previous timer cleared on new paste)", () => {
    render(<GeneratePage />);
    const textarea = screen.getByPlaceholderText(/paste your text/i) as HTMLTextAreaElement;

    act(() => {
      fireEvent.change(textarea, { target: { value: "a".repeat(499_990) } });
    });

    act(() => {
      fireEvent.paste(textarea, {
        clipboardData: { getData: () => "b".repeat(20) },
      });
    });

    expect(screen.getByText(/text was truncated/i)).toBeInTheDocument();

    act(() => {
      vi.advanceTimersByTime(2000);
    });

    act(() => {
      fireEvent.paste(textarea, {
        clipboardData: { getData: () => "c".repeat(20) },
      });
    });

    act(() => {
      vi.advanceTimersByTime(3000);
    });

    expect(screen.getByText(/text was truncated/i)).toBeInTheDocument();

    act(() => {
      vi.advanceTimersByTime(2000);
    });

    expect(screen.queryByText(/text was truncated/i)).not.toBeInTheDocument();
  });

  it("notification disappears after 5 seconds", () => {
    render(<GeneratePage />);
    const textarea = screen.getByPlaceholderText(/paste your text/i) as HTMLTextAreaElement;

    act(() => {
      fireEvent.change(textarea, { target: { value: "a".repeat(499_990) } });
    });

    act(() => {
      fireEvent.paste(textarea, {
        clipboardData: { getData: () => "b".repeat(20) },
      });
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
