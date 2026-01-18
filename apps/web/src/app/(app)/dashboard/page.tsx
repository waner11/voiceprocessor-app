import Link from "next/link";

export default function DashboardPage() {
  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="mb-8 text-3xl font-bold">Dashboard</h1>

      <div className="grid gap-6 md:grid-cols-3">
        <div className="rounded-lg border p-6">
          <h2 className="mb-2 text-lg font-semibold">Credits</h2>
          <p className="text-3xl font-bold">--</p>
          <p className="text-sm text-gray-500">characters remaining</p>
        </div>

        <div className="rounded-lg border p-6">
          <h2 className="mb-2 text-lg font-semibold">Generations</h2>
          <p className="text-3xl font-bold">--</p>
          <p className="text-sm text-gray-500">total generations</p>
        </div>

        <div className="rounded-lg border p-6">
          <h2 className="mb-2 text-lg font-semibold">Audio Duration</h2>
          <p className="text-3xl font-bold">--</p>
          <p className="text-sm text-gray-500">total hours</p>
        </div>
      </div>

      <div className="mt-8">
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-xl font-semibold">Recent Generations</h2>
          <Link href="/generations" className="text-sm hover:underline">
            View all
          </Link>
        </div>
        <div className="rounded-lg border p-8 text-center text-gray-500">
          No generations yet. Start by creating your first audiobook.
        </div>
      </div>

      <div className="mt-8 flex justify-center">
        <Link
          href="/generate"
          className="rounded-lg bg-black px-6 py-3 text-white hover:bg-gray-800"
        >
          Create New Generation
        </Link>
      </div>
    </div>
  );
}
