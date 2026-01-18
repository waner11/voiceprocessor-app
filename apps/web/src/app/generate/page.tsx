export default function GeneratePage() {
  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="mb-8 text-3xl font-bold">Generate Audio</h1>

      <div className="grid gap-8 lg:grid-cols-3">
        <div className="lg:col-span-2 space-y-6">
          {/* TextInput component will go here */}
          <div className="rounded-lg border p-6">
            <h2 className="mb-4 text-lg font-semibold">Text Input</h2>
            <textarea
              className="w-full h-64 rounded-lg border p-4 resize-none"
              placeholder="Paste your text or upload a file..."
            />
            <div className="mt-2 flex justify-between text-sm text-gray-500">
              <span>Detected: --</span>
              <span>0 characters</span>
            </div>
          </div>

          {/* VoiceSelector component will go here */}
          <div className="rounded-lg border p-6">
            <h2 className="mb-4 text-lg font-semibold">Voice Selection</h2>
            <p className="text-gray-500">Select a voice for your audio...</p>
          </div>
        </div>

        <div className="space-y-6">
          {/* Routing strategy */}
          <div className="rounded-lg border p-6">
            <h2 className="mb-4 text-lg font-semibold">Routing Strategy</h2>
            <div className="space-y-2">
              <label className="flex items-center gap-2">
                <input type="radio" name="routing" defaultChecked />
                <span>Balanced (Recommended)</span>
              </label>
              <label className="flex items-center gap-2">
                <input type="radio" name="routing" />
                <span>Best Quality</span>
              </label>
              <label className="flex items-center gap-2">
                <input type="radio" name="routing" />
                <span>Lowest Cost</span>
              </label>
              <label className="flex items-center gap-2">
                <input type="radio" name="routing" />
                <span>Fastest</span>
              </label>
            </div>
          </div>

          {/* CostEstimate component will go here */}
          <div className="rounded-lg border p-6">
            <h2 className="mb-4 text-lg font-semibold">Cost Estimate</h2>
            <p className="text-gray-500">Enter text to see estimate...</p>
          </div>

          <div className="flex flex-col gap-2">
            <button className="rounded-lg border px-6 py-3 hover:bg-gray-50">
              Preview (Free)
            </button>
            <button className="rounded-lg bg-black px-6 py-3 text-white hover:bg-gray-800">
              Generate Full Audio
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
