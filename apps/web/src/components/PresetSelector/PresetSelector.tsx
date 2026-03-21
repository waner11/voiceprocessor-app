"use client";

import { cn } from "@/lib/utils";

export type VoicePreset = "Audiobook" | "Conversational" | "Dramatic" | "Professional";

export type Provider =
  | "ElevenLabs"
  | "OpenAI"
  | "GoogleCloud"
  | "AmazonPolly"
  | "FishAudio"
  | "Cartesia"
  | "Deepgram";

export interface VoiceParams {
  speed: number;
  stability: number;
  similarityBoost: number;
  style: number;
}

interface PresetOption {
  value: VoicePreset;
  label: string;
  description: string;
  icon: string;
}

const presetOptions: PresetOption[] = [
  {
    value: "Audiobook",
    label: "Audiobook",
    description: "Clear, steady narration for long-form content",
    icon: "📖",
  },
  {
    value: "Conversational",
    label: "Conversational",
    description: "Natural, casual tone for dialogue and chat",
    icon: "💬",
  },
  {
    value: "Dramatic",
    label: "Dramatic",
    description: "Expressive, emotional delivery for storytelling",
    icon: "🎭",
  },
  {
    value: "Professional",
    label: "Professional",
    description: "Polished, formal tone for business content",
    icon: "💼",
  },
];

/** Preset parameter values from VoicePresetEngine (backend source of truth) */
const ELEVENLABS_PRESETS: Record<VoicePreset, VoiceParams> = {
  Audiobook: { speed: 1.0, stability: 0.70, similarityBoost: 0.80, style: 0.0 },
  Conversational: { speed: 1.05, stability: 0.50, similarityBoost: 0.75, style: 0.2 },
  Dramatic: { speed: 0.95, stability: 0.35, similarityBoost: 0.70, style: 0.5 },
  Professional: { speed: 1.0, stability: 0.80, similarityBoost: 0.85, style: 0.0 },
};

const OPENAI_PRESETS: Record<VoicePreset, VoiceParams> = {
  Audiobook: { speed: 1.0, stability: 0, similarityBoost: 0, style: 0 },
  Conversational: { speed: 1.05, stability: 0, similarityBoost: 0, style: 0 },
  Dramatic: { speed: 0.95, stability: 0, similarityBoost: 0, style: 0 },
  Professional: { speed: 1.0, stability: 0, similarityBoost: 0, style: 0 },
};

const DEFAULT_PRESETS: Record<VoicePreset, VoiceParams> = {
  Audiobook: { speed: 1.0, stability: 0, similarityBoost: 0, style: 0 },
  Conversational: { speed: 1.05, stability: 0, similarityBoost: 0, style: 0 },
  Dramatic: { speed: 0.95, stability: 0, similarityBoost: 0, style: 0 },
  Professional: { speed: 1.0, stability: 0, similarityBoost: 0, style: 0 },
};

export function getPresetParams(preset: VoicePreset, provider: Provider): VoiceParams {
  switch (provider) {
    case "ElevenLabs":
      return ELEVENLABS_PRESETS[preset];
    case "OpenAI":
      return OPENAI_PRESETS[preset];
    default:
      return DEFAULT_PRESETS[preset];
  }
}

function supportsFullParams(provider: Provider): boolean {
  return provider === "ElevenLabs";
}

interface SliderConfig {
  key: keyof VoiceParams;
  label: string;
  min: number;
  max: number;
  step: number;
}

const SPEED_SLIDER: SliderConfig = { key: "speed", label: "Speed", min: 0.5, max: 2.0, step: 0.1 };
const STABILITY_SLIDER: SliderConfig = { key: "stability", label: "Stability", min: 0.0, max: 1.0, step: 0.05 };
const SIMILARITY_SLIDER: SliderConfig = { key: "similarityBoost", label: "Similarity Boost", min: 0.0, max: 1.0, step: 0.05 };
const STYLE_SLIDER: SliderConfig = { key: "style", label: "Style", min: 0.0, max: 1.0, step: 0.05 };

function getSlidersForProvider(provider: Provider): SliderConfig[] {
  if (supportsFullParams(provider)) {
    return [SPEED_SLIDER, STABILITY_SLIDER, SIMILARITY_SLIDER, STYLE_SLIDER];
  }
  return [SPEED_SLIDER];
}

interface PresetSelectorProps {
  provider: Provider;
  selectedPreset: VoicePreset | null;
  params: VoiceParams;
  onPresetChange: (preset: VoicePreset) => void;
  onParamsChange: (params: VoiceParams) => void;
}

export function PresetSelector({
  provider,
  selectedPreset,
  params,
  onPresetChange,
  onParamsChange,
}: PresetSelectorProps) {
  const sliders = getSlidersForProvider(provider);

  const handleSliderChange = (key: keyof VoiceParams, value: number) => {
    onParamsChange({ ...params, [key]: value });
  };

  return (
    <div className="space-y-4">
      {/* Preset Cards */}
      <div className="grid grid-cols-2 gap-2">
        {presetOptions.map((option) => (
          <button
            key={option.value}
            onClick={() => onPresetChange(option.value)}
            className={cn(
              "flex flex-col items-start gap-1 rounded-lg p-3 text-left transition-all",
              selectedPreset === option.value
                ? "bg-indigo-subtle ring-2 ring-indigo"
                : "bg-bg-sunken hover:bg-bg-surface ring-1 ring-border-subtle"
            )}
          >
            <div className="flex items-center gap-2">
              <span className="text-lg">{option.icon}</span>
              <span className="font-medium text-sm text-text-primary">
                {option.label}
              </span>
            </div>
            <span className="text-xs text-text-muted">
              {option.description}
            </span>
          </button>
        ))}
      </div>

      {/* Parameter Sliders */}
      <div className="space-y-3 pt-2">
        {sliders.map((slider) => (
          <div key={slider.key} className="space-y-1">
            <div className="flex items-center justify-between">
              <label
                htmlFor={`slider-${slider.key}`}
                className="text-sm font-medium text-text-secondary"
              >
                {slider.label}
              </label>
              <span className="text-sm font-mono text-text-muted">
                {params[slider.key].toFixed(2)}
              </span>
            </div>
            <input
              id={`slider-${slider.key}`}
              type="range"
              min={slider.min}
              max={slider.max}
              step={slider.step}
              value={params[slider.key]}
              onChange={(e) => handleSliderChange(slider.key, parseFloat(e.target.value))}
              aria-label={slider.label}
              className="w-full h-2 rounded-lg appearance-none cursor-pointer bg-bg-sunken accent-indigo"
            />
          </div>
        ))}
      </div>
    </div>
  );
}
