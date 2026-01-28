"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { cn } from "@/lib/utils";
import { useVoices, useEstimateCost, useCreateGeneration } from "@/hooks";
import type { components } from "@/lib/api/types";

type RoutingPreference = components["schemas"]["RoutingPreference"];

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

  const { data: voicesData, isLoading: voicesLoading } = useVoices({ pageSize: 20 });
  const { mutate: estimateCost, data: costEstimate, isPending: isEstimating } = useEstimateCost();
  const { mutate: createGeneration, isPending: isGenerating } = useCreateGeneration();

  const voices = voicesData?.items || [];
  const characterCount = text.length;
  const wordCount = text.trim() ? text.trim().split(/\s+/).length : 0;

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

  const handleGenerate = () => {
    if (!text || !selectedVoice) return;

    createGeneration(
      {
        text,
        voiceId: selectedVoice,
        routingPreference: routing,
      },
      {
        onSuccess: (data) => {
          router.push(`/generations/${data?.id}`);
        },
      }
    );
  };

  const selectedVoiceData = voices.find((v) => v.id === selectedVoice);

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
                  <button className="rounded-lg bg-gray-100 dark:bg-gray-800 px-3 py-1.5 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors">
                    Upload File
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
                onChange={(e) => setText(e.target.value)}
                className="w-full h-64 rounded-lg border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 p-4 resize-none text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-all"
                placeholder="Paste your text here or upload a file. Supports books, articles, scripts, and more..."
              />
              <div className="mt-3 flex items-center justify-between text-sm">
                <div className="flex items-center gap-4 text-gray-500 dark:text-gray-400">
                  <span className="flex items-center gap-1.5">
                    <span className="h-2 w-2 rounded-full bg-green-500"></span>
                    Auto-detected: English
                  </span>
                </div>
                <div className="flex gap-4 text-gray-600 dark:text-gray-400">
                  <span>{wordCount.toLocaleString()} words</span>
                  <span className="text-gray-300 dark:text-gray-600">|</span>
                  <span>{characterCount.toLocaleString()} characters</span>
                </div>
              </div>
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

            {/* Cost Estimate */}
            <div className="rounded-xl bg-gradient-to-br from-gray-800 to-gray-900 dark:from-gray-800 dark:to-gray-950 p-6 text-white shadow-sm ring-1 ring-gray-700">
              <h2 className="mb-4 text-lg font-semibold">Cost Estimate</h2>
              {characterCount > 0 ? (
                <div className="space-y-4">
                  <div className="flex items-baseline justify-between">
                    <span className="text-gray-400">Estimated Cost</span>
                    <span className="text-3xl font-bold">
                      {isEstimating ? (
                        <span className="text-xl">...</span>
                      ) : costEstimate ? (
                        costEstimate.creditsRequired && costEstimate.creditsRequired > 0
                          ? `${costEstimate.creditsRequired.toLocaleString()} credits ($${costEstimate.estimatedCost.toFixed(4)})`
                          : `$${costEstimate.estimatedCost.toFixed(4)}`
                      ) : (
                        `$${(characterCount * 0.00003).toFixed(4)}`
                      )}
                    </span>
                  </div>
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-gray-400">Characters</span>
                      <span>{costEstimate?.characterCount?.toLocaleString() || characterCount.toLocaleString()}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-400">Est. Duration</span>
                      <span>~{Math.ceil(wordCount / 150)} min</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-400">Provider</span>
                      <span>
                        {selectedVoiceData?.provider || costEstimate?.recommendedProvider || "Auto-select"}
                      </span>
                    </div>
                  </div>
                  {costEstimate?.providerEstimates && costEstimate.providerEstimates.length > 1 && (
                    <details className="mt-4 text-sm">
                      <summary className="cursor-pointer text-blue-400 hover:underline">
                        Compare all providers
                      </summary>
                      <div className="mt-2 space-y-2">
                        {costEstimate.providerEstimates.map((estimate) => (
                          <div
                            key={estimate.provider}
                            className="flex items-center justify-between rounded p-2 bg-gray-700/50"
                          >
                            <span>{estimate.provider}</span>
                            <span>
                              {estimate.creditsRequired && estimate.creditsRequired > 0
                                ? `${estimate.creditsRequired.toLocaleString()} credits ($${estimate.cost.toFixed(4)})`
                                : `$${estimate.cost.toFixed(4)}`}
                            </span>
                          </div>
                        ))}
                      </div>
                    </details>
                  )}
                </div>
              ) : (
                <div className="py-4 text-center text-gray-400">
                  <p>Enter text to see cost estimate</p>
                </div>
              )}
            </div>

            {/* Action Buttons */}
            <div className="space-y-3">
              <button
                onClick={handleGenerate}
                disabled={!text || !selectedVoice || isGenerating}
                className="w-full rounded-xl bg-blue-600 px-6 py-4 text-lg font-semibold text-white shadow-lg shadow-blue-600/25 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed disabled:shadow-none transition-all"
              >
                {isGenerating ? "Starting Generation..." : "Generate Audio"}
              </button>
              <button
                disabled={!text || !selectedVoice}
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
