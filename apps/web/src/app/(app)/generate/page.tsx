"use client";

import { useState } from "react";
import { cn } from "@/lib/utils";

type RoutingStrategy = "Balanced" | "Quality" | "Cost" | "Speed";

const routingOptions: {
  value: RoutingStrategy;
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

const mockVoices = [
  { id: "1", name: "Emma", provider: "ElevenLabs", language: "English", style: "Natural" },
  { id: "2", name: "James", provider: "OpenAI", language: "English", style: "Professional" },
  { id: "3", name: "Sofia", provider: "Google", language: "Spanish", style: "Warm" },
  { id: "4", name: "Marcus", provider: "Amazon", language: "English", style: "Narrative" },
];

export default function GeneratePage() {
  const [text, setText] = useState("");
  const [selectedVoice, setSelectedVoice] = useState<string | null>(null);
  const [routing, setRouting] = useState<RoutingStrategy>("Balanced");

  const characterCount = text.length;
  const wordCount = text.trim() ? text.trim().split(/\s+/).length : 0;

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-50 to-gray-100">
      <div className="container mx-auto px-4 py-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Generate Audio</h1>
          <p className="mt-2 text-gray-600">
            Convert your text to professional audio using AI voices
          </p>
        </div>

        <div className="grid gap-6 lg:grid-cols-3">
          {/* Main content area */}
          <div className="lg:col-span-2 space-y-6">
            {/* Text Input */}
            <div className="rounded-xl bg-white p-6 shadow-sm ring-1 ring-gray-200">
              <div className="mb-4 flex items-center justify-between">
                <h2 className="text-lg font-semibold text-gray-900">Text Input</h2>
                <div className="flex gap-2">
                  <button className="rounded-lg bg-gray-100 px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-200 transition-colors">
                    Upload File
                  </button>
                  <button className="rounded-lg bg-gray-100 px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-200 transition-colors">
                    Paste
                  </button>
                </div>
              </div>
              <textarea
                value={text}
                onChange={(e) => setText(e.target.value)}
                className="w-full h-64 rounded-lg border-gray-200 bg-gray-50 p-4 resize-none text-gray-900 placeholder-gray-400 focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-all"
                placeholder="Paste your text here or upload a file. Supports books, articles, scripts, and more..."
              />
              <div className="mt-3 flex items-center justify-between text-sm">
                <div className="flex items-center gap-4 text-gray-500">
                  <span className="flex items-center gap-1.5">
                    <span className="h-2 w-2 rounded-full bg-green-500"></span>
                    Auto-detected: English
                  </span>
                </div>
                <div className="flex gap-4 text-gray-600">
                  <span>{wordCount.toLocaleString()} words</span>
                  <span className="text-gray-300">|</span>
                  <span>{characterCount.toLocaleString()} characters</span>
                </div>
              </div>
            </div>

            {/* Voice Selection */}
            <div className="rounded-xl bg-white p-6 shadow-sm ring-1 ring-gray-200">
              <div className="mb-4 flex items-center justify-between">
                <h2 className="text-lg font-semibold text-gray-900">Voice Selection</h2>
                <button className="text-sm font-medium text-blue-600 hover:text-blue-700">
                  Browse All Voices →
                </button>
              </div>
              <div className="grid gap-3 sm:grid-cols-2">
                {mockVoices.map((voice) => (
                  <button
                    key={voice.id}
                    onClick={() => setSelectedVoice(voice.id)}
                    className={cn(
                      "flex items-center gap-3 rounded-lg p-4 text-left transition-all",
                      selectedVoice === voice.id
                        ? "bg-blue-50 ring-2 ring-blue-500"
                        : "bg-gray-50 hover:bg-gray-100 ring-1 ring-gray-200"
                    )}
                  >
                    <div className="flex h-10 w-10 items-center justify-center rounded-full bg-gradient-to-br from-gray-200 to-gray-300 text-lg">
                      🎙️
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2">
                        <span className="font-medium text-gray-900">{voice.name}</span>
                        <span className="rounded-full bg-purple-100 px-2 py-0.5 text-xs font-medium text-purple-700">
                          {voice.provider}
                        </span>
                      </div>
                      <div className="text-sm text-gray-500">
                        {voice.language} · {voice.style}
                      </div>
                    </div>
                    <button
                      className="rounded-full bg-white p-2 shadow-sm ring-1 ring-gray-200 hover:bg-gray-50"
                      onClick={(e) => e.stopPropagation()}
                    >
                      ▶
                    </button>
                  </button>
                ))}
              </div>
            </div>
          </div>

          {/* Sidebar */}
          <div className="space-y-6">
            {/* Routing Strategy */}
            <div className="rounded-xl bg-white p-6 shadow-sm ring-1 ring-gray-200">
              <h2 className="mb-4 text-lg font-semibold text-gray-900">Routing Strategy</h2>
              <div className="space-y-2">
                {routingOptions.map((option) => (
                  <button
                    key={option.value}
                    onClick={() => setRouting(option.value)}
                    className={cn(
                      "w-full flex items-center gap-3 rounded-lg p-3 text-left transition-all",
                      routing === option.value
                        ? "bg-blue-50 ring-2 ring-blue-500"
                        : "bg-gray-50 hover:bg-gray-100"
                    )}
                  >
                    <span className="text-xl">{option.icon}</span>
                    <div className="flex-1">
                      <div className="font-medium text-gray-900">{option.label}</div>
                      <div className="text-xs text-gray-500">{option.description}</div>
                    </div>
                    {option.value === "Balanced" && (
                      <span className="rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-700">
                        Recommended
                      </span>
                    )}
                  </button>
                ))}
              </div>
            </div>

            {/* Cost Estimate */}
            <div className="rounded-xl bg-gradient-to-br from-gray-800 to-gray-900 p-6 text-white shadow-sm">
              <h2 className="mb-4 text-lg font-semibold">Cost Estimate</h2>
              {characterCount > 0 ? (
                <div className="space-y-4">
                  <div className="flex items-baseline justify-between">
                    <span className="text-gray-400">Estimated Cost</span>
                    <span className="text-3xl font-bold">
                      ${(characterCount * 0.00003).toFixed(2)}
                    </span>
                  </div>
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-gray-400">Characters</span>
                      <span>{characterCount.toLocaleString()}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-400">Est. Duration</span>
                      <span>~{Math.ceil(wordCount / 150)} min</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-400">Provider</span>
                      <span>
                        {selectedVoice
                          ? mockVoices.find((v) => v.id === selectedVoice)?.provider
                          : "Auto-select"}
                      </span>
                    </div>
                  </div>
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
                disabled={!text || !selectedVoice}
                className="w-full rounded-xl bg-blue-600 px-6 py-4 text-lg font-semibold text-white shadow-lg shadow-blue-600/25 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed disabled:shadow-none transition-all"
              >
                Generate Audio
              </button>
              <button
                disabled={!text || !selectedVoice}
                className="w-full rounded-xl bg-white px-6 py-3 font-medium text-gray-700 ring-1 ring-gray-200 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
              >
                Preview First 500 Characters (Free)
              </button>
            </div>

            {/* Info */}
            <div className="rounded-lg bg-blue-50 p-4 text-sm text-blue-800">
              <p className="font-medium">Your remaining quota</p>
              <p className="mt-1 text-blue-600">10,000 characters this month</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
