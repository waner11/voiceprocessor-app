import Link from "next/link";

const features = [
  {
    icon: "🎙️",
    title: "Multi-Provider TTS",
    description:
      "Access voices from ElevenLabs, OpenAI, Google, and Amazon Polly through a single API.",
  },
  {
    icon: "⚡",
    title: "Smart Routing",
    description:
      "Automatically select the best provider based on quality, cost, or speed preferences.",
  },
  {
    icon: "📚",
    title: "Long-Form Content",
    description:
      "Convert entire books and documents with automatic chunking and chapter markers.",
  },
  {
    icon: "🌍",
    title: "50+ Languages",
    description:
      "Support for major world languages with native-sounding voices and accents.",
  },
  {
    icon: "🔄",
    title: "Real-time Progress",
    description:
      "Track generation progress in real-time with live updates and notifications.",
  },
  {
    icon: "🔌",
    title: "Developer API",
    description:
      "Full REST API with webhooks for seamless integration into your applications.",
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
      "API access",
      "Priority support",
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
    <div className="flex min-h-screen flex-col bg-white dark:bg-gray-950">
      {/* Header */}
      <header className="sticky top-0 z-50 border-b border-gray-200 dark:border-gray-800 bg-white/95 dark:bg-gray-950/95 backdrop-blur supports-[backdrop-filter]:bg-white/60 dark:supports-[backdrop-filter]:bg-gray-950/60">
        <div className="container mx-auto flex h-16 items-center justify-between px-4">
          <Link href="/" className="text-xl font-bold text-gray-900 dark:text-white">
            VoiceProcessor
          </Link>
          <nav className="hidden md:flex items-center gap-6">
            <a href="#features" className="text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white">
              Features
            </a>
            <a href="#pricing" className="text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white">
              Pricing
            </a>
            <Link href="/api-docs" className="text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white">
              API Docs
            </Link>
            <Link href="/login" className="text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white">
              Sign In
            </Link>
            <Link
              href="/signup"
              className="rounded-lg bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700"
            >
              Get Started
            </Link>
          </nav>
        </div>
      </header>

      <main className="flex-1">
        {/* Hero */}
        <section className="border-b border-gray-200 dark:border-gray-800 bg-gradient-to-b from-gray-50 to-white dark:from-gray-900 dark:to-gray-950">
          <div className="container mx-auto px-4 py-24 text-center">
            <div className="mx-auto max-w-3xl">
              <h1 className="mb-6 text-5xl font-bold tracking-tight md:text-6xl text-gray-900 dark:text-white">
                Convert Text to
                <span className="text-blue-600 dark:text-blue-400"> Professional Audio</span>
              </h1>
              <p className="mb-8 text-xl text-gray-600 dark:text-gray-400">
                Multi-provider Text-to-Speech platform. Create audiobooks,
                podcasts, and voiceovers using the best AI voices from
                ElevenLabs, OpenAI, Google, and Amazon Polly.
              </p>
              <div className="flex flex-col sm:flex-row justify-center gap-4">
                <Link
                  href="/signup"
                  className="rounded-lg bg-blue-600 px-8 py-3 text-lg text-white hover:bg-blue-700 shadow-lg shadow-blue-600/25"
                >
                  Start Free Trial
                </Link>
                <Link
                  href="/api-docs"
                  className="rounded-lg border border-gray-300 dark:border-gray-700 px-8 py-3 text-lg text-gray-900 dark:text-white hover:bg-gray-50 dark:hover:bg-gray-800"
                >
                  View API Docs
                </Link>
              </div>
              <p className="mt-4 text-sm text-gray-500 dark:text-gray-500">
                10,000 free characters. No credit card required.
              </p>
            </div>
          </div>
        </section>

        {/* Demo Section */}
        <section className="border-b border-gray-200 dark:border-gray-800 py-16 bg-white dark:bg-gray-950">
          <div className="container mx-auto px-4">
            <div className="mx-auto max-w-4xl">
              <div className="rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 shadow-lg">
                <div className="mb-4 flex items-center justify-between">
                  <h3 className="font-semibold text-gray-900 dark:text-white">Try it now</h3>
                  <span className="rounded-full bg-green-100 dark:bg-green-900 px-3 py-1 text-xs text-green-700 dark:text-green-300">
                    Live Demo
                  </span>
                </div>
                <div className="mb-4 rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 p-4">
                  <p className="text-gray-600 dark:text-gray-400 italic">
                    &quot;The quick brown fox jumps over the lazy dog. This
                    pangram contains every letter of the English alphabet at
                    least once.&quot;
                  </p>
                </div>
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-4">
                    <select className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-sm text-gray-900 dark:text-white">
                      <option>Emily - Natural (ElevenLabs)</option>
                      <option>James - Professional (OpenAI)</option>
                      <option>Sofia - Warm (Google)</option>
                    </select>
                    <button className="rounded-full bg-blue-600 p-3 text-white hover:bg-blue-700">
                      ▶
                    </button>
                  </div>
                  <Link
                    href="/signup"
                    className="text-sm text-blue-600 dark:text-blue-400 hover:underline"
                  >
                    Try with your own text →
                  </Link>
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* Features */}
        <section id="features" className="border-b border-gray-200 dark:border-gray-800 py-24 bg-gray-50 dark:bg-gray-900">
          <div className="container mx-auto px-4">
            <div className="mb-16 text-center">
              <h2 className="mb-4 text-3xl font-bold text-gray-900 dark:text-white">
                Everything you need for audio content
              </h2>
              <p className="text-gray-600 dark:text-gray-400">
                Powerful features to create professional audio at scale
              </p>
            </div>
            <div className="grid gap-8 md:grid-cols-2 lg:grid-cols-3">
              {features.map((feature) => (
                <div
                  key={feature.title}
                  className="rounded-xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 transition-shadow hover:shadow-md"
                >
                  <div className="mb-4 text-4xl">{feature.icon}</div>
                  <h3 className="mb-2 text-lg font-semibold text-gray-900 dark:text-white">{feature.title}</h3>
                  <p className="text-gray-600 dark:text-gray-400">{feature.description}</p>
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* Pricing */}
        <section id="pricing" className="py-24 bg-white dark:bg-gray-950">
          <div className="container mx-auto px-4">
            <div className="mb-16 text-center">
              <h2 className="mb-4 text-3xl font-bold text-gray-900 dark:text-white">
                Simple, transparent pricing
              </h2>
              <p className="text-gray-600 dark:text-gray-400">
                Start free, upgrade when you need more
              </p>
            </div>
            <div className="mx-auto grid max-w-5xl gap-8 md:grid-cols-3">
              {pricingPlans.map((plan) => (
                <div
                  key={plan.name}
                  className={`rounded-xl border p-8 ${
                    plan.highlighted
                      ? "border-blue-500 ring-2 ring-blue-500 bg-white dark:bg-gray-900"
                      : "border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900"
                  }`}
                >
                  {plan.highlighted && (
                    <div className="mb-4 inline-block rounded-full bg-blue-100 dark:bg-blue-900 px-3 py-1 text-xs font-medium text-blue-700 dark:text-blue-300">
                      Most Popular
                    </div>
                  )}
                  <h3 className="text-xl font-bold text-gray-900 dark:text-white">{plan.name}</h3>
                  <div className="mt-2 mb-4">
                    <span className="text-4xl font-bold text-gray-900 dark:text-white">{plan.price}</span>
                    <span className="text-gray-500 dark:text-gray-400">{plan.period}</span>
                  </div>
                  <p className="mb-6 text-gray-600 dark:text-gray-400">{plan.description}</p>
                  <ul className="mb-8 space-y-3">
                    {plan.features.map((feature) => (
                      <li key={feature} className="flex items-center gap-2">
                        <svg
                          className="h-5 w-5 text-green-500"
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
                        <span className="text-sm text-gray-700 dark:text-gray-300">{feature}</span>
                      </li>
                    ))}
                  </ul>
                  <Link
                    href={plan.href}
                    className={`block w-full rounded-lg py-3 text-center ${
                      plan.highlighted
                        ? "bg-blue-600 text-white hover:bg-blue-700"
                        : "border border-gray-200 dark:border-gray-700 text-gray-900 dark:text-white hover:bg-gray-50 dark:hover:bg-gray-800"
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
        <section className="border-t border-gray-800 bg-gray-900 py-16">
          <div className="container mx-auto px-4 text-center">
            <h2 className="mb-4 text-3xl font-bold text-white">
              Ready to create amazing audio?
            </h2>
            <p className="mb-8 text-gray-400">
              Join thousands of creators using VoiceProcessor
            </p>
            <Link
              href="/signup"
              className="inline-block rounded-lg bg-blue-600 px-8 py-3 text-lg font-medium text-white hover:bg-blue-700 shadow-lg shadow-blue-600/25"
            >
              Get Started Free
            </Link>
          </div>
        </section>
      </main>

      {/* Footer */}
      <footer className="border-t border-gray-200 dark:border-gray-800 py-12 bg-white dark:bg-gray-950">
        <div className="container mx-auto px-4">
          <div className="grid gap-8 md:grid-cols-4">
            <div>
              <h4 className="mb-4 font-bold text-gray-900 dark:text-white">VoiceProcessor</h4>
              <p className="text-sm text-gray-600 dark:text-gray-400">
                Multi-provider TTS platform for creating professional audio
                content.
              </p>
            </div>
            <div>
              <h4 className="mb-4 font-semibold text-gray-900 dark:text-white">Product</h4>
              <ul className="space-y-2 text-sm text-gray-600 dark:text-gray-400">
                <li>
                  <a href="#features" className="hover:text-gray-900 dark:hover:text-white">
                    Features
                  </a>
                </li>
                <li>
                  <a href="#pricing" className="hover:text-gray-900 dark:hover:text-white">
                    Pricing
                  </a>
                </li>
                <li>
                  <Link href="/api-docs" className="hover:text-gray-900 dark:hover:text-white">
                    API Documentation
                  </Link>
                </li>
              </ul>
            </div>
            <div>
              <h4 className="mb-4 font-semibold text-gray-900 dark:text-white">Company</h4>
              <ul className="space-y-2 text-sm text-gray-600 dark:text-gray-400">
                <li>
                  <a href="/about" className="hover:text-gray-900 dark:hover:text-white">
                    About
                  </a>
                </li>
                <li>
                  <a href="/blog" className="hover:text-gray-900 dark:hover:text-white">
                    Blog
                  </a>
                </li>
                <li>
                  <a href="/contact" className="hover:text-gray-900 dark:hover:text-white">
                    Contact
                  </a>
                </li>
              </ul>
            </div>
            <div>
              <h4 className="mb-4 font-semibold text-gray-900 dark:text-white">Legal</h4>
              <ul className="space-y-2 text-sm text-gray-600 dark:text-gray-400">
                <li>
                  <a href="/privacy" className="hover:text-gray-900 dark:hover:text-white">
                    Privacy Policy
                  </a>
                </li>
                <li>
                  <a href="/terms" className="hover:text-gray-900 dark:hover:text-white">
                    Terms of Service
                  </a>
                </li>
              </ul>
            </div>
          </div>
          <div className="mt-8 border-t border-gray-200 dark:border-gray-800 pt-8 text-center text-sm text-gray-500 dark:text-gray-500">
            © 2026 VoiceProcessor. All rights reserved.
          </div>
        </div>
      </footer>
    </div>
  );
}
