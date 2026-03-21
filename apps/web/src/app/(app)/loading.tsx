export default function AppLoading() {
  return (
    <div className="container mx-auto px-4 py-8">
      <div className="animate-pulse">
        <div className="h-8 w-48 bg-bg-sunken rounded mb-8" />
        <div className="grid gap-6 md:grid-cols-3 mb-8">
          {[...Array(3)].map((_, i) => (
            <div
              key={i}
              className="rounded-lg border border-border-subtle bg-bg-elevated p-6"
            >
              <div className="h-4 w-20 bg-bg-sunken rounded mb-3" />
              <div className="h-8 w-24 bg-bg-sunken rounded mb-2" />
              <div className="h-3 w-32 bg-bg-sunken rounded" />
            </div>
          ))}
        </div>
        <div className="rounded-lg border border-border-subtle bg-bg-elevated p-6">
          <div className="space-y-4">
            {[...Array(4)].map((_, i) => (
              <div key={i} className="flex items-center gap-4">
                <div className="h-10 w-10 bg-bg-sunken rounded" />
                <div className="flex-1">
                  <div className="h-4 w-32 bg-bg-sunken rounded mb-2" />
                  <div className="h-3 w-24 bg-bg-sunken rounded" />
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
