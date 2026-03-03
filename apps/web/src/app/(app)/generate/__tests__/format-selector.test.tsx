import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
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

describe("Format Selector on Generate Page", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders format dropdown with 4 options: MP3, WAV, OGG, FLAC", async () => {
    render(<GeneratePage />);

    // Look for the format selector label
    const formatLabel = screen.getByText("Output Format");
    expect(formatLabel).toBeInTheDocument();

    // Check for all 4 format options
    await waitFor(() => {
      expect(screen.getByText(/MP3/)).toBeInTheDocument();
      expect(screen.getByText(/WAV/)).toBeInTheDocument();
      expect(screen.getByText(/OGG/)).toBeInTheDocument();
      expect(screen.getByText(/FLAC/)).toBeInTheDocument();
    });
  });

  it("has MP3 selected by default", async () => {
    render(<GeneratePage />);

    await waitFor(() => {
      const mp3Option = screen.getByDisplayValue("mp3") as HTMLSelectElement;
      expect(mp3Option.value).toBe("mp3");
    });
  });

  it("updates form state when format selection changes", async () => {
    const { container } = render(<GeneratePage />);

    await waitFor(() => {
      const formatSelect = container.querySelector(
        "select[name='audioFormat']"
      ) as HTMLSelectElement;
      expect(formatSelect).toBeInTheDocument();

      // Change to WAV
      fireEvent.change(formatSelect, { target: { value: "wav" } });

      expect(formatSelect.value).toBe("wav");
    });
  });
});
