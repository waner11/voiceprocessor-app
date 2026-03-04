import { render, screen, within } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { ChapterPreview } from "../ChapterPreview";

describe("ChapterPreview", () => {
  it("renders nothing when text is empty", () => {
    const { container } = render(<ChapterPreview text="" />);
    expect(container.querySelector("[data-testid='chapter-preview']")).not.toBeInTheDocument();
  });

  it("renders nothing when text has no chapter markers", () => {
    const { container } = render(<ChapterPreview text="Just a simple paragraph with no chapters." />);
    expect(container.querySelector("[data-testid='chapter-preview']")).not.toBeInTheDocument();
  });

  it("detects chapters from --- dividers", () => {
    const text = `First chapter content here.
---
Second chapter content here.
---
Third chapter content here.`;

    render(<ChapterPreview text={text} />);

    const panel = screen.getByTestId("chapter-preview");
    expect(panel).toBeInTheDocument();

    // Should show 3 chapters
    const chapters = within(panel).getAllByRole("listitem");
    expect(chapters).toHaveLength(3);

    // First chapter should be "Chapter 1"
    expect(within(chapters[0]).getByText(/Chapter 1/)).toBeInTheDocument();
    // Second chapter should be "Chapter 2"
    expect(within(chapters[1]).getByText(/Chapter 2/)).toBeInTheDocument();
    // Third chapter should be "Chapter 3"
    expect(within(chapters[2]).getByText(/Chapter 3/)).toBeInTheDocument();
  });

  it("detects chapters from heading patterns", () => {
    const text = `Chapter 1: The Beginning
Some content for chapter one.
Chapter 2: The Middle
Some content for chapter two.`;

    render(<ChapterPreview text={text} />);

    const panel = screen.getByTestId("chapter-preview");
    const chapters = within(panel).getAllByRole("listitem");
    expect(chapters).toHaveLength(2);

    expect(within(chapters[0]).getByText(/The Beginning/)).toBeInTheDocument();
    expect(within(chapters[1]).getByText(/The Middle/)).toBeInTheDocument();
  });

  it("shows character count per chapter", () => {
    const text = `Chapter 1: Intro
Short.
Chapter 2: Main
This is a longer chapter with more content in it.`;

    render(<ChapterPreview text={text} />);

    const panel = screen.getByTestId("chapter-preview");
    const chapters = within(panel).getAllByRole("listitem");

    // Each chapter should show character count
    chapters.forEach((chapter) => {
      expect(within(chapter).getByText(/chars?/i)).toBeInTheDocument();
    });
  });

  it("shows chapter count summary", () => {
    const text = `Part 1: Beginning
Content here.
Part 2: End
More content.`;

    render(<ChapterPreview text={text} />);

    expect(screen.getByText(/2 chapters? detected/i)).toBeInTheDocument();
  });

  it("handles mixed dividers and headings", () => {
    const text = `Chapter 1: Named Chapter
Some content.
---
More content after divider.`;

    render(<ChapterPreview text={text} />);

    const panel = screen.getByTestId("chapter-preview");
    const chapters = within(panel).getAllByRole("listitem");
    expect(chapters).toHaveLength(2);
  });

  it("uses 'Chapter N' label for divider-based chapters without titles", () => {
    const text = `First section content.
---
Second section content.`;

    render(<ChapterPreview text={text} />);

    const panel = screen.getByTestId("chapter-preview");
    const chapters = within(panel).getAllByRole("listitem");

    // Divider-based chapters should use "Chapter N" naming
    expect(within(chapters[0]).getByText(/Chapter 1/)).toBeInTheDocument();
    expect(within(chapters[1]).getByText(/Chapter 2/)).toBeInTheDocument();
  });
});
