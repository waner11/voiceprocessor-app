import Link from "next/link";

export default function LandingPage() {
  return (
    <div className="flex min-h-screen flex-col">
      <header className="border-b">
        <div className="container mx-auto flex h-16 items-center justify-between px-4">
          <Link href="/" className="text-xl font-bold">
            VoiceProcessor
          </Link>
          <nav className="flex items-center gap-4">
            <Link href="/voices" className="text-sm hover:underline">
              Voices
            </Link>
            <Link href="/dashboard" className="text-sm hover:underline">
              Dashboard
            </Link>
          </nav>
        </div>
      </header>

      <main className="flex-1">
        <section className="container mx-auto px-4 py-24 text-center">
          <h1 className="mb-6 text-5xl font-bold tracking-tight">
            Convert Text to Audiobooks
          </h1>
          <p className="mx-auto mb-8 max-w-2xl text-lg text-gray-600">
            Multi-provider Text-to-Speech platform. Create professional
            audiobooks using voices from ElevenLabs, OpenAI, Google, and Amazon
            Polly.
          </p>
          <div className="flex justify-center gap-4">
            <Link
              href="/generate"
              className="rounded-lg bg-black px-6 py-3 text-white hover:bg-gray-800"
            >
              Get Started
            </Link>
            <Link
              href="/api-docs"
              className="rounded-lg border px-6 py-3 hover:bg-gray-50"
            >
              API Documentation
            </Link>
          </div>
        </section>
      </main>

      <footer className="border-t py-8">
        <div className="container mx-auto px-4 text-center text-sm text-gray-500">
          VoiceProcessor - Multi-provider TTS Platform
        </div>
      </footer>
    </div>
  );
}
