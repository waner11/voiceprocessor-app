"use client";

import { useParams } from 'next/navigation';
import Link from 'next/link';
import { useGeneration } from '@/hooks/useGenerations';

export default function GenerationPage() {
  const params = useParams();
  const id = params?.id as string;
  const { data: generation, error, isLoading } = useGeneration(id);

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="text-center">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900 dark:border-white mx-auto mb-4"></div>
            <p className="text-gray-600 dark:text-gray-400">Loading generation...</p>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center">
          <h1 className="mb-4 text-2xl font-bold text-red-600 dark:text-red-400">Error</h1>
          <p className="mb-6 text-gray-600 dark:text-gray-400">
            Failed to fetch generation: {error.message || 'Unknown error'}
          </p>
          <Link 
            href="/generations" 
            className="inline-block rounded-lg bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
          >
            Back to Generations
          </Link>
        </div>
      </div>
    );
  }

  if (!generation) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center">
          <p className="text-gray-600 dark:text-gray-400">Generation not found</p>
          <Link 
            href="/generations" 
            className="inline-block mt-4 rounded-lg bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
          >
            Back to Generations
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-6">
        <Link 
          href="/generations" 
          className="text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300"
        >
          ← Back to Generations
        </Link>
      </div>
      <h1 className="mb-8 text-3xl font-bold text-gray-900 dark:text-white">Generation Details</h1>

      <div className="grid gap-8 lg:grid-cols-3">
        <div className="lg:col-span-2 space-y-6">
          {/* AudioPlayer component will go here */}
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
            <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Audio Player</h2>
            <div className="h-32 rounded-lg bg-gray-100 dark:bg-gray-800 flex items-center justify-center text-gray-500 dark:text-gray-400">
              Waveform visualization will appear here
            </div>
            <div className="mt-4 flex items-center justify-between">
              <div className="flex items-center gap-2">
                <button className="rounded-full border border-gray-200 dark:border-gray-700 p-2 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800">
                  ▶
                </button>
                <span className="text-sm text-gray-500 dark:text-gray-400">0:00 / --:--</span>
              </div>
              <div className="flex items-center gap-2">
                <select className="rounded border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-2 py-1 text-sm text-gray-900 dark:text-white">
                  <option>1x</option>
                  <option>1.25x</option>
                  <option>1.5x</option>
                  <option>2x</option>
                </select>
                <button className="rounded border border-gray-200 dark:border-gray-700 px-3 py-1 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800">
                  Download
                </button>
              </div>
            </div>
          </div>

          {/* Chapters */}
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
            <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Chapters</h2>
            <p className="text-gray-500 dark:text-gray-400">No chapters detected</p>
          </div>
        </div>

        <div className="space-y-6">
          {/* GenerationStatus component will go here */}
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
            <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Status</h2>
            <p className="text-gray-500 dark:text-gray-400">Loading generation {id}...</p>
          </div>

          {/* Generation info */}
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
            <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Details</h2>
            <dl className="space-y-2 text-sm">
              <div className="flex justify-between">
                <dt className="text-gray-500 dark:text-gray-400">Voice</dt>
                <dd className="text-gray-900 dark:text-white">--</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-gray-500 dark:text-gray-400">Provider</dt>
                <dd className="text-gray-900 dark:text-white">--</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-gray-500 dark:text-gray-400">Duration</dt>
                <dd className="text-gray-900 dark:text-white">--</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-gray-500 dark:text-gray-400">Cost</dt>
                <dd className="text-gray-900 dark:text-white">--</dd>
              </div>
            </dl>
          </div>

          {/* Feedback */}
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
            <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Feedback</h2>
            <p className="text-gray-500 dark:text-gray-400">Rate this generation...</p>
          </div>
        </div>
      </div>
    </div>
  );
}
