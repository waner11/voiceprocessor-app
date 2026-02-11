"use client";

import { useEffect, useCallback, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import {
  startConnection,
  stopConnection,
  onEvent,
  offEvent,
  getConnectionState,
  onStateChange,
  type StatusUpdateEvent,
  type ProgressEvent,
  type CompletedEvent,
  type FailedEvent,
} from "@/lib/signalr";
import * as signalR from "@microsoft/signalr";

export function useGenerationHub() {
  const queryClient = useQueryClient();

  const handleStatusUpdate = useCallback(
    (event: StatusUpdateEvent) => {
      queryClient.invalidateQueries({
        queryKey: ["generation", event.generationId],
      });
      queryClient.invalidateQueries({
        queryKey: ["generations"],
      });
    },
    [queryClient]
  );

  const handleProgress = useCallback(
    (event: ProgressEvent) => {
      queryClient.setQueryData(
        ["generation", event.generationId],
        (old: Record<string, unknown> | undefined) =>
          old
            ? {
                ...old,
                progress: event.progress,
                chunksCompleted: event.currentChunk ?? old.chunksCompleted,
                chunkCount: event.totalChunks ?? old.chunkCount,
              }
            : old
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
      queryClient.invalidateQueries({
        queryKey: ["generations"],
      });
    },
    [queryClient]
  );

  useEffect(() => {
    startConnection().catch((err) => {
      console.error("Failed to establish SignalR connection:", err);
    });

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

export function useSignalRStatus() {
  const [state, setState] = useState<signalR.HubConnectionState>(() => 
    getConnectionState()
  );

  useEffect(() => {
    const cleanup = onStateChange(setState);
    return cleanup;
  }, []);

  return state;
}
