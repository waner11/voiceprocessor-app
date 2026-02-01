"use client";

import { useParams } from 'next/navigation';
import Link from 'next/link';
import { useGeneration, useSubmitFeedback } from '@/hooks/useGenerations';
import { GenerationStatus } from '@/components/GenerationStatus/GenerationStatus';
import { mapGenerationStatus } from '@/lib/utils/mapGenerationStatus';
import { AudioPlayer } from '@/components/AudioPlayer/AudioPlayer';
import { FeedbackForm, type FeedbackData } from '@/components/FeedbackForm/FeedbackForm';
import { formatNumber } from '@/utils/formatNumber';

export default function GenerationPage() {
  const params = useParams();
  const id = params?.id as string;
  const { data: generation, error, isLoading } = useGeneration(id);
  const submitFeedback = useSubmitFeedback();

  const handleFeedbackSubmit = (feedback: FeedbackData) => {
    const tagPrefix = feedback.tags.length > 0
      ? `[Tags: ${feedback.tags.join(", ")}] `
      : "";
    submitFeedback.mutate({
      id,
      rating: feedback.rating,
      comment: tagPrefix + feedback.comment,
    });
  };

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
    const errorObj = error as { code?: string | null; message?: string | null };
    const is404 = errorObj.code === 'GENERATION_NOT_FOUND' || errorObj.code === 'NOT_FOUND';
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-center">
          <h1 className="mb-4 text-2xl font-bold text-red-600 dark:text-red-400">
            {is404 ? 'Generation Not Found' : 'Error'}
          </h1>
          <p className="mb-6 text-gray-600 dark:text-gray-400">
            {is404 
              ? 'The generation you are looking for does not exist or has been deleted.'
              : `Failed to fetch generation: ${error.message || 'Unknown error'}`
            }
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
          <h1 className="mb-4 text-2xl font-bold text-gray-900 dark:text-white">Generation Not Found</h1>
          <p className="mb-6 text-gray-600 dark:text-gray-400">
            The generation you are looking for does not exist.
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
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
            <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Audio Player</h2>
            {generation.audioUrl ? (
              <AudioPlayer audioUrl={generation.audioUrl} />
            ) : (
              <div className="rounded-lg bg-gray-100 dark:bg-gray-800 p-8 text-center">
                <p className="text-gray-500 dark:text-gray-400">
                  {generation.status === 'Completed' 
                    ? 'Audio file not available' 
                    : `Audio will appear here once generation completes${generation.progress ? ` (${generation.progress}%)` : ''}`
                  }
                </p>
              </div>
            )}
          </div>

          {/* Chapters */}
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
            <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Chapters</h2>
            <p className="text-gray-500 dark:text-gray-400">No chapters detected</p>
          </div>
        </div>

        <div className="space-y-6">
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
            <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Status</h2>
            {generation.status === 'Cancelled' ? (
              <div className="rounded-lg bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 p-4">
                <p className="text-yellow-800 dark:text-yellow-200 font-medium">Cancelled</p>
                <p className="text-yellow-700 dark:text-yellow-300 text-sm mt-1">
                  This generation was cancelled before completion.
                </p>
              </div>
            ) : (
              <GenerationStatus
                generationId={generation.id}
                status={mapGenerationStatus(generation.status)}
                progress={generation.progress}
                error={generation.errorMessage ?? undefined}
              />
            )}
          </div>

          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
            <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Details</h2>
            <dl className="space-y-2 text-sm">
              <div className="flex justify-between">
                <dt className="text-gray-500 dark:text-gray-400">Provider</dt>
                <dd className="text-gray-900 dark:text-white">{generation.provider ?? '--'}</dd>
              </div>
               <div className="flex justify-between">
                 <dt className="text-gray-500 dark:text-gray-400">Characters</dt>
                 <dd className="text-gray-900 dark:text-white">{formatNumber(generation.characterCount ?? 0)}</dd>
               </div>
              <div className="flex justify-between">
                <dt className="text-gray-500 dark:text-gray-400">Duration</dt>
                <dd className="text-gray-900 dark:text-white">
                  {generation.audioDurationMs 
                    ? `${Math.floor(generation.audioDurationMs / 60000)}:${String(Math.floor((generation.audioDurationMs % 60000) / 1000)).padStart(2, '0')}`
                    : '--'
                  }
                </dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-gray-500 dark:text-gray-400">Cost</dt>
                <dd className="text-gray-900 dark:text-white">
                  {generation.actualCost != null 
                    ? `$${generation.actualCost.toFixed(4)}`
                    : generation.estimatedCost != null
                    ? `~$${generation.estimatedCost.toFixed(4)}`
                    : '--'
                  }
                </dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-gray-500 dark:text-gray-400">Created</dt>
                <dd className="text-gray-900 dark:text-white">
                  {new Date(generation.createdAt).toLocaleDateString('en-US', { 
                    month: 'short', 
                    day: 'numeric', 
                    year: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit'
                  })}
                </dd>
              </div>
              {generation.completedAt && (
                <div className="flex justify-between">
                  <dt className="text-gray-500 dark:text-gray-400">Completed</dt>
                  <dd className="text-gray-900 dark:text-white">
                    {new Date(generation.completedAt).toLocaleDateString('en-US', { 
                      month: 'short', 
                      day: 'numeric', 
                      year: 'numeric',
                      hour: '2-digit',
                      minute: '2-digit'
                    })}
                  </dd>
                </div>
              )}
            </dl>
          </div>

          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
            <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Feedback</h2>
            {generation.status === 'Completed' ? (
              <FeedbackForm 
                generationId={generation.id}
                onSubmit={handleFeedbackSubmit}
              />
            ) : (
              <p className="text-gray-500 dark:text-gray-400">
                Feedback will be available once generation completes
              </p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
