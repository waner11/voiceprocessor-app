export default function VoicesPage() {
  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="mb-8 text-3xl font-bold">Voice Catalog</h1>

      <div className="mb-6 flex flex-wrap gap-4">
        <input
          type="text"
          placeholder="Search voices..."
          className="flex-1 min-w-[200px] rounded-lg border px-4 py-2"
        />
        <select className="rounded-lg border px-4 py-2">
          <option>All Languages</option>
          <option>English</option>
          <option>Spanish</option>
          <option>French</option>
          <option>German</option>
        </select>
        <select className="rounded-lg border px-4 py-2">
          <option>All Providers</option>
          <option>ElevenLabs</option>
          <option>OpenAI</option>
          <option>Google</option>
          <option>Amazon Polly</option>
        </select>
        <select className="rounded-lg border px-4 py-2">
          <option>All Styles</option>
          <option>Narrator</option>
          <option>Conversational</option>
          <option>Dramatic</option>
          <option>Professional</option>
        </select>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {/* Voice cards will be populated here */}
        <div className="rounded-lg border p-6 text-center text-gray-500">
          Loading voices...
        </div>
      </div>
    </div>
  );
}
