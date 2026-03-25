import Link from "next/link";
import { Mic, Zap, BookOpen, Globe, RefreshCw } from "lucide-react";
import { ApiGatedFeatureCard } from "./(landing)/api-gated-features";
import { ApiGatedPricingFeature } from "./(landing)/api-gated-pricing";
import { ApiGatedLinks } from "./(landing)/api-gated-links";

const features = [
  {
    icon: <Mic className="w-8 h-8 text-indigo" />,
    title: "Multi-Provider TTS",
    description:
      "Access voices from ElevenLabs, OpenAI, Google, and Amazon Polly through a single API.",
  },
  {
    icon: <Zap className="w-8 h-8 text-indigo" />,
    title: "Smart Routing",
    description:
      "Automatically select the best provider based on quality, cost, or speed preferences.",
  },
  {
    icon: <BookOpen className="w-8 h-8 text-indigo" />,
    title: "Long-Form Content",
    description:
      "Convert entire books and documents with automatic chunking and chapter markers.",
  },
  {
    icon: <Globe className="w-8 h-8 text-indigo" />,
    title: "50+ Languages",
    description:
      "Support for major world languages with native-sounding voices and accents.",
  },
  {
    icon: <RefreshCw className="w-8 h-8 text-indigo" />,
    title: "Real-time Progress",
    description:
      "Track generation progress in real-time with live updates and notifications.",
  },
];

const pricingPlans = [
  {
    name: "Free",
    price: "$0",
    period: "forever",
    description: "Perfect for trying out the platform",
    features: [
      "10,000 characters/month",
      "All voice providers",
      "Standard quality",
      "Email support",
    ],
    cta: "Get Started",
    href: "/signup",
    highlighted: false,
  },
    {
      name: "Pro",
      price: "$29",
      period: "/month",
      description: "For content creators and podcasters",
      features: [
        "500,000 characters/month",
        "All voice providers",
        "Premium quality voices",
        "Priority generation",
        "Priority support",
        "API access",
      ],
      cta: "Start Free Trial",
      href: "/signup?plan=pro",
      highlighted: true,
    },
  {
    name: "Enterprise",
    price: "Custom",
    period: "",
    description: "For teams and high-volume needs",
    features: [
      "Unlimited characters",
      "Custom voice cloning",
      "SLA guarantee",
      "Dedicated support",
      "Volume discounts",
      "Custom integrations",
    ],
    cta: "Contact Sales",
    href: "/contact",
    highlighted: false,
  },
];

