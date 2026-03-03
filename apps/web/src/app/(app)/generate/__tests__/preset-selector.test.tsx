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
        {
          id: "voice-2",
          name: "OpenAI Voice",
          provider: "OpenAI",
          language: "English",
          gender: "Female",
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

describe("Preset Selector on Generate Page", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders the Voice Preset section in the sidebar", () => {
    render(<GeneratePage />);
    expect(screen.getByText("Voice Preset")).toBeInTheDocument();
  });

  it("renders all 4 preset cards", () => {
    render(<GeneratePage />);
    expect(screen.getByRole("button", { name: /audiobook/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /conversational/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /dramatic/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /professional/i })).toBeInTheDocument();
  });

  it("shows parameter sliders (Speed visible by default for ElevenLabs)", () => {
    render(<GeneratePage />);
    // Default provider is ElevenLabs (first voice), so all 4 sliders should show
    expect(screen.getByLabelText(/speed/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/stability/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/similarity boost/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/style/i)).toBeInTheDocument();
  });

  it("selecting a preset highlights the card", () => {
    render(<GeneratePage />);
    const audiobookBtn = screen.getByRole("button", { name: /audiobook/i });
    fireEvent.click(audiobookBtn);
    expect(audiobookBtn.className).toMatch(/ring-blue|ring-2|bg-blue/);
  });

  it("shows only Speed slider when OpenAI voice is selected", () => {
    render(<GeneratePage />);

    // Select the OpenAI voice
    const openaiVoiceBtn = screen.getByText("OpenAI Voice").closest("button");
    if (openaiVoiceBtn) {
      fireEvent.click(openaiVoiceBtn);
    }

    // After selecting OpenAI voice, only Speed should be visible
    expect(screen.getByLabelText(/speed/i)).toBeInTheDocument();
    expect(screen.queryByLabelText(/stability/i)).not.toBeInTheDocument();
    expect(screen.queryByLabelText(/similarity boost/i)).not.toBeInTheDocument();
    expect(screen.queryByLabelText(/style/i)).not.toBeInTheDocument();
  });
});
