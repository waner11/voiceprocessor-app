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
    <div className="flex min-h-screen flex-col">
      {/* Header */}
      <header className="border-b">
        <div className="container mx-auto flex h-16 items-center justify-between px-4">
          <Link href="/" className="text-xl font-bold">
            VoiceProcessor
          </Link>
          <nav className="hidden md:flex items-center gap-6">
            <a href="#features" className="text-sm hover:underline">
              Features
            </a>
            <a href="#pricing" className="text-sm hover:underline">
              Pricing
            </a>
            <Link href="/api-docs" className="text-sm hover:underline">
              API Docs
            </Link>
            <Link href="/login" className="text-sm hover:underline">
              Sign In
            </Link>
            <Link
              href="/signup"
              className="rounded-lg bg-black px-4 py-2 text-sm text-white hover:bg-gray-800"
            >
              Get Started
            </Link>
          </nav>
        </div>
      </header>

      <main className="flex-1">
        {/* Hero */}
        <section className="border-b bg-gradient-to-b from-gray-50 to-white">
          <div className="container mx-auto px-4 py-24 text-center">
            <div className="mx-auto max-w-3xl">
              <h1 className="mb-6 text-5xl font-bold tracking-tight md:text-6xl">
                Convert Text to
                <span className="text-blue-600"> Professional Audio</span>
              </h1>
              <p className="mb-8 text-xl text-gray-600">
                Multi-provider Text-to-Speech platform. Create audiobooks,
                podcasts, and voiceovers using the best AI voices from
                ElevenLabs, OpenAI, Google, and Amazon Polly.
              </p>
              <div className="flex flex-col sm:flex-row justify-center gap-4">
                <Link
                  href="/signup"
                  className="rounded-lg bg-black px-8 py-3 text-lg text-white hover:bg-gray-800"
                >
                  Start Free Trial
                </Link>
                <Link
                  href="/api-docs"
                  className="rounded-lg border px-8 py-3 text-lg hover:bg-gray-50"
                >
                  View API Docs
                </Link>
              </div>
              <p className="mt-4 text-sm text-gray-500">
                10,000 free characters. No credit card required.
              </p>
            </div>
          </div>
        </section>

        {/* Demo Section */}
        <section className="border-b py-16">
          <div className="container mx-auto px-4">
            <div className="mx-auto max-w-4xl">
              <div className="rounded-xl border bg-white p-6 shadow-lg">
                <div className="mb-4 flex items-center justify-between">
                  <h3 className="font-semibold">Try it now</h3>
                  <span className="rounded-full bg-green-100 px-3 py-1 text-xs text-green-700">
                    Live Demo
                  </span>
                </div>
                <div className="mb-4 rounded-lg border bg-gray-50 p-4">
                  <p className="text-gray-600 italic">
                    &quot;The quick brown fox jumps over the lazy dog. This
                    pangram contains every letter of the English alphabet at
                    least once.&quot;
                  </p>
                </div>
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-4">
                    <select className="rounded-lg border px-3 py-2 text-sm">
                      <option>Emily - Natural (ElevenLabs)</option>
                      <option>James - Professional (OpenAI)</option>
                      <option>Sofia - Warm (Google)</option>
                    </select>
                    <button className="rounded-full bg-black p-3 text-white hover:bg-gray-800">
                      ▶
                    </button>
                  </div>
                  <Link
                    href="/generate"
                    className="text-sm text-blue-600 hover:underline"
                  >
                    Try with your own text →
                  </Link>
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* Features */}
        <section id="features" className="border-b py-24">
          <div className="container mx-auto px-4">
            <div className="mb-16 text-center">
              <h2 className="mb-4 text-3xl font-bold">
                Everything you need for audio content
              </h2>
              <p className="text-gray-600">
                Powerful features to create professional audio at scale
              </p>
            </div>
            <div className="grid gap-8 md:grid-cols-2 lg:grid-cols-3">
              {features.map((feature) => (
                <div
                  key={feature.title}
                  className="rounded-xl border p-6 transition-shadow hover:shadow-md"
                >
                  <div className="mb-4 text-4xl">{feature.icon}</div>
                  <h3 className="mb-2 text-lg font-semibold">{feature.title}</h3>
                  <p className="text-gray-600">{feature.description}</p>
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* Pricing */}
        <section id="pricing" className="py-24">
          <div className="container mx-auto px-4">
            <div className="mb-16 text-center">
              <h2 className="mb-4 text-3xl font-bold">
                Simple, transparent pricing
              </h2>
              <p className="text-gray-600">
                Start free, upgrade when you need more
              </p>
            </div>
            <div className="mx-auto grid max-w-5xl gap-8 md:grid-cols-3">
              {pricingPlans.map((plan) => (
                <div
                  key={plan.name}
                  className={`rounded-xl border p-8 ${
                    plan.highlighted
                      ? "border-blue-500 ring-2 ring-blue-500"
                      : ""
                  }`}
                >
                  {plan.highlighted && (
                    <div className="mb-4 inline-block rounded-full bg-blue-100 px-3 py-1 text-xs font-medium text-blue-700">
                      Most Popular
                    </div>
                  )}
                  <h3 className="text-xl font-bold">{plan.name}</h3>
                  <div className="mt-2 mb-4">
                    <span className="text-4xl font-bold">{plan.price}</span>
                    <span className="text-gray-500">{plan.period}</span>
                  </div>
                  <p className="mb-6 text-gray-600">{plan.description}</p>
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
                        <span className="text-sm">{feature}</span>
                      </li>
                    ))}
                  </ul>
                  <Link
                    href={plan.href}
                    className={`block w-full rounded-lg py-3 text-center ${
                      plan.highlighted
                        ? "bg-blue-600 text-white hover:bg-blue-700"
                        : "border hover:bg-gray-50"
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
        <section className="border-t bg-gray-900 py-16">
          <div className="container mx-auto px-4 text-center">
            <h2 className="mb-4 text-3xl font-bold text-white">
              Ready to create amazing audio?
            </h2>
            <p className="mb-8 text-gray-400">
              Join thousands of creators using VoiceProcessor
            </p>
            <Link
              href="/signup"
              className="inline-block rounded-lg bg-white px-8 py-3 text-lg font-medium text-gray-900 hover:bg-gray-100"
            >
              Get Started Free
            </Link>
          </div>
        </section>
      </main>

      {/* Footer */}
      <footer className="border-t py-12">
        <div className="container mx-auto px-4">
          <div className="grid gap-8 md:grid-cols-4">
            <div>
              <h4 className="mb-4 font-bold">VoiceProcessor</h4>
              <p className="text-sm text-gray-600">
                Multi-provider TTS platform for creating professional audio
                content.
              </p>
            </div>
            <div>
              <h4 className="mb-4 font-semibold">Product</h4>
              <ul className="space-y-2 text-sm text-gray-600">
                <li>
                  <a href="#features" className="hover:underline">
                    Features
                  </a>
                </li>
                <li>
                  <a href="#pricing" className="hover:underline">
                    Pricing
                  </a>
                </li>
                <li>
                  <Link href="/api-docs" className="hover:underline">
                    API Documentation
                  </Link>
                </li>
              </ul>
            </div>
            <div>
              <h4 className="mb-4 font-semibold">Company</h4>
              <ul className="space-y-2 text-sm text-gray-600">
                <li>
                  <a href="/about" className="hover:underline">
                    About
                  </a>
                </li>
                <li>
                  <a href="/blog" className="hover:underline">
                    Blog
                  </a>
                </li>
                <li>
                  <a href="/contact" className="hover:underline">
                    Contact
                  </a>
                </li>
              </ul>
            </div>
            <div>
              <h4 className="mb-4 font-semibold">Legal</h4>
              <ul className="space-y-2 text-sm text-gray-600">
                <li>
                  <a href="/privacy" className="hover:underline">
                    Privacy Policy
                  </a>
                </li>
                <li>
                  <a href="/terms" className="hover:underline">
                    Terms of Service
                  </a>
                </li>
              </ul>
            </div>
          </div>
          <div className="mt-8 border-t pt-8 text-center text-sm text-gray-500">
            © 2026 VoiceProcessor. All rights reserved.
          </div>
        </div>
      </footer>
    </div>
  );
}
