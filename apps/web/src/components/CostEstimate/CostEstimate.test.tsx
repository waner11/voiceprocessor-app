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

describe("Provider Badges", () => {
  const twoProviders = [
    { provider: "OpenAI" as const, cost: 0.05, creditsRequired: 50, estimatedDurationMs: 3000, qualityTier: null, isAvailable: true },
    { provider: "ElevenLabs" as const, cost: 0.10, creditsRequired: 100, estimatedDurationMs: 2000, qualityTier: "High", isAvailable: true },
  ];

  const mockEstimateWithProviders = (providerEstimates: typeof twoProviders) => ({
    characterCount: 500,
    estimatedChunks: 2,
    estimatedCost: 0.05,
    creditsRequired: 50,
    currency: "USD",
    recommendedProvider: "OpenAI" as const,
    providerEstimates,
  });

  it("shows Premium Quality badge for provider with qualityTier containing 'High'", () => {
    render(
      <CostEstimate
        costEstimate={mockEstimateWithProviders(twoProviders)}
        isEstimating={false}
        characterCount={500}
        wordCount={100}
      />
    );
    expect(screen.getByText("Premium Quality (2x Credits)")).toBeInTheDocument();
  });

  it("shows Best Value badge for cheapest available provider when 2+ providers", () => {
    render(
      <CostEstimate
        costEstimate={mockEstimateWithProviders(twoProviders)}
        isEstimating={false}
        characterCount={500}
        wordCount={100}
      />
    );
    expect(screen.getByText("Best Value")).toBeInTheDocument();
  });

  it("does NOT show Best Value badge when only 1 provider", () => {
    const singleProvider = [
      { provider: "OpenAI" as const, cost: 0.05, creditsRequired: 50, estimatedDurationMs: 3000, qualityTier: null, isAvailable: true },
    ];
    render(
      <CostEstimate
        costEstimate={mockEstimateWithProviders(singleProvider)}
        isEstimating={false}
        characterCount={500}
        wordCount={100}
      />
    );
    expect(screen.queryByText("Best Value")).not.toBeInTheDocument();
  });

  it("shows Best Value on ALL tied providers when costs are equal", () => {
    const tiedProviders = [
      { provider: "OpenAI" as const, cost: 0.05, creditsRequired: 100, estimatedDurationMs: 3000, qualityTier: null, isAvailable: true },
      { provider: "GoogleCloud" as const, cost: 0.05, creditsRequired: 100, estimatedDurationMs: 2500, qualityTier: null, isAvailable: true },
      { provider: "ElevenLabs" as const, cost: 0.10, creditsRequired: 200, estimatedDurationMs: 2000, qualityTier: "High", isAvailable: true },
    ];
    render(
      <CostEstimate
        costEstimate={mockEstimateWithProviders(tiedProviders)}
        isEstimating={false}
        characterCount={500}
        wordCount={100}
      />
    );
    const bestValueBadges = screen.getAllByText("Best Value");
    expect(bestValueBadges).toHaveLength(2);
  });

  it("excludes unavailable providers from Best Value calculation", () => {
    const withUnavailable = [
      { provider: "OpenAI" as const, cost: 0.01, creditsRequired: 10, estimatedDurationMs: 3000, qualityTier: null, isAvailable: false },
      { provider: "GoogleCloud" as const, cost: 0.05, creditsRequired: 50, estimatedDurationMs: 2500, qualityTier: null, isAvailable: true },
    ];
    render(
      <CostEstimate
        costEstimate={mockEstimateWithProviders(withUnavailable)}
        isEstimating={false}
        characterCount={500}
        wordCount={100}
      />
    );
    // Only 1 available provider, so no Best Value badge
    expect(screen.queryByText("Best Value")).not.toBeInTheDocument();
  });

  it("does NOT show Premium badge when qualityTier is null", () => {
    const noQualityTier = [
      { provider: "OpenAI" as const, cost: 0.05, creditsRequired: 50, estimatedDurationMs: 3000, qualityTier: null, isAvailable: true },
      { provider: "GoogleCloud" as const, cost: 0.08, creditsRequired: 80, estimatedDurationMs: 2500, qualityTier: null, isAvailable: true },
    ];
    render(
      <CostEstimate
        costEstimate={mockEstimateWithProviders(noQualityTier)}
        isEstimating={false}
        characterCount={500}
        wordCount={100}
      />
    );
    expect(screen.queryByText("Premium Quality (2x Credits)")).not.toBeInTheDocument();
  });

  it("Premium badge has amber CSS classes", () => {
    render(
      <CostEstimate
        costEstimate={mockEstimateWithProviders(twoProviders)}
        isEstimating={false}
        characterCount={500}
        wordCount={100}
      />
    );
    const premiumBadge = screen.getByText("Premium Quality (2x Credits)");
    expect(premiumBadge.className).toMatch(/amber/);
  });
});
