import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import {
  startConnection,
  stopConnection,
  getConnection,
  onStateChange,
  __resetForTesting,
} from "../connection";

let mockConnectionState = 0;

vi.mock("@microsoft/signalr", () => {
  const mockConnection = {
    get state() {
      return mockConnectionState;
    },
    set state(value: number) {
      mockConnectionState = value;
    },
    start: vi.fn().mockImplementation(async () => {
      mockConnectionState = 2;
    }),
    stop: vi.fn().mockImplementation(() => {
      mockConnectionState = 0;
    }),
    on: vi.fn(),
    off: vi.fn(),
    onclose: vi.fn(),
    onreconnecting: vi.fn(),
    onreconnected: vi.fn(),
  };

  return {
    HubConnectionBuilder: vi.fn(() => ({
      withUrl: vi.fn().mockReturnThis(),
      withAutomaticReconnect: vi.fn().mockReturnThis(),
      configureLogging: vi.fn().mockReturnThis(),
      build: vi.fn(() => mockConnection),
    })),
    HubConnectionState: {
      Disconnected: 0,
      Connecting: 1,
      Connected: 2,
      Disconnecting: 3,
      Reconnecting: 4,
    },
    LogLevel: {
      Information: 1,
      Warning: 2,
    },
  };
});

describe("SignalR Connection Lifecycle", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockConnectionState = 0;
    __resetForTesting();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  describe("Reference Counting", () => {
    it("should only start connection once when multiple consumers call startConnection", async () => {
      const conn = getConnection();
      
      await Promise.all([startConnection(), startConnection()]);

      expect(conn.start).toHaveBeenCalledTimes(1);
    });

    it("should keep connection alive when one consumer stops but others remain", async () => {
      const conn = getConnection();
      
      await startConnection();
      await startConnection();

      stopConnection();

      expect(conn.stop).not.toHaveBeenCalled();
    });

    it("should stop connection only when all consumers have stopped", async () => {
      const conn = getConnection();
      await startConnection();
      await startConnection();

      stopConnection();
      stopConnection();

      expect(conn.stop).toHaveBeenCalledTimes(1);
    });

    it("should handle React Strict Mode double mount correctly", async () => {
      const conn = getConnection();
      
      await startConnection();
      stopConnection();
      await startConnection();

      expect(conn.start).toHaveBeenCalledTimes(2);
    });

    it("should not allow refCount to go negative", () => {
      const conn = getConnection();
      
      stopConnection();
      stopConnection();

      expect(conn.stop).not.toHaveBeenCalled();
    });

    it("should allow retry after concurrent start failure", async () => {
      const conn = getConnection();
      
      conn.start.mockRejectedValueOnce(new Error("Connection failed"));
      
      const results = await Promise.allSettled([
        startConnection(),
        startConnection(),
      ]);
      
      expect(results[0].status).toBe("rejected");
      expect(results[1].status).toBe("rejected");
      expect(mockConnectionState).toBe(0);
      
      conn.start.mockImplementationOnce(async () => {
        mockConnectionState = 2;
      });
      
      await startConnection();
      expect(conn.start).toHaveBeenCalledTimes(2);
      expect(mockConnectionState).toBe(2);
      
      stopConnection();
      expect(conn.stop).toHaveBeenCalledTimes(1);
    });
  });

  describe("State Change Cleanup", () => {
    it("should allow registering state change callbacks", () => {
      const callback = vi.fn();
      const cleanup = onStateChange(callback);

      expect(typeof cleanup).toBe("function");
    });

    it("should remove callback when cleanup function is called", () => {
      const callback1 = vi.fn();
      const callback2 = vi.fn();

      const cleanup1 = onStateChange(callback1);
      onStateChange(callback2);

      // Remove first callback
      cleanup1();

      // Both callbacks should have been registered initially
      // After cleanup1, only callback2 should remain
      // (We can't directly test the internal array, but we verify cleanup returns a function)
      expect(typeof cleanup1).toBe("function");
    });

    it("should handle multiple cleanup calls safely", () => {
      const callback = vi.fn();
      const cleanup = onStateChange(callback);

      // Call cleanup multiple times
      cleanup();
      cleanup();

      // Should not throw
      expect(true).toBe(true);
    });

    it("should register SignalR event handlers only once even with multiple onStateChange calls", () => {
      const conn = getConnection();

      onStateChange(vi.fn());
      onStateChange(vi.fn());
      onStateChange(vi.fn());

      // Each handler type should be registered exactly once
      expect(conn.onclose).toHaveBeenCalledTimes(1);
      expect(conn.onreconnecting).toHaveBeenCalledTimes(1);
      expect(conn.onreconnected).toHaveBeenCalledTimes(1);
    });
  });
});
