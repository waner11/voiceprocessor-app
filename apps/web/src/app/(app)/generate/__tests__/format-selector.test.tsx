import { render, screen, fireEvent } from "@testing-library/react";
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

  it("renders format dropdown with 4 options: MP3, WAV, OGG, FLAC", () => {
    render(<GeneratePage />);
    const formatLabel = screen.getByText("Output Format");
    expect(formatLabel).toBeInTheDocument();
    const options = screen.getAllByRole("option");
    const optionValues = options.map(o => o.textContent);
    expect(optionValues.some(v => v?.includes("MP3"))).toBe(true);
    expect(optionValues.some(v => v?.includes("WAV"))).toBe(true);
    expect(optionValues.some(v => v?.includes("OGG"))).toBe(true);
    expect(optionValues.some(v => v?.includes("FLAC"))).toBe(true);
  });

  it("has MP3 selected by default", () => {
    const { container } = render(<GeneratePage />);
    const formatSelect = container.querySelector("select[name='audioFormat']") as HTMLSelectElement;
    expect(formatSelect).toBeInTheDocument();
    expect(formatSelect.value).toBe("mp3");
  });

  it("updates form state when format selection changes", () => {
    const { container } = render(<GeneratePage />);
    const formatSelect = container.querySelector("select[name='audioFormat']") as HTMLSelectElement;
    expect(formatSelect).toBeInTheDocument();
    fireEvent.change(formatSelect, { target: { value: "wav" } });
    expect(formatSelect.value).toBe("wav");
  });
});
