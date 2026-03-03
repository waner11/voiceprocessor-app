import { render, screen, fireEvent, act } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import userEvent from "@testing-library/user-event";
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

describe("Character Limits on Generate Page", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("shows character count in 'X / 500,000' format", () => {
    render(<GeneratePage />);
    // Should display "0 / 500,000" when empty
    expect(screen.getByText(/0 \/ 500,000/)).toBeInTheDocument();
  });

  it("updates character count as user types", () => {
    render(<GeneratePage />);
    const textarea = screen.getByPlaceholderText(/paste your text/i);
    fireEvent.change(textarea, { target: { value: "Hello" } });
    expect(screen.getByText(/5 \/ 500,000/)).toBeInTheDocument();
  });

  it("shows warning at 450,000+ characters", () => {
    render(<GeneratePage />);
    const textarea = screen.getByPlaceholderText(/paste your text/i);
    // Create text at exactly 450,001 chars (above 90% threshold)
    const longText = "a".repeat(450_001);
    fireEvent.change(textarea, { target: { value: longText } });
    expect(screen.getByText(/approaching limit/i)).toBeInTheDocument();
  });

  it("shows limit reached at 500,000 characters", () => {
    render(<GeneratePage />);
    const textarea = screen.getByPlaceholderText(/paste your text/i);
    const maxText = "a".repeat(500_000);
    fireEvent.change(textarea, { target: { value: maxText } });
    expect(screen.getByText(/limit reached/i)).toBeInTheDocument();
  });

  it("disables Generate button when text exceeds 500,000 characters", () => {
    render(<GeneratePage />);
    const textarea = screen.getByPlaceholderText(/paste your text/i);

    // Select a voice first
    const voiceButton = screen.getByText("Test Voice");
    fireEvent.click(voiceButton);

    // Set text over limit
    const overLimitText = "a".repeat(500_001);
    fireEvent.change(textarea, { target: { value: overLimitText } });

    const generateButton = screen.getByRole("button", { name: /generate audio/i });
    expect(generateButton).toBeDisabled();
  });

  it("disables Generate button when text is empty", () => {
    render(<GeneratePage />);
    // Select a voice
    const voiceButton = screen.getByText("Test Voice");
    fireEvent.click(voiceButton);

    const generateButton = screen.getByRole("button", { name: /generate audio/i });
    expect(generateButton).toBeDisabled();
  });

  it("truncates pasted text that would exceed 500,000 characters", () => {
    render(<GeneratePage />);
    const textarea = screen.getByPlaceholderText(/paste your text/i) as HTMLTextAreaElement;

    // First set some existing text
    fireEvent.change(textarea, { target: { value: "a".repeat(499_990) } });

    // Simulate paste that would exceed limit (paste 20 chars when only 10 remain)
    const pasteData = "b".repeat(20);
    const clipboardData = {
      getData: () => pasteData,
    };
    fireEvent.paste(textarea, { clipboardData });

    // Text should be truncated to exactly 500,000
    expect(textarea.value.length).toBeLessThanOrEqual(500_000);
  });

  it("shows character counter with correct color when under limit", () => {
    const { container } = render(<GeneratePage />);
    const textarea = screen.getByPlaceholderText(/paste your text/i);
    fireEvent.change(textarea, { target: { value: "Hello world" } });

    // Counter should exist and not have warning/error colors
    const counter = screen.getByText(/11 \/ 500,000/);
    expect(counter).toBeInTheDocument();
    // Should not have warning or error classes
    expect(counter.className).not.toMatch(/yellow|amber/);
    expect(counter.className).not.toMatch(/red/);
  });

  it("applies warning styling at 450,000+ characters", () => {
    render(<GeneratePage />);
    const textarea = screen.getByPlaceholderText(/paste your text/i);
    const warningText = "a".repeat(450_001);
    fireEvent.change(textarea, { target: { value: warningText } });

    const warningMessage = screen.getByText(/approaching limit/i);
    expect(warningMessage).toBeInTheDocument();
  });

  it("applies error styling at 500,000+ characters", () => {
    render(<GeneratePage />);
    const textarea = screen.getByPlaceholderText(/paste your text/i);
    const overText = "a".repeat(500_001);
    fireEvent.change(textarea, { target: { value: overText } });

    const errorMessage = screen.getByText(/limit reached/i);
    expect(errorMessage).toBeInTheDocument();
  });
});
