import * as signalR from "@microsoft/signalr";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";
const HUB_URL = `${API_URL}/hubs/generation`;

export type GenerationStatus = "queued" | "processing" | "completed" | "failed" | "cancelled";

export interface StatusUpdateEvent {
  generationId: string;
  status: GenerationStatus;
  message?: string;
}

export interface ProgressEvent {
  generationId: string;
  progress: number;
  currentChunk?: number;
  totalChunks?: number;
}

export interface CompletedEvent {
  generationId: string;
  audioUrl: string;
  duration: number;
}

export interface FailedEvent {
  generationId: string;
  error: string;
}

export type GenerationHubEvents = {
  StatusUpdate: (event: StatusUpdateEvent) => void;
  Progress: (event: ProgressEvent) => void;
  Completed: (event: CompletedEvent) => void;
  Failed: (event: FailedEvent) => void;
};

let connection: signalR.HubConnection | null = null;
let refCount = 0;
let isStarting = false;
let handlersRegistered = false;
let startPromise: Promise<void> | null = null;
const stateChangeCallbacks: Array<(state: signalR.HubConnectionState) => void> = [];

export function getConnection(): signalR.HubConnection {
  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, { withCredentials: true })
      .withAutomaticReconnect()
      .configureLogging(
        process.env.NODE_ENV === "production"
          ? signalR.LogLevel.Warning
          : signalR.LogLevel.Information
      )
      .build();
  }
  return connection;
}

export async function startConnection(): Promise<void> {
  refCount++;
  
  const conn = getConnection();
  
  if (conn.state === signalR.HubConnectionState.Disconnected && !isStarting) {
    isStarting = true;
    startPromise = conn.start()
      .then(() => {
        console.log("SignalR connected");
      })
      .catch((err) => {
        console.error("SignalR connection failed:", err);
        refCount = 0;
        throw err;
      })
      .finally(() => {
        isStarting = false;
        startPromise = null;
      });
  }
  
  if (startPromise) {
    try {
      await startPromise;
    } catch {
      refCount = Math.max(0, refCount - 1);
      throw new Error("SignalR connection failed");
    }
  }
}

export function stopConnection(): void {
  refCount = Math.max(0, refCount - 1);
  
  if (refCount === 0) {
    const conn = getConnection();
    if (conn.state !== signalR.HubConnectionState.Disconnected) {
      conn.stop();
      console.log("SignalR disconnected");
    }
  }
}

export function onEvent<K extends keyof GenerationHubEvents>(
  event: K,
  callback: GenerationHubEvents[K]
): void {
  const conn = getConnection();
  conn.on(event, callback);
}

export function offEvent<K extends keyof GenerationHubEvents>(
  event: K,
  callback: GenerationHubEvents[K]
): void {
  const conn = getConnection();
  conn.off(event, callback);
}

export function getConnectionState(): signalR.HubConnectionState {
  return connection?.state ?? signalR.HubConnectionState.Disconnected;
}

export function onStateChange(callback: (state: signalR.HubConnectionState) => void): () => void {
  stateChangeCallbacks.push(callback);
  
  if (!handlersRegistered) {
    const conn = getConnection();
    conn.onclose(() => {
      stateChangeCallbacks.forEach(cb => cb(signalR.HubConnectionState.Disconnected));
    });
    conn.onreconnecting(() => {
      stateChangeCallbacks.forEach(cb => cb(signalR.HubConnectionState.Reconnecting));
    });
    conn.onreconnected(() => {
      stateChangeCallbacks.forEach(cb => cb(signalR.HubConnectionState.Connected));
    });
    handlersRegistered = true;
  }
  
  return () => {
    const index = stateChangeCallbacks.indexOf(callback);
    if (index > -1) {
      stateChangeCallbacks.splice(index, 1);
    }
  };
}

export function __resetForTesting(): void {
  connection = null;
  refCount = 0;
  isStarting = false;
  handlersRegistered = false;
  startPromise = null;
  stateChangeCallbacks.length = 0;
}
