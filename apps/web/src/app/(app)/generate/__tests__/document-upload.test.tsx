import { render, screen, fireEvent, waitFor, act } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import GeneratePage from "../page";

// Mock the hooks
vi.mock("@/hooks", () => ({
  useVoices: vi.fn(() => ({
    data: {
      items: [
        {
          id: "voice-1",
          name: "Test Voice",
          provider: "ElevenLabs",
          language: "English",
          gender: "Male",
          previewUrl: null,
        },
      ],
    },
    isLoading: false,
  })),
  useEstimateCost: vi.fn(() => ({
    mutate: vi.fn(),
    data: null,
    isPending: false,
  })),
  useCreateGeneration: vi.fn(() => ({
    mutate: vi.fn(),
    isPending: false,
  })),
}));

vi.mock("next/navigation", () => ({
  useRouter: vi.fn(() => ({
    push: vi.fn(),
  })),
}));

// Mock fetch for document extraction API
const mockFetch = vi.fn();
vi.stubGlobal("fetch", mockFetch);

describe("Document Upload on Generate Page", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockFetch.mockReset();
  });

  it("renders Upload File button", () => {
    render(<GeneratePage />);
    expect(screen.getByRole("button", { name: /upload file/i })).toBeInTheDocument();
  });

  it("has a hidden file input accepting .pdf and .docx", () => {
    const { container } = render(<GeneratePage />);
    const fileInput = container.querySelector("input[type='file']") as HTMLInputElement;
    expect(fileInput).toBeInTheDocument();
    expect(fileInput.accept).toContain(".pdf");
    expect(fileInput.accept).toContain(".docx");
  });

  it("clicking Upload File button triggers file input click", () => {
    const { container } = render(<GeneratePage />);
    const fileInput = container.querySelector("input[type='file']") as HTMLInputElement;
    const clickSpy = vi.spyOn(fileInput, "click");

    const uploadButton = screen.getByRole("button", { name: /upload file/i });
    fireEvent.click(uploadButton);

    expect(clickSpy).toHaveBeenCalled();
  });

  it("shows loading state during extraction", async () => {
    // Mock a slow API response
    mockFetch.mockImplementation(
      () =>
        new Promise((resolve) =>
          setTimeout(
            () =>
              resolve({
                ok: true,
                json: () =>
                  Promise.resolve({
                    text: "Extracted text",
                    pageCount: 1,
                    wordCount: 2,
                    characterCount: 14,
                  }),
              }),
            100
          )
        )
    );

    const { container } = render(<GeneratePage />);
    const fileInput = container.querySelector("input[type='file']") as HTMLInputElement;

    const file = new File(["dummy content"], "test.pdf", { type: "application/pdf" });
    await act(async () => {
      fireEvent.change(fileInput, { target: { files: [file] } });
    });

    // Should show loading state
    expect(screen.getByText(/extracting/i)).toBeInTheDocument();
  });

  it("populates textarea with extracted text on success", async () => {
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: () =>
        Promise.resolve({
          text: "Hello from the document",
          pageCount: 1,
          wordCount: 4,
          characterCount: 23,
        }),
    });

    const { container } = render(<GeneratePage />);
    const fileInput = container.querySelector("input[type='file']") as HTMLInputElement;

    const file = new File(["dummy content"], "test.pdf", { type: "application/pdf" });
    await act(async () => {
      fireEvent.change(fileInput, { target: { files: [file] } });
    });

    await waitFor(() => {
      const textarea = screen.getByPlaceholderText(/paste your text/i) as HTMLTextAreaElement;
      expect(textarea.value).toBe("Hello from the document");
    });
  });

  it("shows warning when extracted text exceeds 500,000 characters", async () => {
    const longText = "a".repeat(500_001);
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: () =>
        Promise.resolve({
          text: longText,
          pageCount: 10,
          wordCount: 500001,
          characterCount: 500001,
        }),
    });

    const { container } = render(<GeneratePage />);
    const fileInput = container.querySelector("input[type='file']") as HTMLInputElement;

    const file = new File(["dummy content"], "large.pdf", { type: "application/pdf" });
    await act(async () => {
      fireEvent.change(fileInput, { target: { files: [file] } });
    });

    await waitFor(() => {
      expect(screen.getByText(/document text exceeds/i)).toBeInTheDocument();
    });
  });

  it("shows error toast on API failure", async () => {
    mockFetch.mockResolvedValueOnce({
      ok: false,
      status: 415,
      json: () =>
        Promise.resolve({
          code: "UNSUPPORTED_FORMAT",
          message: "Unsupported file format",
        }),
    });

    const { container } = render(<GeneratePage />);
    const fileInput = container.querySelector("input[type='file']") as HTMLInputElement;

    const file = new File(["dummy content"], "test.txt", { type: "text/plain" });
    await act(async () => {
      fireEvent.change(fileInput, { target: { files: [file] } });
    });

    await waitFor(() => {
      expect(screen.getByText(/failed to extract/i)).toBeInTheDocument();
    });
  });
});
