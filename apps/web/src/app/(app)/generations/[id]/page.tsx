interface GenerationPageProps {
  params: Promise<{ id: string }>;
}

export default async function GenerationPage({ params }: GenerationPageProps) {
  const { id } = await params;

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="mb-8 text-3xl font-bold text-gray-900 dark:text-white">Generation Details</h1>

      <div className="grid gap-8 lg:grid-cols-3">
        <div className="lg:col-span-2 space-y-6">
          {/* AudioPlayer component will go here */}
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
            <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Audio Player</h2>
            <div className="h-32 rounded-lg bg-gray-100 dark:bg-gray-800 flex items-center justify-center text-gray-500 dark:text-gray-400">
              Waveform visualization will appear here
            </div>
            <div className="mt-4 flex items-center justify-between">
              <div className="flex items-center gap-2">
                <button className="rounded-full border border-gray-200 dark:border-gray-700 p-2 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800">
                  ▶
                </button>
                <span className="text-sm text-gray-500 dark:text-gray-400">0:00 / --:--</span>
              </div>
              <div className="flex items-center gap-2">
                <select className="rounded border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-2 py-1 text-sm text-gray-900 dark:text-white">
                  <option>1x</option>
                  <option>1.25x</option>
                  <option>1.5x</option>
                  <option>2x</option>
                </select>
                <button className="rounded border border-gray-200 dark:border-gray-700 px-3 py-1 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800">
                  Download
                </button>
              </div>
            </div>
          </div>

          {/* Chapters */}
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
            <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Chapters</h2>
            <p className="text-gray-500 dark:text-gray-400">No chapters detected</p>
          </div>
        </div>

        <div className="space-y-6">
          {/* GenerationStatus component will go here */}
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
            <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Status</h2>
            <p className="text-gray-500 dark:text-gray-400">Loading generation {id}...</p>
          </div>

          {/* Generation info */}
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
            <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Details</h2>
            <dl className="space-y-2 text-sm">
              <div className="flex justify-between">
                <dt className="text-gray-500 dark:text-gray-400">Voice</dt>
                <dd className="text-gray-900 dark:text-white">--</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-gray-500 dark:text-gray-400">Provider</dt>
                <dd className="text-gray-900 dark:text-white">--</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-gray-500 dark:text-gray-400">Duration</dt>
                <dd className="text-gray-900 dark:text-white">--</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-gray-500 dark:text-gray-400">Cost</dt>
                <dd className="text-gray-900 dark:text-white">--</dd>
              </div>
            </dl>
          </div>

          {/* Feedback */}
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
            <h2 className="mb-4 text-lg font-semibold text-gray-900 dark:text-white">Feedback</h2>
            <p className="text-gray-500 dark:text-gray-400">Rate this generation...</p>
          </div>
        </div>
      </div>
    </div>
  );
}
