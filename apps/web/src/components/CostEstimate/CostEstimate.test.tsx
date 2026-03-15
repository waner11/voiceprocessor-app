import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";
import { CostEstimate } from "./CostEstimate";

const mockCostEstimate = {
  characterCount: 500,
  estimatedChunks: 2,
  estimatedCost: 0.05,
  creditsRequired: 50,
  currency: "USD",
  recommendedProvider: "OpenAI" as const,
  providerEstimates: [
    { provider: "OpenAI" as const, cost: 0.05, creditsRequired: 50, estimatedDurationMs: 3000, qualityTier: null, isAvailable: true },
    { provider: "ElevenLabs" as const, cost: 0.10, creditsRequired: 100, estimatedDurationMs: 2000, qualityTier: "High", isAvailable: true },
  ],
};

describe("CostEstimate", () => {
  it("renders Estimated Credits heading", () => {
    render(<CostEstimate costEstimate={null} isEstimating={false} characterCount={0} wordCount={0} />);
    expect(screen.getByText("Estimated Credits")).toBeInTheDocument();
  });

  it("shows empty state when characterCount is 0", () => {
    render(<CostEstimate costEstimate={null} isEstimating={false} characterCount={0} wordCount={0} />);
    expect(screen.getByText("Enter text to see cost estimate")).toBeInTheDocument();
  });

  it("shows loading dots when isEstimating is true", () => {
    render(<CostEstimate costEstimate={null} isEstimating={true} characterCount={100} wordCount={20} />);
    expect(screen.getByText("...")).toBeInTheDocument();
  });

  it("shows formatted credits when costEstimate is provided", () => {
    render(<CostEstimate costEstimate={mockCostEstimate} isEstimating={false} characterCount={500} wordCount={100} />);
    expect(screen.getByRole("heading", { name: "Estimated Credits" })).toBeInTheDocument();
    const creditsElements = screen.getAllByText(/50 credits/);
    expect(creditsElements.length).toBeGreaterThan(0);
  });

  it("shows dash when characterCount > 0 but no estimate and not loading", () => {
    render(<CostEstimate costEstimate={null} isEstimating={false} characterCount={100} wordCount={20} />);
    expect(screen.getByText("—")).toBeInTheDocument();
  });

  it("shows character count and estimated duration", () => {
    render(<CostEstimate costEstimate={mockCostEstimate} isEstimating={false} characterCount={500} wordCount={150} />);
    expect(screen.getByText("Characters")).toBeInTheDocument();
    expect(screen.getByText("Est. Duration")).toBeInTheDocument();
  });

  it("shows Compare all providers when 2+ provider estimates exist", () => {
    render(<CostEstimate costEstimate={mockCostEstimate} isEstimating={false} characterCount={500} wordCount={100} />);
    expect(screen.getByText("Compare all providers")).toBeInTheDocument();
  });
});
