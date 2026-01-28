"use client";

import { useState, useCallback } from "react";
import { cn } from "@/lib/utils";

interface FeedbackFormProps {
  generationId: string;
  onSubmit?: (feedback: FeedbackData) => void;
  className?: string;
}

export interface FeedbackData {
  generationId: string;
  rating: number;
  tags: string[];
  comment: string;
}

const QUICK_TAGS = [
  { id: "too-fast", label: "Too fast" },
  { id: "too-slow", label: "Too slow" },
  { id: "pronunciation", label: "Pronunciation issue" },
  { id: "unnatural", label: "Sounds unnatural" },
  { id: "great-quality", label: "Great quality" },
  { id: "perfect-pacing", label: "Perfect pacing" },
];

export function FeedbackForm({
  generationId,
  onSubmit,
  className,
}: FeedbackFormProps) {
  const [rating, setRating] = useState(0);
  const [hoveredRating, setHoveredRating] = useState(0);
  const [selectedTags, setSelectedTags] = useState<string[]>([]);
  const [comment, setComment] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSubmitted, setIsSubmitted] = useState(false);

  const toggleTag = useCallback((tagId: string) => {
    setSelectedTags((prev) =>
      prev.includes(tagId)
        ? prev.filter((t) => t !== tagId)
        : [...prev, tagId]
    );
  }, []);

  const handleSubmit = async () => {
    if (rating === 0) return;

    setIsSubmitting(true);

    const feedback: FeedbackData = {
      generationId,
      rating,
      tags: selectedTags,
      comment,
    };

    try {
      // TODO: Replace with actual API call
      // await api.POST("/api/v1/generations/{id}/feedback", { body: feedback });
      await new Promise((resolve) => setTimeout(resolve, 500));

      onSubmit?.(feedback);
      setIsSubmitted(true);
    } catch (err) {
      console.error("Failed to submit feedback:", err);
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isSubmitted) {
    return (
      <div className={cn("rounded-lg border p-6 text-center", className)}>
        <div className="mx-auto mb-3 flex h-10 w-10 items-center justify-center rounded-full bg-green-100">
          <svg
            className="h-5 w-5 text-green-600"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M5 13l4 4L19 7"
            />
          </svg>
        </div>
        <p className="font-medium">Thanks for your feedback!</p>
        <p className="mt-1 text-sm text-gray-500">
          Your input helps us improve our voices.
        </p>
      </div>
    );
  }

  return (
    <div className={cn("rounded-lg border p-6 space-y-4", className)}>
      <h3 className="font-semibold">Rate this generation</h3>

      {/* Star rating */}
      <div className="flex items-center gap-1">
        {[1, 2, 3, 4, 5].map((star) => (
          <button
            key={star}
            type="button"
            onClick={() => setRating(star)}
            onMouseEnter={() => setHoveredRating(star)}
            onMouseLeave={() => setHoveredRating(0)}
            className="p-1 transition-transform hover:scale-110"
          >
            <svg
              className={cn(
                "h-8 w-8 transition-colors",
                (hoveredRating || rating) >= star
                  ? "fill-yellow-400 text-yellow-400"
                  : "fill-gray-200 text-gray-200"
              )}
              viewBox="0 0 24 24"
            >
              <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" />
            </svg>
          </button>
        ))}
        {rating > 0 && (
          <span className="ml-2 text-sm text-gray-500">{rating}/5</span>
        )}
      </div>

      {/* Quick tags */}
      <div>
        <p className="mb-2 text-sm text-gray-500">Quick feedback (optional)</p>
        <div className="flex flex-wrap gap-2">
          {QUICK_TAGS.map((tag) => (
            <button
              key={tag.id}
              type="button"
              onClick={() => toggleTag(tag.id)}
              className={cn(
                "rounded-full px-3 py-1 text-sm transition-colors",
                selectedTags.includes(tag.id)
                  ? "bg-blue-100 text-blue-700"
                  : "bg-gray-100 text-gray-600 hover:bg-gray-200"
              )}
            >
              {tag.label}
            </button>
          ))}
        </div>
      </div>

      {/* Comment */}
      <div>
        <label htmlFor="comment" className="mb-1 block text-sm text-gray-500">
          Additional comments (optional)
        </label>
        <textarea
          id="comment"
          value={comment}
          onChange={(e) => setComment(e.target.value)}
          placeholder="Tell us more about your experience..."
          className="w-full rounded-lg border p-3 text-sm resize-none focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          rows={3}
        />
      </div>

      {/* Submit */}
      <button
        type="button"
        onClick={handleSubmit}
        disabled={rating === 0 || isSubmitting}
        className="w-full rounded-lg bg-black py-2 text-white hover:bg-gray-800 disabled:opacity-50"
      >
        {isSubmitting ? "Submitting..." : "Submit Feedback"}
      </button>
    </div>
  );
}
