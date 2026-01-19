export default function AuthLoading() {
  return (
    <div className="space-y-6 animate-pulse">
      <div className="text-center">
        <div className="h-7 w-40 bg-gray-700 rounded mx-auto mb-3" />
        <div className="h-4 w-48 bg-gray-800 rounded mx-auto" />
      </div>
      <div className="space-y-4">
        {[...Array(3)].map((_, i) => (
          <div key={i}>
            <div className="h-4 w-16 bg-gray-800 rounded mb-2" />
            <div className="h-10 w-full bg-gray-800 rounded" />
          </div>
        ))}
        <div className="h-10 w-full bg-gray-700 rounded mt-6" />
      </div>
    </div>
  );
}
