import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { PresetSelector } from "./PresetSelector";
import type { VoicePreset, VoiceParams } from "./PresetSelector";

describe("PresetSelector", () => {
  const defaultProps = {
    provider: "ElevenLabs" as const,
    selectedPreset: null as VoicePreset | null,
    params: { speed: 1.0, stability: 0.5, similarityBoost: 0.75, style: 0.0 },
    onPresetChange: (_preset: VoicePreset) => {},
    onParamsChange: (_params: VoiceParams) => {},
  };

  describe("Preset Cards", () => {
    it("renders 4 preset cards: Audiobook, Conversational, Dramatic, Professional", () => {
      render(<PresetSelector {...defaultProps} />);

      expect(screen.getByText("Audiobook")).toBeInTheDocument();
      expect(screen.getByText("Conversational")).toBeInTheDocument();
      expect(screen.getByText("Dramatic")).toBeInTheDocument();
      expect(screen.getByText("Professional")).toBeInTheDocument();
    });

    it("each preset card has a description", () => {
      render(<PresetSelector {...defaultProps} />);

      expect(screen.getByText(/clear.*narration/i)).toBeInTheDocument();
      expect(screen.getByText(/natural.*casual/i)).toBeInTheDocument();
      expect(screen.getByText(/expressive.*emotional/i)).toBeInTheDocument();
      expect(screen.getByText(/polished.*formal/i)).toBeInTheDocument();
    });

    it("highlights the selected preset card", () => {
      render(<PresetSelector {...defaultProps} selectedPreset="Audiobook" />);

      const audiobookCard = screen.getByRole("button", { name: /audiobook/i });
      expect(audiobookCard.className).toMatch(/ring-blue|ring-2|bg-blue/);
    });

    it("calls onPresetChange when a preset card is clicked", () => {
      const onPresetChange = vi.fn();
      render(<PresetSelector {...defaultProps} onPresetChange={onPresetChange} />);

      fireEvent.click(screen.getByRole("button", { name: /dramatic/i }));
      expect(onPresetChange).toHaveBeenCalledWith("Dramatic");
    });
  });

  describe("Parameter Sliders - ElevenLabs (all 4 sliders)", () => {
    it("shows Speed, Stability, Similarity Boost, and Style sliders for ElevenLabs", () => {
      render(<PresetSelector {...defaultProps} provider="ElevenLabs" />);

      expect(screen.getByLabelText(/speed/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/stability/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/similarity boost/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/style/i)).toBeInTheDocument();
    });

    it("displays current slider values", () => {
      render(
        <PresetSelector
          {...defaultProps}
          provider="ElevenLabs"
          params={{ speed: 1.05, stability: 0.70, similarityBoost: 0.80, style: 0.2 }}
        />
      );

      expect(screen.getByText("1.05")).toBeInTheDocument();
      expect(screen.getByText("0.70")).toBeInTheDocument();
      expect(screen.getByText("0.80")).toBeInTheDocument();
      expect(screen.getByText("0.20")).toBeInTheDocument();
    });

    it("calls onParamsChange when a slider is adjusted", () => {
      const onParamsChange = vi.fn();
      render(
        <PresetSelector
          {...defaultProps}
          provider="ElevenLabs"
          params={{ speed: 1.0, stability: 0.5, similarityBoost: 0.75, style: 0.0 }}
          onParamsChange={onParamsChange}
        />
      );

      const speedSlider = screen.getByLabelText(/speed/i);
      fireEvent.change(speedSlider, { target: { value: "1.5" } });

      expect(onParamsChange).toHaveBeenCalledWith(
        expect.objectContaining({ speed: 1.5 })
      );
    });
  });

  describe("Parameter Sliders - OpenAI (only Speed)", () => {
    it("shows only Speed slider for OpenAI", () => {
      render(<PresetSelector {...defaultProps} provider="OpenAI" />);

      expect(screen.getByLabelText(/speed/i)).toBeInTheDocument();
      expect(screen.queryByLabelText(/stability/i)).not.toBeInTheDocument();
      expect(screen.queryByLabelText(/similarity boost/i)).not.toBeInTheDocument();
      expect(screen.queryByLabelText(/style/i)).not.toBeInTheDocument();
    });
  });

  describe("Parameter Sliders - Other providers (Speed only)", () => {
    it("shows only Speed slider for GoogleCloud", () => {
      render(<PresetSelector {...defaultProps} provider="GoogleCloud" />);

      expect(screen.getByLabelText(/speed/i)).toBeInTheDocument();
      expect(screen.queryByLabelText(/stability/i)).not.toBeInTheDocument();
    });

    it("shows only Speed slider for AmazonPolly", () => {
      render(<PresetSelector {...defaultProps} provider="AmazonPolly" />);

      expect(screen.getByLabelText(/speed/i)).toBeInTheDocument();
      expect(screen.queryByLabelText(/stability/i)).not.toBeInTheDocument();
    });
  });

  describe("Preset populates sliders", () => {
    it("populates ElevenLabs sliders with Audiobook preset values when selected", () => {
      const onParamsChange = vi.fn();
      const onPresetChange = vi.fn();
      render(
        <PresetSelector
          {...defaultProps}
          provider="ElevenLabs"
          onPresetChange={onPresetChange}
          onParamsChange={onParamsChange}
        />
      );

      fireEvent.click(screen.getByRole("button", { name: /audiobook/i }));

      expect(onPresetChange).toHaveBeenCalledWith("Audiobook");
    });
  });
});

// Import vi for the mock function
import { vi } from "vitest";
