interface GenerationPageProps {
  params: Promise<{ id: string }>;
}

export default async function GenerationPage({ params }: GenerationPageProps) {
  const { id } = await params;

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="mb-8 text-3xl font-bold">Generation Details</h1>

      <div className="grid gap-8 lg:grid-cols-3">
        <div className="lg:col-span-2 space-y-6">
          {/* AudioPlayer component will go here */}
          <div className="rounded-lg border p-6">
            <h2 className="mb-4 text-lg font-semibold">Audio Player</h2>
            <div className="h-32 rounded-lg bg-gray-100 flex items-center justify-center text-gray-500">
              Waveform visualization will appear here
            </div>
            <div className="mt-4 flex items-center justify-between">
              <div className="flex items-center gap-2">
                <button className="rounded-full border p-2 hover:bg-gray-50">
                  ▶
                </button>
                <span className="text-sm text-gray-500">0:00 / --:--</span>
              </div>
              <div className="flex items-center gap-2">
                <select className="rounded border px-2 py-1 text-sm">
                  <option>1x</option>
                  <option>1.25x</option>
                  <option>1.5x</option>
                  <option>2x</option>
                </select>
                <button className="rounded border px-3 py-1 text-sm hover:bg-gray-50">
                  Download
                </button>
              </div>
            </div>
          </div>

          {/* Chapters */}
          <div className="rounded-lg border p-6">
            <h2 className="mb-4 text-lg font-semibold">Chapters</h2>
            <p className="text-gray-500">No chapters detected</p>
          </div>
        </div>

        <div className="space-y-6">
          {/* GenerationStatus component will go here */}
          <div className="rounded-lg border p-6">
            <h2 className="mb-4 text-lg font-semibold">Status</h2>
            <p className="text-gray-500">Loading generation {id}...</p>
          </div>

          {/* Generation info */}
          <div className="rounded-lg border p-6">
            <h2 className="mb-4 text-lg font-semibold">Details</h2>
            <dl className="space-y-2 text-sm">
              <div className="flex justify-between">
                <dt className="text-gray-500">Voice</dt>
                <dd>--</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-gray-500">Provider</dt>
                <dd>--</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-gray-500">Duration</dt>
                <dd>--</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-gray-500">Cost</dt>
                <dd>--</dd>
              </div>
            </dl>
          </div>

          {/* Feedback */}
          <div className="rounded-lg border p-6">
            <h2 className="mb-4 text-lg font-semibold">Feedback</h2>
            <p className="text-gray-500">Rate this generation...</p>
          </div>
        </div>
      </div>
    </div>
  );
}
