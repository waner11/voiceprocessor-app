import { render, screen, fireEvent, act, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { VoicePreviewPlayer } from "../VoicePreviewPlayer";

// Mock HTMLAudioElement
let mockAudioInstances: MockAudio[] = [];

class MockAudio {
  src = "";
  currentTime = 0;
  paused = true;
  private _eventListeners: Record<string, Function[]> = {};

  constructor() {
    mockAudioInstances.push(this);
  }

  play = vi.fn(() => {
    this.paused = false;
    this._emit("playing");
    return Promise.resolve();
  });

  pause = vi.fn(() => {
    this.paused = true;
    this._emit("pause");
  });

  load = vi.fn();

  addEventListener = vi.fn((event: string, handler: Function) => {
    if (!this._eventListeners[event]) this._eventListeners[event] = [];
    this._eventListeners[event].push(handler);
  });

  removeEventListener = vi.fn((event: string, handler: Function) => {
    if (this._eventListeners[event]) {
      this._eventListeners[event] = this._eventListeners[event].filter(
        (h) => h !== handler
      );
    }
  });

  _emit(event: string, data?: unknown) {
    this._eventListeners[event]?.forEach((h) => h(data));
  }
}

beforeEach(() => {
  mockAudioInstances = [];
  vi.stubGlobal("Audio", MockAudio);
});

afterEach(() => {
  vi.restoreAllMocks();
});

describe("VoicePreviewPlayer", () => {
  describe("rendering", () => {
    it("renders a play button when previewUrl is provided", () => {
      render(
        <VoicePreviewPlayer previewUrl="https://example.com/preview.mp3" />
      );
      const button = screen.getByRole("button", { name: /preview voice/i });
      expect(button).toBeInTheDocument();
      expect(button).not.toBeDisabled();
    });

    it("renders a disabled button when previewUrl is null", () => {
      render(<VoicePreviewPlayer previewUrl={null} />);
      const button = screen.getByRole("button", { name: /no preview/i });
      expect(button).toBeInTheDocument();
      expect(button).toBeDisabled();
    });

    it("renders nothing visible when previewUrl is undefined", () => {
      render(<VoicePreviewPlayer />);
      const button = screen.getByRole("button", { name: /no preview/i });
      expect(button).toBeDisabled();
    });
  });

  describe("play/pause behavior", () => {
    it("plays audio when play button is clicked", async () => {
      render(
        <VoicePreviewPlayer previewUrl="https://example.com/preview.mp3" />
      );
      const button = screen.getByRole("button", { name: /preview voice/i });

      await act(async () => {
        fireEvent.click(button);
      });

      expect(mockAudioInstances.length).toBeGreaterThan(0);
      const audio = mockAudioInstances[0];
      expect(audio.play).toHaveBeenCalled();
    });

    it("pauses audio when pause button is clicked while playing", async () => {
      render(
        <VoicePreviewPlayer previewUrl="https://example.com/preview.mp3" />
      );
      const button = screen.getByRole("button", { name: /preview voice/i });

      // Click play
      await act(async () => {
        fireEvent.click(button);
      });

      // Now should show pause button
      const pauseButton = screen.getByRole("button", { name: /pause/i });

      await act(async () => {
        fireEvent.click(pauseButton);
      });

      const audio = mockAudioInstances[0];
      expect(audio.pause).toHaveBeenCalled();
    });

    it("shows play icon in idle state and pause icon when playing", async () => {
      render(
        <VoicePreviewPlayer previewUrl="https://example.com/preview.mp3" />
      );

      // Initially shows play
      expect(
        screen.getByRole("button", { name: /preview voice/i })
      ).toBeInTheDocument();

      // Click play
      await act(async () => {
        fireEvent.click(
          screen.getByRole("button", { name: /preview voice/i })
        );
      });

      // Now shows pause
      expect(
        screen.getByRole("button", { name: /pause/i })
      ).toBeInTheDocument();
    });
  });

  describe("error handling", () => {
    it("shows error state when audio fails to load", async () => {
      render(
        <VoicePreviewPlayer previewUrl="https://example.com/bad-url.mp3" />
      );

      const button = screen.getByRole("button", { name: /preview voice/i });

      // Click play, then simulate error

      // Re-render to get fresh audio mock
      await act(async () => {
        fireEvent.click(button);
      });

      // Simulate error event
      await act(async () => {
        mockAudioInstances[0]?._emit("error", {
          target: { error: { code: 4 } },
        });
      });

      // Should show error state
      const errorButton = screen.getByRole("button", { name: /preview unavailable/i });
      expect(errorButton).toBeInTheDocument();
      expect(errorButton).toBeDisabled();
    });
  });

  describe("single-active-preview behavior", () => {
    it("stops previous preview when a new one starts via onPlay callback", async () => {
      const stopOthers = vi.fn();

      render(
        <VoicePreviewPlayer
          previewUrl="https://example.com/preview.mp3"
          onPlay={stopOthers}
        />
      );

      const button = screen.getByRole("button", { name: /preview voice/i });

      await act(async () => {
        fireEvent.click(button);
      });

      expect(stopOthers).toHaveBeenCalled();
    });

    it("stops playback when stop() is called externally", async () => {
      let stopRef: (() => void) | undefined;

      render(
        <VoicePreviewPlayer
          previewUrl="https://example.com/preview.mp3"
          onPlay={() => {}}
          stopRef={(stop) => {
            stopRef = stop;
          }}
        />
      );

      const button = screen.getByRole("button", { name: /preview voice/i });

      // Start playing
      await act(async () => {
        fireEvent.click(button);
      });

      // Externally stop
      await act(async () => {
        stopRef?.();
      });

      const audio = mockAudioInstances[0];
      expect(audio.pause).toHaveBeenCalled();
    });
  });

  describe("audio ended", () => {
    it("resets to idle state when audio finishes playing", async () => {
      render(
        <VoicePreviewPlayer previewUrl="https://example.com/preview.mp3" />
      );

      const button = screen.getByRole("button", { name: /preview voice/i });

      // Start playing
      await act(async () => {
        fireEvent.click(button);
      });

      // Simulate audio ended
      await act(async () => {
        mockAudioInstances[0]?._emit("ended");
      });

      // Should be back to play state
      expect(
        screen.getByRole("button", { name: /preview voice/i })
      ).toBeInTheDocument();
    });
  });
});
