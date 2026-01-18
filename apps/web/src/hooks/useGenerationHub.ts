"use client";

import { useEffect, useCallback } from "react";
import { useQueryClient } from "@tanstack/react-query";
import {
  startConnection,
  stopConnection,
  onEvent,
  offEvent,
  type StatusUpdateEvent,
  type ProgressEvent,
  type CompletedEvent,
  type FailedEvent,
} from "@/lib/signalr";

export function useGenerationHub() {
  const queryClient = useQueryClient();

  const handleStatusUpdate = useCallback(
    (event: StatusUpdateEvent) => {
      queryClient.invalidateQueries({
        queryKey: ["generation", event.generationId],
      });
    },
    [queryClient]
  );

  const handleProgress = useCallback(
    (event: ProgressEvent) => {
      queryClient.setQueryData(
        ["generation", event.generationId, "progress"],
        event
      );
    },
    [queryClient]
  );

  const handleCompleted = useCallback(
    (event: CompletedEvent) => {
      queryClient.invalidateQueries({
        queryKey: ["generation", event.generationId],
      });
      queryClient.invalidateQueries({
        queryKey: ["generations"],
      });
    },
    [queryClient]
  );

  const handleFailed = useCallback(
    (event: FailedEvent) => {
      queryClient.invalidateQueries({
        queryKey: ["generation", event.generationId],
      });
    },
    [queryClient]
  );

  useEffect(() => {
    startConnection();

    onEvent("StatusUpdate", handleStatusUpdate);
    onEvent("Progress", handleProgress);
    onEvent("Completed", handleCompleted);
    onEvent("Failed", handleFailed);

    return () => {
      offEvent("StatusUpdate", handleStatusUpdate);
      offEvent("Progress", handleProgress);
      offEvent("Completed", handleCompleted);
      offEvent("Failed", handleFailed);
      stopConnection();
    };
  }, [handleStatusUpdate, handleProgress, handleCompleted, handleFailed]);
}
