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
      setTimeout(() => setPasteNotification(null), 5000);
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
    <div className="min-h-screen bg-gradient-to-b from-gray-50 to-gray-100 dark:from-gray-950 dark:to-gray-900">
      <div className="container mx-auto px-4 py-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Generate Audio</h1>
          <p className="mt-2 text-gray-600 dark:text-gray-400">
            Convert your text to professional audio using AI voices
          </p>
        </div>

        <div className="grid gap-6 lg:grid-cols-3">
          {/* Main content area */}
          <div className="lg:col-span-2 space-y-6">
            {/* Text Input */}
            <div className="rounded-xl bg-white dark:bg-gray-900 p-6 shadow-sm ring-1 ring-gray-200 dark:ring-gray-800">
              <div className="mb-4 flex items-center justify-between">
                <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Text Input</h2>
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
                    className="rounded-lg bg-gray-100 dark:bg-gray-800 px-3 py-1.5 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                  >
                    {isExtracting ? "Extracting..." : "Upload File"}
                  </button>
                  <button
                    onClick={() => navigator.clipboard.readText().then(setText)}
                    className="rounded-lg bg-gray-100 dark:bg-gray-800 px-3 py-1.5 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors"
                  >
                    Paste
                  </button>
                </div>
              </div>
              <textarea
                value={text}
                onChange={handleTextChange}
                className="w-full h-64 rounded-lg border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 p-4 resize-none text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-all"
                placeholder="Paste your text here or upload a file. Supports books, articles, scripts, and more..."
                onPaste={handlePaste}
              />
              {pasteNotification && (
                <div className="mt-2 rounded-lg bg-amber-50 dark:bg-amber-950 px-3 py-2 text-sm text-amber-700 dark:text-amber-300">
                  {pasteNotification}
                </div>
              )}
              {uploadWarning && (
                <div className="mt-2 rounded-lg bg-amber-50 dark:bg-amber-950 px-3 py-2 text-sm text-amber-700 dark:text-amber-300">
                  {uploadWarning}
                </div>
              )}
              {uploadError && (
                <div className="mt-2 rounded-lg bg-red-50 dark:bg-red-950 px-3 py-2 text-sm text-red-700 dark:text-red-300">
                  {uploadError}
                </div>
              )}
              <div className="mt-3 flex items-center justify-between text-sm">
                <div className="flex items-center gap-4 text-gray-500 dark:text-gray-400">
                  <span className="flex items-center gap-1.5">
                    <span className="h-2 w-2 rounded-full bg-green-500"></span>
                    Auto-detected: English
                  </span>
                  {isNearLimit && (
                    <span className="text-amber-600 dark:text-amber-400 font-medium">
                      Approaching limit
                    </span>
                  )}
                  {isOverLimit && (
                    <span className="text-red-600 dark:text-red-400 font-medium">
                      Limit reached
                    </span>
                  )}
                </div>
                <div className="flex gap-4">
                   <span className="text-gray-600 dark:text-gray-400">{formatNumber(wordCount)} words</span>
                   <span className="text-gray-300 dark:text-gray-600">|</span>
                   <span
                     className={cn(
                       "tabular-nums",
                       isOverLimit
                         ? "text-red-500 font-semibold"
                         : isNearLimit
                           ? "text-amber-500 font-medium"
                           : "text-gray-600 dark:text-gray-400"
                     )}
                   >
                     {formatNumber(characterCount)} / {formatNumber(MAX_CHAR_LENGTH)}
                   </span>
                 </div>
              </div>
              <p className="mt-3 text-xs text-gray-500 dark:text-gray-400">
                Tip: Use <code className="rounded bg-gray-200 dark:bg-gray-700 px-1 py-0.5 font-mono text-xs">---</code> on its own line to mark chapter breaks
              </p>
              <ChapterPreview text={debouncedText} />
            </div>
            {/* Voice Selection */}
            <div className="rounded-xl bg-white dark:bg-gray-900 p-6 shadow-sm ring-1 ring-gray-200 dark:ring-gray-800">
              <div className="mb-4 flex items-center justify-between">
                <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Voice Selection</h2>
                <button
                  onClick={() => router.push("/voices")}
                  className="text-sm font-medium text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300"
                >
                  Browse All Voices →
                </button>
              </div>

              {voicesLoading ? (
                <div className="grid gap-3 sm:grid-cols-2">
                  {[...Array(4)].map((_, i) => (
                    <div key={i} className="rounded-lg bg-gray-50 dark:bg-gray-800 p-4 animate-pulse">
                      <div className="flex items-center gap-3">
                        <div className="h-10 w-10 rounded-full bg-gray-200 dark:bg-gray-700" />
                        <div className="flex-1">
                          <div className="h-4 w-20 bg-gray-200 dark:bg-gray-700 rounded mb-2" />
                          <div className="h-3 w-16 bg-gray-200 dark:bg-gray-700 rounded" />
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
                          ? "bg-blue-50 dark:bg-blue-950 ring-2 ring-blue-500"
                          : "bg-gray-50 dark:bg-gray-800 hover:bg-gray-100 dark:hover:bg-gray-700 ring-1 ring-gray-200 dark:ring-gray-700"
                      )}
                    >
                      <div className="flex h-10 w-10 items-center justify-center rounded-full bg-gradient-to-br from-gray-200 to-gray-300 dark:from-gray-700 dark:to-gray-600 text-lg">
                        🎙️
                      </div>
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2">
                          <span className="font-medium text-gray-900 dark:text-white truncate">
                            {voice.name}
                          </span>
                          <span className="rounded-full bg-purple-100 dark:bg-purple-900 px-2 py-0.5 text-xs font-medium text-purple-700 dark:text-purple-300">
                            {voice.provider}
                          </span>
                        </div>
                        <div className="text-sm text-gray-500 dark:text-gray-400">
                          {voice.language} {voice.gender && `· ${voice.gender}`}
                        </div>
                      </div>
                      {voice.previewUrl && (
                        <button
                          className="rounded-full bg-white dark:bg-gray-700 p-2 shadow-sm ring-1 ring-gray-200 dark:ring-gray-600 hover:bg-gray-50 dark:hover:bg-gray-600"
                          onClick={(e) => e.stopPropagation()}
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
            <div className="rounded-xl bg-white dark:bg-gray-900 p-6 shadow-sm ring-1 ring-gray-200 dark:ring-gray-800">
              <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Routing Strategy</h2>
              <div className="space-y-2">
                {routingOptions.map((option) => (
                  <button
                    key={option.value}
                    onClick={() => setRouting(option.value)}
                    className={cn(
                      "w-full flex items-center gap-3 rounded-lg p-3 text-left transition-all",
                      routing === option.value
                        ? "bg-blue-50 dark:bg-blue-950 ring-2 ring-blue-500"
                        : "bg-gray-50 dark:bg-gray-800 hover:bg-gray-100 dark:hover:bg-gray-700"
                    )}
                  >
                    <span className="text-xl">{option.icon}</span>
                    <div className="flex-1">
                      <div className="font-medium text-gray-900 dark:text-white">{option.label}</div>
                      <div className="text-xs text-gray-500 dark:text-gray-400">{option.description}</div>
                    </div>
                    {option.value === "Balanced" && (
                      <span className="rounded-full bg-green-100 dark:bg-green-900 px-2 py-0.5 text-xs font-medium text-green-700 dark:text-green-300">
                        Recommended
                      </span>
                    )}
                  </button>
                ))}
              </div>
            </div>

            {/* Voice Preset */}
            <div className="rounded-xl bg-white dark:bg-gray-900 p-6 shadow-sm ring-1 ring-gray-200 dark:ring-gray-800">
              <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Voice Preset</h2>
              <PresetSelector
                provider={selectedProvider}
                selectedPreset={selectedPreset}
                params={voiceParams}
                onPresetChange={handlePresetChange}
                onParamsChange={handleParamsChange}
              />
            </div>

            {/* Output Format Selection */}
            <div className="rounded-xl bg-white dark:bg-gray-900 p-6 shadow-sm ring-1 ring-gray-200 dark:ring-gray-800">
              <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Output Format</h2>
              <div className="space-y-3">
                <select
                  name="audioFormat"
                  value={audioFormat}
                  onChange={(e) => setAudioFormat(e.target.value)}
                  className="w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-2 text-gray-900 dark:text-white focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-all"
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
                className="w-full rounded-xl bg-blue-600 px-6 py-4 text-lg font-semibold text-white shadow-lg shadow-blue-600/25 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed disabled:shadow-none transition-all"
              >
                {isGenerating ? "Starting Generation..." : "Generate Audio"}
              </button>
              <button
                disabled={!text || !selectedVoice || isOverLimit}
                className="w-full rounded-xl bg-white dark:bg-gray-800 px-6 py-3 font-medium text-gray-700 dark:text-gray-300 ring-1 ring-gray-200 dark:ring-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
              >
                Preview First 500 Characters (Free)
              </button>
            </div>

            {/* Info */}
            <div className="rounded-lg bg-blue-50 dark:bg-blue-950 p-4 text-sm text-blue-800 dark:text-blue-200">
              <p className="font-medium">Your remaining quota</p>
              <p className="mt-1 text-blue-600 dark:text-blue-400">10,000 characters this month</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
