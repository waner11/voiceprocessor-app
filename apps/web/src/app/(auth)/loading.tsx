export default function AuthLoading() {
  return (
    <div className="space-y-6 animate-pulse">
      <div className="text-center">
        <div className="h-7 w-40 bg-bg-sunken rounded mx-auto mb-3" />
        <div className="h-4 w-48 bg-bg-sunken rounded mx-auto" />
      </div>
      <div className="space-y-4">
        {[...Array(3)].map((_, i) => (
          <div key={i}>
            <div className="h-4 w-16 bg-bg-sunken rounded mb-2" />
            <div className="h-10 w-full bg-bg-sunken rounded" />
          </div>
        ))}
        <div className="h-10 w-full bg-bg-sunken rounded mt-6" />
      </div>
    </div>
  );
}
