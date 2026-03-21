"use client";

import { useState, useEffect, useCallback, useRef } from "react";
import { useRouter } from "next/navigation";
import { cn } from "@/lib/utils";
import { useVoices, useEstimateCost, useCreateGeneration } from "@/hooks";
import type { components } from "@/lib/api/types";
import { formatNumber } from "@/utils/formatNumber";
import { PresetSelector, getPresetParams } from "@/components/PresetSelector";
import type { VoicePreset, VoiceParams } from "@/components/PresetSelector";
import { ChapterPreview } from "@/components/ChapterPreview/ChapterPreview";
import { CostEstimate } from "@/components/CostEstimate";

type RoutingPreference = components["schemas"]["RoutingPreference"];

const MAX_CHAR_LENGTH = 500_000;
const WARNING_THRESHOLD = 450_000;

const routingOptions: {
  value: RoutingPreference;
  label: string;
  description: string;
  icon: string;
}[] = [
  {
    value: "Balanced",
    label: "Balanced",
    description: "Best mix of quality, cost & speed",
    icon: "⚖️",
  },
  {
    value: "Quality",
    label: "Best Quality",
    description: "Premium voices, highest fidelity",
    icon: "✨",
  },
  {
    value: "Cost",
    label: "Lowest Cost",
    description: "Most affordable option",
    icon: "💰",
  },
  {
    value: "Speed",
    label: "Fastest",
    description: "Quickest generation time",
    icon: "⚡",
  },
];

