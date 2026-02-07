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

export function getConnection(): signalR.HubConnection {
  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, { withCredentials: true })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();
  }
  return connection;
}

export async function startConnection(): Promise<void> {
  const conn = getConnection();
  if (conn.state === signalR.HubConnectionState.Disconnected) {
    await conn.start();
  }
}

export async function stopConnection(): Promise<void> {
  if (connection && connection.state === signalR.HubConnectionState.Connected) {
    await connection.stop();
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

export function onStateChange(callback: (state: signalR.HubConnectionState) => void): void {
  const conn = getConnection();
  conn.onclose(() => callback(signalR.HubConnectionState.Disconnected));
  conn.onreconnecting(() => callback(signalR.HubConnectionState.Reconnecting));
  conn.onreconnected(() => callback(signalR.HubConnectionState.Connected));
}
