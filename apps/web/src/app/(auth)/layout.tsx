import Link from "next/link";

export default function AuthLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex min-h-screen flex-col bg-bg-base">
      <header className="sticky top-0 z-50 border-b border-border-subtle bg-bg-base/95 backdrop-blur supports-[backdrop-filter]:bg-bg-base/60">
        <div className="container mx-auto flex h-14 items-center px-4">
          <Link href="/" className="text-xl font-bold text-indigo font-heading">
            VoiceProcessor
          </Link>
        </div>
      </header>

      <main className="flex flex-1 items-center justify-center p-4">
        <div className="w-full max-w-md">{children}</div>
      </main>

      <footer className="border-t border-border-subtle py-4">
        <div className="container mx-auto px-4 text-center text-sm text-text-muted">
          VoiceProcessor - Multi-provider TTS Platform
        </div>
      </footer>
    </div>
  );
}
