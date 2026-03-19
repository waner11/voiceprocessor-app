import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import GeneratePage from "../page";
import { useVoices, useEstimateCost, useCreateGeneration } from "@/hooks";

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

describe("Cost Estimate Badges on Generate Page", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders Premium badge for Premium quality tier", () => {
    // Override useEstimateCost to return Premium tier data
    vi.mocked(useEstimateCost).mockReturnValue({
      mutate: vi.fn(),
      data: {
        characterCount: 100,
        estimatedChunks: 1,
        estimatedCost: 0.5,
        creditsRequired: 50,
        currency: "USD",
        recommendedProvider: "ElevenLabs",
        providerEstimates: [
          {
            provider: "ElevenLabs",
            cost: 0.5,
            creditsRequired: 50,
            estimatedDurationMs: 5000,
            qualityTier: "Premium",
            isAvailable: true,
          },
        ],
      },
      isPending: false,
    });

    // Override useVoices to return ElevenLabs voice
    vi.mocked(useVoices).mockReturnValue({
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
    });

    render(<GeneratePage />);

    // Type text > 10 chars into textarea
    const textarea = screen.getByPlaceholderText(/paste your text/i);
    fireEvent.change(textarea, { target: { value: "Hello world test" } });

    // Click the ElevenLabs voice button
    const voiceButton = screen.getByText("Test Voice");
    fireEvent.click(voiceButton);

    // Assert Premium badge exists
    expect(screen.getByText(/premium quality/i)).toBeInTheDocument();
  });

  it("does NOT render Premium badge for non-Premium quality tier", () => {
    // Override useEstimateCost to return High tier (not Premium)
    vi.mocked(useEstimateCost).mockReturnValue({
      mutate: vi.fn(),
      data: {
        characterCount: 100,
        estimatedChunks: 1,
        estimatedCost: 0.3,
        creditsRequired: 30,
        currency: "USD",
        recommendedProvider: "OpenAI",
        providerEstimates: [
          {
            provider: "OpenAI",
            cost: 0.3,
            creditsRequired: 30,
            estimatedDurationMs: 5000,
            qualityTier: "High",
            isAvailable: true,
          },
        ],
      },
      isPending: false,
    });

    // Override useVoices to return OpenAI voice
    vi.mocked(useVoices).mockReturnValue({
      data: {
        items: [
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
    });

    render(<GeneratePage />);

    // Type text > 10 chars
    const textarea = screen.getByPlaceholderText(/paste your text/i);
    fireEvent.change(textarea, { target: { value: "Hello world test" } });

    // Click voice button
    const voiceButton = screen.getByText("OpenAI Voice");
    fireEvent.click(voiceButton);

    // Assert Premium badge does NOT exist
    expect(screen.queryByText(/premium quality/i)).toBeNull();
  });

  it("renders Best Value badge when routing is Balanced", () => {
    // Use default routing (Balanced is default)
    vi.mocked(useEstimateCost).mockReturnValue({
      mutate: vi.fn(),
      data: {
        characterCount: 100,
        estimatedChunks: 1,
        estimatedCost: 0.4,
        creditsRequired: 40,
        currency: "USD",
        recommendedProvider: "OpenAI",
        providerEstimates: [
          {
            provider: "OpenAI",
            cost: 0.4,
            creditsRequired: 40,
            estimatedDurationMs: 5000,
            qualityTier: "High",
            isAvailable: true,
          },
        ],
      },
      isPending: false,
    });

    render(<GeneratePage />);

    // Type text > 10 chars
    const textarea = screen.getByPlaceholderText(/paste your text/i);
    fireEvent.change(textarea, { target: { value: "Hello world test" } });

    // Click voice button
    const voiceButton = screen.getByText("Test Voice");
    fireEvent.click(voiceButton);

    // Assert Best Value badge exists (routing is Balanced by default)
    expect(screen.getByText(/best value/i)).toBeInTheDocument();
  });

  it("does NOT render Best Value badge when routing is not Balanced", () => {
    vi.mocked(useEstimateCost).mockReturnValue({
      mutate: vi.fn(),
      data: {
        characterCount: 100,
        estimatedChunks: 1,
        estimatedCost: 0.5,
        creditsRequired: 50,
        currency: "USD",
        recommendedProvider: "ElevenLabs",
        providerEstimates: [
          {
            provider: "ElevenLabs",
            cost: 0.5,
            creditsRequired: 50,
            estimatedDurationMs: 5000,
            qualityTier: "Premium",
            isAvailable: true,
          },
        ],
      },
      isPending: false,
    });

    render(<GeneratePage />);

    // Type text > 10 chars
    const textarea = screen.getByPlaceholderText(/paste your text/i);
    fireEvent.change(textarea, { target: { value: "Hello world test" } });

    // Click voice button
    const voiceButton = screen.getByText("Test Voice");
    fireEvent.click(voiceButton);

    // Click "Best Quality" routing button to change from Balanced
    const bestQualityButton = screen.getByRole("button", { name: /best quality/i });
    fireEvent.click(bestQualityButton);

    // Assert Best Value badge does NOT exist
    expect(screen.queryByText(/best value/i)).toBeNull();
  });

  it("does NOT render any badges when cost estimate is null", () => {
    // Use default mock (useEstimateCost returns data: null)
    render(<GeneratePage />);

    // Type text > 10 chars
    const textarea = screen.getByPlaceholderText(/paste your text/i);
    fireEvent.change(textarea, { target: { value: "Hello world test" } });

    // Click voice button
    const voiceButton = screen.getByText("Test Voice");
    fireEvent.click(voiceButton);

    // Assert neither badge exists
    expect(screen.queryByText(/premium quality/i)).toBeNull();
    expect(screen.queryByText(/best value/i)).toBeNull();
  });
});
