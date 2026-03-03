import { render, screen, fireEvent, waitFor } from "@testing-library/react";
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

describe("Chapter Preview on Generate Page", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("shows help text about --- chapter markers near the textarea", () => {
    render(<GeneratePage />);

    // Help text contains 'Tip: Use' and '---' in a code element
    const helpText = screen.getByText(/tip.*use/i);
    expect(helpText).toBeInTheDocument();
    expect(helpText.querySelector("code")).toHaveTextContent("---");
  });

  it("shows chapter preview panel when text contains --- dividers", async () => {
    render(<GeneratePage />);

    const textarea = screen.getByPlaceholderText(/paste your text/i);

    fireEvent.change(textarea, {
      target: {
        value: "First chapter content.\n---\nSecond chapter content.",
      },
    });

    // Wait for debounce to fire
    await waitFor(() => {
      expect(screen.getByTestId("chapter-preview")).toBeInTheDocument();
    });

    expect(screen.getByText(/2 chapters detected/i)).toBeInTheDocument();
  });

  it("does not show chapter preview when text has no markers", async () => {
    render(<GeneratePage />);

    const textarea = screen.getByPlaceholderText(/paste your text/i);

    fireEvent.change(textarea, {
      target: { value: "Just some plain text without any chapter markers." },
    });

    // Wait for debounce, then verify no preview
    await waitFor(() => {
      // After debounce, the text should be processed but no chapters found
      expect(screen.queryByTestId("chapter-preview")).not.toBeInTheDocument();
    }, { timeout: 500 });
  });

  it("updates chapter preview when text changes", async () => {
    render(<GeneratePage />);

    const textarea = screen.getByPlaceholderText(/paste your text/i);

    // First: 2 chapters
    fireEvent.change(textarea, {
      target: {
        value: "Part one.\n---\nPart two.",
      },
    });

    await waitFor(() => {
      expect(screen.getByText(/2 chapters detected/i)).toBeInTheDocument();
    });

    // Then: 3 chapters
    fireEvent.change(textarea, {
      target: {
        value: "Part one.\n---\nPart two.\n---\nPart three.",
      },
    });

    await waitFor(() => {
      expect(screen.getByText(/3 chapters detected/i)).toBeInTheDocument();
    });
  });
});