export default function GeneratePage() {
  const router = useRouter();
  const [text, setText] = useState("");
  const [selectedVoice, setSelectedVoice] = useState<string | null>(null);
  const [routing, setRouting] = useState<RoutingPreference>("Balanced");
  const [audioFormat, setAudioFormat] = useState<string>("mp3");
  const [pasteNotification, setPasteNotification] = useState<string | null>(null);
  const [selectedPreset, setSelectedPreset] = useState<VoicePreset | null>(null);
  const [voiceParams, setVoiceParams] = useState<VoiceParams>({
    speed: 1.0,
    stability: 0.5,
    similarityBoost: 0.75,
    style: 0.0,
  });
  const [isExtracting, setIsExtracting] = useState(false);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [uploadWarning, setUploadWarning] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const pasteNotificationTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const { data: voicesData, isLoading: voicesLoading } = useVoices({ pageSize: 20 });
  const { mutate: estimateCost, data: costEstimate, isPending: isEstimating } = useEstimateCost();
  const { mutate: createGeneration, isPending: isGenerating } = useCreateGeneration();

  const voices = voicesData?.items || [];
  const characterCount = text.length;
  const isOverLimit = characterCount >= MAX_CHAR_LENGTH;
  const isNearLimit = characterCount >= WARNING_THRESHOLD && characterCount < MAX_CHAR_LENGTH;
  const wordCount = text.trim() ? text.trim().split(/\s+/).length : 0;

  // Debounced text for chapter detection (300ms)
  const [debouncedText, setDebouncedText] = useState("");

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedText(text);
    }, 300);
    return () => clearTimeout(timer);
  }, [text]);

  useEffect(() => {
    return () => {
      if (pasteNotificationTimerRef.current) {
        clearTimeout(pasteNotificationTimerRef.current);
      }
    };
  }, []);

  const handleTextChange = useCallback((e: React.ChangeEvent<HTMLTextAreaElement>) => {
    setText(e.target.value);
  }, []);

  const handlePaste = useCallback((e: React.ClipboardEvent<HTMLTextAreaElement>) => {
    const pastedText = e.clipboardData.getData("text");
    const currentText = text;
    const selectionStart = (e.target as HTMLTextAreaElement).selectionStart ?? currentText.length;
    const selectionEnd = (e.target as HTMLTextAreaElement).selectionEnd ?? currentText.length;
    const beforeSelection = currentText.slice(0, selectionStart);
    const afterSelection = currentText.slice(selectionEnd);
    const newText = beforeSelection + pastedText + afterSelection;

    if (newText.length > MAX_CHAR_LENGTH) {
      e.preventDefault();
      const availableSpace = MAX_CHAR_LENGTH - beforeSelection.length - afterSelection.length;
      const truncatedPaste = pastedText.slice(0, Math.max(0, availableSpace));
      setText(beforeSelection + truncatedPaste + afterSelection);
      setPasteNotification(`Text was truncated to fit the ${formatNumber(MAX_CHAR_LENGTH)} character limit.`);
      if (pasteNotificationTimerRef.current) {
        clearTimeout(pasteNotificationTimerRef.current);
      }
      pasteNotificationTimerRef.current = setTimeout(() => {
        setPasteNotification(null);
        pasteNotificationTimerRef.current = null;
      }, 5000);
    }
  }, [text]);

  const handleUploadClick = useCallback(() => {
    fileInputRef.current?.click();
  }, []);

  const handleFileChange = useCallback(async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Reset input so same file can be re-selected
    e.target.value = "";

    setIsExtracting(true);
    setUploadError(null);
    setUploadWarning(null);

    try {
      const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";
      const formData = new FormData();
      formData.append("file", file);

      const response = await fetch(`${API_URL}/api/v1/Documents/extract`, {
        method: "POST",
        body: formData,
        credentials: "include",
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        const message = (errorData as { message?: string }).message || `Extraction failed (${response.status})`;
        throw new Error(message);
      }

      const data = await response.json() as { text?: string; pageCount?: number; wordCount?: number; characterCount?: number };
      const extractedText: string = data.text ?? "";

      if (extractedText.length > MAX_CHAR_LENGTH) {
        setUploadWarning(
          `Document text exceeds the ${MAX_CHAR_LENGTH.toLocaleString()} character limit. ` +
          `Only the first ${MAX_CHAR_LENGTH.toLocaleString()} characters will be used.`
        );
        setText(extractedText.slice(0, MAX_CHAR_LENGTH));
      } else {
        setText(extractedText);
      }
    } catch (err) {
      const message = err instanceof Error ? err.message : "Unknown error";
      setUploadError(`Failed to extract text: ${message}`);
    } finally {
      setIsExtracting(false);
    }
  }, []);

  // Estimate cost when text or voice changes
  useEffect(() => {
    if (text.length > 10) {
      const debounce = setTimeout(() => {
        estimateCost({
          text,
          voiceId: selectedVoice || undefined,
          routingPreference: routing,
        });
      }, 500);
      return () => clearTimeout(debounce);
    }
  }, [text, selectedVoice, routing, estimateCost]);

  const selectedVoiceData = voices.find((v) => v.id === selectedVoice);

  const selectedProvider = (selectedVoiceData?.provider ?? "ElevenLabs") as
    | "ElevenLabs"
    | "OpenAI"
    | "GoogleCloud"
    | "AmazonPolly"
    | "FishAudio"
    | "Cartesia"
    | "Deepgram";

  const handlePresetChange = useCallback(
    (preset: VoicePreset) => {
      setSelectedPreset(preset);
      setVoiceParams(getPresetParams(preset, selectedProvider));
    },
    [selectedProvider]
  );

  const handleParamsChange = useCallback((params: VoiceParams) => {
    setVoiceParams(params);
  }, []);

  const handleGenerate = () => {
    if (!text || !selectedVoice) return;

    createGeneration(
      {
        text,
        voiceId: selectedVoice,
        routingPreference: routing,
        audioFormat,
        ...(selectedPreset && { preset: selectedPreset }),
        speed: voiceParams.speed,
        stability: voiceParams.stability,
        similarityBoost: voiceParams.similarityBoost,
        style: voiceParams.style,
      },
      {
        onSuccess: (data) => {
          router.push(`/generations/${data?.id}`);
        },
      }
    );
  };


  return (
    <div className="min-h-screen bg-bg-surface">
      <div className="container mx-auto px-4 py-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-text-primary">Generate Audio</h1>
          <p className="mt-2 text-text-secondary">
            Convert your text to professional audio using AI voices
          </p>
        </div>

        <div className="grid gap-6 lg:grid-cols-3">
          {/* Main content area */}
          <div className="lg:col-span-2 space-y-6">
            {/* Text Input */}
            <div className="rounded-xl bg-bg-elevated p-6 shadow-sm ring-1 ring-border-subtle">
              <div className="mb-4 flex items-center justify-between">
                <h2 className="text-lg font-semibold text-text-primary">Text Input</h2>
                <div className="flex gap-2">
                  {/* Hidden file input */}
                  <input
                    ref={fileInputRef}
                    type="file"
                    accept=".pdf,.docx"
                    className="hidden"
                    onChange={handleFileChange}
                    aria-label="Upload document file"
                  />
                  <button
                    onClick={handleUploadClick}
                    disabled={isExtracting}
                    className="rounded-lg bg-bg-sunken px-3 py-1.5 text-sm font-medium text-text-secondary hover:bg-bg-surface disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                  >
                    {isExtracting ? "Extracting..." : "Upload File"}
                  </button>
                  <button
                    onClick={() => navigator.clipboard.readText().then(setText)}
                    className="rounded-lg bg-bg-sunken px-3 py-1.5 text-sm font-medium text-text-secondary hover:bg-bg-surface transition-colors"
                  >
                    Paste
                  </button>
                </div>
              </div>
              <textarea
                value={text}
                onChange={handleTextChange}
                className="w-full h-64 rounded-lg border-border-subtle bg-bg-sunken p-4 resize-none text-text-primary placeholder-text-muted focus:border-border-focus focus:ring-2 focus:ring-border-focus/20 transition-all"
                placeholder="Paste your text here or upload a file. Supports books, articles, scripts, and more..."
                onPaste={handlePaste}
              />
              {pasteNotification && (
                <div className="mt-2 rounded-lg bg-warning-subtle px-3 py-2 text-sm text-state-warning-text">
                  {pasteNotification}
                </div>
              )}
              {uploadWarning && (
                <div className="mt-2 rounded-lg bg-warning-subtle px-3 py-2 text-sm text-state-warning-text">
                  {uploadWarning}
                </div>
              )}
              {uploadError && (
                <div className="mt-2 rounded-lg bg-error-subtle px-3 py-2 text-sm text-state-error-text">
                  {uploadError}
                </div>
              )}
              <div className="mt-3 flex items-center justify-between text-sm">
                <div className="flex items-center gap-4 text-text-muted">
                  <span className="flex items-center gap-1.5">
                    <span className="h-2 w-2 rounded-full bg-success"></span>
                    Auto-detected: English
                  </span>
                  {isNearLimit && (
                    <span className="text-warning font-medium">
                      Approaching limit
                    </span>
                  )}
                  {isOverLimit && (
                    <span className="text-error font-medium">
                      Limit reached
                    </span>
                  )}
                </div>
                <div className="flex gap-4">
                   <span className="text-text-secondary">{formatNumber(wordCount)} words</span>
                   <span className="text-text-muted">|</span>
                   <span
                     className={cn(
                       "tabular-nums",
                       isOverLimit
                         ? "text-error font-semibold"
                         : isNearLimit
                           ? "text-warning font-medium"
                           : "text-text-secondary"
                     )}
                   >
                     {formatNumber(characterCount)} / {formatNumber(MAX_CHAR_LENGTH)}
                   </span>
                 </div>
              </div>
              <p className="mt-3 text-xs text-text-muted">
                Tip: Use <code className="rounded bg-bg-sunken px-1 py-0.5 font-mono text-xs">---</code> on its own line to mark chapter breaks
              </p>
              <ChapterPreview text={debouncedText} />
            </div>
            {/* Voice Selection */}
            <div className="rounded-xl bg-bg-elevated p-6 shadow-sm ring-1 ring-border-subtle">
              <div className="mb-4 flex items-center justify-between">
                <h2 className="text-lg font-semibold text-text-primary">Voice Selection</h2>
                <button
                  onClick={() => router.push("/voices")}
                  className="text-sm font-medium text-text-link hover:underline"
                >
                  Browse All Voices →
                </button>
              </div>

              {voicesLoading ? (
                <div className="grid gap-3 sm:grid-cols-2">
                  {[...Array(4)].map((_, i) => (
                    <div key={i} className="rounded-lg bg-bg-sunken p-4 animate-pulse">
                      <div className="flex items-center gap-3">
                        <div className="h-10 w-10 rounded-full bg-bg-sunken" />
                        <div className="flex-1">
                          <div className="h-4 w-20 bg-bg-sunken rounded mb-2" />
                          <div className="h-3 w-16 bg-bg-sunken rounded" />
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="grid gap-3 sm:grid-cols-2">
                  {voices.slice(0, 6).map((voice) => (
                    <button
                      key={voice.id}
                      onClick={() => setSelectedVoice(voice.id)}
                      className={cn(
                        "flex items-center gap-3 rounded-lg p-4 text-left transition-all",
                        selectedVoice === voice.id
                          ? "bg-indigo-subtle ring-2 ring-indigo"
                          : "bg-bg-sunken hover:bg-bg-surface ring-1 ring-border-subtle"
                      )}
                    >
                      <div className="flex h-10 w-10 items-center justify-center rounded-full bg-bg-sunken text-text-muted text-lg">
                        🎙️
                      </div>
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2">
                          <span className="font-medium text-text-primary truncate">
                            {voice.name}
                          </span>
                          <span className="rounded-full bg-indigo-subtle px-2 py-0.5 text-xs font-medium text-indigo">
                            {voice.provider}
                          </span>
                        </div>
                        <div className="text-sm text-text-muted">
                          {voice.language} {voice.gender && `· ${voice.gender}`}
                        </div>
                      </div>
                      {voice.previewUrl && (
                        <button
                          className="rounded-full bg-bg-elevated p-2 shadow-sm ring-1 ring-border-subtle hover:bg-bg-sunken"
                          onClick={(e) => e.stopPropagation()}
                          aria-label="Preview voice sample"
                        >
                          ▶
                        </button>
                      )}
                    </button>
                  ))}
                </div>
              )}
            </div>
          </div>

          {/* Sidebar */}
          <div className="space-y-6">
            {/* Routing Strategy */}
            <div className="rounded-xl bg-bg-elevated p-6 shadow-sm ring-1 ring-border-subtle">
              <h2 className="mb-4 text-lg font-semibold text-text-primary">Routing Strategy</h2>
              <div className="space-y-2">
                {routingOptions.map((option) => (
                  <button
                    key={option.value}
                    onClick={() => setRouting(option.value)}
                    className={cn(
                      "w-full flex items-center gap-3 rounded-lg p-3 text-left transition-all",
                      routing === option.value
                        ? "bg-indigo-subtle ring-2 ring-indigo"
                        : "bg-bg-sunken hover:bg-bg-surface"
                    )}
                  >
                    <span className="text-xl">{option.icon}</span>
                    <div className="flex-1">
                      <div className="font-medium text-text-primary">{option.label}</div>
                      <div className="text-xs text-text-muted">{option.description}</div>
                    </div>
                    {option.value === "Balanced" && (
                      <span className="rounded-full bg-success-subtle px-2 py-0.5 text-xs font-medium text-state-success-text">
                        Recommended
                      </span>
                    )}
                  </button>
                ))}
              </div>
            </div>

            {/* Voice Preset */}
            <div className="rounded-xl bg-bg-elevated p-6 shadow-sm ring-1 ring-border-subtle">
              <h2 className="mb-4 text-lg font-semibold text-text-primary">Voice Preset</h2>
              <PresetSelector
                provider={selectedProvider}
                selectedPreset={selectedPreset}
                params={voiceParams}
                onPresetChange={handlePresetChange}
                onParamsChange={handleParamsChange}
              />
            </div>

            {/* Output Format Selection */}
            <div className="rounded-xl bg-bg-elevated p-6 shadow-sm ring-1 ring-border-subtle">
              <h2 className="mb-4 text-lg font-semibold text-text-primary">Output Format</h2>
              <div className="space-y-3">
                <select
                  name="audioFormat"
                  value={audioFormat}
                  onChange={(e) => setAudioFormat(e.target.value)}
                  className="w-full rounded-lg border border-border-subtle bg-bg-elevated px-4 py-2 text-text-primary focus:border-border-focus focus:ring-2 focus:ring-border-focus/20 transition-all"
                >
                  <option value="mp3">MP3 (Recommended) - Best compatibility, smaller file size</option>
                  <option value="wav">WAV - Lossless, larger file size</option>
                  <option value="ogg">OGG - Good compression, smaller than WAV</option>
                  <option value="flac">FLAC - Lossless, best quality</option>
                </select>
              </div>
            </div>

            {/* Cost Estimate */}
            <CostEstimate
              costEstimate={costEstimate}
              isEstimating={isEstimating}
              characterCount={characterCount}
              wordCount={wordCount}
              selectedProvider={selectedVoiceData?.provider}
            />

            {/* Action Buttons */}
            <div className="space-y-3">
              <button
                onClick={handleGenerate}
                disabled={!text || !selectedVoice || isGenerating || isOverLimit}
                className="w-full rounded-xl bg-indigo px-6 py-4 text-lg font-semibold text-white shadow-lg shadow-indigo/25 hover:bg-indigo-dark disabled:opacity-50 disabled:cursor-not-allowed disabled:shadow-none transition-all"
              >
                {isGenerating ? "Starting Generation..." : "Generate Audio"}
              </button>
              <button
                disabled={!text || !selectedVoice || isOverLimit}
                className="w-full rounded-xl bg-bg-elevated px-6 py-3 font-medium text-text-secondary ring-1 ring-border-subtle hover:bg-bg-sunken disabled:opacity-50 disabled:cursor-not-allowed transition-all"
              >
                Preview First 500 Characters (Free)
              </button>
            </div>


          </div>
        </div>
      </div>
    </div>
  );
}