export default function LandingPage() {
  return (
    <div className="flex min-h-screen flex-col bg-bg-base">
      {/* Header */}
      <header className="sticky top-0 z-50 border-b border-border-subtle bg-bg-base">
        <div className="container mx-auto flex h-14 items-center justify-between px-4">
          <Link href="/" className="text-xl font-bold text-indigo font-heading">
            VoiceProcessor
          </Link>
           <nav className="hidden md:flex items-center gap-6">
             <a href="#features" className="text-sm text-text-secondary hover:text-text-primary">
               Features
             </a>
             <a href="#pricing" className="text-sm text-text-secondary hover:text-text-primary">
               Pricing
             </a>
             <ApiGatedLinks type="nav" />
             <Link href="/login" className="text-sm text-text-secondary hover:text-text-primary">
               Sign In
             </Link>
            <Link
              href="/signup"
              className="rounded-lg bg-indigo px-4 py-2 text-sm text-text-inverse hover:bg-indigo-dark"
            >
              Get Started
            </Link>
          </nav>
        </div>
      </header>

      <main className="flex-1">
        {/* Hero */}
        <section className="border-b border-border-subtle bg-bg-base">
          <div className="container mx-auto px-4 py-24 text-center">
            <div className="mx-auto max-w-3xl">
              <h1 className="mb-6 text-5xl font-bold tracking-tight md:text-6xl text-text-primary font-heading">
                Convert Text to
                <span className="text-indigo"> Professional Audio</span>
              </h1>
              <p className="mb-8 text-xl text-text-secondary">
                Multi-provider Text-to-Speech platform. Create audiobooks,
                podcasts, and voiceovers using the best AI voices from
                ElevenLabs, OpenAI, Google, and Amazon Polly.
              </p>
               <div className="flex flex-col sm:flex-row justify-center gap-4">
                 <Link
                   href="/signup"
                   className="rounded-lg bg-indigo px-8 py-3 text-lg text-text-inverse hover:bg-indigo-dark"
                 >
                   Start Free Trial
                 </Link>
                 <ApiGatedLinks type="hero" />
               </div>
              <p className="mt-4 text-sm text-text-muted">
                10,000 free characters. No credit card required.
              </p>
            </div>
          </div>
        </section>

        {/* Demo Section */}
        <section className="border-b border-border-subtle py-16 bg-bg-base">
          <div className="container mx-auto px-4">
            <div className="mx-auto max-w-4xl">
              <div className="rounded-xl border border-border-subtle bg-bg-elevated p-6 shadow-soft-2">
                <div className="mb-4 flex items-center justify-between">
                  <h3 className="font-semibold text-text-primary">Try it now</h3>
                  <span className="rounded-full bg-success-subtle px-3 py-1 text-xs text-state-success-text">
                    Live Demo
                  </span>
                </div>
                <div className="mb-4 rounded-lg border border-border-subtle bg-bg-sunken p-4">
                  <p className="text-text-secondary italic">
                    &quot;The quick brown fox jumps over the lazy dog. This
                    pangram contains every letter of the English alphabet at
                    least once.&quot;
                  </p>
                </div>
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-4">
                    <select className="rounded-lg border border-border-subtle bg-bg-elevated px-3 py-2 text-sm text-text-primary">
                      <option>Emily - Natural (ElevenLabs)</option>
                      <option>James - Professional (OpenAI)</option>
                      <option>Sofia - Warm (Google)</option>
                    </select>
                    <button className="rounded-full bg-indigo p-3 text-text-inverse hover:bg-indigo-dark">
                      ▶
                    </button>
                  </div>
                  <Link
                    href="/signup"
                    className="text-sm text-text-link hover:underline"
                  >
                    Try with your own text →
                  </Link>
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* Features */}
        <section id="features" className="border-b border-border-subtle py-24 bg-bg-surface">
          <div className="container mx-auto px-4">
            <div className="mb-16 text-center">
              <h2 className="mb-4 text-3xl font-bold text-text-primary font-heading">
                Everything you need for audio content
              </h2>
              <p className="text-text-secondary">
                Powerful features to create professional audio at scale
              </p>
            </div>
             <div className="grid gap-8 md:grid-cols-2 lg:grid-cols-3">
               {features.map((feature) => (
                 <div
                   key={feature.title}
                   className="rounded-xl border border-border-subtle bg-bg-elevated p-6 transition-shadow hover:shadow-soft-2"
                 >
                   <div className="mb-4">{feature.icon}</div>
                   <h3 className="mb-2 text-lg font-semibold text-text-primary">{feature.title}</h3>
                   <p className="text-text-secondary">{feature.description}</p>
                 </div>
               ))}
               <ApiGatedFeatureCard />
             </div>
          </div>
        </section>

        {/* Pricing */}
        <section id="pricing" className="py-24 bg-bg-base">
          <div className="container mx-auto px-4">
            <div className="mb-16 text-center">
              <h2 className="mb-4 text-3xl font-bold text-text-primary font-heading">
                Simple, transparent pricing
              </h2>
              <p className="text-text-secondary">
                Start free, upgrade when you need more
              </p>
            </div>
            <div className="mx-auto grid max-w-5xl gap-8 md:grid-cols-3">
              {pricingPlans.map((plan) => (
                <div
                  key={plan.name}
                  className={`rounded-xl border p-8 ${
                    plan.highlighted
                      ? "border-indigo ring-2 ring-indigo bg-bg-elevated"
                      : "border-border-subtle bg-bg-elevated"
                  }`}
                >
                  {plan.highlighted && (
                    <div className="mb-4 inline-block rounded-full bg-indigo-subtle px-3 py-1 text-xs font-medium text-indigo">
                      Most Popular
                    </div>
                  )}
                  <h3 className="text-xl font-bold text-text-primary">{plan.name}</h3>
                  <div className="mt-2 mb-4">
                    <span className="text-4xl font-bold text-text-primary">{plan.price}</span>
                    <span className="text-text-muted">{plan.period}</span>
                  </div>
                   <p className="mb-6 text-text-secondary">{plan.description}</p>
                    <ul className="mb-8 space-y-3">
                      {plan.features.map((feature) => (
                        feature === "API access" && plan.name === "Pro" ? (
                          <ApiGatedPricingFeature key={feature} />
                        ) : (
                          <li key={feature} className="flex items-center gap-2">
                            <svg
                              className="h-5 w-5 text-success"
                              fill="none"
                              viewBox="0 0 24 24"
                              stroke="currentColor"
                            >
                              <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                strokeWidth={2}
                                d="M5 13l4 4L19 7"
                              />
                            </svg>
                            <span className="text-sm text-text-secondary">{feature}</span>
                          </li>
                        )
                      ))}
                    </ul>
                  <Link
                    href={plan.href}
                    className={`block w-full rounded-lg py-3 text-center ${
                      plan.highlighted
                        ? "bg-indigo text-text-inverse hover:bg-indigo-dark"
                        : "border border-border-subtle text-text-primary hover:bg-bg-sunken"
                    }`}
                  >
                    {plan.cta}
                  </Link>
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* CTA */}
        <section className="border-t border-border-subtle bg-bg-elevated py-16">
          <div className="container mx-auto px-4 text-center">
            <h2 className="mb-4 text-3xl font-bold text-text-primary font-heading">
              Ready to create amazing audio?
            </h2>
            <p className="mb-8 text-text-secondary">
              Join thousands of creators using VoiceProcessor
            </p>
            <Link
              href="/signup"
              className="inline-block rounded-lg bg-indigo px-8 py-3 text-lg font-medium text-text-inverse hover:bg-indigo-dark"
            >
              Get Started Free
            </Link>
          </div>
        </section>
      </main>

      {/* Footer */}
      <footer className="border-t border-border-subtle py-12 bg-bg-base">
        <div className="container mx-auto px-4">
          <div className="grid gap-8 md:grid-cols-4">
            <div>
              <h4 className="mb-4 font-bold text-text-primary">VoiceProcessor</h4>
              <p className="text-sm text-text-secondary">
                Multi-provider TTS platform for creating professional audio
                content.
              </p>
            </div>
              <div>
                <h4 className="mb-4 font-semibold text-text-primary">Product</h4>
                <ul className="space-y-2 text-sm text-text-secondary">
                  <li>
                    <a href="#features" className="hover:text-text-primary">
                      Features
                    </a>
                  </li>
                  <li>
                    <a href="#pricing" className="hover:text-text-primary">
                      Pricing
                    </a>
                  </li>
                  <ApiGatedLinks type="footer" />
                </ul>
              </div>
            <div>
              <h4 className="mb-4 font-semibold text-text-primary">Company</h4>
              <ul className="space-y-2 text-sm text-text-secondary">
                <li>
                  <a href="/about" className="hover:text-text-primary">
                    About
                  </a>
                </li>
                <li>
                  <a href="/blog" className="hover:text-text-primary">
                    Blog
                  </a>
                </li>
                <li>
                  <a href="/contact" className="hover:text-text-primary">
                    Contact
                  </a>
                </li>
              </ul>
            </div>
            <div>
              <h4 className="mb-4 font-semibold text-text-primary">Legal</h4>
              <ul className="space-y-2 text-sm text-text-secondary">
                <li>
                  <a href="/privacy" className="hover:text-text-primary">
                    Privacy Policy
                  </a>
                </li>
                <li>
                  <a href="/terms" className="hover:text-text-primary">
                    Terms of Service
                  </a>
                </li>
              </ul>
            </div>
          </div>
          <div className="mt-8 border-t border-border-subtle pt-8 text-center text-sm text-text-muted">
            © 2026 VoiceProcessor. All rights reserved.
          </div>
        </div>
      </footer>
    </div>
  );
}
