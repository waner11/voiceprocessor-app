import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import GlobalError from "../global-error";

vi.mock("@sentry/nextjs", () => ({
  captureException: vi.fn(),
}));

describe("GlobalError", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders without throwing when given a mock Error", () => {
    const mockError = new Error("Test error");
    const mockReset = vi.fn();

    expect(() => {
      render(<GlobalError error={mockError} reset={mockReset} />);
    }).not.toThrow();
  });

  it("displays error message heading", () => {
    const mockError = new Error("Test error");
    const mockReset = vi.fn();

    render(<GlobalError error={mockError} reset={mockReset} />);

    expect(screen.getByText("Something went wrong!")).toBeInTheDocument();
  });

  it("renders a try again button", () => {
    const mockError = new Error("Test error");
    const mockReset = vi.fn();

    render(<GlobalError error={mockError} reset={mockReset} />);

    const button = screen.getByRole("button", { name: /try again/i });
    expect(button).toBeInTheDocument();
  });

  it("calls reset function when try again button is clicked", () => {
    const mockError = new Error("Test error");
    const mockReset = vi.fn();

    render(<GlobalError error={mockError} reset={mockReset} />);

    const button = screen.getByRole("button", { name: /try again/i });
    fireEvent.click(button);

    expect(mockReset).toHaveBeenCalled();
  });

  it("renders html and body elements", () => {
    const mockError = new Error("Test error");
    const mockReset = vi.fn();

    render(<GlobalError error={mockError} reset={mockReset} />);

    expect(document.querySelector("html")).toBeInTheDocument();
    expect(document.querySelector("body")).toBeInTheDocument();
  });
});
